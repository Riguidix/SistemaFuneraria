using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Api.Controllers.V1;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Ventas;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Api;

/// <summary>
/// Pruebas unitarias para VentasController (SF-061).
/// Valida registro de venta de contado, consulta por ID, validaciones de entrada y manejo de errores HTTP.
/// </summary>
[Trait("Ticket", "SF-061")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Unit")]
public class VentasControllerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly VentaService _ventaService;

    public VentasControllerTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestVentasApi_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _ventaService = new VentaService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private VentasController CrearController(string vendedorId = "vendedor-user-001", string rol = "Vendedor")
    {
        var controller = new VentasController(_ventaService);
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, vendedorId));
        identity.AddClaim(new Claim(ClaimTypes.Name, "AgenteVentas"));
        identity.AddClaim(new Claim(ClaimTypes.Role, rol));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }

    private async Task<Cliente> CrearClienteAsync(string nombre = "Mariana", string apellidos = "Torres Aguirre")
    {
        var cliente = Cliente.Crear(nombre, apellidos, "TOAM850423MDFRRG01", "5512345678", "mariana.torres@email.mx", "Av. Insurgentes 456");
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();
        return cliente;
    }

    private async Task<Lote> CrearLoteAsync(string clave, EstadoLote estado = EstadoLote.Disponible, decimal precio = 120000m)
    {
        var lote = Lote.Crear(clave, "Jardín Norte", "A", 2, new Dinero(precio));
        if (estado == EstadoLote.Reservado)
        {
            lote.Reservar();
        }
        else if (estado == EstadoLote.Vendido)
        {
            lote.MarcarVendido();
        }
        await _dbContext.Lotes.AddAsync(lote);
        await _dbContext.SaveChangesAsync();
        return lote;
    }

    [Fact(DisplayName = "SF-061 (BDD): Intento de vender por API un lote ya Vendido responde 409 Conflict")]
    public async Task RegistrarContado_ConLoteYaVendido_DebeResponder409Conflict()
    {
        // Arrange
        var cliente = await CrearClienteAsync();
        var lote = await CrearLoteAsync("LM-A-014", EstadoLote.Vendido);
        var controller = CrearController();
        var request = new RegistrarVentaContadoRequest(cliente.Id, lote.Id);

        // Act
        var actionResult = await controller.RegistrarContado(request);

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Contain("El lote seleccionado ya fue vendido");
    }

    [Fact(DisplayName = "SF-061: Registrar venta contado con datos válidos responde 201 Created")]
    public async Task RegistrarContado_ConDatosValidos_DebeResponder201Created()
    {
        // Arrange
        var cliente = await CrearClienteAsync();
        var lote = await CrearLoteAsync("JN-A-042", EstadoLote.Disponible, 120000m);
        var controller = CrearController("vendedor-001");
        var request = new RegistrarVentaContadoRequest(cliente.Id, lote.Id, MetodoPago.TransferenciaBancaria, "TRANSF-001");

        // Act
        var actionResult = await controller.RegistrarContado(request);

        // Assert
        var createdResult = actionResult.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var dto = createdResult.Value.Should().BeOfType<ContratoResumenDto>().Subject;
        dto.FolioContrato.Should().StartWith("CTR-");
        dto.Estado.Should().Be("Liquidado");
        dto.Modalidad.Should().Be("Contado");
        dto.PrecioPactado.Should().Be(120000m);
    }

    [Fact(DisplayName = "SF-061: Registrar contado con ClienteId o LoteId vacío responde 400 BadRequest")]
    public async Task RegistrarContado_ConIdsVacios_DebeResponder400BadRequest()
    {
        // Arrange
        var controller = CrearController();
        var requestSinCliente = new RegistrarVentaContadoRequest(Guid.Empty, Guid.NewGuid());
        var requestSinLote = new RegistrarVentaContadoRequest(Guid.NewGuid(), Guid.Empty);

        // Act
        var res1 = await controller.RegistrarContado(requestSinCliente);
        var res2 = await controller.RegistrarContado(requestSinLote);

        // Assert
        var obj1 = res1.Should().BeOfType<ObjectResult>().Subject;
        obj1.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var obj2 = res2.Should().BeOfType<ObjectResult>().Subject;
        obj2.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact(DisplayName = "SF-061: ObtenerPorId con venta existente responde 200 OK")]
    public async Task ObtenerPorId_ConVentaExistente_DebeResponder200Ok()
    {
        // Arrange
        var cliente = await CrearClienteAsync();
        var lote = await CrearLoteAsync("JN-A-055");
        var regRes = await _ventaService.RegistrarVentaContadoAsync(new RegistrarVentaContadoCommand(cliente.Id, lote.Id, "vend-1"));
        var ventaId = regRes.Valor.VentaId;
        var controller = CrearController();

        // Act
        var actionResult = await controller.ObtenerPorId(ventaId);

        // Assert
        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var dto = okResult.Value.Should().BeOfType<ContratoResumenDto>().Subject;
        dto.VentaId.Should().Be(ventaId);
        dto.FolioContrato.Should().Be(regRes.Valor.FolioContrato);
    }

    [Fact(DisplayName = "SF-061: ObtenerPorId con venta inexistente responde 404 NotFound")]
    public async Task ObtenerPorId_ConVentaInexistente_DebeResponder404NotFound()
    {
        // Arrange
        var controller = CrearController();

        // Act
        var actionResult = await controller.ObtenerPorId(Guid.NewGuid());

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
