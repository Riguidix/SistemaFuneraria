using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

/// <summary>
/// Pruebas unitarias para el wrapper JS simuladorApi.js y el gestor de persistencia reactiva wizardState.js (SF-050).
/// Valida la paridad del contrato con SimuladorController y la persistencia del borrador en sessionStorage.
/// </summary>
[Trait("Ticket", "SF-050")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class SimuladorApiAndWizardStateTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    #region 1. simuladorApi.js Tests

    [Fact(DisplayName = "SF-050: simuladorApi.js debe existir físicamente en wwwroot/js/api/")]
    public void SimuladorApi_ArchivoDebeExistirEnWwwroot()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "simuladorApi.js");
        File.Exists(path).Should().BeTrue("el archivo simuladorApi.js debe existir en wwwroot/js/api/");
    }

    [Fact(DisplayName = "SF-050: simuladorApi.js debe exponer obtenerPlanes y cotizar delegando en apiClient")]
    public void SimuladorApi_DebeExponerMetodosDelContratoYDelegarEnApiClient()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "simuladorApi.js");
        File.Exists(path).Should().BeTrue();
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("/api/v1/simulador", "debe apuntar a la ruta base del simulador");
        contenido.Should().Contain("obtenerPlanes:", "debe exponer método obtenerPlanes");
        contenido.Should().Contain("cotizar:", "debe exponer método cotizar");
        contenido.Should().Contain("resolveClient", "debe resolver apiClient de manera agnóstica");
        contenido.Should().Contain(".get(", "debe realizar llamada GET para planes");
        contenido.Should().Contain(".post(", "debe realizar llamada POST para cotizar");
        contenido.Should().Contain("window.simuladorApi = simuladorApi;", "debe soportar navegador");
        contenido.Should().Contain("module.exports = simuladorApi;", "debe soportar Node/CommonJS");
    }

    #endregion

    #region 2. wizardState.js Tests

    [Fact(DisplayName = "SF-050: wizardState.js debe existir físicamente en wwwroot/js/ventas/")]
    public void WizardState_ArchivoDebeExistirEnWwwroot()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "wizardState.js");
        File.Exists(path).Should().BeTrue("el archivo wizardState.js debe existir en wwwroot/js/ventas/");
    }

    [Fact(DisplayName = "SF-050: wizardState.js debe usar la clave sf_nueva_venta_draft y exponer getDraft, updateDraft y clearDraft")]
    public void WizardState_DebeExponerContratoYMetodosDeBorrador()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "wizardState.js");
        File.Exists(path).Should().BeTrue();
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("sf_nueva_venta_draft", "debe usar la clave oficial de borrador");
        contenido.Should().Contain("getDraft", "debe permitir consultar el borrador actual");
        contenido.Should().Contain("updateDraft", "debe permitir actualizar de forma incremental");
        contenido.Should().Contain("clearDraft", "debe permitir limpiar el borrador");
        contenido.Should().Contain("sf:draft-updated", "debe emitir el evento custom sf:draft-updated");
        contenido.Should().Contain("window.wizardState = wizardState;", "debe exponerse globalmente");
        contenido.Should().Contain("module.exports = wizardState;", "debe soportar CommonJS");
    }

    [Fact(DisplayName = "SF-050 (BDD): wizardState.js debe sincronizar el pill 'Borrador guardado' (draftBadge)")]
    public void WizardState_DebeSincronizarPillBorradorGuardado()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "wizardState.js");
        File.Exists(path).Should().BeTrue();
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("draftBadge", "debe referenciar el badge de borrador guardado");
        contenido.Should().Contain("syncDraftBadge", "debe tener helper de sincronización de badge");
    }

    [Fact(DisplayName = "SF-050: wizardState.js debe sincronizar en vivo el panel derecho RESUMEN DE COTIZACIÓN")]
    public void WizardState_DebeSincronizarPanelResumenCotizacion()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "wizardState.js");
        File.Exists(path).Should().BeTrue();
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("syncSummaryPanel", "debe sincronizar el panel lateral");
        contenido.Should().Contain("resumenLoteTitulo");
        contenido.Should().Contain("resumenLoteClave");
        contenido.Should().Contain("resumenLoteCapacidad");
        contenido.Should().Contain("resumenLotePrecio");
        contenido.Should().Contain("resumenTotalEstimado");
    }

    [Fact(DisplayName = "SF-050: Pages/Ventas/Nueva.cshtml debe incluir simuladorApi.js y wizardState.js")]
    public void NuevaVenta_DebeIncluirScriptsEnScriptsSection()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Ventas", "Nueva.cshtml");
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("simuladorApi.js", "debe referenciar simuladorApi.js");
        contenido.Should().Contain("wizardState.js", "debe referenciar wizardState.js");
    }

    #endregion
}
