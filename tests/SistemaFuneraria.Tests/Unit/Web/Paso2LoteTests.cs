using FluentAssertions;
using SistemaFuneraria.Web.Pages.Ventas;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class Paso2LoteTests
{
    [Fact]
    public void ObtenerOpcionesLotes_DebeRetornarTresOpcionesFigmaCaptura6()
    {
        // Act
        var opciones = LoteOpcionesCatalogo.ObtenerOpcionesDisponibles();

        // Assert
        opciones.Should().HaveCount(3);

        var lote1 = opciones[0];
        lote1.Clave.Should().Be("JN-A-042");
        lote1.Jardin.Should().Be("Jardín Norte");
        lote1.CapacidadGavetas.Should().Be(2);
        lote1.Andador.Should().Be("Andador A");
        lote1.Precio.Should().Be(120000.00m);
        lote1.Tipo.Should().Be("Lote Básico");
        lote1.SlotId.Should().Be("42");

        var lote2 = opciones[1];
        lote2.Clave.Should().Be("JC-C-118");
        lote2.Jardin.Should().Be("Jardín Central");
        lote2.CapacidadGavetas.Should().Be(3);
        lote2.Precio.Should().Be(150000.00m);
        lote2.Tipo.Should().Be("Lote Familiar");

        var lote3 = opciones[2];
        lote3.Clave.Should().Be("RO-E-081");
        lote3.Jardin.Should().Be("Robles");
        lote3.CapacidadGavetas.Should().Be(4);
        lote3.Precio.Should().Be(180000.00m);
        lote3.Tipo.Should().Be("Lote Memorial");
    }

    [Fact]
    public void BuscarPorClave_ConClaveExistente_DebeRetornarLoteCorrecto()
    {
        // Act
        var lote = LoteOpcionesCatalogo.BuscarPorClave("JC-C-118");

        // Assert
        lote.Should().NotBeNull();
        lote!.Clave.Should().Be("JC-C-118");
        lote.PrecioFormateado.Should().Be("$150,000.00");
    }
}
