using FluentAssertions;
using SistemaFuneraria.Application.Servicios.Simulador;
using Xunit;

namespace SistemaFuneraria.Tests.Application;

/// <summary>
/// Pruebas de límites y particiones de equivalencia para SimuladorService (SF-027, CP-10).
/// Valida el rechazo estricto de parámetros no permitidos (RN-01, RN-04, RN-05).
/// </summary>
[Trait("Ticket", "SF-027")]
[Trait("Capa", "Application")]
[Trait("Tipo", "CasoPrueba")]
[Trait("CP", "CP-10")]
public class SimuladorLimitesTests
{
    private readonly ISimuladorService _simuladorService;

    public SimuladorLimitesTests()
    {
        _simuladorService = new SimuladorService();
    }

    [Fact(DisplayName = "CP-10 Caso A: Enganche 15% debe ser rechazado con mensaje explícito")]
    public void Simular_ConEnganche15PorCiento_DebeRetornarFalloConMensajeExactoCP10()
    {
        // Arrange (CP-10 Caso A)
        var request = new SimularFinanciamientoRequest(
            PrecioLote: 120_000m,
            PorcentajeEnganche: 15,
            PlazoMeses: 12);

        // Act
        var resultado = _simuladorService.Simular(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue();
        resultado.EsExitoso.Should().BeFalse();
        resultado.Errores.Should().Contain(e => e.Contains("Enganche no permitido. Solo se acepta 10%, 20% o 30%"));
    }

    [Fact(DisplayName = "CP-10 Caso B: Plazo 36 meses debe ser rechazado con mensaje explícito")]
    public void Simular_ConPlazo36Meses_DebeRetornarFalloConMensajeExactoCP10()
    {
        // Arrange (CP-10 Caso B)
        var request = new SimularFinanciamientoRequest(
            PrecioLote: 120_000m,
            PorcentajeEnganche: 20,
            PlazoMeses: 36);

        // Act
        var resultado = _simuladorService.Simular(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsFallo.Should().BeTrue();
        resultado.EsExitoso.Should().BeFalse();
        resultado.Errores.Should().Contain(e => e.Contains("Plazo inválido. Solo se admiten plazos de 12, 18, 24 o 48 meses"));
    }

    [Theory(DisplayName = "SF-027 / CP-10: Enganches fuera de {10, 20, 30}% deben ser rechazados")]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(15)]
    [InlineData(25)]
    [InlineData(35)]
    [InlineData(50)]
    public void Simular_ConEnganchesInvalidos_DebeRechazar(int porcentajeInvalido)
    {
        // Arrange
        var request = new SimularFinanciamientoRequest(
            PrecioLote: 150_000m,
            PorcentajeEnganche: porcentajeInvalido,
            PlazoMeses: 24);

        // Act
        var resultado = _simuladorService.Simular(request);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Errores.Should().Contain(e => e.Contains("Enganche no permitido. Solo se acepta 10%, 20% o 30%"));
    }

    [Theory(DisplayName = "SF-027 / CP-10: Plazos fuera de {12, 18, 24, 48} meses deben ser rechazados")]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(36)]
    [InlineData(60)]
    public void Simular_ConPlazosInvalidos_DebeRechazar(int plazoInvalido)
    {
        // Arrange
        var request = new SimularFinanciamientoRequest(
            PrecioLote: 180_000m,
            PorcentajeEnganche: 30,
            PlazoMeses: plazoInvalido);

        // Act
        var resultado = _simuladorService.Simular(request);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Errores.Should().Contain(e => e.Contains("Plazo inválido. Solo se admiten plazos de 12, 18, 24 o 48 meses"));
    }

    [Theory(DisplayName = "SF-027: Precios fuera de catálogo oficial deben ser rechazados")]
    [InlineData(99_999.99)]
    [InlineData(100_000)]
    [InlineData(135_000)]
    [InlineData(200_000)]
    public void Simular_ConPreciosInvalidos_DebeRechazar(decimal precioInvalido)
    {
        // Arrange
        var request = new SimularFinanciamientoRequest(
            PrecioLote: precioInvalido,
            PorcentajeEnganche: 20,
            PlazoMeses: 12);

        // Act
        var resultado = _simuladorService.Simular(request);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Errores.Should().Contain(e => e.Contains("Precio no permitido. Solo se autorizan lotes de $120,000.00, $150,000.00 y $180,000.00"));
    }

    [Fact(DisplayName = "SF-027: Múltiples parámetros inválidos simultáneos deben acumular todos los errores")]
    public void Simular_ConMultiplesParametrosInvalidos_DebeRetornarTodosLosErroresAcumulados()
    {
        // Arrange: precio, enganche y plazo inválidos simultáneamente
        var request = new SimularFinanciamientoRequest(
            PrecioLote: 99_000m,
            PorcentajeEnganche: 15,
            PlazoMeses: 36);

        // Act
        var resultado = _simuladorService.Simular(request);

        // Assert
        resultado.EsFallo.Should().BeTrue();
        resultado.Errores.Should().HaveCount(3);
        resultado.Errores.Should().Contain(e => e.Contains("Precio no permitido"));
        resultado.Errores.Should().Contain(e => e.Contains("Enganche no permitido"));
        resultado.Errores.Should().Contain(e => e.Contains("Plazo inválido"));
    }

    [Fact(DisplayName = "SF-027: Parámetros válidos deben permitir simulación exitosa")]
    public void Simular_ConParametrosValidos_DebeRetornarExito()
    {
        // Arrange
        var request = new SimularFinanciamientoRequest(
            PrecioLote: 150_000m,
            PorcentajeEnganche: 10,
            PlazoMeses: 48);

        // Act
        var resultado = _simuladorService.Simular(request);

        // Assert
        resultado.EsExitoso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor.MontoEnganche.Should().Be(15_000m);
        resultado.Valor.CapitalFinanciado.Should().Be(135_000m);
    }
}
