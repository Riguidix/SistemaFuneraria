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
        page.Enganche.Should().Be(30000.00m);
        page.PlazoMeses.Should().Be(24);
        page.Amortizacion.Should().HaveCount(24);

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
