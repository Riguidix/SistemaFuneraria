using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

/// <summary>
/// Pruebas unitarias para el Endpoint y Modal "Ver detalle" de Lote e Historial de Estado (SF-046).
/// Valida la integración entre LotesController, la vista de Catálogo y el modal de detalle técnico.
/// </summary>
[Trait("Ticket", "SF-046")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class DetalleLoteModalTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact(DisplayName = "SF-046: El componente _DetalleLoteModal.cshtml debe existir en Pages/Shared/")]
    public void ModalDetalleLote_DebeExistirEnShared()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Shared", "_DetalleLoteModal.cshtml");
        File.Exists(path).Should().BeTrue("el modal _DetalleLoteModal.cshtml debe existir en Pages/Shared");
    }

    [Fact(DisplayName = "SF-046 (BDD): Modal debe contener campos técnicos requeridos (Clave, Ubicación, Gavetas, Precio, Estado, Historial)")]
    public void ModalDetalleLote_DebeContenerCamposTecnicosYHistorial()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Shared", "_DetalleLoteModal.cshtml");
        File.Exists(path).Should().BeTrue();
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("modalDetalleLote", "id del modal");
        contenido.Should().Contain("detalleLoteClave", "contenedor de clave");
        contenido.Should().Contain("detalleLoteUbicacion", "contenedor de ubicación");
        contenido.Should().Contain("detalleLoteCapacidad", "contenedor de capacidad");
        contenido.Should().Contain("detalleLotePrecio", "contenedor de precio de lista");
        contenido.Should().Contain("detalleLoteEstado", "contenedor de badge de estado");
        contenido.Should().Contain("historialEstadoLote", "sección de historial de cambios");
        contenido.Should().Contain("btnCotizarDesdeDetalle", "botón para cotizar cuando el lote esté libre");
    }

    [Fact(DisplayName = "SF-046: Pages/Catalogo/Index.cshtml debe incluir modales y conectar lotesApi.js y catalogoPage.js")]
    public void CatalogoIndex_DebeIncluirModalYConectarBotonVerDetalle()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Catalogo", "Index.cshtml");
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("<partial name=\"_DetalleLoteModal\"");
        contenido.Should().Contain("<partial name=\"_FormularioLoteModal\"");
        contenido.Should().Contain("~/js/api/lotesApi.js");
        contenido.Should().Contain("~/js/pages/catalogoPage.js");
        contenido.Should().Contain("href=\"/Ventas/Nueva\"");
        contenido.Should().Contain("aria-sort=\"none\"");
        contenido.Should().Contain("Mantenimiento");
    }

    [Fact(DisplayName = "SF-046: catalogoPage.js debe enlazar lotes vendidos a /Contratos/Detalle y exponer abrirModalDetalleLote")]
    public void CatalogoIndex_LoteVendido_DebeVincularAContratoDetalle()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "pages", "catalogoPage.js");
        File.Exists(path).Should().BeTrue();
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("abrirModalDetalleLote");
        contenido.Should().Contain("/Contratos/Detalle?folio=");
        contenido.Should().Contain("/Contratos/Detalle?loteClave=");
        contenido.Should().Contain("/Ventas/Nueva?lote=");
        contenido.Should().Contain("cambiarEstadoLote");
        contenido.Should().Contain("obtenerSiguienteUbicacion");
        contenido.Should().Contain("crearLote");
        contenido.Should().Contain("actualizarLote");
    }

    [Fact(DisplayName = "SF-046: DetalleModel y contratoDetallePage.js deben soportar parámetro de búsqueda por loteClave")]
    public void ContratosDetalleModel_DebeSoportarParametroLoteClave()
    {
        var modelPath = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Contratos", "Detalle.cshtml.cs");
        var modelContenido = File.ReadAllText(modelPath);
        var jsPath = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "pages", "contratoDetallePage.js");
        var jsContenido = File.ReadAllText(jsPath);

        modelContenido.Should().Contain("loteClave");
        modelContenido.Should().Contain("LoteClaveParam");
        jsContenido.Should().Contain("loteClave");
        jsContenido.Should().Contain("obtenerCartera");
    }

    [Fact(DisplayName = "SF-046: lotesApi.js debe exponer método obtenerLotePorClave")]
    public void LotesApi_DebeExponerObtenerLotePorClave()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "lotesApi.js");
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("obtenerLotePorClave:");
    }
}
