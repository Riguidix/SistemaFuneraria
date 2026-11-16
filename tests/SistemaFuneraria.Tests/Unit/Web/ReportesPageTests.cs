using FluentAssertions;
using SistemaFuneraria.Web.Pages.Reportes;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ReportesPageTests
{
    [Fact]
    public void Reportes_OnGet_DebeCargarKpisAnaliticaYMetasConformeFigmaCaptura11()
    {
        // Arrange
        var pageModel = new IndexModel();

        // Act
        pageModel.OnGet();

        // Assert - Filtros
        pageModel.PeriodoSeleccionado.Should().Be("Noviembre 2025");
        pageModel.SucursalSeleccionada.Should().Be("Jardines del Valle");

        // Assert - 3 KPI Cards requeridos por Captura 11 (SF-090)
        pageModel.ConversionCotizacion.Should().Be(32.8m, "Conversión de cotización según Figma");
        pageModel.ConversionComparativa.Should().Be("+4.2 pts vs. octubre");

        pageModel.TicketPromedio.Should().Be(147420m, "Ticket promedio según Figma");
        pageModel.TicketSubtexto.Should().Be("Lotes de 2 y 3 gavetas");

        pageModel.IndiceMorosidad.Should().Be(6.2m, "Índice de morosidad según Figma");
        pageModel.MorosidadMeta.Should().Be("Meta menor a 8.0%");

        // Assert - Ventas por tipo de lote (3 barras horizontales)
        pageModel.VentasPorTipoLote.Should().HaveCount(3);
        pageModel.VentasPorTipoLote[0].TipoLote.Should().Be("Lote 2 gavetas");
        pageModel.VentasPorTipoLote[0].Monto.Should().Be(198000m);
        pageModel.VentasPorTipoLote[0].Porcentaje.Should().Be(41);

        pageModel.VentasPorTipoLote[1].TipoLote.Should().Be("Lote 3 gavetas");
        pageModel.VentasPorTipoLote[1].Monto.Should().Be(168500m);
        pageModel.VentasPorTipoLote[1].Porcentaje.Should().Be(35);

        pageModel.VentasPorTipoLote[2].TipoLote.Should().Be("Lote 4 gavetas");
        pageModel.VentasPorTipoLote[2].Monto.Should().Be(120000m);
        pageModel.VentasPorTipoLote[2].Porcentaje.Should().Be(24);

        // Assert - Meta Mensual (Dona 81%)
        pageModel.PorcentajeCumplimientoMeta.Should().Be(81);
        pageModel.AcumuladoMes.Should().Be(486500m);
        pageModel.ObjetivoMensual.Should().Be(600000m);
    }
}
