using FluentAssertions;
using SistemaFuneraria.Web.Pages.Clientes;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ClientesPageTests
{
    [Fact]
    public void Clientes_OnGet_DebeCargarCorrectamente()
    {
        // Arrange
        var pageModel = new IndexModel();

        // Act
        pageModel.OnGet();

        // Assert
        pageModel.Should().NotBeNull();
    }

    [Fact(DisplayName = "SF-034: La vista Clientes/Index debe incluir buscador, tabla y botón Exportar directorio")]
    public void Clientes_VistaRazor_DebeContenerElementosPrincipales()
    {
        var raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var vistaPath = Path.Combine(raiz, "src", "SistemaFuneraria.Web", "Pages", "Clientes", "Index.cshtml");

        File.Exists(vistaPath).Should().BeTrue();
        var html = File.ReadAllText(vistaPath);
        html.Should().Contain("inputBuscarClientes");
        html.Should().Contain("tbodyClientes");
        html.Should().Contain("Exportar directorio");
        html.Should().NotContain("_RegistrarPagoModal");
    }

    [Fact(DisplayName = "SF-035: El modal _NuevoClienteModal debe existir y estar vinculado en Clientes/Index")]
    public void NuevoClienteModal_DebeExistirYUsarseEnClientesIndex()
    {
        var raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var modalPath = Path.Combine(raiz, "src", "SistemaFuneraria.Web", "Pages", "Shared", "_NuevoClienteModal.cshtml");
        var vistaPath = Path.Combine(raiz, "src", "SistemaFuneraria.Web", "Pages", "Clientes", "Index.cshtml");

        File.Exists(modalPath).Should().BeTrue();
        File.ReadAllText(modalPath).Should().Contain("modalNuevoCliente");
        File.ReadAllText(vistaPath).Should().Contain("<partial name=\"_NuevoClienteModal\" />");
    }

    [Fact(DisplayName = "PR1: Clientes/Index debe consumir clientesApi.js y clientesPage.js sin filas mock")]
    public void Clientes_VistaRazor_DebeCargarScriptsRealesYDrawerSinMock()
    {
        var raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var vistaPath = Path.Combine(raiz, "src", "SistemaFuneraria.Web", "Pages", "Clientes", "Index.cshtml");

        var html = File.ReadAllText(vistaPath);
        html.Should().Contain("~/js/api/clientesApi.js");
        html.Should().Contain("~/js/pages/clientesPage.js");
        html.Should().Contain("drawerExpedienteCliente");
        html.Should().Contain("clientesAccesoRestringido");
        html.Should().Contain("filaErrorClientes");
        html.Should().Contain("data-roles=\"Administrador\"");
        html.Should().NotContain("Laura Castañeda");
        html.Should().NotContain("CAL951118MDFRR01");
    }

    [Fact(DisplayName = "PR1: _NuevoClienteModal debe incluir campo Direccion y alerta de error")]
    public void NuevoClienteModal_DebeIncluirDireccionYContenedorError()
    {
        var raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var modalPath = Path.Combine(raiz, "src", "SistemaFuneraria.Web", "Pages", "Shared", "_NuevoClienteModal.cshtml");

        var html = File.ReadAllText(modalPath);
        html.Should().Contain("modalClienteDireccion");
        html.Should().Contain("modalNuevoClienteError");
    }

    [Fact(DisplayName = "PR1: clientesPage.js debe existir con soporte UMD e integración clientesApi/authApi")]
    public void ClientesPageJs_DebeExistirConUmdYReferenciasApi()
    {
        var raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var jsPath = Path.Combine(raiz, "src", "SistemaFuneraria.Web", "wwwroot", "js", "pages", "clientesPage.js");

        File.Exists(jsPath).Should().BeTrue();
        var js = File.ReadAllText(jsPath);
        js.Should().Contain("module.exports");
        js.Should().Contain("root.clientesPage");
        js.Should().Contain("clientesApi");
        js.Should().Contain("authApi");
    }
}
