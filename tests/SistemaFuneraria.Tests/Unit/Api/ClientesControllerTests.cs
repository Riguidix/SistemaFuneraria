using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Api.Controllers.V1;
using SistemaFuneraria.Application.Servicios.Clientes;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Api;

/// <summary>
/// Pruebas unitarias para ClientesController (SF-032).
/// Valida el ciclo CRUD, permisos RBAC por rol y la exportación a CSV UTF-8.
/// </summary>
[Trait("Ticket", "SF-032")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Unit")]
public class ClientesControllerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly ClienteService _clienteService;

    public ClientesControllerTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestClientesApi_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _clienteService = new ClienteService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private ClientesController CrearController(string rol = "Administrador")
    {
        var controller = new ClientesController(_clienteService);
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

    private async Task<Cliente> CrearClienteSemillaAsync(
        string nombre = "Laura",
        string apellidos = "Castañeda",
        string curp = "CALA951118MDFRR012",
        string telefono = "5512849021",
        string correo = "laura.c@email.mx",
        string direccion = "Av. Insurgentes Sur 123")
    {
        var cliente = Cliente.Crear(nombre, apellidos, curp, telefono, correo, direccion);
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();
        return cliente;
    }

    #region 1. Metadatos de Seguridad y Roles (RBAC)

    [Fact(DisplayName = "SF-032: ClientesController debe tener Route api/v1/clientes y Authorize Administrador,Vendedor")]
    public void Controlador_DebeTenerAtributosDeRutaYAutorizacionEsperados()
    {
        var type = typeof(ClientesController);

        var routeAttr = type.GetCustomAttribute<RouteAttribute>();
        routeAttr.Should().NotBeNull();
        routeAttr!.Template.Should().Be("api/v1/clientes");

        var apiCtrlAttr = type.GetCustomAttribute<ApiControllerAttribute>();
        apiCtrlAttr.Should().NotBeNull();

        var authAttr = type.GetCustomAttribute<AuthorizeAttribute>();
        authAttr.Should().NotBeNull();
        authAttr!.Roles.Should().Be("Administrador,Vendedor");
    }

    [Fact(DisplayName = "SF-032 (BDD): Endpoint Eliminar debe restringirse exclusivamente al rol Administrador")]
    public void EndpointEliminar_DebeEstarRestringidoExclusivamenteAAdministrador()
    {
        var method = typeof(ClientesController).GetMethod(nameof(ClientesController.Eliminar));
        method.Should().NotBeNull();

        var httpDeleteAttr = method!.GetCustomAttribute<HttpDeleteAttribute>();
        httpDeleteAttr.Should().NotBeNull();
        httpDeleteAttr!.Template.Should().Be("{id:guid}");

        var authAttr = method!.GetCustomAttribute<AuthorizeAttribute>();
        authAttr.Should().NotBeNull();
        authAttr!.Roles.Should().Be("Administrador");
    }

    #endregion

    #region 2. Búsqueda y Consulta (GET, GET /{id})

    [Fact(DisplayName = "SF-032: Buscar sin filtro debe retornar 200 OK con todos los clientes")]
    public async Task Buscar_SinFiltro_DebeRetornar200OkConTodosLosClientes()
    {
        // Arrange
        await CrearClienteSemillaAsync("Laura", "Castañeda", "CALA951118MDFRR012", "5512849021", "laura@email.mx");
        await CrearClienteSemillaAsync("Carlos", "Vega", "VEMC800101HDFRRN01", "5599887766", "carlos@email.mx");
        var controller = CrearController();

        // Act
        var result = await controller.Buscar(null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var clientes = okResult.Value.Should().BeAssignableTo<IReadOnlyList<ClienteDto>>().Subject;
        clientes.Should().HaveCount(2);
    }

    [Fact(DisplayName = "SF-032: Buscar con filtro debe retornar 200 OK con clientes coincidentes")]
    public async Task Buscar_ConFiltro_DebeRetornar200OkConClientesCoincidentes()
    {
        // Arrange
        await CrearClienteSemillaAsync("Laura", "Castañeda", "CALA951118MDFRR012", "5512849021", "laura@email.mx");
        await CrearClienteSemillaAsync("Carlos", "Vega", "VEMC800101HDFRRN01", "5599887766", "carlos@email.mx");
        var controller = CrearController();

        // Act
        var result = await controller.Buscar("Vega", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var clientes = okResult.Value.Should().BeAssignableTo<IReadOnlyList<ClienteDto>>().Subject;
        clientes.Should().ContainSingle();
        clientes[0].Nombre.Should().Be("Carlos");
    }

    [Fact(DisplayName = "SF-032: ObtenerPorId con cliente existente debe retornar 200 OK")]
    public async Task ObtenerPorId_ClienteExistente_DebeRetornar200OkConCliente()
    {
        // Arrange
        var cliente = await CrearClienteSemillaAsync();
        var controller = CrearController();

        // Act
        var result = await controller.ObtenerPorId(cliente.Id, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var dto = okResult.Value.Should().BeOfType<ClienteDto>().Subject;
        dto.Id.Should().Be(cliente.Id);
        dto.Nombre.Should().Be("Laura");
    }

    [Fact(DisplayName = "SF-032: ObtenerPorId con cliente inexistente debe retornar 404 Not Found")]
    public async Task ObtenerPorId_ClienteInexistente_DebeRetornar404NotFound()
    {
        // Arrange
        var controller = CrearController();

        // Act
        var result = await controller.ObtenerPorId(Guid.NewGuid(), CancellationToken.None);

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    #region 3. Registro (POST)

    [Fact(DisplayName = "SF-032: Registrar con datos válidos debe retornar 201 Created")]
    public async Task Registrar_ConDatosValidos_DebeRetornar201CreatedYRutaObtenerPorId()
    {
        // Arrange
        var controller = CrearController();
        var request = new RegistrarClienteRequest(
            "Roberto",
            "Mendoza",
            "MENR850505MDFRRN01",
            "5544332211",
            "roberto.m@email.mx",
            "Calle Reforma 456");

        // Act
        var result = await controller.Registrar(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.ActionName.Should().Be(nameof(ClientesController.ObtenerPorId));
        createdResult.RouteValues.Should().ContainKey("id");
    }

    [Fact(DisplayName = "SF-032: Registrar con datos inválidos debe retornar 400 Bad Request")]
    public async Task Registrar_ConDatosInvalidos_DebeRetornar400BadRequest()
    {
        // Arrange
        var controller = CrearController();
        var requestInvalido = new RegistrarClienteRequest(
            "",
            "Mendoza",
            "MENR850505MDFRRN01",
            "5544332211",
            "roberto.m@email.mx",
            "Calle Reforma 456");

        // Act
        var result = await controller.Registrar(requestInvalido, CancellationToken.None);

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    #endregion

    #region 4. Actualización (PUT /{id})

    [Fact(DisplayName = "SF-032 (BDD): Actualizar con datos válidos por Administrador debe retornar 200 OK")]
    public async Task Actualizar_ConDatosValidos_DebeRetornar200Ok()
    {
        // Arrange
        var cliente = await CrearClienteSemillaAsync();
        var controller = CrearController(rol: "Administrador");
        var request = new ActualizarClienteRequest(
            "Laura Elena",
            "Castañeda Morales",
            "CALA951118MDFRR012",
            "5599881122",
            "laura.nueva@email.mx",
            "Av. Universidad 999");

        // Act
        var result = await controller.Actualizar(cliente.Id, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        var clienteActualizado = await _dbContext.Clientes.FindAsync(cliente.Id);
        clienteActualizado!.Nombre.Should().Be("Laura Elena");
        clienteActualizado.Telefono.Should().Be("5599881122");
    }

    [Fact(DisplayName = "SF-032: Actualizar cliente inexistente debe retornar 404 Not Found")]
    public async Task Actualizar_ClienteInexistente_DebeRetornar404NotFound()
    {
        // Arrange
        var controller = CrearController();
        var request = new ActualizarClienteRequest(
            "Laura", "Castañeda", "CALA951118MDFRR012", "5512849021", "laura@email.mx");

        // Act
        var result = await controller.Actualizar(Guid.NewGuid(), request, CancellationToken.None);

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    #region 5. Eliminación (DELETE /{id})

    [Fact(DisplayName = "SF-032 (BDD): Eliminar cliente sin contratos por Administrador debe retornar 200 OK o 204 NoContent")]
    public async Task Eliminar_ClienteSinContratos_DebeRemoverDeBaseDeDatosYRetornarExito()
    {
        // Arrange
        var cliente = await CrearClienteSemillaAsync();
        var controller = CrearController(rol: "Administrador");

        // Act
        var result = await controller.Eliminar(cliente.Id, CancellationToken.None);

        // Assert
        if (result is NoContentResult noContent)
        {
            noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
        else if (result is OkResult okResult)
        {
            okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
        else
        {
            result.Should().BeAssignableTo<StatusCodeResult>()
                .Which.StatusCode.Should().BeOneOf(StatusCodes.Status200OK, StatusCodes.Status204NoContent);
        }

        var clienteEnDb = await _dbContext.Clientes.FindAsync(cliente.Id);
        clienteEnDb.Should().BeNull();
    }

    [Fact(DisplayName = "SF-032: Eliminar cliente inexistente debe retornar 404 Not Found")]
    public async Task Eliminar_ClienteInexistente_DebeRetornar404NotFound()
    {
        // Arrange
        var controller = CrearController(rol: "Administrador");

        // Act
        var result = await controller.Eliminar(Guid.NewGuid(), CancellationToken.None);

        // Assert
        var problemResult = result.Should().BeOfType<ObjectResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    #region 6. Exportación a CSV UTF-8 (GET /exportar)

    [Fact(DisplayName = "SF-032: ExportarDirectorio debe retornar archivo CSV UTF-8 con cabecera y datos")]
    public async Task ExportarDirectorio_DebeRetornarArchivoCsvUtf8ConNombreDirectorioClientes()
    {
        // Arrange
        await CrearClienteSemillaAsync("Laura", "Castañeda", "CALA951118MDFRR012", "5512849021", "laura@email.mx", "Av. Insurgentes 123");
        var controller = CrearController();

        // Act
        var result = await controller.ExportarDirectorio(CancellationToken.None);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Contain("text/csv");
        fileResult.FileDownloadName.Should().Be("directorio-clientes.csv");

        var contenidoCsv = Encoding.UTF8.GetString(fileResult.FileContents);
        contenidoCsv.TrimStart('\uFEFF').Should().StartWith("Id,Nombre,Apellidos,CURP,Telefono,Correo,Direccion");
        contenidoCsv.Should().Contain("Laura");
        contenidoCsv.Should().Contain("Castañeda");
        contenidoCsv.Should().Contain("CALA951118MDFRR012");
        contenidoCsv.Should().Contain("5512849021");
    }

    #endregion
}
