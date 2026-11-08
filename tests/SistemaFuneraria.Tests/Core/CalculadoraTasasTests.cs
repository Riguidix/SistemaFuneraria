using System.Globalization;
using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;
using Xunit;

namespace SistemaFuneraria.Tests.Core;

/// <summary>
/// Pruebas unitarias para la asignación y cálculo de tasas indexadas al plazo.
/// Valida los requisitos RF-05, RF-06, RN-05, RN-06 y el caso de prueba CP-04 con precisión decimal pura.
/// </summary>
public class CalculadoraTasasTests
{
    [Fact]
    public void ObtenerTasas_ConPlazoDe48Meses_DebeRetornarTasaAnual24PorCientoYTasaMensualDosPorCiento_CP04()
    {
        // Act
        var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(48);

        // Assert
        tasaAnual.Should().Be(0.24m, "la regla RN-06 establece 24% anual para plazo de 48 meses");
        tasaMensual.Should().Be(0.0200m, "la tasa mensual debe ser exactamente 2.0% (0.0200m)");
    }

    [Theory]
    [InlineData(12, "0.16", "0.0133333333333333333333333333")]
    [InlineData(18, "0.18", "0.015")]
    [InlineData(24, "0.20", "0.0166666666666666666666666667")]
    [InlineData(48, "0.24", "0.02")]
    public void ObtenerTasas_ConPlazosAutorizados_DebeRetornarTasasCorrectas(
        int plazoMeses, string tasaAnualEsperadaStr, string tasaMensualEsperadaStr)
    {
        // Arrange
        decimal tasaAnualEsperada = decimal.Parse(tasaAnualEsperadaStr, CultureInfo.InvariantCulture);
        decimal tasaMensualEsperada = decimal.Parse(tasaMensualEsperadaStr, CultureInfo.InvariantCulture);

        // Act
        var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(plazoMeses);

        // Assert
        tasaAnual.Should().Be(tasaAnualEsperada);
        tasaMensual.Should().Be(tasaMensualEsperada);
    }

    [Theory]
    [InlineData(12, "0.0133333333333333333333333333")]
    [InlineData(18, "0.015")]
    [InlineData(24, "0.0166666666666666666666666667")]
    [InlineData(48, "0.02")]
    public void ObtenerTasaMensual_DebeRetornarTasaMensualOracularDirecta(int plazoMeses, string tasaMensualEsperadaStr)
    {
        decimal tasaMensualEsperada = decimal.Parse(tasaMensualEsperadaStr, CultureInfo.InvariantCulture);
        decimal tasaMensualDirecta = CalculadoraFinanciamiento.ObtenerTasaMensual(plazoMeses);
        tasaMensualDirecta.Should().Be(tasaMensualEsperada);
    }

    [Theory]
    [InlineData(36)] // CP-10 explícito
    [InlineData(6)]  // Menor al mínimo
    [InlineData(0)]  // Valor frontera cero
    [InlineData(-12)]// Negativo
    [InlineData(60)] // Superior al máximo
    [InlineData(15)] // Intermedio no autorizado
    public void ObtenerTasas_ConPlazoNoAutorizado_DebeLanzarArgumentOutOfRangeException(int plazoInvalido)
    {
        var accion = () => CalculadoraFinanciamiento.ObtenerTasas(plazoInvalido);
        accion.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("plazoMeses")
            .WithMessage($"*{plazoInvalido}*");
    }

    [Fact]
    public void ObtenerTasaMensual_ConPlazoDe36Meses_DebeLanzarArgumentOutOfRangeException_CP10()
    {
        var accion = () => CalculadoraFinanciamiento.ObtenerTasaMensual(36);
        accion.Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("plazoMeses");
    }

    [Fact]
    public void ParametrosNegocio_DebeContenerPlazosYTasasAutorizadasInmutables()
    {
        ParametrosNegocio.PlazosAutorizados.Should().BeEquivalentTo([12, 18, 24, 48]);
        ParametrosNegocio.TasasAnualesPorPlazo.Should().Contain(new KeyValuePair<int, decimal>(12, 0.16m));
        ParametrosNegocio.TasasAnualesPorPlazo.Should().Contain(new KeyValuePair<int, decimal>(18, 0.18m));
        ParametrosNegocio.TasasAnualesPorPlazo.Should().Contain(new KeyValuePair<int, decimal>(24, 0.20m));
        ParametrosNegocio.TasasAnualesPorPlazo.Should().Contain(new KeyValuePair<int, decimal>(48, 0.24m));
    }
}
