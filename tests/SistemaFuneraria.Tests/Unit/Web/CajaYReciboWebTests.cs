using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

[Trait("Ticket", "SF-082")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class CajaYReciboWebTests
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

    [Fact(DisplayName = "SF-079 / SF-080 / SF-081: Caja/Index.cshtml y cajaPage.js conectan cobranzaApi con X-Idempotency-Key")]
    public void CajaIndex_DebeConectarCobranzaApiYCajaPage()
    {
        var cajaHtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Caja", "Index.cshtml")));
        var cajaJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "pages", "cajaPage.js")));
        var apiJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "api", "cobranzaApi.js")));

        cajaHtml.Should().Contain("/Contratos?tab=Vencidos");
        cajaHtml.Should().Contain("~/js/api/cobranzaApi.js");
        cajaHtml.Should().Contain("~/js/pages/cajaPage.js");
        cajaHtml.Should().Contain("lnkVerReciboPago");

        apiJs.Should().Contain("crypto.randomUUID");
        apiJs.Should().Contain("X-Idempotency-Key");
        apiJs.Should().Contain("/api/v1/cobranza");

        cajaJs.Should().Contain("cobranzaApi.aplicarPagoMensualidad");
        cajaJs.Should().Contain("cobranzaApi.obtenerResumenCaja");
        cajaJs.Should().Contain("/Caja/Recibo?folio=");
    }

    [Fact(DisplayName = "SF-082: Recibo.cshtml y reciboPagoPage.js muestran desglose financiero e impresión @media print")]
    public void CajaRecibo_DebeRenderizarDesgloseEImpresion()
    {
        var reciboHtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Caja", "Recibo.cshtml")));
        var reciboJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "pages", "reciboPagoPage.js")));

        reciboHtml.Should().Contain("@media print");
        reciboHtml.Should().Contain("reciboSaldoAnterior");
        reciboHtml.Should().Contain("reciboMontoCapital");
        reciboHtml.Should().Contain("reciboMontoInteres");
        reciboHtml.Should().Contain("reciboMontoTotal");
        reciboHtml.Should().Contain("reciboSaldoPosterior");
        reciboHtml.Should().Contain("~/js/pages/reciboPagoPage.js");

        reciboJs.Should().Contain("cobranzaApi.obtenerRecibo");
    }
}
