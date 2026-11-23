using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Application.Servicios.Contratos;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Application;

[Trait("Ticket", "SF-068")]
[Trait("Capa", "Application")]
[Trait("Tipo", "Unit")]
public class CarteraContratosServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly CarteraContratosService _carteraService;

    public CarteraContratosServiceTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestCarteraContratos_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _carteraService = new CarteraContratosService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact(DisplayName = "SF-068 (BDD): Tab Vencidos devuelve contrato en mora con acción Cobrar y calcula MontoPorRecuperar y KPIs")]
    public async Task ObtenerCarteraAsync_DebeCalcularKpisYFiltrarTabsCorrectamente()
    {
        var cliCorriente = Cliente.Crear("Laura", "Castañeda", "CASL850315MDFRRN01", "5511111111", "laura@test.mx", "Dir 1");
        var cliVencida = Cliente.Crear("Elena", "Vargas", "VARE750810MDFRGL03", "5522222222", "elena@test.mx", "Dir 2");
        var cliLiquidada = Cliente.Crear("Sofía", "Lozano", "LOZS900101MDFRRN05", "5533333333", "sofia@test.mx", "Dir 3");

        var lote1 = Lote.Crear("JC-C-118", "Jardín Central", "C", 3, new Dinero(150000m));
        var lote2 = Lote.Crear("RO-B-027", "Rosales", "B", 2, new Dinero(120000m));
        var lote3 = Lote.Crear("JN-A-099", "Jardín Norte", "A", 2, new Dinero(120000m));

        lote1.MarcarVendido();
        lote2.MarcarVendido();
        lote3.MarcarVendido();

        await _dbContext.Clientes.AddRangeAsync(cliCorriente, cliVencida, cliLiquidada);
        await _dbContext.Lotes.AddRangeAsync(lote1, lote2, lote3);

        var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(12);
        var renglones1 = GeneradorAmortizacion.Generar(120000m, tasaMensual, 12);
        var venta1 = Venta.CrearFinanciada("CTR-2025-0189", cliCorriente.Id, lote1.Id, "vend-1", lote1.PrecioContado, DateTimeOffset.UtcNow);
        var fin1 = Financiamiento.Crear(venta1.Id, 20m, new Dinero(30000m), new Dinero(120000m), 12, tasaAnual, renglones1, new DateOnly(2026, 12, 18));

        var renglones2 = GeneradorAmortizacion.Generar(96000m, tasaMensual, 12);
        var venta2 = Venta.CrearFinanciada("CTR-2024-0118", cliVencida.Id, lote2.Id, "vend-1", lote2.PrecioContado, DateTimeOffset.UtcNow);
        var fin2 = Financiamiento.Crear(venta2.Id, 20m, new Dinero(24000m), new Dinero(96000m), 12, tasaAnual, renglones2, new DateOnly(2025, 10, 2));
        fin2.Mensualidades.First().MarcarVencida();
        fin2.MarcarEnMora();

        var venta3 = Venta.CrearContado("CTR-2026-0001", cliLiquidada.Id, lote3.Id, "vend-1", lote3.PrecioContado, DateTimeOffset.UtcNow);

        await _dbContext.Ventas.AddRangeAsync(venta1, venta2, venta3);
        await _dbContext.Financiamientos.AddRangeAsync(fin1, fin2);
        await _dbContext.SaveChangesAsync();

        var resVencidos = await _carteraService.ObtenerCarteraAsync(tab: "Vencidos");
        resVencidos.EsExitoso.Should().BeTrue();
        resVencidos.Valor.Kpis.TotalActivos.Should().Be(2);
        resVencidos.Valor.Kpis.TotalAlCorriente.Should().Be(1);
        resVencidos.Valor.Kpis.TotalConAtraso.Should().Be(1);
        resVencidos.Valor.Kpis.TotalLiquidadosAnio.Should().Be(1);
        resVencidos.Valor.Kpis.MontoPorRecuperar.Should().Be(renglones2[0].PagoMensual);

        resVencidos.Valor.Items.Should().ContainSingle();
        var itemVencido = resVencidos.Valor.Items[0];
        itemVencido.FolioContrato.Should().Be("CTR-2024-0118");
        itemVencido.Estado.Should().Be("Vencido");
        itemVencido.AccionSugerida.Should().Be("Cobrar");

        var resLiquidados = await _carteraService.ObtenerCarteraAsync(tab: "Liquidados");
        resLiquidados.Valor.Items.Should().ContainSingle();
        resLiquidados.Valor.Items[0].SaldoInsoluto.Should().Be(0m);
        resLiquidados.Valor.Items[0].ProximoPagoTexto.Should().Be("Liquidado");

        var resBusqueda = await _carteraService.ObtenerCarteraAsync(buscar: "Elena", tab: "Todos");
        resBusqueda.Valor.Items.Should().ContainSingle(i => i.FolioContrato == "CTR-2024-0118");

        var resDetallePorFolio = await _carteraService.ObtenerDetalleAsync("CTR-2024-0118");
        resDetallePorFolio.EsExitoso.Should().BeTrue();
        resDetallePorFolio.Valor.EstadoVisual.Should().Be("Vencido");
        resDetallePorFolio.Valor.Amortizacion.Should().HaveCount(12);
    }
}
