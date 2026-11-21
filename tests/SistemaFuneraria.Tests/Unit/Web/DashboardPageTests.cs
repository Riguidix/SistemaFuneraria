using FluentAssertions;
using ResumenModel = SistemaFuneraria.Web.Pages.Resumen.IndexModel;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class DashboardPageTests
{
    [Fact]
    public void Dashboard_OnGet_DebeCargarKpisGraficasYActividadRecienteConformeFigma()
    {
        // Arrange
        var pageModel = new ResumenModel();

        // Act
        pageModel.OnGet();

        // Assert - 4 KPI Cards requeridos por Captura 1 (SF-089)
        pageModel.VentasMes.Should().Be(486500m, "Ventas del mes según Figma Captura 1");
        pageModel.CobranzaMensual.Should().Be(238240m, "Cobranza mensual según Figma Captura 1");
        pageModel.ContratosActivos.Should().Be(128, "Contratos activos según Figma Captura 1");
        pageModel.PagosVencidos.Should().Be(8, "Pagos vencidos según Figma Captura 1");
        pageModel.MontoPorRecuperar.Should().Be(24860m, "Monto por recuperar según Figma Captura 1");

        // Assert - Disponibilidad de Lotes
        pageModel.LotesDisponibles.Should().Be(64, "Lotes disponibles totales");
        pageModel.Lotes2Gavetas.Should().Be(28);
        pageModel.Lotes3Gavetas.Should().Be(21);
        pageModel.Lotes4Gavetas.Should().Be(15);
        pageModel.OcupacionPorcentaje.Should().Be(64);

        // Assert - Gráfica de 6 Meses
        pageModel.DatosGrafica.Should().HaveCount(6);
        pageModel.DatosGrafica[0].Mes.Should().Be("Jun");
        pageModel.DatosGrafica[5].Mes.Should().Be("Nov");

        // Assert - Actividad Reciente
        pageModel.ActividadReciente.Should().HaveCount(4);
        pageModel.ActividadReciente[0].Cliente.Should().Be("Roberto Méndez");
        pageModel.ActividadReciente[0].Referencia.Should().Be("CTR-2025-0182");
        pageModel.ActividadReciente[0].Importe.Should().Be(6248.50m);
        pageModel.ActividadReciente[0].Estado.Should().Be("Aplicado");
    }
}
