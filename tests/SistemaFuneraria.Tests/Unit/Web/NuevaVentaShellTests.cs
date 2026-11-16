using FluentAssertions;
using SistemaFuneraria.Web.Pages.Ventas;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class NuevaVentaShellTests
{
    [Fact]
    public void NuevaVenta_OnGet_DebeInicializarPaso1YResumenDeCotizacionConformeFigma()
    {
        // Arrange
        var pageModel = new NuevaModel();

        // Act
        pageModel.OnGet();

        // Assert - Encabezado y Stepper Paso 1
        pageModel.Kicker.Should().Be("PROCESO COMERCIAL");
        pageModel.Titulo.Should().Be("Nueva venta");
        pageModel.PasoActual.Should().Be(1);

        // Assert - Panel Derecho "RESUMEN DE COTIZACIÓN" (Captura 5 de Figma)
        pageModel.LoteTipo.Should().Be("Lote Básico");
        pageModel.LoteClave.Should().Be("JN-A-042");
        pageModel.GavetaSeleccionada.Should().Be(42);
        pageModel.CapacidadGavetas.Should().Be(2);
        pageModel.PrecioLista.Should().Be(120000.00m);
        pageModel.TotalEstimado.Should().Be(120000.00m);
    }
}
