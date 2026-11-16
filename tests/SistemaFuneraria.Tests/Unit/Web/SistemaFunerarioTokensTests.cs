using FluentAssertions;

namespace SistemaFuneraria.Tests.Unit.Web;

/// <summary>
/// Verifica la fidelidad de las 6 variables CSS oficiales de Figma (Captura 13)
/// y la existencia de la pantalla /ParametrosUi (SF-025).
/// </summary>
[Trait("Ticket", "SF-025")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class SistemaFunerarioTokensTests
{
    private static string ObtenerRutaWeb(params string[] segmentos)
    {
        var raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        return Path.Combine(new[] { raiz, "src", "SistemaFuneraria.Web" }.Concat(segmentos).ToArray());
    }

    [Fact(DisplayName = "SF-025 (BDD): sistema-tokens.css debe definir las 6 variables CSS institucionales de Captura 13")]
    public void SistemaTokensCss_DebeContenerLas6VariablesOficialesDeFigma()
    {
        var cssPath = ObtenerRutaWeb("wwwroot", "css", "sistema-tokens.css");
        File.Exists(cssPath).Should().BeTrue($"debe existir {cssPath}");

        var contenido = File.ReadAllText(cssPath);
        contenido.Should().Contain("--forest: #173f3a;");
        contenido.Should().Contain("--forest-2: #245c53;");
        contenido.Should().Contain("--gold: #c59d52;");
        contenido.Should().Contain("--rose: #a84f4b;");
        contenido.Should().Contain("--canvas: #f4f6f3;");
        contenido.Should().Contain("--ink: #17211f;");
        contenido.Should().Contain("tabular-nums");
    }

    [Fact(DisplayName = "SF-025: La vista /ParametrosUi debe existir y exponer el título Design System")]
    public void ParametrosUi_DebeExistirYContenerDesignSystem()
    {
        var pagePath = ObtenerRutaWeb("Pages", "ParametrosUi.cshtml");
        var modelPath = ObtenerRutaWeb("Pages", "ParametrosUi.cshtml.cs");

        File.Exists(pagePath).Should().BeTrue();
        File.Exists(modelPath).Should().BeTrue();

        var html = File.ReadAllText(pagePath);
        html.Should().Contain("DESIGN SYSTEM");
        html.Should().Contain("Parámetros UI");
    }
}
