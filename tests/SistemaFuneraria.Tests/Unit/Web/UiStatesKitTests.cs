using System;
using System.IO;
using FluentAssertions;
using SistemaFuneraria.Web.Pages;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class UiStatesKitTests
{
    private static string GetSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SistemaFuneraria.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? AppContext.BaseDirectory;
    }

    [Fact]
    public void ParametrosUiModel_OnGet_DebeInicializarCorrectamente()
    {
        // Arrange
        var model = new ParametrosUiModel();

        // Act
        model.OnGet();

        // Assert
        model.Should().NotBeNull();
    }

    [Fact]
    public void UiStatesScript_DebeExistirYContenerLosCincoEstadosEstandarizados()
    {
        // Arrange
        var root = GetSolutionRoot();
        var filePath = Path.Combine(root, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ui", "uiStates.js");
        File.Exists(filePath).Should().BeTrue("El archivo uiStates.js debe existir en wwwroot/js/ui/");

        // Act
        var content = File.ReadAllText(filePath);

        // Assert - 5 estados requeridos por DoD SF-027
        content.Should().Contain("showSkeleton", "Debe soportar estado 1: Loading / Skeleton");
        content.Should().Contain("showEmpty", "Debe soportar estado 2: Empty State");
        content.Should().Contain("showError", "Debe soportar estado 3: Error Banner con ProblemDetails");
        content.Should().Contain("showToast", "Debe soportar estado 4: Success Toast");
        content.Should().Contain("confirm", "Debe soportar estado 5: Modal Redondeado");
    }

    [Fact]
    public void SistemaTokensCss_DebeContenerVariablesOficialesFigma()
    {
        // Arrange
        var root = GetSolutionRoot();
        var filePath = Path.Combine(root, "src", "SistemaFuneraria.Web", "wwwroot", "css", "sistema-tokens.css");
        File.Exists(filePath).Should().BeTrue("El archivo sistema-tokens.css debe existir");

        // Act
        var content = File.ReadAllText(filePath);

        // Assert - Tokens oficiales de Captura 13
        content.Should().Contain("--forest", "Debe definir token primario forest");
        content.Should().Contain("--gold", "Debe definir token de acento gold");
        content.Should().Contain("--rose", "Debe definir token de error/mora rose");
        content.Should().Contain("--canvas", "Debe definir fondo cálido canvas");
    }
}
