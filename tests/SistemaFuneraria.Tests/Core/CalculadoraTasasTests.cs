using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using Xunit;

namespace SistemaFuneraria.Tests.Core;

/// <summary>
/// Pruebas unitarias para la asignación y cálculo de tasas indexadas al plazo.
/// Valida los requisitos RF-05, RF-06, RN-05, RN-06 y el caso de prueba CP-04.
/// </summary>
public class CalculadoraTasasTests
{
    /// <summary>
    /// CP-04: Asignación de Tasa Anual Indexada al Plazo (48 Meses).
    /// Verifica que para un plazo de 48 meses se asigne la tasa anual del 24% y tasa mensual del 2.0% (0.0200m).
    /// </summary>
    [Fact]
    public void ObtenerTasas_ConPlazoDe48Meses_DebeRetornarTasaAnual24PorCientoYTasaMensualDosPorCiento_CP04()
    {
        // Arrange
        const int plazo48Meses = 48;
        const decimal tasaAnualEsperada = 0.24m;
        const decimal tasaMensualEsperada = 0.0200m;

        // Act
        var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(plazo48Meses);

        // Assert
        tasaAnual.Should().Be(tasaAnualEsperada, "la regla RN-06 establece 24% anual para plazo de 48 meses");
        tasaMensual.Should().Be(tasaMensualEsperada, "la tasa mensual debe ser exactamente 24% / 12 = 2.0%");
    }

    /// <summary>
    /// Valida exhaustivamente todos los plazos autorizados por RN-05 (12, 18, 24 y 48 meses) y sus tasas fijas (RN-06).
    /// </summary>
    [Theory]
    [InlineData(12, 0.16, 0.16 / 12.0)]
    [InlineData(18, 0.18, 0.015)]
    [InlineData(24, 0.20, 0.20 / 12.0)]
    [InlineData(48, 0.24, 0.02)]
    public void ObtenerTasas_ConPlazosAutorizados_DebeRetornarTasasCorrectas(
        int plazoMeses,
        decimal tasaAnualEsperada,
        double tasaMensualEsperadaAprox)
    {
        // Act
        var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(plazoMeses);

        // Assert
        tasaAnual.Should().Be(tasaAnualEsperada);
        tasaMensual.Should().Be(tasaAnualEsperada / 12m);
        // Comprobación de consistencia numérica
        ((double)tasaMensual).Should().BeApproximately(tasaMensualEsperadaAprox, 1e-8);
    }

    /// <summary>
    /// Método de conveniencia ObtenerTasaMensual debe retornar el mismo valor que la tupla ObtenerTasas.
    /// </summary>
    [Theory]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(24)]
    [InlineData(48)]
    public void ObtenerTasaMensual_DebeCoincidirConComponenteMensualDeObtenerTasas(int plazoMeses)
    {
        // Act
        decimal tasaMensualDirecta = CalculadoraFinanciamiento.ObtenerTasaMensual(plazoMeses);
        var (_, tasaMensualTupla) = CalculadoraFinanciamiento.ObtenerTasas(plazoMeses);

        // Assert
        tasaMensualDirecta.Should().Be(tasaMensualTupla);
    }

    /// <summary>
    /// CP-10 / RN-05: El sistema debe rechazar plazos no autorizados arrojando ArgumentOutOfRangeException.
    /// Se evalúan casos de frontera, intermedios y negativos: 36 meses, 6 meses, 0 meses, -12 meses, 60 meses.
    /// </summary>
    [Theory]
    [InlineData(36)] // CP-10 explícito
    [InlineData(6)]  // Menor al mínimo
    [InlineData(0)]  // Valor frontera cero
    [InlineData(-12)]// Negativo
    [InlineData(60)] // Superior al máximo
    [InlineData(15)] // Intermedio no autorizado
    public void ObtenerTasas_ConPlazoNoAutorizado_DebeLanzarArgumentOutOfRangeException(int plazoInvalido)
    {
        // Act
        var accion = () => CalculadoraFinanciamiento.ObtenerTasas(plazoInvalido);

        // Assert
        accion.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("plazoMeses")
            .WithMessage($"*{plazoInvalido}*");
    }

    /// <summary>
    /// Verifica que ObtenerTasaMensual también lance ArgumentOutOfRangeException para plazos no permitidos.
    /// </summary>
    [Fact]
    public void ObtenerTasaMensual_ConPlazoDe36Meses_DebeLanzarArgumentOutOfRangeException_CP10()
    {
        // Act
        var accion = () => CalculadoraFinanciamiento.ObtenerTasaMensual(36);

        // Assert
        accion.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("plazoMeses");
    }
}
