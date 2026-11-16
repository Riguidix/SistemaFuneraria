using System.Globalization;
using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit;

public class CulturaEsMxTests
{
    [Theory]
    [InlineData(120000.00, "$120,000.00")]
    [InlineData(150000.00, "$150,000.00")]
    [InlineData(180000.00, "$180,000.00")]
    [InlineData(8710.16, "$8,710.16")]
    [InlineData(0.00, "$0.00")]
    public void Moneda_EnCulturaEsMx_DebeFormatearConSignoPesosComaDeMilesYDosDecimales(decimal valor, string formatoEsperado)
    {
        // Arrange
        var cultura = new CultureInfo("es-MX");

        // Act
        var formateado = valor.ToString("C", cultura);

        // Assert
        formateado.Should().Be(formatoEsperado);
    }

    [Fact]
    public void CulturaEsMx_SeparadorDecimal_DebeSerPuntoYSeparadorMiles_DebeSerComa()
    {
        // Arrange
        var cultura = new CultureInfo("es-MX");

        // Assert
        cultura.NumberFormat.CurrencySymbol.Should().Be("$");
        cultura.NumberFormat.CurrencyDecimalSeparator.Should().Be(".");
        cultura.NumberFormat.CurrencyGroupSeparator.Should().Be(",");
        cultura.NumberFormat.NumberDecimalSeparator.Should().Be(".");
        cultura.NumberFormat.NumberGroupSeparator.Should().Be(",");
    }

    [Fact]
    public void FormatoFecha_EnCulturaEsMx_DebeTenerEstructuraDiaMesAnio()
    {
        // Arrange
        var cultura = new CultureInfo("es-MX");
        var fecha = new DateTime(2026, 11, 7);

        // Act
        var fechaFormateada = fecha.ToString("d", cultura);

        // Assert
        fechaFormateada.Should().Be("07/11/2026");
    }
}
