using FluentAssertions;
using SistemaFuneraria.Web.Pages.Ventas;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class Paso3FinanciamientoTests
{
    [Fact]
    public void CalcularFinanciamiento_Credito24Meses20Porciento_DebeCoincidirConFigmaCaptura7()
    {
        // Arrange
        decimal precioLote = 120000.00m;
        int porcentajeEnganche = 20;
        int mesesPlazo = 24;

        // Act
        var resultado = FinanciamientoCalculadora.Calcular(precioLote, "Credito", porcentajeEnganche, mesesPlazo);

        // Assert
        resultado.MontoEnganche.Should().Be(24000.00m);
        resultado.CapitalAFinanciar.Should().Be(96000.00m);
        resultado.MensualidadEstimada.Should().Be(4887.54m);
        resultado.TituloBanner.Should().Be("Mensualidad estimada");
    }

    [Fact]
    public void CalcularFinanciamiento_ModalidadContado_DebeRetornarPagoUnicoSinIntereses()
    {
        // Arrange
        decimal precioLote = 120000.00m;

        // Act
        var resultado = FinanciamientoCalculadora.Calcular(precioLote, "Contado", 0, 0);

        // Assert
        resultado.MontoEnganche.Should().Be(0m);
        resultado.CapitalAFinanciar.Should().Be(0m);
        resultado.MensualidadEstimada.Should().Be(120000.00m);
        resultado.TituloBanner.Should().Be("Pago único de contado");
        resultado.SubtituloBanner.Should().Be("Sin intereses · Liquidación inmediata");
    }
}
