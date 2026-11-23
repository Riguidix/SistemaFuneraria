using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Application.Servicios.Dashboard;
using SistemaFuneraria.Core.Entidades;
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
}
