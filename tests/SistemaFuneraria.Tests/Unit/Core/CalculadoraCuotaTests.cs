using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Suite de pruebas unitarias para la fórmula de amortización francesa y cálculo de cuota mensual ordinaria (SF-010).
/// Valida el cumplimiento estricto de CP-05, RF-07, RN-05, RN-07 y RNF-02.
/// </summary>
public class CalculadoraCuotaTests
{
    #region Escenario Canónico Gherkin CP-05

    [Fact]
    [Trait("Ticket", "SF-010")]
    [Trait("CP", "CP-05")]
    [Trait("Regla", "RN-07")]
    public void CalcularMensualidadOrdinaria_ConEscenarioCanonicoCP05_DebeRetornarCuotaExacta()
    {
        // GIVEN (Dado): Un capital financiado de $96,000.00, plazo de 12 meses y tasa anual de 16% (RN-05)
        decimal capital = 96000.00m;
        int plazo = 12;
        decimal tasaMensual = 0.16m / 12m;

        // WHEN (Cuando): Se calcula la mensualidad ordinaria
        decimal cuota = CalculadoraFinanciamiento.CalcularMensualidadOrdinaria(capital, tasaMensual, plazo);

        // THEN (Entonces): La mensualidad debe ser exactamente $8,710.16 tras redondeo bancario
        cuota.Should().Be(8710.16m, "la cuota para $96,000.00 a 12 meses al 16% anual debe ser exactamente $8,710.16 (CP-05)");
    }

    [Fact]
    [Trait("Ticket", "SF-010")]
    [Trait("CP", "CP-05")]
    public void CalcularCuota_SobrecargaConPlazo_DebeCalcularMismoValorQueMensualidadOrdinaria()
    {
        // Arrange
        decimal capital = 96000.00m;
        int plazo = 12;

        // Act
        decimal cuota = CalculadoraFinanciamiento.CalcularCuota(capital, plazo);

        // Assert
        cuota.Should().Be(8710.16m);
    }

    #endregion

    #region Casos Autorizados del Oráculo Financiero (Plazos 18, 24 y 48 meses)

    [Theory]
    [Trait("Ticket", "SF-010")]
    [Trait("CP", "CP-05")]
    [InlineData(96000.00, 18, 6125.36)]  // 18 meses, 18% anual
    [InlineData(96000.00, 24, 4886.00)]  // 24 meses, 20% anual
    [InlineData(96000.00, 48, 3129.78)]  // 48 meses, 24% anual
    [InlineData(108000.00, 12, 9798.93)] // Lote $120k con 10% enganche a 12 meses
    [InlineData(108000.00, 24, 5496.75)] // Lote $120k con 10% enganche a 24 meses
    [InlineData(84000.00, 12, 7621.39)]  // Lote $120k con 30% enganche a 12 meses
    public void CalcularCuota_ConPlazosYCapitalesDelOraculo_DebeRetornarMensualidadExacta(
        decimal capital,
        int plazo,
        decimal cuotaEsperada)
    {
        // Act
        decimal cuota = CalculadoraFinanciamiento.CalcularCuota(capital, plazo);

        // Assert
        cuota.Should().Be(cuotaEsperada);
    }

    #endregion

    #region Validaciones de Seguridad (M.E.S.T.)

    [Theory]
    [Trait("Ticket", "SF-010")]
    [Trait("Regla", "RN-04")]
    [Trait("Regla", "RNF-02")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-96000.00)]
    public void CalcularMensualidadOrdinaria_ConCapitalInvalido_DebeLanzarArgumentOutOfRangeException(decimal capitalInvalido)
    {
        // Arrange
        decimal tasaMensual = 0.16m / 12m;
        int plazoValido = 12;

        // Act
        Action act = () => CalculadoraFinanciamiento.CalcularMensualidadOrdinaria(capitalInvalido, tasaMensual, plazoValido);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("capital")
           .WithMessage("*RN-04*");
    }

    [Theory]
    [Trait("Ticket", "SF-010")]
    [Trait("Regla", "RN-05")]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(15)]
    [InlineData(36)]
    [InlineData(60)]
    [InlineData(-12)]
    public void CalcularMensualidadOrdinaria_ConPlazoInvalido_DebeLanzarArgumentOutOfRangeException(int plazoInvalido)
    {
        // Arrange
        decimal capitalValido = 96000.00m;
        decimal tasaMensual = 0.16m / 12m;

        // Act
        Action act = () => CalculadoraFinanciamiento.CalcularMensualidadOrdinaria(capitalValido, tasaMensual, plazoInvalido);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("plazo")
           .WithMessage("*RN-05*");
    }

    [Theory]
    [Trait("Ticket", "SF-010")]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-0.16)]
    public void CalcularMensualidadOrdinaria_ConTasaMensualInvalida_DebeLanzarArgumentOutOfRangeException(decimal tasaInvalida)
    {
        // Arrange
        decimal capitalValido = 96000.00m;
        int plazoValido = 12;

        // Act
        Action act = () => CalculadoraFinanciamiento.CalcularMensualidadOrdinaria(capitalValido, tasaInvalida, plazoValido);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("tasaMensual");
    }

    [Theory]
    [Trait("Ticket", "SF-010")]
    [Trait("Regla", "RN-05")]
    [InlineData(6)]
    [InlineData(36)]
    public void CalcularCuota_ConPlazoInvalido_DebeLanzarArgumentOutOfRangeException(int plazoInvalido)
    {
        // Arrange
        decimal capitalValido = 96000.00m;

        // Act
        Action act = () => CalculadoraFinanciamiento.CalcularCuota(capitalValido, plazoInvalido);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}
