using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace SistemaFuneraria.Tests.Arquitectura;

/// <summary>
/// Suite de pruebas automatizadas de Clean Architecture con NetArchTest.Rules y FluentAssertions.
/// Garantiza el cumplimiento estricto del desacoplamiento unidireccional (SF-003).
/// </summary>
public class ReglasArquitecturaTests
{
    private static readonly Assembly CoreAssembly = Assembly.Load("SistemaFuneraria.Core");
    private static readonly Assembly ApplicationAssembly = Assembly.Load("SistemaFuneraria.Application");
    private static readonly Assembly InfrastructureAssembly = Assembly.Load("SistemaFuneraria.Infrastructure");
    private static readonly Assembly WebAssembly = Assembly.Load("SistemaFuneraria.Web");

    private const string CoreNamespace = "SistemaFuneraria.Core";
    private const string ApplicationNamespace = "SistemaFuneraria.Application";
    private const string InfrastructureNamespace = "SistemaFuneraria.Infrastructure";
    private const string WebNamespace = "SistemaFuneraria.Web";

    [Fact(DisplayName = "RF-ARCH-01: Core no debe tener dependencias hacia Infrastructure")]
    public void Core_NoDebeTenerDependenciaDe_Infrastructure()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("el dominio (Core) debe ser agnóstico a la infraestructura de persistencia y servicios externos.");
    }

    [Fact(DisplayName = "RF-ARCH-02: Core no debe tener dependencias hacia Web")]
    public void Core_NoDebeTenerDependenciaDe_Web()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn(WebNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("el dominio (Core) no debe conocer detalles de la capa de presentación Web.");
    }

    [Fact(DisplayName = "RF-ARCH-03: Core no debe tener dependencias hacia Application")]
    public void Core_NoDebeTenerDependenciaDe_Application()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("el dominio (Core) es el núcleo y no debe depender de casos de uso u orquestadores.");
    }

    [Fact(DisplayName = "RF-ARCH-04: Application no debe tener dependencias hacia Infrastructure")]
    public void Application_NoDebeTenerDependenciaDe_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("la capa de aplicación debe desacoplarse de la infraestructura usando contratos e interfaces.");
    }

    [Fact(DisplayName = "RF-ARCH-05: Application no debe tener dependencias hacia Web")]
    public void Application_NoDebeTenerDependenciaDe_Web()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(WebNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("la capa de aplicación no debe depender de controladores o vistas Web.");
    }

    [Fact(DisplayName = "RF-ARCH-06: Infrastructure no debe tener dependencias hacia Web")]
    public void Infrastructure_NoDebeTenerDependenciaDe_Web()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(WebNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("la capa de infraestructura no debe conocer la capa de presentación.");
    }

    [Fact(DisplayName = "RF-ARCH-07: Web no debe referenciar Infrastructure, EntityFrameworkCore ni Npgsql (SF-019)")]
    public void Web_NoDebeTenerDependenciaDe_InfrastructureNiPersistencia()
    {
        var result = Types.InAssembly(WebAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, "Microsoft.EntityFrameworkCore", "Npgsql")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("el cliente Web no debe acceder directamente a infraestructura ni persistencia.");

        var webCsprojPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "src", "SistemaFuneraria.Web", "SistemaFuneraria.Web.csproj"));

        File.Exists(webCsprojPath).Should().BeTrue($"debe existir el archivo de proyecto en {webCsprojPath}");
        var csprojContent = File.ReadAllText(webCsprojPath);

        csprojContent.Should().NotContain("SistemaFuneraria.Infrastructure",
            "SistemaFuneraria.Web no debe referenciar al proyecto Infrastructure.");
        csprojContent.Should().NotContain("Microsoft.EntityFrameworkCore",
            "SistemaFuneraria.Web no debe referenciar paquetes de EntityFrameworkCore.");
        csprojContent.Should().NotContain("Npgsql",
            "SistemaFuneraria.Web no debe referenciar paquetes de Npgsql.");
    }
}
