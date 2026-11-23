using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Pruebas unitarias para las transiciones de estado de la entidad Mensualidad (SF-058).
/// </summary>
[Trait("Ticket", "SF-058")]
[Trait("Capa", "Core")]
[Trait("Tipo", "Unit")]
public class MensualidadTests
{
    private static Mensualidad CrearMensualidadPendiente()
    {
        var renglon = new RenglonAmortizacion(1, 96000m, 1600m, 3296.05m, 4896.05m, 92703.95m);
        return Mensualidad.Crear(Guid.NewGuid(), renglon, new DateOnly(2026, 12, 18));
    }

    [Fact]
    public void Crear_ConRenglonValido_DebeIniciarEnEstadoPendiente()
    {
        var mensualidad = CrearMensualidadPendiente();

        mensualidad.Id.Should().NotBeEmpty();
        mensualidad.NumeroCuota.Should().Be(1);
        mensualidad.SaldoInicialPeriodo.Should().Be(new Dinero(96000m));
        mensualidad.InteresProgramado.Should().Be(new Dinero(1600m));
        mensualidad.CapitalProgramado.Should().Be(new Dinero(3296.05m));
        mensualidad.MontoTotalCuota.Should().Be(new Dinero(4896.05m));
        mensualidad.SaldoFinalPeriodo.Should().Be(new Dinero(92703.95m));
        mensualidad.Estado.Should().Be(EstadoMensualidad.Pendiente);
        mensualidad.FechaPago.Should().BeNull();
    }

    [Fact]
    public void MarcarPagada_DesdePendienteOVencida_DebeActualizarEstadoYFechaPago()
    {
        var pendiente = CrearMensualidadPendiente();
        var fechaPago = new DateTimeOffset(2026, 12, 18, 11, 30, 0, TimeSpan.Zero);

        pendiente.MarcarPagada(fechaPago);
        pendiente.Estado.Should().Be(EstadoMensualidad.Pagada);
        pendiente.FechaPago.Should().Be(fechaPago);

        var vencida = CrearMensualidadPendiente();
        vencida.MarcarVencida();
        vencida.MarcarPagada(fechaPago);
        vencida.Estado.Should().Be(EstadoMensualidad.Pagada);
        vencida.FechaPago.Should().Be(fechaPago);
    }

    [Fact]
    public void MarcarPagada_CuandoYaEstaPagada_DebeLanzarReglaNegocioException()
    {
        var mensualidad = CrearMensualidadPendiente();
        var fechaPago = new DateTimeOffset(2026, 12, 18, 11, 30, 0, TimeSpan.Zero);
        mensualidad.MarcarPagada(fechaPago);

        Action doblePago = () => mensualidad.MarcarPagada(fechaPago.AddDays(1));

        doblePago.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void MarcarVencida_CuandoNoEstaPendiente_DebeLanzarReglaNegocioException()
    {
        var mensualidad = CrearMensualidadPendiente();
        mensualidad.MarcarPagada(DateTimeOffset.UtcNow);

        Action vencerPagada = () => mensualidad.MarcarVencida();

        vencerPagada.Should().Throw<ReglaNegocioException>();
    }
}
