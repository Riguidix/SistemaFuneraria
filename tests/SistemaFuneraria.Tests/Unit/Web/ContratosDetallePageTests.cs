using FluentAssertions;
using SistemaFuneraria.Web.Pages.Contratos;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ContratosDetallePageTests
{
    [Fact]
    public void Detalle_OnGet_ConFolioPorDefecto_DebeCargarContratoYAmortizacion24Cuotas()
    {
        // Arrange
        var page = new DetalleModel();

        // Act
        page.OnGet(null);

        // Assert
        page.Folio.Should().Be("CTR-2025-0189");
        page.Titular.Should().Be("Laura Castañeda");
        page.Lote.Should().Contain("JC-C-118");
        page.PrecioPactado.Should().Be(150000.00m);
        page.Enganche.Should().Be(22500.00m);
        page.SaldoInsoluto.Should().Be(127500.00m);
        page.MensualidadRegular.Should().Be(6187.50m);
        page.PlazoMeses.Should().Be(24);
        page.Amortizacion.Should().HaveCount(24);

        // Cuota 1 coincidente con Captura 9 / Expediente Contrato
        var cuota1 = page.Amortizacion[0];
        cuota1.SaldoInicial.Should().Be(127500.00m);
        cuota1.Capital.Should().Be(4062.50m);
        cuota1.Interes.Should().Be(2125.00m);
        cuota1.CuotaTotal.Should().Be(6187.50m);
        cuota1.SaldoFinal.Should().Be(123437.50m);
        cuota1.Estado.Should().Be("Pagada");

        // Primeras 3 pagadas, resto pendientes
        page.Amortizacion.Take(3).Should().OnlyContain(c => c.Estado == "Pagada");
        page.Amortizacion.Skip(3).Should().OnlyContain(c => c.Estado == "Pendiente");

        // Saldo final de la cuota 24 debe ser cero
        page.Amortizacion.Last().SaldoFinal.Should().Be(0.00m);
    }

    [Fact]
    public void Detalle_OnGet_ConFolioExistente_DebePersonalizarTitularYLote()
    {
        // Arrange
        var page = new DetalleModel();

        // Act
        page.OnGet("CTR-2024-0118");

        // Assert
        page.Folio.Should().Be("CTR-2024-0118");
        page.Titular.Should().Be("Elena Vargas");
        page.Lote.Should().Contain("RO-B-027");
        page.Amortizacion.Should().NotBeEmpty();
    }
}
