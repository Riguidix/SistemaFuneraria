using FluentAssertions;
using SistemaFuneraria.Core.Comun;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Core;

[Trait("Capa", "Core")]
[Trait("Tipo", "Unit")]
public class TextoHelperTests
{
    [Theory]
    [InlineData("Andrea Morales", "AM")]
    [InlineData("  Carlos   Vega  ", "CV")]
    [InlineData("Juan Carlos Mendoza", "JC")]
    [InlineData("Andrea", "AN")]
    [InlineData("X", "X")]
    [InlineData("", "US")]
    [InlineData("   ", "US")]
    [InlineData(null, "US")]
    public void ObtenerIniciales_NombreCompleto_DebeRetornarInicialesEsperadas(string? nombre, string esperado)
    {
        TextoHelper.ObtenerIniciales(nombre).Should().Be(esperado);
    }

    [Fact]
    public void ObtenerIniciales_NombreYApellidos_DebeCombinarIniciales()
    {
        TextoHelper.ObtenerIniciales("Luis", "Torres").Should().Be("LT");
        TextoHelper.ObtenerIniciales(null, null, "--").Should().Be("--");
    }
}
