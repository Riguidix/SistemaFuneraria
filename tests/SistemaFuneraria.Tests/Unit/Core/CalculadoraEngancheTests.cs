using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Suite de pruebas unitarias para la validación y cálculo del enganche y capital financiado (SF-008).
/// Cumple con las especificaciones de CP-03, RF-04, RN-01 y RN-04.
/// </summary>
public class CalculadoraEngancheTests
{
    #region CP-03: Escenario Gherkin Oficial

    [Fact]
    [Trait("Ticket", "SF-008")]
    [Trait("CP", "CP-03")]
    [Trait("Regla", "RN-04")]
    public void CalcularEnganche_ConLoteBasicoYEngancheVeintePorCiento_DebeCalcularEngancheYCapitalExactos()
    {
        // GIVEN (Dado): Un precio de lote oficial de $120,000.00 (LOT-02) y un enganche estándar del 20%
        decimal precioLote = 120000.00m;
        int porcentajeEnganche = 20;

        // WHEN (Cuando): Se invoca CalcularEnganche
        var (enganche, capitalFinanciado) = CalculadoraFinanciamiento.CalcularEnganche(precioLote, porcentajeEnganche);

        // THEN (Entonces):
        // Enganche = $120,000.00 * 0.20 = $24,000.00
        // Capital Financiado = $120,000.00 - $24,000.00 = $96,000.00
        enganche.Should().Be(24000.00m);
        capitalFinanciado.Should().Be(96000.00m);
        (enganche + capitalFinanciado).Should().Be(precioLote, "la suma del enganche y el capital debe ser exactamente igual al precio del lote");
    }

    #endregion

    #region Matriz Exhaustiva de Combinaciones Autorizadas (3 Lotes x 3 Enganches = 9 Casos)

    [Theory]
    [Trait("Ticket", "SF-008")]
    [Trait("CP", "CP-03")]
    [Trait("Regla", "RN-01")]
    [Trait("Regla", "RN-04")]
    // Lote Básico ($120,000.00)
    [InlineData(120000.00, 10, 12000.00, 108000.00)]
    [InlineData(120000.00, 20, 24000.00, 96000.00)]
    [InlineData(120000.00, 30, 36000.00, 84000.00)]
    // Lote Familiar ($150,000.00)
    [InlineData(150000.00, 10, 15000.00, 135000.00)]
    [InlineData(150000.00, 20, 30000.00, 120000.00)]
    [InlineData(150000.00, 30, 45000.00, 105000.00)]
    // Lote Ampliado ($180,000.00)
    [InlineData(180000.00, 10, 18000.00, 162000.00)]
    [InlineData(180000.00, 20, 36000.00, 144000.00)]
    [InlineData(180000.00, 30, 54000.00, 126000.00)]
    public void CalcularEnganche_ConCombinacionesCatalogoAutorizado_DebeCalcularEngancheYCapitalAlCentavo(
        decimal precioLote,
        int porcentajeEnganche,
        decimal engancheEsperado,
        decimal capitalEsperado)
    {
        // Act
        var resultado = CalculadoraFinanciamiento.CalcularEnganche(precioLote, porcentajeEnganche);

        // Assert
        resultado.Enganche.Should().Be(engancheEsperado);
        resultado.CapitalFinanciado.Should().Be(capitalEsperado);
        (resultado.Enganche + resultado.CapitalFinanciado).Should().Be(precioLote, "la suma debe preservar la conservación total del capital sin redondeo espurio");
    }

    #endregion

    #region Validación de Porcentaje No Autorizado (RN-04)

    [Theory]
    [Trait("Ticket", "SF-008")]
    [Trait("CP", "CP-10")]
    [Trait("Regla", "RN-04")]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(15)]
    [InlineData(25)]
    [InlineData(35)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(-10)]
    public void CalcularEnganche_ConPorcentajeNoAutorizado_DebeLanzarArgumentOutOfRangeException(int porcentajeInvalido)
    {
        // Arrange
        decimal precioValido = 120000.00m;

        // Act
        Action act = () => CalculadoraFinanciamiento.CalcularEnganche(precioValido, porcentajeInvalido);

        // Assert: RN-04 exige rechazar cualquier porcentaje distinto de 10%, 20% o 30%
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("porcentajeEnganche")
           .WithMessage("*RN-04*");
    }

    #endregion

    #region Validación de Precio de Lote No Autorizado (RN-01)

    [Theory]
    [Trait("Ticket", "SF-008")]
    [Trait("CP", "CP-10")]
    [Trait("Regla", "RN-01")]
    [InlineData(0)]
    [InlineData(-50000.00)]
    [InlineData(100000.00)]
    [InlineData(99999.99)]
    [InlineData(120000.01)]
    [InlineData(200000.00)]
    public void CalcularEnganche_ConPrecioNoAutorizado_DebeLanzarArgumentOutOfRangeException(decimal precioInvalido)
    {
        // Arrange
        int porcentajeValido = 20;

        // Act
        Action act = () => CalculadoraFinanciamiento.CalcularEnganche(precioInvalido, porcentajeValido);

        // Assert: RN-01 exige rechazar cualquier precio de lote no contenido en el catálogo oficial
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("precioLote")
           .WithMessage("*RN-01*");
    }

    #endregion
}
