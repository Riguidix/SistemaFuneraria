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

    [Fact(DisplayName = "SF-046: Pages/Catalogo/Index.cshtml debe incluir el partial _DetalleLoteModal y conectar botón Ver detalle")]
    public void CatalogoIndex_DebeIncluirModalYConectarBotonVerDetalle()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Catalogo", "Index.cshtml");
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("<partial name=\"_DetalleLoteModal\"");
        contenido.Should().Contain("RO-B-027");
        contenido.Should().Contain("abrirModalDetalleLote");
    }

    [Fact(DisplayName = "SF-046: Enlace 'Ver contrato' para lote Vendido (LM-A-014) debe apuntar a /Contratos/Detalle?loteClave=LM-A-014")]
    public void CatalogoIndex_LoteVendido_DebeVincularAContratoDetalle()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Catalogo", "Index.cshtml");
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("/Contratos/Detalle?loteClave=LM-A-014");
    }

    [Fact(DisplayName = "SF-046: DetalleModel debe soportar parámetro de búsqueda por loteClave")]
    public void ContratosDetalleModel_DebeSoportarParametroLoteClave()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Contratos", "Detalle.cshtml.cs");
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("loteClave");
        contenido.Should().Contain("LM-A-014");
    }

    [Fact(DisplayName = "SF-046: lotesApi.js debe exponer método obtenerLotePorClave")]
    public void LotesApi_DebeExponerObtenerLotePorClave()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "lotesApi.js");
        var contenido = File.ReadAllText(path);

        contenido.Should().Contain("obtenerLotePorClave:");
    }
}
