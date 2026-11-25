using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Application.Servicios.Dashboard;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Application;

[Trait("Ticket", "SF-088")]
[Trait("Capa", "Application")]
[Trait("Tipo", "Unit")]
public class DashboardReportesServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly DashboardReportesService _service;

    public DashboardReportesServiceTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestDashboardReportes_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _service = new DashboardReportesService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    [Fact(DisplayName = "SF-088: Base de datos vacía protege divisiones entre cero devolviendo 0%")]
    public async Task BaseDeDatosVacia_NoDebeFallarPorDivisionEntreCero()
    {
        var resDash = await _service.ObtenerResumenDashboardAsync();
        resDash.EsExitoso.Should().BeTrue();
        resDash.Valor.Disponibilidad.PorcentajeOcupacion.Should().Be(0m);
        resDash.Valor.VentasYCobranza.Should().HaveCount(6);

        var resRep = await _service.ObtenerIndicadoresReportesAsync();
        resRep.EsExitoso.Should().BeTrue();
        resRep.Valor.ConversionCotizacion.Should().Be(0m);
        resRep.Valor.TicketPromedio.Should().Be(0m);
        resRep.Valor.IndiceMorosidad.Should().Be(0m);
    }

    [Fact(DisplayName = "SF-088 (BDD): Dashboard resumen calcula disponibilidad por gavetas, ventas del mes y actividad reciente")]
    public async Task ObtenerResumenYReportes_ConDatosReales_DebeCalcularAgregadosExactos()
    {
        var cliente = Cliente.Crear("Laura", "Castañeda", "CASL850315MDFRRN01", "5511112222", "laura@ejemplo.mx", "Dir");
        var lote2 = Lote.Crear("JN-A-001", "Jardín Norte", "A", 2, new Dinero(120000m));
        var lote3 = Lote.Crear("JC-B-002", "Jardín Central", "B", 3, new Dinero(150000m));
        var lote4 = Lote.Crear("RO-C-003", "Rosales", "C", 4, new Dinero(180000m));

        lote3.MarcarVendido();
        var venta = Venta.CrearContado("CTR-2026-0200", cliente.Id, lote3.Id, "vend-1", lote3.PrecioContado, DateTimeOffset.UtcNow);

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.Lotes.AddRangeAsync(lote2, lote3, lote4);
        await _dbContext.Ventas.AddAsync(venta);
        await _dbContext.SaveChangesAsync();

        var resDash = await _service.ObtenerResumenDashboardAsync();
        resDash.EsExitoso.Should().BeTrue();
        resDash.Valor.Kpis.VentasDelMes.Should().Be(150000m);
        resDash.Valor.Disponibilidad.TotalDisponibles.Should().Be(2);
        resDash.Valor.Disponibilidad.Gavetas2.Should().Be(1);
        resDash.Valor.Disponibilidad.Gavetas4.Should().Be(1);
        resDash.Valor.ActividadReciente.Should().ContainSingle(a => a.Referencia == "CTR-2026-0200");

        var resRep = await _service.ObtenerIndicadoresReportesAsync("Noviembre 2025", "Jardines del Valle");
        resRep.EsExitoso.Should().BeTrue();
        resRep.Valor.TicketPromedio.Should().Be(150000m);
        resRep.Valor.MetaMensual.ObjetivoMensual.Should().Be(600000m);
        resRep.Valor.MetaMensual.Acumulado.Should().Be(150000m);
        resRep.Valor.MetaMensual.PorcentajeAvance.Should().Be(25m);
    }

    [Fact(DisplayName = "Dashboard: Soporta rango anual (12m), unifica Ventas + Pagos con FolioContrato y calcula variaciones")]
    public async Task ObtenerResumenDashboard_ConRangoAnioPagosYMesAnterior_DebeUnificarActividadYVariacion()
    {
        var ahora = DateTimeOffset.UtcNow;
        var mesAnterior = ahora.AddMonths(-1);

        var cliente = Cliente.Crear("Carlos", "Mendoza", "MEMC800101HDFRRN01", "5522223333", "carlos@ejemplo.mx", "Dir");
        var lote1 = Lote.Crear("JN-A-010", "Jardín Norte", "A", 2, new Dinero(120000m));
        var lote2 = Lote.Crear("JC-B-011", "Jardín Central", "B", 3, new Dinero(150000m));

        lote1.MarcarVendido();
        lote2.MarcarVendido();

        var ventaMesAnterior = Venta.CrearContado("CTR-2026-0101", cliente.Id, lote1.Id, "vend-1", lote1.PrecioContado, mesAnterior);
        var ventaActual = Venta.CrearContado("CTR-2026-0102", cliente.Id, lote2.Id, "vend-1", lote2.PrecioContado, ahora.AddMinutes(-10));
        var pagoReciente = Pago.Crear(
            folio: "PAG-2026-000050",
            ventaId: ventaActual.Id,
            financiamientoId: null,
            mensualidadId: null,
            cajeroId: "cajero-1",
            tipo: TipoPago.Contado,
            metodo: MetodoPago.TransferenciaBancaria,
            referencia: "REF-50",
            montoTotal: new Dinero(150000m),
            montoCapital: new Dinero(150000m),
            montoInteres: new Dinero(0m),
            saldoAnterior: new Dinero(150000m),
            saldoPosterior: new Dinero(0m),
            claveIdempotencia: "idem-dash-50",
            fechaPago: ahora);

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.Lotes.AddRangeAsync(lote1, lote2);
        await _dbContext.Ventas.AddRangeAsync(ventaMesAnterior, ventaActual);
        await _dbContext.Pagos.AddAsync(pagoReciente);
        await _dbContext.SaveChangesAsync();

        var resAnio = await _service.ObtenerResumenDashboardAsync("anio");

        resAnio.EsExitoso.Should().BeTrue();
        resAnio.Valor.VentasYCobranza.Should().HaveCount(12);
        resAnio.Valor.Kpis.VariacionVentasPct.Should().Be(25.0m);
        resAnio.Valor.ActividadReciente.Should().Contain(a =>
            a.Referencia == "PAG-2026-000050" &&
            a.FolioContrato == "CTR-2026-0102" &&
            a.Movimiento == "Pago de contado");
        resAnio.Valor.ActividadReciente.Should().Contain(a =>
            a.Referencia == "CTR-2026-0102" &&
            a.FolioContrato == "CTR-2026-0102");
    }

    [Fact(DisplayName = "Reportes: ObtenerIndicadoresReportesAsync filtra por periodo mes-actual vs historico")]
    public async Task ObtenerIndicadoresReportes_DebeFiltrarPorPeriodoMesActualEHistorico()
    {
        var hoy = DateTimeOffset.UtcNow;
        var enMesActual = new DateTimeOffset(hoy.Year, hoy.Month, 1, 12, 0, 0, TimeSpan.Zero);
        var haceDosMeses = enMesActual.AddMonths(-2);

        var cliente = Cliente.Crear("Elena", "Vargas", "VARE800101MDFRRN02", "5533334444", "elena@ejemplo.mx", "Dir");
        var lote1 = Lote.Crear("RO-A-001", "Rosales", "A", 2, new Dinero(120000m));
        var lote2 = Lote.Crear("RO-A-002", "Rosales", "A", 4, new Dinero(180000m));

        lote1.MarcarVendido();
        lote2.MarcarVendido();

        var ventaAntigua = Venta.CrearContado("CTR-2026-0301", cliente.Id, lote1.Id, "vend-1", lote1.PrecioContado, haceDosMeses);
        var ventaActual = Venta.CrearContado("CTR-2026-0302", cliente.Id, lote2.Id, "vend-1", lote2.PrecioContado, enMesActual);

        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.Lotes.AddRangeAsync(lote1, lote2);
        await _dbContext.Ventas.AddRangeAsync(ventaAntigua, ventaActual);
        await _dbContext.SaveChangesAsync();

        var resMes = await _service.ObtenerIndicadoresReportesAsync("mes-actual", "Jardines del Valle");
        resMes.EsExitoso.Should().BeTrue();
        resMes.Valor.MetaMensual.Acumulado.Should().Be(180000m);
        resMes.Valor.TicketPromedio.Should().Be(180000m);

        var resHist = await _service.ObtenerIndicadoresReportesAsync("historico", "Todas las sucursales");
        resHist.EsExitoso.Should().BeTrue();
        resHist.Valor.Periodo.Should().Be("Histórico completo");
        resHist.Valor.MetaMensual.Acumulado.Should().Be(300000m);
        resHist.Valor.TicketPromedio.Should().Be(150000m);
    }
}
