using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

/// <summary>
/// Pruebas unitarias para el wrapper JS clientesApi.js (SF-033).
/// Valida la exposición de los métodos CRUD, sanitización de datos (teléfono y CURP),
/// integración con apiClient y exportación a CSV.
/// </summary>
[Trait("Ticket", "SF-033")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class ClientesApiWrapperTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string LeerScriptClientesApi()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "clientesApi.js");
        File.Exists(path).Should().BeTrue($"debe existir el archivo '{path}'");
        return File.ReadAllText(path);
    }

    [Fact(DisplayName = "SF-033: clientesApi.js debe existir físicamente en wwwroot/js/api/")]
    public void ClientesApi_ArchivoDebeExistirEnWwwroot()
    {
        var path = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "clientesApi.js");
        File.Exists(path).Should().BeTrue("el archivo clientesApi.js debe existir en el directorio wwwroot/js/api");
    }

    [Fact(DisplayName = "SF-033: clientesApi.js debe exponer listar, obtenerPorId, registrar, actualizar, eliminar y exportarDirectorio")]
    public void ClientesApi_DebeExponerMetodosCrudYExportar()
    {
        var contenido = LeerScriptClientesApi();

        contenido.Should().Contain("listar:");
        contenido.Should().Contain("obtenerPorId:");
        contenido.Should().Contain("registrar:");
        contenido.Should().Contain("actualizar:");
        contenido.Should().Contain("eliminar:");
        contenido.Should().Contain("exportarDirectorio:");
    }

    [Fact(DisplayName = "SF-033: clientesApi.js debe utilizar la ruta base /api/v1/clientes y /exportar")]
    public void ClientesApi_DebeUsarRutaBaseClientes()
    {
        var contenido = LeerScriptClientesApi();

        contenido.Should().Contain("/api/v1/clientes");
        contenido.Should().Contain("/exportar");
    }

    [Fact(DisplayName = "SF-033: clientesApi.js debe soportar UMD (window.clientesApi y module.exports)")]
    public void ClientesApi_DebeSoportarUmd()
    {
        var contenido = LeerScriptClientesApi();

        contenido.Should().Contain("window.clientesApi = clientesApi;");
        contenido.Should().Contain("module.exports = clientesApi;");
    }

    [Fact(DisplayName = "SF-033: clientesApi.js debe delegar las operaciones HTTP en apiClient")]
    public void ClientesApi_DebeDelegarEnApiClient()
    {
        var contenido = LeerScriptClientesApi();

        contenido.Should().Contain("resolveClient");
        contenido.Should().Contain(".get(");
        contenido.Should().Contain(".post(");
        contenido.Should().Contain(".put(");
        contenido.Should().Contain(".delete(");
    }

    [Fact(DisplayName = "SF-033 (BDD): registrar y actualizar deben sanitizar teléfono (sin espacios) y CURP (mayúsculas)")]
    public void ClientesApi_DebeSanitizarTelefonoYCurp()
    {
        var contenido = LeerScriptClientesApi();

        // Verificación de sanitización de teléfono eliminando espacios (ej. \s+ o replace)
        Regex.IsMatch(contenido, @"telefono.*replace\(/\\s\+/g,\s*""""\)").Should().BeTrue(
            "debe limpiar espacios en el teléfono antes de enviar a la API");

        // Verificación de normalización de CURP a mayúsculas
        Regex.IsMatch(contenido, @"curp.*toUpperCase\(\)").Should().BeTrue(
            "debe convertir la CURP a mayúsculas antes de enviar a la API");

        // Verificación de que tanto registrar como actualizar aplican la sanitización
        contenido.Should().Contain("sanitizarPayload");
    }

    [Fact(DisplayName = "SF-033: exportarDirectorio debe configurar descarga del archivo directorio-clientes.csv")]
    public void ClientesApi_ExportarDirectorio_DebeDescargarDirectorioClientesCsv()
    {
        var contenido = LeerScriptClientesApi();

        contenido.Should().Contain("directorio-clientes.csv");
        contenido.Should().Contain("text/csv");
    }
}
