using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Pruebas unitarias de dominio para la evaluación de vencimientos de créditos mediante TimeProvider (SF-086, CP-09, RF-13, RF-14, RN-10).
/// </summary>
public class EvaluadorVencimientosTests
{
    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void EstablecerFecha(DateTimeOffset nuevaFecha) => _utcNow = nuevaFecha;
    }

    [Fact]
    public void Evaluar_CasoCP09_CuotaAtrasada_DebeTransicionarAVencidaSinRecargoMoratorio()
    {
        var ventaId = Guid.NewGuid();
        var capital = new Dinero(96000.00m);
        var enganche = new Dinero(24000.00m);
        var filas = GeneradorAmortizacion.Generar(96000.00m, 12);
        var fechaPrimerPago = new DateOnly(2025, 8, 2);

        var financiamiento = Financiamiento.Crear(
            ventaId,
            20m,
            enganche,
            capital,
            12,
            0.16m,
            filas,
            fechaPrimerPago);

        financiamiento.AplicarPagoCuota(1, new DateTimeOffset(2025, 8, 2, 10, 0, 0, TimeSpan.Zero));
        financiamiento.AplicarPagoCuota(2, new DateTimeOffset(2025, 9, 2, 10, 0, 0, TimeSpan.Zero));
        financiamiento.AplicarPagoCuota(3, new DateTimeOffset(2025, 10, 2, 10, 0, 0, TimeSpan.Zero));

        var cuota4 = financiamiento.Mensualidades.First(m => m.NumeroCuota == 4);
        cuota4.FechaVencimiento.Should().Be(new DateOnly(2025, 11, 2));
        var montoOriginal = cuota4.MontoTotalCuota;

        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 11, 18, 12, 0, 0, TimeSpan.Zero));

        int cuotasVencidas = EvaluadorVencimientos.Evaluar(financiamiento, fakeTimeProvider);

        cuotasVencidas.Should().Be(1);
        cuota4.Estado.Should().Be(EstadoMensualidad.Vencida);
        financiamiento.Estado.Should().Be(EstadoFinanciamiento.EnMora);
        cuota4.MontoTotalCuota.Should().Be(montoOriginal);
    }

    [Fact]
    public void Evaluar_CuotaQueVenceHoy_DebePermanecerEnEstadoPendiente()
    {
        var ventaId = Guid.NewGuid();
        var capital = new Dinero(96000.00m);
        var enganche = new Dinero(24000.00m);
        var filas = GeneradorAmortizacion.Generar(96000.00m, 12);
        var fechaHoy = new DateOnly(2025, 11, 18);

        var financiamiento = Financiamiento.Crear(
            ventaId,
            20m,
            enganche,
            capital,
            12,
            0.16m,
            filas,
            fechaHoy);

        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 11, 18, 15, 0, 0, TimeSpan.Zero));

        int cuotasVencidas = EvaluadorVencimientos.Evaluar(financiamiento, fakeTimeProvider);

        cuotasVencidas.Should().Be(0);
        var primeraCuota = financiamiento.Mensualidades.First(m => m.NumeroCuota == 1);
        primeraCuota.Estado.Should().Be(EstadoMensualidad.Pendiente);
        financiamiento.Estado.Should().Be(EstadoFinanciamiento.Activo);
    }

    [Fact]
    public void Evaluar_FinanciamientoYaLiquidado_NoDebeAlterarEstadosNiLanzarExcepcion()
    {
        var ventaId = Guid.NewGuid();
        var capital = new Dinero(96000.00m);
        var enganche = new Dinero(24000.00m);
        var filas = GeneradorAmortizacion.Generar(96000.00m, 12);
        var fechaPrimerPago = new DateOnly(2025, 1, 1);

        var financiamiento = Financiamiento.Crear(
            ventaId,
            20m,
            enganche,
            capital,
            12,
            0.16m,
            filas,
            fechaPrimerPago);

        financiamiento.AplicarLiquidacion(DateTimeOffset.UtcNow);

        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        int cuotasVencidas = EvaluadorVencimientos.Evaluar(financiamiento, fakeTimeProvider);

        cuotasVencidas.Should().Be(0);
        financiamiento.Estado.Should().Be(EstadoFinanciamiento.Liquidado);
    }

    [Fact]
    public void Evaluar_SinCuotasVencidas_FinanciamientoPermaneceActivo()
    {
        var ventaId = Guid.NewGuid();
        var capital = new Dinero(96000.00m);
        var enganche = new Dinero(24000.00m);
        var filas = GeneradorAmortizacion.Generar(96000.00m, 12);
        var fechaPrimerPago = new DateOnly(2026, 1, 1);

        var financiamiento = Financiamiento.Crear(
            ventaId,
            20m,
            enganche,
            capital,
            12,
            0.16m,
            filas,
            fechaPrimerPago);

        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero));

        int cuotasVencidas = EvaluadorVencimientos.Evaluar(financiamiento, fakeTimeProvider);

        cuotasVencidas.Should().Be(0);
        financiamiento.Estado.Should().Be(EstadoFinanciamiento.Activo);
        financiamiento.Mensualidades.Should().OnlyContain(m => m.Estado == EstadoMensualidad.Pendiente);
    }

    [Fact]
    public void Evaluar_FinanciamientoNulo_DebeLanzarArgumentNullException()
    {
        Action accion = () => EvaluadorVencimientos.Evaluar(null!);

        accion.Should().Throw<ArgumentNullException>();
    }
}
