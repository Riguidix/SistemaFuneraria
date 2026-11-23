using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

[Trait("Ticket", "SF-089")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class Milestone9WebTests
{
    private static string RutaWeb(string rutaRelativa)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "SistemaFuneraria.sln")))
        {
            dir = dir.Parent;
        }
        dir.Should().NotBeNull("debe existir SistemaFuneraria.sln en el repositorio");
        return Path.Combine(dir!.FullName, "src", "SistemaFuneraria.Web", rutaRelativa);
    }

    [Fact(DisplayName = "SF-087: cobranzaApi.js y cajaPage.js conectan mensualidad, abono-capital y liquidación")]
    public void CajaWeb_DebeConectarTresTarjetasDeOperacion()
    {
        var apiJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "api", "cobranzaApi.js")));
        var cajaJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "pages", "cajaPage.js")));

        apiJs.Should().Contain("abono-capital");
        apiJs.Should().Contain("liquidacion");
        cajaJs.Should().Contain("cobranzaApi.aplicarAbonoCapital");
        cajaJs.Should().Contain("cobranzaApi.aplicarLiquidacion");
        cajaJs.Should().NotContain("estarán habilitados en el Milestone v0.9.0");
    }

    [Fact(DisplayName = "SF-089 / SF-090 / SF-092: Resumen, Reportes y Configuracion conectan con sus APIs reales")]
    public void ResumenReportesYConfiguracion_DebenEstarConectados()
    {
        var resumenHtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Resumen", "Index.cshtml")));
        var reportesHtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Reportes", "Index.cshtml")));
        var configHtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Configuracion", "Index.cshtml")));

        resumenHtml.Should().Contain("kpiVentasMes");
        resumenHtml.Should().Contain("~/js/api/reportesApi.js");
        resumenHtml.Should().Contain("~/js/pages/resumenPage.js");

        reportesHtml.Should().Contain("kpiConversion");
        reportesHtml.Should().Contain("~/js/api/reportesApi.js");
        reportesHtml.Should().Contain("~/js/pages/reportesPage.js");

        configHtml.Should().Contain("tbodyUsuariosConfig");
        configHtml.Should().Contain("~/js/api/configuracionApi.js");
        configHtml.Should().Contain("~/js/pages/configuracionPage.js");
    }
}
