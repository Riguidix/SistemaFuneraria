using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

/// <summary>
/// Pruebas unitarias para el wrapper JS lotesApi.js (SF-043).
/// Valida la exposición de los métodos del catálogo, formateadores es-MX,
/// badges de estado e integración con apiClient.
/// </summary>
[Trait("Ticket", "SF-043")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class LotesApiWrapperTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string LeerScriptLotesApi()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "lotesApi.js");
        File.Exists(path).Should().BeTrue($"debe existir el archivo '{path}'");
        return File.ReadAllText(path);
    }

    [Fact(DisplayName = "SF-043: lotesApi.js debe existir físicamente en wwwroot/js/api/")]
    public void LotesApi_ArchivoDebeExistirEnWwwroot()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "lotesApi.js");
        File.Exists(path).Should().BeTrue("el archivo lotesApi.js debe existir en el directorio wwwroot/js/api");
    }

    [Fact(DisplayName = "SF-043: lotesApi.js debe exponer obtenerCatalogo, obtenerLotePorId, crearLote, actualizarLote y cambiarEstadoLote")]
    public void LotesApi_DebeExponerMetodosDelContrato()
    {
        var contenido = LeerScriptLotesApi();

        contenido.Should().Contain("obtenerCatalogo:");
        contenido.Should().Contain("obtenerLotePorId:");
        contenido.Should().Contain("obtenerSiguienteUbicacion:");
        contenido.Should().Contain("crearLote:");
        contenido.Should().Contain("actualizarLote:");
        contenido.Should().Contain("cambiarEstadoLote:");
    }

    [Fact(DisplayName = "SF-043: lotesApi.js debe utilizar la ruta base /api/v1/lotes")]
    public void LotesApi_DebeUsarRutaBaseLotes()
    {
        var contenido = LeerScriptLotesApi();

        contenido.Should().Contain("/api/v1/lotes");
        contenido.Should().Contain("/estado");
    }

    [Fact(DisplayName = "SF-043: lotesApi.js debe soportar UMD (window.lotesApi y module.exports)")]
    public void LotesApi_DebeSoportarUmd()
    {
        var contenido = LeerScriptLotesApi();

        contenido.Should().Contain("window.lotesApi = lotesApi;");
        contenido.Should().Contain("module.exports = lotesApi;");
    }

    [Fact(DisplayName = "SF-043: lotesApi.js debe delegar las operaciones HTTP en apiClient")]
    public void LotesApi_DebeDelegarEnApiClient()
    {
        var contenido = LeerScriptLotesApi();

        contenido.Should().Contain("resolveClient");
        contenido.Should().Contain(".get(");
        contenido.Should().Contain(".post(");
        contenido.Should().Contain(".put(");
        contenido.Should().Contain(".patch(");
    }

    [Fact(DisplayName = "SF-043 (BDD): formatearMonedaMXN debe formatear en cultura es-MX ($120,000.00)")]
    public void LotesApi_DebeFormatearMonedaEsMx()
    {
        var contenido = LeerScriptLotesApi();

        contenido.Should().Contain("formatearMonedaMXN");
        contenido.Should().Contain("es-MX");
        contenido.Should().Contain("currency");
        contenido.Should().Contain("MXN");
    }

    [Fact(DisplayName = "SF-043: renderizarBadgeEstadoLote debe mapear Disponible, Reservado y Vendido según Figma Captura 2")]
    public void LotesApi_RenderizarBadgeEstadoLote_DebeMapearEstadosMockup()
    {
        var contenido = LeerScriptLotesApi();

        contenido.Should().Contain("renderizarBadgeEstadoLote");
        contenido.Should().Contain("badge-sf-success");
        contenido.Should().Contain("badge-sf-warning");
        contenido.Should().Contain("badge-sf-neutral");
        contenido.Should().Contain("badge-sf-maintenance");
        contenido.Should().Contain("Mantenimiento");
        contenido.Should().Contain("Cotizar");
        contenido.Should().Contain("Ver detalle");
        contenido.Should().Contain("Ver contrato");
    }
}
