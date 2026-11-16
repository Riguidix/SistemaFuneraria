using FluentAssertions;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Tests.Unit.Core;

public class ParametrosNegocioTests
{
    [Fact]
    [Trait("Ticket", "SF-005")]
    [Trait("CP", "CP-04")]
    public void PlazosAutorizados_DebeContenerDoceDieciochoVeinticuatroYCuarentaYOchoMeses()
    {
        // RN-05: Plazos autorizados de 12, 18, 24 y 48 meses
        ParametrosNegocio.PlazosAutorizados.Should().BeEquivalentTo(new[] { 12, 18, 24, 48 });
    }

    [Fact]
    [Trait("Ticket", "SF-005")]
    [Trait("CP", "CP-01")]
    public void PreciosLotesAutorizados_DebeContenerExactamenteLosTresPreciosOficiales()
    {
        // RN-01: Lotes de $120k (2 gavetas), $150k (3 gavetas) y $180k (4 gavetas)
        ParametrosNegocio.PreciosLotesAutorizados.Should().BeEquivalentTo(new[] { 120000.00m, 150000.00m, 180000.00m });
        ParametrosNegocio.EsPrecioAutorizado(120000.00m).Should().BeTrue();
        ParametrosNegocio.EsPrecioAutorizado(99999.00m).Should().BeFalse();
    }

    [Fact]
    [Trait("Ticket", "SF-005")]
    [Trait("CP", "CP-03")]
    public void EnganchesAutorizados_DebeContenerDiezVeinteYTreintaPorCiento()
    {
        // RN-04: Enganches autorizados del 10%, 20% y 30%
        ParametrosNegocio.EnganchesAutorizados.Should().BeEquivalentTo(new[] { 10, 20, 30 });
        ParametrosNegocio.EsEngancheAutorizado(20).Should().BeTrue();
        ParametrosNegocio.EsEngancheAutorizado(15).Should().BeFalse();
    }

    [Theory]
    [Trait("Ticket", "SF-005")]
    [Trait("CP", "CP-04")]
    [InlineData(12, 0.16)]
    [InlineData(18, 0.18)]
    [InlineData(24, 0.20)]
    [InlineData(48, 0.24)]
    public void ObtenerTasaAnual_ConPlazosValidos_DebeRetornarTasaExacta(int plazo, decimal tasaEsperada)
    {
        // RN-05: Tasas fijas por plazo
        ParametrosNegocio.EsPlazoAutorizado(plazo).Should().BeTrue();
        ParametrosNegocio.ObtenerTasaAnual(plazo).Should().Be(tasaEsperada);
    }

    [Theory]
    [Trait("Ticket", "SF-005")]
    [Trait("CP", "CP-10")]
    [InlineData(6)]
    [InlineData(36)]
    [InlineData(60)]
    public void ObtenerTasaAnual_ConPlazoNoAutorizado_DebeLanzarArgumentOutOfRangeException(int plazoInvalido)
    {
        ParametrosNegocio.EsPlazoAutorizado(plazoInvalido).Should().BeFalse();
        Action act = () => ParametrosNegocio.ObtenerTasaAnual(plazoInvalido);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
