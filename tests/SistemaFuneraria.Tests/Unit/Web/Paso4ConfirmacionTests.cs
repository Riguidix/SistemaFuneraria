using FluentAssertions;
using SistemaFuneraria.Web.Pages.Ventas;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class Paso4ConfirmacionTests
{
    [Fact]
    public void PuedeGenerarContrato_ConDatosCompletosYCheckboxMarcado_DebeSerVerdadero()
    {
        // Arrange
        var contrato = new ContratoConfirmacionDto
        {
            ClienteNombre = "Mariana Torres Aguirre",
            ClienteEmail = "mariana.torres@email.mx",
            LoteClave = "JN-A-042",
            LoteJardin = "Jardín Norte",
            CapacidadGavetas = 2,
            PrecioTotal = 120000.00m,
            MontoEnganche = 24000.00m,
            Modalidad = "Credito",
            PlazoMeses = 24,
            MensualidadEstimada = 4887.54m,
            TasaAnual = 20,
            DatosRevisadosConCliente = true
        };

        // Act
        var puedeGenerar = contrato.PuedeGenerarContrato();

        // Assert
        puedeGenerar.Should().BeTrue();
        contrato.ResumenCliente.Should().Be("Mariana Torres Aguirre");
        contrato.ResumenLote.Should().Be("JN-A-042 · Jardín Norte");
        contrato.ResumenPrecio.Should().Be("$120,000.00");
        contrato.ResumenFinanciamiento.Should().Be("24 pagos de $4,887.54");
    }

    [Fact]
    public void PuedeGenerarContrato_SinCheckboxMarcado_DebeSerFalso()
    {
        // Arrange
        var contrato = new ContratoConfirmacionDto
        {
            ClienteNombre = "Mariana Torres Aguirre",
            LoteClave = "JN-A-042",
            DatosRevisadosConCliente = false // Checkbox desmarcado
        };

        // Act
        var puedeGenerar = contrato.PuedeGenerarContrato();

        // Assert
        puedeGenerar.Should().BeFalse();
    }
}
