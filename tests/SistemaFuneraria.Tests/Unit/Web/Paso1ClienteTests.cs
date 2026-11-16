using FluentAssertions;
using SistemaFuneraria.Web.Pages.Ventas;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class Paso1ClienteTests
{
    [Fact]
    public void ValidarPaso1_ConDatosCompletos_DebeSerValidoYGenerarBorrador()
    {
        // Arrange
        var cliente = new ClienteDraftDto
        {
            Nombre = "Mariana",
            Apellidos = "Torres Aguirre",
            Curp = "TOAM850423MDFRRG01",
            Telefono = "5512345678",
            Email = "mariana.torres@email.mx"
        };

        // Act
        var esValido = cliente.EsValido(out var errores);

        // Assert
        esValido.Should().BeTrue();
        errores.Should().BeEmpty();
        cliente.NombreCompleto.Should().Be("Mariana Torres Aguirre");
    }

    [Fact]
    public void ValidarPaso1_SinNombreOApellidos_DebeRetornarInvalido()
    {
        // Arrange
        var cliente = new ClienteDraftDto
        {
            Nombre = "",
            Apellidos = "Torres Aguirre"
        };

        // Act
        var esValido = cliente.EsValido(out var errores);

        // Assert
        esValido.Should().BeFalse();
        errores.Should().Contain(e => e.Contains("nombre", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidarPaso1_ConTelefonoInvalido_DebeRetornarInvalido()
    {
        // Arrange
        var cliente = new ClienteDraftDto
        {
            Nombre = "Mariana",
            Apellidos = "Torres",
            Telefono = "123" // Menor a 10 dígitos
        };

        // Act
        var esValido = cliente.EsValido(out var errores);

        // Assert
        esValido.Should().BeFalse();
        errores.Should().Contain(e => e.Contains("teléfono", System.StringComparison.OrdinalIgnoreCase));
    }
}
