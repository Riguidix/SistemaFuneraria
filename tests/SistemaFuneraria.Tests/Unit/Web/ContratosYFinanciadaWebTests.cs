using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

[Trait("Ticket", "SF-070")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class ContratosYFinanciadaWebTests
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

    [Fact(DisplayName = "SF-070: Contratos/Index.cshtml y contratosIndexPage.js conectan los 4 KPIs, tabs y acción Cobrar -> /Caja")]
    public void ContratosIndex_DebeConectarKpisTabsYContratosApi()
    {
        var indexHtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Contratos", "Index.cshtml")));
        var indexJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "pages", "contratosIndexPage.js")));
        var apiJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "api", "contratosApi.js")));

        indexHtml.Should().Contain("kpiTotalActivos");
        indexHtml.Should().Contain("kpiTotalAlCorriente");
        indexHtml.Should().Contain("kpiTotalConAtraso");
        indexHtml.Should().Contain("kpiTotalLiquidados");
        indexHtml.Should().Contain("~/js/api/contratosApi.js");
        indexHtml.Should().Contain("~/js/pages/contratosIndexPage.js");

        indexJs.Should().Contain("contratosApi.obtenerCartera");
        indexJs.Should().Contain("/Caja?contrato=");
        indexJs.Should().Contain("/Contratos/Detalle?folio=");

        apiJs.Should().Contain("/api/v1/contratos");
        apiJs.Should().Contain("obtenerTablaAmortizacion");
    }

    [Fact(DisplayName = "SF-071 / SF-073: Paso 4 soporta Crédito vía registrarVentaFinanciada y Detalle renderiza amortización")]
    public void Paso4YContratoDetalle_DebenSoportarCreditoYAmortizacion()
    {
        var paso4Js = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "ventas", "paso4Confirmacion.js")));
        var detalleCshtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Contratos", "Detalle.cshtml")));
        var detalleJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "pages", "contratoDetallePage.js")));

        paso4Js.Should().Contain("registrarVentaFinanciada");
        paso4Js.Should().Contain("construirPayloadVentaFinanciada");
        paso4Js.Should().NotContain("estará disponible en el Milestone v0.7.0");

        detalleCshtml.Should().Contain("id=\"tbodyAmortizacion\"");
        detalleCshtml.Should().Contain("id=\"badgeConteoAmortizacion\"");
        detalleCshtml.Should().Contain("~/js/api/contratosApi.js");

        detalleJs.Should().Contain("contratosApi.obtenerDetalleContrato");
        detalleJs.Should().Contain("tbodyAmortizacion");
    }
}
