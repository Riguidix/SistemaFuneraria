using FluentAssertions;
using ResumenModel = SistemaFuneraria.Web.Pages.Resumen.IndexModel;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class DashboardPageTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string LeerArchivoWeb(params string[] segmentos)
    {
        var ruta = Path.Combine(new[] { RepoRoot, "src", "SistemaFuneraria.Web" }.Concat(segmentos).ToArray());
        File.Exists(ruta).Should().BeTrue($"debe existir '{ruta}'");
        return File.ReadAllText(ruta);
    }

    [Fact]
    public void Dashboard_OnGet_DebeInicializarEstadoLimpioSinDatosMockHardcodeados()
    {
        var pageModel = new ResumenModel();

        pageModel.OnGet();

        pageModel.FechaKicker.Should().NotBeNullOrWhiteSpace();
        pageModel.AnioActual.Should().BeGreaterThanOrEqualTo(2025);
        pageModel.VentasMes.Should().Be(0m);
        pageModel.CobranzaMensual.Should().Be(0m);
        pageModel.ContratosActivos.Should().Be(0);
        pageModel.PagosVencidos.Should().Be(0);
        pageModel.MontoPorRecuperar.Should().Be(0m);
        pageModel.LotesDisponibles.Should().Be(0);
        pageModel.DatosGrafica.Should().BeEmpty();
        pageModel.ActividadReciente.Should().BeEmpty();
    }

    [Fact]
    public void DashboardVistaYJs_DebenCorregirBotonesYConectarContratosReales()
    {
        var vistaHtml = LeerArchivoWeb("Pages", "Resumen", "Index.cshtml");
        var layoutHtml = LeerArchivoWeb("Pages", "Shared", "_Layout.cshtml");
        var resumenJs = LeerArchivoWeb("wwwroot", "js", "pages", "resumenPage.js");
        var reportesApiJs = LeerArchivoWeb("wwwroot", "js", "api", "reportesApi.js");

        vistaHtml.Should().NotContain("+ Nueva venta");
        layoutHtml.Should().NotContain("+ Nueva venta");
        vistaHtml.Should().Contain("Nueva venta");
        vistaHtml.Should().Contain("id=\"selectRangoGrafica\"");
        vistaHtml.Should().Contain("id=\"yAxisGridLines\"");
        vistaHtml.Should().Contain("id=\"dashboardErrorBanner\"");
        vistaHtml.Should().Contain("id=\"btnReintentarDashboard\"");
        vistaHtml.Should().Contain("href=\"/Contratos\"");

        layoutHtml.Should().Contain("id=\"sidebarCajaBadge\"");

        resumenJs.Should().Contain("requireAuth(");
        resumenJs.Should().Contain("calcularEscalaGrafica");
        resumenJs.Should().Contain("variacionVentasPct");
        resumenJs.Should().Contain("selectRangoGrafica");
        resumenJs.Should().Contain("/Contratos/Detalle?folio=");
        resumenJs.Should().Contain("Sin movimientos registrados todavía.");

        reportesApiJs.Should().Contain("rango");
    }
}
