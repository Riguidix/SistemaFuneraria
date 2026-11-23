using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

[Trait("Ticket", "SF-051")]
[Trait("Ticket", "SF-052")]
[Trait("Ticket", "SF-053")]
[Trait("Ticket", "SF-054")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class WizardApiConnectionTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact(DisplayName = "SF-051..054: Nueva.cshtml debe cargar wrappers reales de API y guardia 403")]
    public void NuevaVenta_DebeCargarWrappersApiYContenedor403()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Ventas", "Nueva.cshtml");
        var html = File.ReadAllText(path);

        html.Should().Contain("~/js/api/clientesApi.js");
        html.Should().Contain("~/js/api/lotesApi.js");
        html.Should().Contain("~/js/api/simuladorApi.js");
        html.Should().Contain("~/js/ventas/wizardState.js");
        html.Should().Contain("wizardAccesoRestringido");
        html.Should().Contain("wizardContenidoPrincipal");
    }

    [Fact(DisplayName = "SF-052: _Paso1Cliente debe incluir Dirección, clienteId oculto y cero datos demo hardcoded")]
    public void Paso1ClientePartial_DebeIncluirDireccionYSinHardcode()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Ventas", "Partials", "_Paso1Cliente.cshtml");
        var html = File.ReadAllText(path);

        html.Should().Contain("txtClienteDireccion");
        html.Should().Contain("hdnClienteId");
        html.Should().Contain("badgeClienteSeleccionado");
        html.Should().NotContain("value=\"Mariana\"");
        html.Should().NotContain("value=\"Torres Aguirre\"");
        html.Should().NotContain("value=\"mariana.torres@email.mx\"");
    }

    [Fact(DisplayName = "SF-052..054: Los JS de Pasos 1-3 deben usar UMD y consumir clientesApi, lotesApi y simuladorApi")]
    public void WizardScripts_DebenConsumirApisRealesYSoportarUmd()
    {
        var p1 = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "paso1Cliente.js"));
        var p2 = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "paso2Lote.js"));
        var p3 = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "paso3Financiamiento.js"));

        p1.Should().Contain("module.exports");
        p1.Should().Contain("clientesApi");
        p1.Should().Contain("wizardState");
        p1.Should().NotContain("clientesDemo");

        p2.Should().Contain("module.exports");
        p2.Should().Contain("lotesApi");
        p2.Should().Contain("obtenerCatalogo");
        p2.Should().Contain("wizardState");

        p3.Should().Contain("module.exports");
        p3.Should().Contain("simuladorApi");
        p3.Should().Contain("obtenerPlanes");
        p3.Should().Contain("cotizar");
        p3.Should().Contain("wizardState");
    }
}
