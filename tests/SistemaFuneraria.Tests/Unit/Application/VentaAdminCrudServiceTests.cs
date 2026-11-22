using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Ventas;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Application;

/// <summary>
/// Pruebas unitarias para las operaciones administrativas CRUD de VentaService (SF-063).
/// Valida la cancelación con liberación de lote, transiciones de estado y validaciones de motivo.
/// </summary>
[Trait("Ticket", "SF-063")]
[Trait("Capa", "Application")]
[Trait("Tipo", "Unit")]
public class VentaAdminCrudServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly VentaService _ventaService;

    public VentaAdminCrudServiceTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestVentaAdmin_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _ventaService = new VentaService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private async Task<(Cliente cliente, Lote lote, ContratoResumenDto venta)> CrearVentaContadoAsync(string clave = "JN-A-042")
    {
        var cliente = Cliente.Crear("Mariana", "Torres Aguirre", "TOAM850423MDFRRG01", "5512345678", "mariana@email.mx", "Av. Universidad 100");
        var lote = Lote.Crear(clave, "Jardín Norte", "A", 2, new Dinero(120000m));
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.Lotes.AddAsync(lote);
        await _dbContext.SaveChangesAsync();

        var res = await _ventaService.RegistrarVentaContadoAsync(new RegistrarVentaContadoCommand(cliente.Id, lote.Id, "vend-1"));
        return (cliente, lote, res.Valor);
    }

    [Fact(DisplayName = "SF-063 (BDD): Cancelar venta errónea pasa contrato a Cancelado y regresa lote a Disponible")]
    public async Task CambiarEstadoContratoAdminAsync_CancelarVenta_DebeLiberarLoteADisponible()
    {
        // Arrange
        var (_, lote, ventaDto) = await CrearVentaContadoAsync("JN-A-042");
        lote.Estado.Should().Be(EstadoLote.Vendido);

        // Act
        var res = await _ventaService.CambiarEstadoContratoAdminAsync(
            ventaDto.VentaId,
            EstadoVenta.Cancelado,
            "Error de captura en titular");

        // Assert
        res.EsExitoso.Should().BeTrue();

        var ventaEnDb = await _dbContext.Ventas.FindAsync(ventaDto.VentaId);
        ventaEnDb!.Estado.Should().Be(EstadoVenta.Cancelado);
        ventaEnDb.MotivoCancelacion.Should().Be("Error de captura en titular");

        var loteEnDb = await _dbContext.Lotes.FindAsync(lote.Id);
        loteEnDb!.Estado.Should().Be(EstadoLote.Disponible);
    }

    [Fact(DisplayName = "SF-063: Motivo menor a 10 caracteres debe retornar error de validación")]
    public async Task CambiarEstadoContratoAdminAsync_ConMotivoCorto_DebeRetornarFallo()
    {
        // Arrange
        var (_, _, ventaDto) = await CrearVentaContadoAsync("JN-A-099");

        // Act
        var res = await _ventaService.CambiarEstadoContratoAdminAsync(
            ventaDto.VentaId,
            EstadoVenta.Cancelado,
            "Error");

        // Assert
        res.EsFallo.Should().BeTrue();
        res.Error.Should().Contain("10 caracteres");
    }

    [Fact(DisplayName = "SF-063: Venta inexistente debe retornar No Encontrado")]
    public async Task CambiarEstadoContratoAdminAsync_ConVentaInexistente_DebeRetornarNoEncontrado()
    {
        // Act
        var res = await _ventaService.CambiarEstadoContratoAdminAsync(
            Guid.NewGuid(),
            EstadoVenta.Cancelado,
            "Cancelación de prueba válida");

        // Assert
        res.EsFallo.Should().BeTrue();
        res.Error.Should().StartWith("[NO_ENCONTRADO]");
    }
}
