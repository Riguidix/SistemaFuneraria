using FluentAssertions;
using SistemaFuneraria.Web.Pages.Contratos;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ContratosPageTests
{
    [Fact]
    public void Contratos_OnGet_DebeInicializarKpisYListaContratosFigma()
    {
        // Arrange
        var pageModel = new IndexModel();

        // Act
        pageModel.OnGet();

        // Assert - 4 KPI Cards requeridos por Captura 9 (SF-070)
        pageModel.TotalActivos.Should().Be(128, "Total vigentes en cartera debe ser 128");
        pageModel.TotalAlCorriente.Should().Be(120, "Sin atraso de cuotas debe ser 120");
        pageModel.TotalConAtraso.Should().Be(8, "Mensualidades vencidas debe ser 8");
        pageModel.TotalLiquidados.Should().Be(32, "Cierre total de amortización debe ser 32");

        // Assert - Tabla de Cartera
        pageModel.Contratos.Should().HaveCount(4);
        pageModel.Contratos[0].Folio.Should().Be("CTR-2024-0118");
        pageModel.Contratos[0].Cliente.Should().Be("Elena Vargas");
        pageModel.Contratos[0].Estado.Should().Be("Vencido");
        pageModel.Contratos[0].RequiereCobro.Should().BeTrue("El contrato vencido debe habilitar el botón Cobrar hacia Caja");
    }
}
