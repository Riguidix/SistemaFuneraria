using FluentAssertions;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Core;

public class DineroTests
{
    [Fact]
    public void Constructor_ConMontoPositivoValido_DebeCrearInstanciaConMontoExacto()
    {
        // Arrange
        const decimal montoEsperado = 1500.50m;

        // Act
        var dinero = new Dinero(montoEsperado);

        // Assert
        dinero.Monto.Should().Be(montoEsperado);
    }

    [Fact]
    public void Constructor_ConMontoCero_DebePermitirse()
    {
        // Arrange & Act
        var dinero = new Dinero(0m);

        // Assert
        dinero.Monto.Should().Be(0m);
    }

    [Fact]
    public void PropiedadZero_DebeTenerMontoCero()
    {
        // Act
        var zero = Dinero.Zero;

        // Assert
        zero.Monto.Should().Be(0m);
    }

    [Fact]
    public void Constructor_ConMontoNegativo_DebeLanzarReglaNegocioException()
    {
        // Arrange
        const decimal montoNegativo = -0.01m;

        // Act
        Action accion = () => _ = new Dinero(montoNegativo);

        // Assert
        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("*no puede ser negativo*");
    }

    [Theory]
    [InlineData(10.555, 10.56)] // 5 después de impar (5) -> redondea al par más cercano (6)
    [InlineData(10.545, 10.54)] // 5 después de par (4) -> redondea al par más cercano (4)
    [InlineData(10.554, 10.55)] // Menor a 5 -> hacia abajo
    [InlineData(10.556, 10.56)] // Mayor a 5 -> hacia arriba
    public void Constructor_ConMasDeDosDecimales_DebeAplicarRedondeoBancarioToEven(decimal montoEntrada, decimal montoEsperado)
    {
        // Act
        var dinero = new Dinero(montoEntrada);

        // Assert
        dinero.Monto.Should().Be(montoEsperado);
    }

    [Fact]
    public void OperadorSuma_ConDosImportesValidos_DebeRetornarSumaExacta()
    {
        // Arrange
        var a = new Dinero(100.25m);
        var b = new Dinero(50.75m);

        // Act
        var resultado = a + b;

        // Assert
        resultado.Monto.Should().Be(151.00m);
    }

    [Fact]
    public void OperadorResta_ConDiezYCincoDolares_DebeRetornarCincoDolaresExacto()
    {
        // Arrange (Criterio de Aceptación BDD SF-006)
        var a = new Dinero(10.00m);
        var b = new Dinero(5.00m);

        // Act
        var resultado = a - b;

        // Assert
        resultado.Monto.Should().Be(5.00m);
    }

    [Fact]
    public void OperadorResta_MontosIguales_DebeRetornarCero()
    {
        // Arrange
        var a = new Dinero(25.50m);
        var b = new Dinero(25.50m);

        // Act
        var resultado = a - b;

        // Assert
        resultado.Monto.Should().Be(0.00m);
    }

    [Fact]
    public void OperadorResta_CuandoResultadoSeriaNegativo_DebeLanzarReglaNegocioException()
    {
        // Arrange (Criterio de Aceptación BDD SF-006)
        var a = new Dinero(5.00m);
        var b = new Dinero(10.00m);

        // Act
        Action accion = () => _ = a - b;

        // Assert
        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("*no puede ser negativo*");
    }

    [Fact]
    public void OperadorMultiplicacion_DineroPorEscalar_DebeCalcularYRedondearCorrectamente()
    {
        // Arrange
        var capital = new Dinero(100.00m);
        const decimal factorTasa = 0.165m; // 100 * 0.165 = 16.5 -> ToEven = 16.50

        // Act
        var resultado = capital * factorTasa;

        // Assert
        resultado.Monto.Should().Be(16.50m);
    }

    [Fact]
    public void OperadorMultiplicacion_EscalarPorDinero_DebeSerConmutativo()
    {
        // Arrange
        const decimal factor = 0.20m;
        var precioLote = new Dinero(120000.00m);

        // Act
        var enganche = factor * precioLote;

        // Assert
        enganche.Monto.Should().Be(24000.00m);
    }

    [Fact]
    public void OperadorMultiplicacion_ConFactorNegativo_DebeLanzarReglaNegocioException()
    {
        // Arrange
        var dinero = new Dinero(50.00m);

        // Act
        Action accion = () => _ = dinero * -2m;

        // Assert
        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void OperadorDivision_DineroEntreEscalar_DebeCalcularYRedondearCorrectamente()
    {
        // Arrange
        var total = new Dinero(100.00m);
        const decimal divisor = 3m; // 100 / 3 = 33.3333... -> 33.33

        // Act
        var resultado = total / divisor;

        // Assert
        resultado.Monto.Should().Be(33.33m);
    }

    [Fact]
    public void OperadorDivision_PorCero_DebeLanzarDivideByZeroException()
    {
        // Arrange
        var dinero = new Dinero(100.00m);

        // Act
        Action accion = () => _ = dinero / 0m;

        // Assert
        accion.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void OperadorDivision_DineroEntreDinero_DebeRetornarRazonDecimal()
    {
        // Arrange
        var parte = new Dinero(25.00m);
        var total = new Dinero(100.00m);

        // Act
        var razon = parte / total;

        // Assert
        razon.Should().Be(0.25m);
    }

    [Fact]
    public void OperadoresComparacion_DebenEvaluarCorrectamenteRelacionesDeOrden()
    {
        // Arrange
        var menor = new Dinero(50.00m);
        var mayor = new Dinero(100.00m);
        var igual = new Dinero(50.00m);

        // Assert
        (menor < mayor).Should().BeTrue();
        (menor <= mayor).Should().BeTrue();
        (mayor > menor).Should().BeTrue();
        (mayor >= menor).Should().BeTrue();
        (menor <= igual).Should().BeTrue();
        (menor >= igual).Should().BeTrue();
        (menor == igual).Should().BeTrue();
        (menor != mayor).Should().BeTrue();
    }

    [Fact]
    public void IComparable_DebePermitirOrdenarListasCorrectamente()
    {
        // Arrange
        var d1 = new Dinero(300.00m);
        var d2 = new Dinero(100.00m);
        var d3 = new Dinero(200.00m);
        var lista = new List<Dinero> { d1, d2, d3 };

        // Act
        lista.Sort();

        // Assert
        lista.Should().ContainInOrder(new Dinero(100.00m), new Dinero(200.00m), new Dinero(300.00m));
    }

    [Fact]
    public void Conversiones_ImplicitaADecimalYExplicitaADinero_DebenFuncionarCorrectamente()
    {
        // Arrange
        var dinero = new Dinero(850.50m);

        // Act
        decimal valorDecimal = dinero; // Implícita
        var dineroDesdeDecimal = (Dinero)120.75m; // Explícita

        // Assert
        valorDecimal.Should().Be(850.50m);
        dineroDesdeDecimal.Monto.Should().Be(120.75m);
    }
}
