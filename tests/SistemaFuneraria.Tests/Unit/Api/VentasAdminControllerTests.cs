using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
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
/// Pruebas unitarias para los endpoints administrativos de VentasController (SF-063).
/// Valida autorización exclusiva Administrador, PATCH de estado y DELETE con liberación de lote.
/// </summary>
[Trait("Ticket", "SF-063")]
[Trait("Capa", "Api")]
[Trait("Tipo", "Unit")]
public class VentasAdminControllerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly VentaService _ventaService;

    public VentasAdminControllerTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestVentasAdminApi_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _ventaService = new VentaService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private VentasController CrearController(string rol = "Administrador")
    {
        var controller = new VentasController(_ventaService);
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimTypes.Role, rol));
        identity.AddClaim(new Claim(ClaimTypes.Name, "AdminUser"));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "admin-001"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }

    private async Task<(Lote lote, ContratoResumenDto venta)> CrearVentaContadoAsync(string clave = "JN-A-042")
    {
        var cliente = Cliente.Crear("Mariana", "Torres Aguirre", "TOAM850423MDFRRG01", "5512345678", "mariana@email.mx", "Av. Universidad 100");
        var lote = Lote.Crear(clave, "Jardín Norte", "A", 2, new Dinero(120000m));
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.Lotes.AddAsync(lote);
        await _dbContext.SaveChangesAsync();

        var res = await _ventaService.RegistrarVentaContadoAsync(new RegistrarVentaContadoCommand(cliente.Id, lote.Id, "vend-1"));
        return (lote, res.Valor);
    }

    [Fact(DisplayName = "SF-063 (BDD): Administrador cancela venta errónea por PATCH y lote vuelve a estar Disponible")]
    public async Task CambiarEstado_CancelarVenta_DebeRetornar200OkYLiberarLote()
    {
        // Arrange
        var (lote, ventaDto) = await CrearVentaContadoAsync("JN-A-042");
        var controller = CrearController("Administrador");
        var request = new CambiarEstadoVentaRequest(EstadoVenta.Cancelado, "Error de captura en titular");

        // Act
        var actionResult = await controller.CambiarEstado(ventaDto.VentaId, request);

        // Assert
        actionResult.Should().BeOfType<OkResult>();

        var loteEnDb = await _dbContext.Lotes.FindAsync(lote.Id);
        loteEnDb!.Estado.Should().Be(EstadoLote.Disponible);
    }

    [Fact(DisplayName = "SF-063: DELETE venta con motivo válido cancela y libera lote")]
    public async Task CancelarVenta_DeleteConMotivoValido_DebeRetornar200OkYLiberarLote()
    {
        // Arrange
        var (lote, ventaDto) = await CrearVentaContadoAsync("JN-A-088");
        var controller = CrearController("Administrador");

        // Act
        var actionResult = await controller.CancelarVenta(ventaDto.VentaId, "Cancelación por solicitud de cliente");

        // Assert
        actionResult.Should().BeOfType<OkResult>();

        var loteEnDb = await _dbContext.Lotes.FindAsync(lote.Id);
        loteEnDb!.Estado.Should().Be(EstadoLote.Disponible);
    }

    [Fact(DisplayName = "SF-063: PATCH con motivo menor a 10 caracteres debe retornar 400 BadRequest")]
    public async Task CambiarEstado_ConMotivoCorto_DebeRetornar400BadRequest()
    {
        // Arrange
        var (_, ventaDto) = await CrearVentaContadoAsync("JN-A-099");
        var controller = CrearController("Administrador");
        var request = new CambiarEstadoVentaRequest(EstadoVenta.Cancelado, "Corto");

        // Act
        var actionResult = await controller.CambiarEstado(ventaDto.VentaId, request);

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact(DisplayName = "SF-063: Métodos CambiarEstado y CancelarVenta deben exigir rol Administrador")]
    public void EndpointsAdmin_DebenTenerAuthorizeRolAdministrador()
    {
        var tipo = typeof(VentasController);

        var metodoPatch = tipo.GetMethod(nameof(VentasController.CambiarEstado));
        metodoPatch.Should().NotBeNull();
        var authPatch = metodoPatch!.GetCustomAttribute<AuthorizeAttribute>();
        authPatch.Should().NotBeNull();
        authPatch!.Roles.Should().Be("Administrador");

        var metodoDelete = tipo.GetMethod(nameof(VentasController.CancelarVenta));
        metodoDelete.Should().NotBeNull();
        var authDelete = metodoDelete!.GetCustomAttribute<AuthorizeAttribute>();
        authDelete.Should().NotBeNull();
        authDelete!.Roles.Should().Be("Administrador");
    }
}
