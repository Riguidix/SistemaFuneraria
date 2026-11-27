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
/// Pruebas unitarias de dominio para el cálculo de abonos extraordinarios a capital (SF-084, CP-07).
/// </summary>
public class CalculadorAbonoCapitalTests
{
    [Fact]
    public void Recalcular_CasoCP07_ConSaldo74078_DebeDescontarCapitalYCalcularCuotaFrancesaPura()
    {
        decimal saldoInsoluto = 74078.00m;
        decimal montoAbono = 10000.00m;
        int cuotasRestantes = 9;
        decimal tasaAnual = 0.16m;
        int periodoInicial = 4;

        var resultado = CalculadorAbonoCapital.Recalcular(
            saldoInsoluto,
            montoAbono,
            cuotasRestantes,
            tasaAnual,
            periodoInicial,
            tieneMensualidadesVencidas: false);

        resultado.SaldoAnterior.Should().Be(74078.00m);
        resultado.MontoAbono.Should().Be(10000.00m);
        resultado.NuevoSaldoInsoluto.Should().Be(64078.00m);
        resultado.NuevaCuota.Should().Be(7602.81m);
        resultado.CuotasRestantes.Should().Be(9);
        resultado.TablaRecalculada.Should().HaveCount(9);

        for (int i = 0; i < 8; i++)
        {
            resultado.TablaRecalculada[i].PagoMensual.Should().Be(7602.81m);
            resultado.TablaRecalculada[i].Periodo.Should().Be(periodoInicial + i);
        }

        var ultimaCuota = resultado.TablaRecalculada[^1];
        ultimaCuota.Periodo.Should().Be(12);
        ultimaCuota.SaldoFinal.Should().Be(0.00m);
    }

    [Fact]
    public void Recalcular_CasoCP07_DesdeContratoFinanciado_DebeAlinearConOraculoCsv()
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

        financiamiento.SaldoInsoluto.Monto.Should().Be(73410.99m);

        var abono = new Dinero(10000.00m);
        var resultado = CalculadorAbonoCapital.Recalcular(financiamiento, abono);

        resultado.SaldoAnterior.Should().Be(73410.99m);
        resultado.MontoAbono.Should().Be(10000.00m);
        resultado.NuevoSaldoInsoluto.Should().Be(63410.99m);
        resultado.NuevaCuota.Should().Be(7523.67m);
        resultado.CuotasRestantes.Should().Be(9);

        var ultimaCuota = resultado.TablaRecalculada[^1];
        ultimaCuota.Periodo.Should().Be(12);
        ultimaCuota.SaldoFinal.Should().Be(0.00m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public void Recalcular_ConMontoAbonoInvalido_DebeLanzarReglaNegocioException(decimal abonoInvalido)
    {
        Action accion = () => CalculadorAbonoCapital.Recalcular(
            saldoInsoluto: 50000m,
            montoAbono: abonoInvalido,
            cuotasRestantes: 6,
            tasaInteresAnual: 0.16m);

        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("*mayor a cero*");
    }

    [Theory]
    [InlineData(50000, 50000)]
    [InlineData(50000, 55000)]
    public void Recalcular_ConMontoAbonoMayorOIgualASaldoInsoluto_DebeLanzarReglaNegocioException(decimal saldo, decimal abono)
    {
        Action accion = () => CalculadorAbonoCapital.Recalcular(
            saldoInsoluto: saldo,
            montoAbono: abono,
            cuotasRestantes: 6,
            tasaInteresAnual: 0.16m);

        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("*liquidación*");
    }

    [Fact]
    public void Recalcular_ConMensualidadesVencidas_DebeLanzarReglaNegocioException()
    {
        Action accion = () => CalculadorAbonoCapital.Recalcular(
            saldoInsoluto: 60000m,
            montoAbono: 5000m,
            cuotasRestantes: 8,
            tasaInteresAnual: 0.16m,
            periodoInicial: 5,
            tieneMensualidadesVencidas: true);

        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("*vencida*");
    }

    [Fact]
    public void Recalcular_ConFinanciamientoEnMora_DebeLanzarReglaNegocioException()
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

        financiamiento.Mensualidades.First().MarcarVencida();

        Action accion = () => CalculadorAbonoCapital.Recalcular(financiamiento, new Dinero(5000m));

        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("*vencida*");
    }
}
