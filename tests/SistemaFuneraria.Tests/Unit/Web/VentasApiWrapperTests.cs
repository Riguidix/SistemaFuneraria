using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

/// <summary>
/// Pruebas unitarias para el wrapper JS ventasApi.js (SF-061).
/// Valida la exposición de métodos de venta (contado, financiada, consulta por ID)
/// y compatibilidad UMD.
/// </summary>
[Trait("Ticket", "SF-061")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class VentasApiWrapperTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string LeerScriptVentasApi()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "ventasApi.js");
        File.Exists(path).Should().BeTrue($"debe existir el archivo '{path}'");
        return File.ReadAllText(path);
    }

    [Fact(DisplayName = "SF-061: ventasApi.js debe existir físicamente en wwwroot/js/api/")]
    public void VentasApi_ArchivoDebeExistirEnWwwroot()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "ventasApi.js");
        File.Exists(path).Should().BeTrue("el archivo ventasApi.js debe existir en el directorio wwwroot/js/api");
    }

    [Fact(DisplayName = "SF-061: ventasApi.js debe exponer registrarVentaContado, registrarVentaFinanciada y obtenerVentaPorId")]
    public void VentasApi_DebeExponerMetodosDelContrato()
    {
        var contenido = LeerScriptVentasApi();

        contenido.Should().Contain("registrarVentaContado:");
        contenido.Should().Contain("registrarVentaFinanciada:");
        contenido.Should().Contain("obtenerVentaPorId:");
    }

    [Fact(DisplayName = "SF-061: ventasApi.js debe utilizar la ruta base /api/v1/ventas")]
    public void VentasApi_DebeUsarRutaBaseVentas()
    {
        var contenido = LeerScriptVentasApi();

        contenido.Should().Contain("/api/v1/ventas");
        contenido.Should().Contain("/contado");
        contenido.Should().Contain("/financiada");
    }

    [Fact(DisplayName = "SF-061: ventasApi.js debe soportar UMD (window.ventasApi y module.exports)")]
    public void VentasApi_DebeSoportarUmd()
    {
        var contenido = LeerScriptVentasApi();

        contenido.Should().Contain("window.ventasApi = ventasApi;");
        contenido.Should().Contain("module.exports = ventasApi;");
    }
}
