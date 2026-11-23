using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Pruebas unitarias para la entidad Financiamiento y sus reglas de cobranza secuencial (SF-058).
/// </summary>
[Trait("Ticket", "SF-058")]
[Trait("Capa", "Core")]
[Trait("Tipo", "Unit")]
public class FinanciamientoTests
{
    private static readonly Guid VentaIdValida = Guid.NewGuid();
    private static readonly DateOnly FechaPrimerPagoValida = new(2026, 12, 18);

    private static Financiamiento CrearFinanciamientoValido(int plazoMeses = 24, decimal tasaAnual = 0.20m)
    {
        var capital = new Dinero(96000m);
        var filas = GeneradorAmortizacion.Generar(capital.Monto, plazoMeses);

        return Financiamiento.Crear(
            VentaIdValida,
            20m,
            new Dinero(24000m),
            capital,
            plazoMeses,
            tasaAnual,
            filas,
            FechaPrimerPagoValida);
    }

    [Fact]
    public void Crear_ConTabla24Meses_DebeGenerar24MensualidadesConCuadreExactoDeCapital()
    {
        var financiamiento = CrearFinanciamientoValido();

        financiamiento.Id.Should().NotBeEmpty();
        financiamiento.VentaId.Should().Be(VentaIdValida);
        financiamiento.PorcentajeEnganche.Should().Be(20m);
        financiamiento.MontoEnganche.Should().Be(new Dinero(24000m));
        financiamiento.CapitalFinanciado.Should().Be(new Dinero(96000m));
        financiamiento.SaldoInsoluto.Should().Be(new Dinero(96000m));
        financiamiento.PlazoMeses.Should().Be(24);
        financiamiento.TasaInteresAnual.Should().Be(0.20m);
        financiamiento.CuotaMensualActual.Monto.Should().BeGreaterThan(0m);
        financiamiento.Estado.Should().Be(EstadoFinanciamiento.Activo);
        financiamiento.Mensualidades.Should().HaveCount(24);
        financiamiento.Mensualidades.Should().OnlyContain(m => m.Estado == EstadoMensualidad.Pendiente);
        financiamiento.Mensualidades.Sum(m => m.CapitalProgramado.Monto).Should().Be(96000m);
        financiamiento.Mensualidades.First().FechaVencimiento.Should().Be(FechaPrimerPagoValida);
        financiamiento.Mensualidades.Last().FechaVencimiento.Should().Be(FechaPrimerPagoValida.AddMonths(23));
    }

    [Fact]
    public void Crear_ConParametrosInvalidos_DebeLanzarReglaNegocioException()
    {
        var filas24 = GeneradorAmortizacion.Generar(96000m, 24);
        var filasDescuadradas = filas24.Take(23).ToList();

        Action sinVenta = () => Financiamiento.Crear(Guid.Empty, 20m, new Dinero(24000m), new Dinero(96000m), 24, 0.20m, filas24, FechaPrimerPagoValida);
        Action engancheInvalido = () => Financiamiento.Crear(VentaIdValida, 15m, new Dinero(18000m), new Dinero(102000m), 24, 0.20m, filas24, FechaPrimerPagoValida);
        Action plazoInvalido = () => Financiamiento.Crear(VentaIdValida, 20m, new Dinero(24000m), new Dinero(96000m), 36, 0.20m, filas24, FechaPrimerPagoValida);
        Action tasaIncorrecta = () => Financiamiento.Crear(VentaIdValida, 20m, new Dinero(24000m), new Dinero(96000m), 24, 0.16m, filas24, FechaPrimerPagoValida);
        Action conteoIncorrecto = () => Financiamiento.Crear(VentaIdValida, 20m, new Dinero(24000m), new Dinero(96000m), 24, 0.20m, filasDescuadradas, FechaPrimerPagoValida);
        Action capitalDescuadrado = () => Financiamiento.Crear(VentaIdValida, 20m, new Dinero(24000m), new Dinero(95000m), 24, 0.20m, filas24, FechaPrimerPagoValida);

        sinVenta.Should().Throw<ReglaNegocioException>();
        engancheInvalido.Should().Throw<ReglaNegocioException>();
        plazoInvalido.Should().Throw<ReglaNegocioException>();
        tasaIncorrecta.Should().Throw<ReglaNegocioException>();
        conteoIncorrecto.Should().Throw<ReglaNegocioException>();
        capitalDescuadrado.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void AplicarPagoCuota_EnOrdenSecuencial_DebeDescontarSaldoYLiquidarAlCompletarPlazo()
    {
        var financiamiento = CrearFinanciamientoValido(plazoMeses: 12, tasaAnual: 0.16m);
        var fechaPago = new DateTimeOffset(2026, 12, 18, 10, 0, 0, TimeSpan.Zero);

        foreach (var numero in Enumerable.Range(1, 12))
        {
            financiamiento.AplicarPagoCuota(numero, fechaPago.AddMonths(numero - 1));
        }

        financiamiento.SaldoInsoluto.Should().Be(Dinero.Zero);
        financiamiento.Estado.Should().Be(EstadoFinanciamiento.Liquidado);
        financiamiento.Mensualidades.Should().OnlyContain(m => m.Estado == EstadoMensualidad.Pagada);
    }

    [Fact]
    public void AplicarPagoCuota_SaltandoMensualidadPendienteOVencida_DebeLanzarReglaNegocioException()
    {
        var financiamiento = CrearFinanciamientoValido(plazoMeses: 12, tasaAnual: 0.16m);
        var fechaPago = new DateTimeOffset(2026, 12, 18, 10, 0, 0, TimeSpan.Zero);

        Action saltarPrimera = () => financiamiento.AplicarPagoCuota(2, fechaPago);
        saltarPrimera.Should().Throw<ReglaNegocioException>();

        financiamiento.Mensualidades.Single(m => m.NumeroCuota == 1).MarcarVencida();
        Action saltarVencida = () => financiamiento.AplicarPagoCuota(2, fechaPago);
        saltarVencida.Should().Throw<ReglaNegocioException>();

        financiamiento.AplicarPagoCuota(1, fechaPago);
        financiamiento.Mensualidades.Single(m => m.NumeroCuota == 1).Estado.Should().Be(EstadoMensualidad.Pagada);
    }
}
