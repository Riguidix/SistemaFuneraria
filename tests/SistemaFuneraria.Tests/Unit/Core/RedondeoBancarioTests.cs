using FluentAssertions;
using SistemaFuneraria.Core.Comun;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Core;

public class RedondeoBancarioTests
{
    [Theory]
    // Casos límite de punto medio (MidpointRounding.ToEven hacia el dígito par más cercano)
    [InlineData(10.555, 10.56)] // 5 es impar -> redondea al par superior (6)
    [InlineData(10.545, 10.54)] // 4 es par -> redondea al par actual (4)
    [InlineData(10.525, 10.52)] // 2 es par -> mantiene el par (2)
    [InlineData(10.535, 10.54)] // 3 es impar -> sube al par (4)
    [InlineData(0.005, 0.00)]   // 0 es par -> redondea a 0.00
    [InlineData(0.015, 0.02)]   // 1 es impar -> redondea a 0.02
    // Casos no equidistantes estándar
    [InlineData(10.554, 10.55)] // < 5 -> redondea hacia abajo
    [InlineData(10.556, 10.56)] // > 5 -> redondea hacia arriba
    [InlineData(100.000, 100.00)]
    [InlineData(0.000, 0.00)]
    public void Redondear_ConDosDecimalesPorDefecto_DebeAplicarBankersRoundingCorrectamente(decimal valorEntrada, decimal valorEsperado)
    {
        // Act
        var resultado = RedondeoBancario.Redondear(valorEntrada);

        // Assert
        resultado.Should().Be(valorEsperado);
    }

    [Theory]
    [InlineData(-10.555, -10.56)]
    [InlineData(-10.545, -10.54)]
    [InlineData(-10.554, -10.55)]
    [InlineData(-10.556, -10.56)]
    public void Redondear_ConValoresNegativos_DebeAplicarBankersRoundingSimetrico(decimal valorEntrada, decimal valorEsperado)
    {
        // Act
        var resultado = RedondeoBancario.Redondear(valorEntrada);

        // Assert
        resultado.Should().Be(valorEsperado);
    }

    [Theory]
    [InlineData(0.01235, 4, 0.0124)] // 3 impar -> redondea a 4
    [InlineData(0.01245, 4, 0.0124)] // 4 par -> mantiene en 4
    [InlineData(2.5, 0, 2.0)]        // 2 par -> mantiene en 2
    [InlineData(3.5, 0, 4.0)]        // 3 impar -> redondea a 4
    public void RedondearConNDecimales_ConPrecisionPersonalizada_DebeRespetarNumeroDeDecimales(decimal valorEntrada, int decimales, decimal valorEsperado)
    {
        // Act
        var resultado = RedondeoBancario.RedondearConNDecimales(valorEntrada, decimales);

        // Assert
        resultado.Should().Be(valorEsperado);
    }
}
