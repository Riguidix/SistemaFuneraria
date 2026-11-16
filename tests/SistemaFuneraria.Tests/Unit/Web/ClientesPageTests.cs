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
}
