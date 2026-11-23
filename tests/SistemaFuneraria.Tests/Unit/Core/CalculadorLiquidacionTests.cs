using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Pruebas unitarias para el motor de liquidación anticipada de créditos (SF-085, CP-08, RF-12, RN-09).
/// </summary>
public class CalculadorLiquidacionTests
{
    [Fact]
    public void Calcular_CasoCP08_SobrecargaPura_DebeCertificarOraculoAlCentavo()
    {
        var capitalPendiente = new Dinero(73410.99m);
        var interesesFuturos = new Dinero(4980.45m);
        decimal porcentajeDescuento = 0.10m;

        var resultado = CalculadorLiquidacion.Calcular(capitalPendiente, interesesFuturos, porcentajeDescuento);

        resultado.CapitalPendiente.Monto.Should().Be(73410.99m);
        resultado.InteresesFuturosProgramados.Monto.Should().Be(4980.45m);
        resultado.PorcentajeDescuentoIntereses.Should().Be(0.10m);
        resultado.DescuentoIntereses.Monto.Should().Be(498.05m);
        resultado.InteresesNetosAPagar.Monto.Should().Be(4482.40m);
        resultado.Penalizacion.Monto.Should().Be(0.00m);
        resultado.MontoTotalLiquidacion.Monto.Should().Be(77893.39m);
    }

    [Fact]
    public void Calcular_CasoCP08_DesdeFinanciamiento_DebeCalcularYDesglosarCorrectamente()
    {
        var ventaId = Guid.NewGuid();
        var capital = new Dinero(96000.00m);
        var enganche = new Dinero(24000.00m);
        var filas = GeneradorAmortizacion.Generar(96000.00m, 12);
        var primerPago = new DateOnly(2025, 1, 1);

        var financiamiento = Financiamiento.Crear(
            ventaId,
            20m,
            enganche,
            capital,
            12,
            0.16m,
            filas,
            primerPago);

        financiamiento.AplicarPagoCuota(1, DateTimeOffset.UtcNow);
        financiamiento.AplicarPagoCuota(2, DateTimeOffset.UtcNow);
        financiamiento.AplicarPagoCuota(3, DateTimeOffset.UtcNow);

        var resultado = CalculadorLiquidacion.Calcular(financiamiento);

        resultado.CapitalPendiente.Monto.Should().Be(73410.99m);
        resultado.InteresesFuturosProgramados.Monto.Should().Be(4980.45m);
        resultado.DescuentoIntereses.Monto.Should().Be(498.05m);
        resultado.InteresesNetosAPagar.Monto.Should().Be(4482.40m);
        resultado.Penalizacion.Monto.Should().Be(0.00m);
        resultado.MontoTotalLiquidacion.Monto.Should().Be(77893.39m);
    }

    [Fact]
    public void AplicarLiquidacion_DebeDejarSaldoEnCeroYEstadoLiquidado()
    {
        var ventaId = Guid.NewGuid();
        var capital = new Dinero(96000.00m);
        var enganche = new Dinero(24000.00m);
        var filas = GeneradorAmortizacion.Generar(96000.00m, 12);
        var primerPago = new DateOnly(2025, 1, 1);

        var financiamiento = Financiamiento.Crear(
            ventaId,
            20m,
            enganche,
            capital,
            12,
            0.16m,
            filas,
            primerPago);

        financiamiento.AplicarPagoCuota(1, DateTimeOffset.UtcNow);
        financiamiento.AplicarPagoCuota(2, DateTimeOffset.UtcNow);
        financiamiento.AplicarPagoCuota(3, DateTimeOffset.UtcNow);

        var fechaLiquidacion = DateTimeOffset.UtcNow;
        CalculadorLiquidacion.AplicarLiquidacion(financiamiento, fechaLiquidacion);

        financiamiento.SaldoInsoluto.Monto.Should().Be(0.00m);
        financiamiento.Estado.Should().Be(EstadoFinanciamiento.Liquidado);
        financiamiento.Mensualidades.Should().OnlyContain(m => m.Estado == EstadoMensualidad.Pagada);
    }

    [Fact]
    public void Calcular_FinanciamientoYaLiquidado_DebeLanzarReglaNegocioException()
    {
        var ventaId = Guid.NewGuid();
        var capital = new Dinero(96000.00m);
        var enganche = new Dinero(24000.00m);
        var filas = GeneradorAmortizacion.Generar(96000.00m, 12);
        var primerPago = new DateOnly(2025, 1, 1);

        var financiamiento = Financiamiento.Crear(
            ventaId,
            20m,
            enganche,
            capital,
            12,
            0.16m,
            filas,
            primerPago);

        CalculadorLiquidacion.AplicarLiquidacion(financiamiento, DateTimeOffset.UtcNow);

        Action accion = () => CalculadorLiquidacion.Calcular(financiamiento);

        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("*ya se encuentra liquidado*");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Calcular_ConPorcentajeDescuentoInvalido_DebeLanzarReglaNegocioException(decimal porcentajeInvalido)
    {
        Action accion = () => CalculadorLiquidacion.Calcular(
            new Dinero(50000m),
            new Dinero(3000m),
            porcentajeInvalido);

        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("*entre 0% y 100%*");
    }

    [Fact]
    public void Calcular_ConCapitalMenorOIgualACero_DebeLanzarReglaNegocioException()
    {
        Action accion = () => CalculadorLiquidacion.Calcular(
            new Dinero(0m),
            new Dinero(3000m),
            0.10m);

        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("*mayor a cero*");
    }
}
