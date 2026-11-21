using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Entidad de dominio Mensualidad que representa una cuota individual del calendario de amortización (SF-058).
/// </summary>
public class Mensualidad
{
    public Guid Id { get; private set; }
    public Guid FinanciamientoId { get; private set; }
    public int NumeroCuota { get; private set; }
    public DateOnly FechaVencimiento { get; private set; }
    public Dinero SaldoInicialPeriodo { get; private set; }
    public Dinero CapitalProgramado { get; private set; }
    public Dinero InteresProgramado { get; private set; }
    public Dinero MontoTotalCuota { get; private set; }
    public Dinero SaldoFinalPeriodo { get; private set; }
    public EstadoMensualidad Estado { get; private set; }
    public DateTimeOffset? FechaPago { get; private set; }

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core.
    /// </summary>
    protected Mensualidad() { }

    private Mensualidad(Guid id, Guid financiamientoId, int numeroCuota, DateOnly fechaVencimiento, Dinero saldoInicialPeriodo, Dinero capitalProgramado, Dinero interesProgramado, Dinero montoTotalCuota, Dinero saldoFinalPeriodo)
    {
        Id = id;
        FinanciamientoId = financiamientoId;
        NumeroCuota = numeroCuota;
        FechaVencimiento = fechaVencimiento;
        SaldoInicialPeriodo = saldoInicialPeriodo;
        CapitalProgramado = capitalProgramado;
        InteresProgramado = interesProgramado;
        MontoTotalCuota = montoTotalCuota;
        SaldoFinalPeriodo = saldoFinalPeriodo;
        Estado = EstadoMensualidad.Pendiente;
    }

    /// <summary>
    /// Crea una mensualidad en estado Pendiente a partir de un renglón de amortización.
    /// </summary>
    public static Mensualidad Crear(Guid financiamientoId, RenglonAmortizacion renglon, DateOnly fechaVencimiento)
    {
        if (financiamientoId == Guid.Empty)
            throw new ReglaNegocioException("El identificador del financiamiento es obligatorio.");

        ArgumentNullException.ThrowIfNull(renglon);

        if (renglon.Periodo < 1)
            throw new ReglaNegocioException("El número de cuota debe ser mayor o igual a 1.");

        if (renglon.PagoMensual <= 0m)
            throw new ReglaNegocioException("El monto total de la cuota debe ser mayor a cero.");

        return new Mensualidad(Guid.NewGuid(), financiamientoId, renglon.Periodo, fechaVencimiento, new Dinero(renglon.SaldoInicial), new Dinero(renglon.CapitalAmortizado), new Dinero(renglon.Interes), new Dinero(renglon.PagoMensual), new Dinero(renglon.SaldoFinal));
    }

    /// <summary>
    /// Registra el pago de la cuota si se encuentra Pendiente o Vencida.
    /// </summary>
    public void MarcarPagada(DateTimeOffset fechaPago)
    {
        if (Estado == EstadoMensualidad.Pagada)
            throw new ReglaNegocioException($"La mensualidad {NumeroCuota} ya se encuentra pagada.");

        Estado = EstadoMensualidad.Pagada;
        FechaPago = fechaPago;
    }

    /// <summary>
    /// Marca la cuota como Vencida únicamente cuando se encuentra en estado Pendiente.
    /// </summary>
    public void MarcarVencida()
    {
        if (Estado != EstadoMensualidad.Pendiente)
            throw new ReglaNegocioException("Solo es posible marcar como vencida una mensualidad en estado Pendiente.");

        Estado = EstadoMensualidad.Vencida;
    }
}
