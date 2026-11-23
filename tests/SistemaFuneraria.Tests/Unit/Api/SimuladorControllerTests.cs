using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Api.Controllers.V1;
using SistemaFuneraria.Application.DTOs.Simulador;
using SistemaFuneraria.Application.Servicios.Lotes;
using SistemaFuneraria.Application.Servicios.Simulador;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Api;

/// <summary>
/// Pruebas unitarias para SimuladorController (SF-049).
/// Valida los endpoints de catálogo de planes y cotización en modalidad Contado/Crédito bajo CP-10.
/// </summary>
[Trait("Ticket", "SF-049")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Unit")]
public class SimuladorControllerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly ILoteService _loteService;
    private readonly ISimuladorService _simuladorService;

    public SimuladorControllerTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestSimuladorApi_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _loteService = new LoteService(_dbContext);
        _simuladorService = new SimuladorService();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private SimuladorController CrearController()
    {
        var controller = new SimuladorController(_simuladorService, _loteService);
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimTypes.Role, "Vendedor"));
        identity.AddClaim(new Claim(ClaimTypes.Name, "AgenteVentas"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }

    private async Task<Lote> CrearLoteAsync(string clave, int gavetas, decimal precio)
    {
        var lote = Lote.Crear(clave, "Jardín Central", "Andador C · Fila 11", gavetas, new Dinero(precio));
        await _dbContext.Lotes.AddAsync(lote);
        await _dbContext.SaveChangesAsync();
        return lote;
    }

    #region 1. Metadatos de Ruta y Seguridad

    [Fact(DisplayName = "SF-049: SimuladorController debe tener Route api/v1/simulador, ApiController y Authorize")]
    public void Controlador_DebeTenerAtributosDeRutaYAutorizacionEsperados()
    {
        var type = typeof(SimuladorController);

        type.BaseType.Should().Be(typeof(ApiControllerBase));

        var routeAttr = type.GetCustomAttribute<RouteAttribute>();
        routeAttr.Should().NotBeNull();
        routeAttr!.Template.Should().Be("api/v1/simulador");

        var apiCtrlAttr = type.GetCustomAttribute<ApiControllerAttribute>();
        apiCtrlAttr.Should().NotBeNull();

        var authAttr = type.GetCustomAttribute<AuthorizeAttribute>();
        authAttr.Should().NotBeNull();
    }

    #endregion

    #region 2. GET /api/v1/simulador/planes

    [Fact(DisplayName = "SF-049: ObtenerPlanes debe retornar 200 OK con enganches y plazos autorizados de Figma Captura 7")]
    public void ObtenerPlanes_DebeRetornar200OkConPlanesAutorizados()
    {
        // Arrange
        var controller = CrearController();

        // Act
        var result = controller.ObtenerPlanes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var dto = okResult.Value.Should().BeOfType<PlanesFinanciamientoDto>().Subject;
        dto.PorcentajesEnganche.Should().BeEquivalentTo(new[] { 10, 20, 30 });
        dto.Plazos.Should().HaveCount(4);

        dto.Plazos.Select(p => p.Meses).Should().ContainInOrder(12, 18, 24, 48);
        dto.Plazos.Select(p => p.TasaAnualPorcentaje).Should().ContainInOrder(16m, 18m, 20m, 24m);
        dto.Plazos.First(p => p.Meses == 24).Descripcion.Should().Be("24 meses · 20% anual");
    }

    #endregion

    #region 3. POST /api/v1/simulador/cotizar

    [Fact(DisplayName = "SF-049: Cotizar modalidad Contado debe retornar 200 OK con liquidación sin tabla")]
    public async Task Cotizar_ModalidadContadoValida_DebeRetornar200Ok()
    {
        // Arrange
        var controller = CrearController();
        var request = new SimularCotizacionRequest(
            LoteId: null,
            Gavetas: 2,
            Modalidad: "Contado",
            PorcentajeEnganche: 0,
            PlazoMeses: 0,
            FechaPrimerPago: new DateOnly(2026, 12, 1));

        // Act
        var result = await controller.Cotizar(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var dto = okResult.Value.Should().BeOfType<SimulacionCotizacionDto>().Subject;
        dto.Modalidad.Should().Be("Contado");
        dto.PrecioLista.Should().Be(120000.00m);
        dto.MontoEnganche.Should().Be(120000.00m);
        dto.SaldoAFinanciar.Should().Be(0.00m);
        dto.TablaAmortizacion.Should().BeEmpty();
    }

    [Fact(DisplayName = "SF-049: Cotizar modalidad Crédito Captura 7 ($120k, 20%, 24 meses) debe retornar 200 OK con cuota exacta $4,887.54")]
    public async Task Cotizar_ModalidadCreditoValidaCaptura7_DebeRetornar200Ok()
    {
        // Arrange
        var controller = CrearController();
        var request = new SimularCotizacionRequest(
            LoteId: null,
            Gavetas: 2,
            Modalidad: "Credito",
            PorcentajeEnganche: 20,
            PlazoMeses: 24,
            FechaPrimerPago: new DateOnly(2026, 12, 1));

        // Act
        var result = await controller.Cotizar(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var dto = okResult.Value.Should().BeOfType<SimulacionCotizacionDto>().Subject;
        dto.Modalidad.Should().Be("Credito");
        dto.PrecioLista.Should().Be(120000.00m);
        dto.MontoEnganche.Should().Be(24000.00m);
        dto.SaldoAFinanciar.Should().Be(96000.00m);
        dto.MensualidadEstimada.Should().Be(4886.00m);
        dto.TablaAmortizacion.Should().HaveCount(24);
    }

    [Fact(DisplayName = "SF-049: Cotizar con LoteId existente debe consultar gavetas del lote y retornar 200 OK")]
    public async Task Cotizar_ConLoteIdExistente_DebeResolverGavetasYRetornar200Ok()
    {
        // Arrange
        var lote = await CrearLoteAsync("JC-C-118", 3, 150000m);
        var controller = CrearController();
        var request = new SimularCotizacionRequest(
            LoteId: lote.Id,
            Gavetas: null,
            Modalidad: "Credito",
            PorcentajeEnganche: 10,
            PlazoMeses: 12,
            FechaPrimerPago: null);

        // Act
        var result = await controller.Cotizar(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<SimulacionCotizacionDto>().Subject;
        dto.PrecioLista.Should().Be(150000.00m);
        dto.MontoEnganche.Should().Be(15000.00m);
        dto.SaldoAFinanciar.Should().Be(135000.00m);
    }

    [Fact(DisplayName = "SF-049: Cotizar con LoteId inexistente debe retornar 404 Not Found")]
    public async Task Cotizar_ConLoteIdInexistente_DebeRetornar404NotFound()
    {
        // Arrange
        var controller = CrearController();
        var request = new SimularCotizacionRequest(
            LoteId: Guid.NewGuid(),
            Gavetas: 2,
            Modalidad: "Credito",
            PorcentajeEnganche: 20,
            PlazoMeses: 24,
            FechaPrimerPago: null);

        // Act
        var result = await controller.Cotizar(request);

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact(DisplayName = "SF-049 (BDD / CP-10): Rechazo de plazo inválido de 15 meses por API debe retornar 400 Bad Request")]
    public async Task Cotizar_PlazoOEngancheInvalido_CP10_DebeRetornar400BadRequest()
    {
        // Arrange
        var controller = CrearController();
        var request = new SimularCotizacionRequest(
            LoteId: null,
            Gavetas: 2,
            Modalidad: "Credito",
            PorcentajeEnganche: 15,
            PlazoMeses: 15,
            FechaPrimerPago: null);

        // Act
        var result = await controller.Cotizar(request);

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var details = problemResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        details.Detail.Should().Contain("12, 18, 24 o 48 meses");
    }

    #endregion
}
