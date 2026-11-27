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
/// Pruebas unitarias para IVentaService.RegistrarVentaContadoAsync bajo escenario CP-02 (SF-060).
/// Valida la atomicidad transaccional: Venta Liquidada + Lote Vendido + Pago Único en caja.
/// </summary>
[Trait("Ticket", "SF-060")]
[Trait("Capa", "Application")]
[Trait("Tipo", "Unit")]
public class VentaContadoServiceTests : IDisposable
{
    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly IVentaService _ventaService;
    private readonly FakeTimeProvider _timeProvider;

    public VentaContadoServiceTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestVentaContado_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 11, 22, 10, 0, 0, TimeSpan.Zero));
        _ventaService = new VentaService(_dbContext, _timeProvider);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private async Task<Cliente> CrearClienteAsync(string nombre = "Mariana", string apellidos = "Torres Aguirre")
    {
        var cliente = Cliente.Crear(nombre, apellidos, "TOAM850423MDFRRG01", "5512345678", "mariana.torres@email.mx", "Av. Revolución 123, Col. Centro");
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.SaveChangesAsync();
        return cliente;
    }

    private async Task<Lote> CrearLoteAsync(
        string clave = "JN-A-042",
        EstadoLote estado = EstadoLote.Disponible,
        decimal precio = 120000m)
    {
        int gavetas = precio == 180000m ? 4 : precio == 150000m ? 3 : 2;
        var lote = Lote.Crear(clave, "Jardín Norte", "Andador A · Fila 4", gavetas, new Dinero(precio));
        if (estado == EstadoLote.Reservado)
        {
            lote.Reservar();
        }
        else if (estado == EstadoLote.Vendido)
        {
            lote.MarcarVendido();
        }
        else if (estado == EstadoLote.Mantenimiento)
        {
            lote.ActualizarDatosAdmin("Jardín Norte", "Andador A · Fila 4", gavetas, new Dinero(precio), EstadoLote.Mantenimiento);
        }

        await _dbContext.Lotes.AddAsync(lote);
        await _dbContext.SaveChangesAsync();
        return lote;
    }

    [Fact(DisplayName = "SF-060 (BDD / CP-02): Venta de contado debe crear Venta Liquidada, lote Vendido y Pago único")]
    public async Task RegistrarVentaContadoAsync_ConDatosValidos_DebeCrearVentaLiquidadaLoteVendidoYPagoUnico()
    {
        // Arrange
        var cliente = await CrearClienteAsync("Mariana", "Torres Aguirre");
        var lote = await CrearLoteAsync("JN-A-042", EstadoLote.Disponible, 120000m);
        var command = new RegistrarVentaContadoCommand(
            ClienteId: cliente.Id,
            LoteId: lote.Id,
            VendedorId: "vendedor-user-001",
            MetodoPago: MetodoPago.TransferenciaBancaria,
            ReferenciaPago: "SPEI-998822");

        // Act
        var resultado = await _ventaService.RegistrarVentaContadoAsync(command);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var resumen = resultado.Valor;
        resumen.FolioContrato.Should().Be("CTR-2026-0190");
        resumen.Estado.Should().Be("Liquidado");
        resumen.Modalidad.Should().Be("Contado");
        resumen.PrecioPactado.Should().Be(120000m);
        resumen.ClienteNombre.Should().Be("Mariana Torres Aguirre");
        resumen.LoteClave.Should().Be("JN-A-042");
        resumen.FolioPago.Should().StartWith("PAG-2026-");

        // Verificación en base de datos
        var loteEnDb = await _dbContext.Lotes.FindAsync(lote.Id);
        loteEnDb!.Estado.Should().Be(EstadoLote.Vendido);

        var ventaEnDb = await _dbContext.Ventas.FirstOrDefaultAsync(v => v.Id == resumen.VentaId);
        ventaEnDb.Should().NotBeNull();
        ventaEnDb!.FolioContrato.Should().Be("CTR-2026-0190");
        ventaEnDb.Modalidad.Should().Be(ModalidadVenta.Contado);
        ventaEnDb.Estado.Should().Be(EstadoVenta.Liquidado);
        ventaEnDb.PrecioPactado.Monto.Should().Be(120000m);

        var pagoEnDb = await _dbContext.Pagos.FirstOrDefaultAsync(p => p.VentaId == resumen.VentaId);
        pagoEnDb.Should().NotBeNull();
        pagoEnDb!.Tipo.Should().Be(TipoPago.Contado);
        pagoEnDb.Metodo.Should().Be(MetodoPago.TransferenciaBancaria);
        pagoEnDb.Referencia.Should().Be("SPEI-998822");
        pagoEnDb.MontoTotal.Monto.Should().Be(120000m);
        pagoEnDb.SaldoPosterior.Monto.Should().Be(0m);
    }

    [Fact(DisplayName = "SF-060: Venta de contado sobre lote Reservado debe permitir venta y marcar como Vendido")]
    public async Task RegistrarVentaContadoAsync_ConLoteReservado_DebePermitirVentaYPasarAVendido()
    {
        // Arrange
        var cliente = await CrearClienteAsync();
        var lote = await CrearLoteAsync("RO-B-027", EstadoLote.Reservado, 180000m);
        var command = new RegistrarVentaContadoCommand(cliente.Id, lote.Id, "vendedor-001");

        // Act
        var resultado = await _ventaService.RegistrarVentaContadoAsync(command);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        var loteEnDb = await _dbContext.Lotes.FindAsync(lote.Id);
        loteEnDb!.Estado.Should().Be(EstadoLote.Vendido);
    }

    [Fact(DisplayName = "SF-060: Venta de contado sobre lote ya Vendido debe retornar conflicto y no duplicar venta")]
    public async Task RegistrarVentaContadoAsync_ConLoteYaVendido_DebeRetornarConflictoYNoModificarEstado()
    {
        // Arrange
        var cliente = await CrearClienteAsync();
        var lote = await CrearLoteAsync("LM-A-014", EstadoLote.Vendido, 120000m);
        var command = new RegistrarVentaContadoCommand(cliente.Id, lote.Id, "vendedor-001");

        // Act
        var resultado = await _ventaService.RegistrarVentaContadoAsync(command);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().Contain("vendido");
        resultado.Error.Should().StartWith("[CONFLICTO]");

        var totalVentas = await _dbContext.Ventas.CountAsync();
        totalVentas.Should().Be(0);
    }

    [Fact(DisplayName = "SF-060: Venta de contado sobre lote en Mantenimiento debe retornar conflicto")]
    public async Task RegistrarVentaContadoAsync_ConLoteMantenimiento_DebeRetornarConflicto()
    {
        // Arrange
        var cliente = await CrearClienteAsync();
        var lote = await CrearLoteAsync("JN-M-001", EstadoLote.Mantenimiento, 120000m);
        var command = new RegistrarVentaContadoCommand(cliente.Id, lote.Id, "vendedor-001");

        // Act
        var resultado = await _ventaService.RegistrarVentaContadoAsync(command);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().StartWith("[CONFLICTO]");
    }

    [Fact(DisplayName = "SF-060: Venta de contado con cliente inexistente debe retornar No Encontrado")]
    public async Task RegistrarVentaContadoAsync_ConClienteInexistente_DebeRetornarNoEncontrado()
    {
        // Arrange
        var lote = await CrearLoteAsync("JN-A-042");
        var command = new RegistrarVentaContadoCommand(Guid.NewGuid(), lote.Id, "vendedor-001");

        // Act
        var resultado = await _ventaService.RegistrarVentaContadoAsync(command);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().StartWith("[NO_ENCONTRADO]");
    }

    [Fact(DisplayName = "SF-060: Venta de contado con lote inexistente debe retornar No Encontrado")]
    public async Task RegistrarVentaContadoAsync_ConLoteInexistente_DebeRetornarNoEncontrado()
    {
        // Arrange
        var cliente = await CrearClienteAsync();
        var command = new RegistrarVentaContadoCommand(cliente.Id, Guid.NewGuid(), "vendedor-001");

        // Act
        var resultado = await _ventaService.RegistrarVentaContadoAsync(command);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().StartWith("[NO_ENCONTRADO]");
    }
}
