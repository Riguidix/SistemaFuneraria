using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

[Trait("Ticket", "SF-096")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class AccesibilidadWcagAuditTests
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

    [Fact(DisplayName = "SF-096: Layout maestro y Login declaran idioma es-MX y estructura accesible")]
    public void LayoutYLogin_DebenCumplirAtributosBaseWcag()
    {
        var layout = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Shared", "_Layout.cshtml")));
        var login = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Cuenta", "Login.cshtml")));

        layout.Should().Contain("lang=\"es-MX\"");
        layout.Should().Contain("aria-label=");
        login.Should().Contain("lang=\"es-MX\"");
        login.Should().Contain("aria-");
    }

    [Theory(DisplayName = "SF-096: Pantallas financieras mantienen cifras tabulares (sf-money/text-tabular) y atributos ARIA")]
    [InlineData("Pages/Resumen/Index.cshtml")]
    [InlineData("Pages/Catalogo/Index.cshtml")]
    [InlineData("Pages/Clientes/Index.cshtml")]
    [InlineData("Pages/Contratos/Index.cshtml")]
    [InlineData("Pages/Contratos/Detalle.cshtml")]
    [InlineData("Pages/Caja/Index.cshtml")]
    [InlineData("Pages/Caja/Recibo.cshtml")]
    [InlineData("Pages/Reportes/Index.cshtml")]
    [InlineData("Pages/Configuracion/Index.cshtml")]
    public void PantallasFinancieras_DebenUsarCifrasTabularesYAtributosAria(string rutaRelativa)
    {
        var ruta = RutaWeb(rutaRelativa.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(ruta).Should().BeTrue($"debe existir {rutaRelativa}");

        var html = File.ReadAllText(ruta);
        (html.Contains("sf-money", StringComparison.Ordinal) || html.Contains("text-tabular", StringComparison.Ordinal))
            .Should().BeTrue($"{rutaRelativa} debe aplicar cifras tabulares");
        html.Contains("aria-", StringComparison.Ordinal)
            .Should().BeTrue($"{rutaRelativa} debe incluir atributos ARIA de accesibilidad");
    }
}
