using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Simulador;
using SistemaFuneraria.Application.Servicios.Simulador;

namespace SistemaFuneraria.Tests.Unit.Application;

/// <summary>
/// Pruebas unitarias de cotización comercial y validaciones CP-10 para <see cref="ISimuladorService.Cotizar"/> (SF-048).
/// </summary>
[Trait("Ticket", "SF-048")]
[Trait("Capa", "Application")]
[Trait("Tipo", "Unit")]
public class SimuladorCotizacionTests
{
    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    [Fact]
    public void Cotizar_Captura7Credito24Meses_DebeRetornarMensualidad488754YCuadreExacto()
    {
        var relojFijo = new FakeTimeProvider(new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero));
        ISimuladorService servicio = new SimuladorService(relojFijo);
        var request = new SimularCotizacionRequest(null, 2, "Credito", 20m, 24, null);

        var resultado = servicio.Cotizar(request);

        resultado.EsExitoso.Should().BeTrue();
        var dto = resultado.Valor;
        dto.PrecioLista.Should().Be(120_000.00m);
        dto.Modalidad.Should().Be("Credito");
        dto.PorcentajeEnganche.Should().Be(20m);
        dto.MontoEnganche.Should().Be(24_000.00m);
        dto.SaldoAFinanciar.Should().Be(96_000.00m);
        dto.PlazoMeses.Should().Be(24);
        dto.TasaAnualPorcentaje.Should().Be(20m);
        dto.MensualidadEstimada.Should().Be(4_886.00m);
        dto.FechaPrimerPago.Should().Be(new DateOnly(2026, 12, 19));
        dto.TablaAmortizacion.Should().HaveCount(24);
        dto.TablaAmortizacion.Sum(f => f.Capital).Should().Be(96_000.00m);
        dto.TablaAmortizacion[^1].SaldoFinal.Should().Be(0.00m);
        dto.TotalAPagar.Should().Be(dto.MontoEnganche + dto.TablaAmortizacion.Sum(f => f.CuotaTotal));
    }

    [Fact]
    public void Cotizar_ModalidadContado_DebeRetornarTablaVaciaYSaldoCero()
    {
        var relojFijo = new FakeTimeProvider(new DateTimeOffset(2026, 11, 19, 10, 0, 0, TimeSpan.Zero));
        ISimuladorService servicio = new SimuladorService(relojFijo);
        var fechaManual = new DateOnly(2026, 11, 25);
        var request = new SimularCotizacionRequest(null, 3, "Contado", 0m, 0, fechaManual);

        var resultado = servicio.Cotizar(request);

        resultado.EsExitoso.Should().BeTrue();
        var dto = resultado.Valor;
        dto.PrecioLista.Should().Be(150_000.00m);
        dto.Modalidad.Should().Be("Contado");
        dto.MontoEnganche.Should().Be(150_000.00m);
        dto.SaldoAFinanciar.Should().Be(0m);
        dto.PlazoMeses.Should().Be(0);
        dto.TasaAnualPorcentaje.Should().Be(0m);
        dto.MensualidadEstimada.Should().Be(0m);
        dto.FechaPrimerPago.Should().Be(fechaManual);
        dto.TotalAPagar.Should().Be(150_000.00m);
        dto.TablaAmortizacion.Should().BeEmpty();
    }

    [Theory]
    [InlineData(2, "Credito", 15, 24)]
    [InlineData(2, "Credito", 10.5, 24)]
    [InlineData(2, "Credito", 20, 36)]
    [InlineData(2, "Credito", 20, 15)]
    [InlineData(5, "Credito", 20, 24)]
    [InlineData(null, "Credito", 20, 24)]
    [InlineData(2, "Arrendamiento", 20, 24)]
    [InlineData(2, "", 20, 24)]
    public void Cotizar_ConParametrosInvalidosCP10_DebeRetornarResultadoFallido(
        int? gavetas,
        string modalidad,
        decimal porcentajeEnganche,
        int plazoMeses)
    {
        ISimuladorService servicio = new SimuladorService();
        var request = new SimularCotizacionRequest(null, gavetas, modalidad, porcentajeEnganche, plazoMeses, null);

        var resultado = servicio.Cotizar(request);

        resultado.EsExitoso.Should().BeFalse();
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().NotBeNullOrWhiteSpace();
    }
}
