using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Application.DTOs.Cobranza;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Cobranza;
using SistemaFuneraria.Application.Servicios.Ventas;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Application;

[Trait("Ticket", "SF-077")]
[Trait("Capa", "Application")]
[Trait("Tipo", "Unit")]
public class CobranzaServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly VentaService _ventaService;
    private readonly CobranzaService _cobranzaService;

    public CobranzaServiceTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestCobranza_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _ventaService = new VentaService(_dbContext);
        _cobranzaService = new CobranzaService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private async Task<(ContratoFinanciadoResumenDto Contrato, CuotaAmortizacionContratoDto PrimeraCuota)> CrearContratoFinanciadoAsync()
    {
        var sufijo = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var cliente = Cliente.Crear("Elena", "Vargas", $"SF77{sufijo}"[..18], "5511223344", $"elena.{sufijo[..6].ToLowerInvariant()}@test.mx", "Dir 1");
        var lote = Lote.Crear($"RO-B-{Random.Shared.Next(100, 999)}", "Rosales", "B", 2, new Dinero(120000m));
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.Lotes.AddAsync(lote);
        await _dbContext.SaveChangesAsync();

        var res = await _ventaService.RegistrarVentaFinanciadaAsync(
            new RegistrarVentaFinanciadaCommand(cliente.Id, lote.Id, "vend-1", 20, 24));
        res.EsExitoso.Should().BeTrue();

        var amort = await _ventaService.ObtenerAmortizacionPorVentaIdAsync(res.Valor.VentaId);
        amort.EsExitoso.Should().BeTrue();
        return (res.Valor, amort.Valor[0]);
    }

    [Fact(DisplayName = "SF-077 (BDD): AplicarPagoMensualidadAsync paga cuota #1, descuenta capital, emite folio PAG- y suma al Corte del día")]
    public async Task AplicarPagoMensualidadAsync_DebeActualizarCuotaSaldoYCorteDelDia()
    {
        var (contrato, cuota1) = await CrearContratoFinanciadoAsync();

        var resultado = await _cobranzaService.AplicarPagoMensualidadAsync(
            new AplicarPagoMensualidadCommand(
                FolioContrato: contrato.FolioContrato,
                ImporteRecibido: cuota1.CuotaTotal,
                MetodoPago: MetodoPago.TransferenciaBancaria,
                Referencia: "9841",
                ClaveIdempotencia: Guid.NewGuid().ToString(),
                CajeroId: "cajero-01"));

        resultado.EsExitoso.Should().BeTrue();
        var recibo = resultado.Valor;
        recibo.FolioPago.Should().StartWith("PAG-");
        recibo.SaldoAnterior.Should().Be(contrato.CapitalFinanciado);
        recibo.MontoCapital.Should().Be(cuota1.Capital);
        recibo.MontoInteres.Should().Be(cuota1.Interes);
        recibo.SaldoPosterior.Should().Be(contrato.CapitalFinanciado - cuota1.Capital);

        var resumen = await _cobranzaService.ObtenerResumenCajaAsync();
        resumen.EsExitoso.Should().BeTrue();
        resumen.Valor.PagosRecibidosHoy.Should().Be(1);
        resumen.Valor.IngresosDelDia.Should().Be(cuota1.CuotaTotal);
        resumen.Valor.TotalEnCaja.Should().Be(2000.00m + cuota1.CuotaTotal);
    }

    [Fact(DisplayName = "SF-077: Reintento con misma ClaveIdempotencia devuelve el mismo recibo sin duplicar cobro")]
    public async Task AplicarPagoMensualidadAsync_ConMismaClaveIdempotencia_DebeSerIdempotente()
    {
        var (contrato, cuota1) = await CrearContratoFinanciadoAsync();
        var clave = Guid.NewGuid().ToString();
        var cmd = new AplicarPagoMensualidadCommand(contrato.FolioContrato, cuota1.CuotaTotal, MetodoPago.Efectivo, null, clave, "cajero-01");

        var primero = await _cobranzaService.AplicarPagoMensualidadAsync(cmd);
        var segundo = await _cobranzaService.AplicarPagoMensualidadAsync(cmd);

        primero.EsExitoso.Should().BeTrue();
        segundo.EsExitoso.Should().BeTrue();
        segundo.Valor.FolioPago.Should().Be(primero.Valor.FolioPago);

        var fin = await _dbContext.Financiamientos.FindAsync(contrato.FinanciamientoId);
        fin!.SaldoInsoluto.Monto.Should().Be(contrato.CapitalFinanciado - cuota1.Capital);
    }

    [Fact(DisplayName = "SF-077: Importe distinto a la cuota exigible retorna error de validación")]
    public async Task AplicarPagoMensualidadAsync_ConImporteDistinto_DebeRetornarFallo()
    {
        var (contrato, cuota1) = await CrearContratoFinanciadoAsync();

        var res = await _cobranzaService.AplicarPagoMensualidadAsync(
            new AplicarPagoMensualidadCommand(
                contrato.FolioContrato,
                cuota1.CuotaTotal - 100m,
                MetodoPago.Efectivo,
                null,
                Guid.NewGuid().ToString(),
                "cajero-01"));

        res.EsFallo.Should().BeTrue();
    }

    [Fact(DisplayName = "SF-077: DbUpdateConcurrencyException retorna [CONFLICTO] 409")]
    public async Task AplicarPagoMensualidadAsync_ConColisionXmin_DebeRetornarConflicto()
    {
        var dbName = $"TestConcurrency_{Guid.NewGuid():N}";
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using (var seedDb = new AppDbContext(opts))
        {
            var ventaSvc = new VentaService(seedDb);
            var cliente = Cliente.Crear("Carlos", "Ruiz", "RUIC900101HDFRRN09", "5599887766", "carlos@test.mx", "Dir");
            var lote = Lote.Crear("JN-A-777", "Jardín Norte", "A", 2, new Dinero(120000m));
            await seedDb.Clientes.AddAsync(cliente);
            await seedDb.Lotes.AddAsync(lote);
            await seedDb.SaveChangesAsync();

            await ventaSvc.RegistrarVentaFinanciadaAsync(new RegistrarVentaFinanciadaCommand(cliente.Id, lote.Id, "vend", 20, 12));
        }

        await using var conflictDb = new ConcurrencyConflictAppDbContext(opts);
        var venta = await conflictDb.Ventas.FirstAsync();
        var cuota1 = await conflictDb.Mensualidades.OrderBy(m => m.NumeroCuota).FirstAsync();
        var servicio = new CobranzaService(conflictDb);

        var res = await servicio.AplicarPagoMensualidadAsync(
            new AplicarPagoMensualidadCommand(
                venta.FolioContrato,
                cuota1.MontoTotalCuota.Monto,
                MetodoPago.Efectivo,
                null,
                Guid.NewGuid().ToString(),
                "cajero-01"));

        res.EsFallo.Should().BeTrue();
        res.Error.Should().StartWith("[CONFLICTO]");
    }

    private sealed class ConcurrencyConflictAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            throw new DbUpdateConcurrencyException("Simulación de colisión optimista xmin.");
    }
}
