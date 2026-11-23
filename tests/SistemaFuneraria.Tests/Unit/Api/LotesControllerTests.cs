using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Api.Controllers.V1;
using SistemaFuneraria.Application.DTOs.Lotes;
using SistemaFuneraria.Application.Servicios.Lotes;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Api;

/// <summary>
/// Pruebas unitarias para LotesController (SF-042).
/// Valida el catálogo paginado, endpoints CRUD y políticas de seguridad RBAC.
/// </summary>
[Trait("Ticket", "SF-042")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Unit")]
public class LotesControllerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly LoteService _loteService;

    public LotesControllerTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestLotesApi_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _loteService = new LoteService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private LotesController CrearController(string rol = "Administrador")
    {
        var controller = new LotesController(_loteService);
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimTypes.Role, rol));
        identity.AddClaim(new Claim(ClaimTypes.Name, "TestUser"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }

    private async Task<Lote> CrearLoteSemillaAsync(
        string clave = "JN-A-042",
        string seccion = "Jardín Norte",
        string andadorFila = "Andador A · Fila 4",
        int gavetas = 2,
        decimal precio = 120000m)
    {
        var lote = Lote.Crear(clave, seccion, andadorFila, gavetas, new Dinero(precio));
        await _dbContext.Lotes.AddAsync(lote);
        await _dbContext.SaveChangesAsync();
        return lote;
    }

    #region 1. Metadatos de Seguridad y Roles RBAC

    [Fact(DisplayName = "SF-042: LotesController debe tener Route api/v1/lotes y Authorize")]
    public void Controlador_DebeTenerAtributosDeRutaYAutorizacionEsperados()
    {
        var type = typeof(LotesController);

        var routeAttr = type.GetCustomAttribute<RouteAttribute>();
        routeAttr.Should().NotBeNull();
        routeAttr!.Template.Should().Be("api/v1/lotes");

        var apiCtrlAttr = type.GetCustomAttribute<ApiControllerAttribute>();
        apiCtrlAttr.Should().NotBeNull();

        var authAttr = type.GetCustomAttribute<AuthorizeAttribute>();
        authAttr.Should().NotBeNull();
    }

    [Theory(DisplayName = "SF-042 (BDD): Endpoints POST, PUT y PATCH deben restringirse al rol Administrador")]
    [InlineData(nameof(LotesController.Crear))]
    [InlineData(nameof(LotesController.Actualizar))]
    [InlineData(nameof(LotesController.CambiarEstado))]
    public void EndpointsMutacion_DebenEstarRestringidosExclusivamenteAAdministrador(string nombreMetodo)
    {
        var method = typeof(LotesController).GetMethod(nombreMetodo);
        method.Should().NotBeNull();

        var authAttr = method!.GetCustomAttribute<AuthorizeAttribute>();
        authAttr.Should().NotBeNull();
        authAttr!.Roles.Should().Be("Administrador");
    }

    #endregion

    #region 2. Catálogo Paginado y Búsqueda (GET /api/v1/lotes)

    [Fact(DisplayName = "SF-042: ObtenerCatalogo sin filtros debe retornar 200 OK con KPIs e items")]
    public async Task ObtenerCatalogo_SinFiltros_DebeRetornar200OkConResumen()
    {
        // Arrange
        await CrearLoteSemillaAsync("JN-A-042", "Jardín Norte", "Andador A · Fila 4", 2, 120000m);
        await CrearLoteSemillaAsync("JC-C-118", "Jardín Central", "Andador C · Fila 11", 3, 150000m);
        var controller = CrearController();

        // Act
        var result = await controller.ObtenerCatalogo();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var resumen = okResult.Value.Should().BeOfType<ResumenCatalogoLotesDto>().Subject;
        resumen.TotalLotes.Should().Be(2);
        resumen.TotalDisponibles.Should().Be(2);
        resumen.PrecioDesde.Should().Be(120000m);
    }

    [Fact(DisplayName = "SF-042: ObtenerPorId existente debe retornar 200 OK con DTO")]
    public async Task ObtenerPorId_LoteExistente_DebeRetornar200Ok()
    {
        // Arrange
        var lote = await CrearLoteSemillaAsync();
        var controller = CrearController();

        // Act
        var result = await controller.ObtenerPorId(lote.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<LoteItemDto>().Subject;
        dto.Id.Should().Be(lote.Id);
        dto.Clave.Should().Be("JN-A-042");
    }

    [Fact(DisplayName = "SF-042: ObtenerPorId inexistente debe retornar 404 Not Found")]
    public async Task ObtenerPorId_LoteInexistente_DebeRetornar404NotFound()
    {
        // Arrange
        var controller = CrearController();

        // Act
        var result = await controller.ObtenerPorId(Guid.NewGuid());

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact(DisplayName = "SF-046: ObtenerPorClave existente debe retornar 200 OK con DTO técnico")]
    public async Task ObtenerPorClave_LoteExistente_DebeRetornar200Ok()
    {
        // Arrange
        await CrearLoteSemillaAsync("RO-B-027", "Robles", "Andador B · Fila 2", 4, 180000m);
        var controller = CrearController();

        // Act
        var result = await controller.ObtenerPorClave("RO-B-027");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<LoteItemDto>().Subject;
        dto.Clave.Should().Be("RO-B-027");
        dto.Seccion.Should().Be("Robles");
        dto.AndadorFila.Should().Be("Andador B · Fila 2");
        dto.Gavetas.Should().Be(4);
        dto.PrecioContado.Should().Be(180000m);
    }

    [Fact(DisplayName = "SF-046: ObtenerPorClave inexistente debe retornar 404 Not Found")]
    public async Task ObtenerPorClave_LoteInexistente_DebeRetornar404NotFound()
    {
        // Arrange
        var controller = CrearController();

        // Act
        var result = await controller.ObtenerPorClave("NO-EXISTE-999");

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    #region 3. CRUD Administrativo (POST, PUT, PATCH)

    [Fact(DisplayName = "SF-042: Crear lote válido por Administrador debe retornar 201 Created")]
    public async Task Crear_ConDatosValidos_DebeRetornar201CreatedYRutaObtenerPorId()
    {
        // Arrange
        var controller = CrearController(rol: "Administrador");
        var command = new CrearLoteCommand("JN-B-010", "Jardín Norte", "Andador B · Fila 1", 3, 150000m);

        // Act
        var result = await controller.Crear(command);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.ActionName.Should().Be(nameof(LotesController.ObtenerPorId));
        createdResult.RouteValues.Should().ContainKey("id");
    }

    [Fact(DisplayName = "SF-042: Crear lote con clave duplicada debe retornar 409 Conflict")]
    public async Task Crear_ConClaveDuplicada_DebeRetornar409Conflict()
    {
        // Arrange
        await CrearLoteSemillaAsync("JN-A-042");
        var controller = CrearController(rol: "Administrador");
        var command = new CrearLoteCommand("JN-A-042", "Jardín Central", "Andador C", 2, 120000m);

        // Act
        var result = await controller.Crear(command);

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact(DisplayName = "SF-042 (BDD): Actualizar lote existente por Administrador debe retornar 200 OK")]
    public async Task Actualizar_ConDatosValidos_DebeRetornar200Ok()
    {
        // Arrange
        var lote = await CrearLoteSemillaAsync();
        var controller = CrearController(rol: "Administrador");
        var command = new ActualizarLoteCommand("Jardín Norte", "Andador A · Fila 5", 3, 150000m, EstadoLote.Reservado);

        // Act
        var result = await controller.Actualizar(lote.Id, command);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact(DisplayName = "SF-042: CambiarEstado operativo por Administrador debe retornar 200 OK")]
    public async Task CambiarEstado_ConEstadoValido_DebeRetornar200Ok()
    {
        // Arrange
        var lote = await CrearLoteSemillaAsync();
        var controller = CrearController(rol: "Administrador");
        var request = new CambiarEstadoLoteRequest(EstadoLote.Reservado);

        // Act
        var result = await controller.CambiarEstado(lote.Id, request);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    #endregion
}
