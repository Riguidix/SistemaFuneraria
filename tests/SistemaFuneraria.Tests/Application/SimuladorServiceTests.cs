using FluentAssertions;
using SistemaFuneraria.Application.Servicios.Simulador;
using Xunit;

namespace SistemaFuneraria.Tests.Application;

public class SimuladorServiceTests
{
    private readonly ISimuladorService _simuladorService;

    public SimuladorServiceTests()
    {
        _simuladorService = new SimuladorService();
    }

    [Fact(DisplayName = "SF-013 / BDD: Simular financiamiento $120k, 20% enganche, 12 meses debe retornar cuota $8,710.16 y 12 cuotas")]
    public void Simular_ConParametrosValidosCP06_DebeRetornarSimulacionExitosaYTablaCompleta()
    {
        // Arrange
        var request = new SimularFinanciamientoRequest(
            PrecioLote: 120_000m,
            PorcentajeEnganche: 20,
            PlazoMeses: 12
        );

        // Act
        var resultado = _simuladorService.Simular(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EsExitoso.Should().BeTrue();
        resultado.Error.Should().BeEmpty();

        var dto = resultado.Valor;
        dto.Should().NotBeNull();
        dto.PrecioLote.Should().Be(120_000m);
        dto.PorcentajeEnganche.Should().Be(20);
        dto.MontoEnganche.Should().Be(24_000m);
        dto.CapitalFinanciado.Should().Be(96_000m);
        dto.PlazoMeses.Should().Be(12);
        dto.TasaAnual.Should().Be(0.16m);
        dto.MensualidadOrdinaria.Should().Be(8_710.16m);

        // Verificación de la tabla de amortización
        dto.TablaAmortizacion.Should().HaveCount(12);
        dto.TablaAmortizacion[0].Periodo.Should().Be(1);
        dto.TablaAmortizacion[0].SaldoInicial.Should().Be(96_000.00m);
        dto.TablaAmortizacion[0].Cuota.Should().Be(8_710.16m);
        dto.TablaAmortizacion[0].Interes.Should().Be(1_280.00m);
        dto.TablaAmortizacion[0].Amortizacion.Should().Be(7_430.16m);
        dto.TablaAmortizacion[0].SaldoFinal.Should().Be(88_569.84m);

        // Verificación de la última cuota con absorción de centavos y saldo $0.00
        var cuotaFinal = dto.TablaAmortizacion[11];
        cuotaFinal.Periodo.Should().Be(12);
        cuotaFinal.SaldoInicial.Should().Be(8_595.58m);
        cuotaFinal.Interes.Should().Be(114.61m);
        cuotaFinal.Amortizacion.Should().Be(8_595.58m);
        cuotaFinal.Cuota.Should().Be(8_710.19m);
        cuotaFinal.SaldoFinal.Should().Be(0.00m);
    }

    [Theory(DisplayName = "SF-013: Parámetros no autorizados deben retornar Resultado fallido con error de negocio")]
    [InlineData(100_000, 20, 12)] // Precio no autorizado
    [InlineData(120_000, 15, 12)] // Enganche no autorizado
    [InlineData(120_000, 20, 36)] // Plazo no autorizado
    public void Simular_ConParametrosNoAutorizados_DebeRetornarFallo(decimal precio, int enganche, int plazo)
    {
        // Arrange
        var request = new SimularFinanciamientoRequest(precio, enganche, plazo);

        // Act
        var resultado = _simuladorService.Simular(request);

        // Assert
        resultado.EsExitoso.Should().BeFalse();
        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Should().NotBeNullOrWhiteSpace();
    }
}
