using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Representa una cuota periódica proyectada en el calendario financiero del crédito (SF-030, ERD 3.5).
/// Inmutable en sus importes y controlada en sus transiciones de estado de cobro.
/// </summary>
public class Mensualidad
{
    public Guid Id { get; private set; }
    public Guid FinanciamientoId { get; private set; }
    public int NumeroPeriodo { get; private set; }
    public DateTimeOffset FechaVencimiento { get; private set; }
    public decimal MontoCuota { get; private set; }
    public decimal MontoCapital { get; private set; }
    public decimal MontoInteres { get; private set; }
    public decimal SaldoInsolutoRestante { get; private set; }
    public EstadoMensualidad Estado { get; private set; }

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core para materialización.
    /// </summary>
    protected Mensualidad() { }

    private Mensualidad(
        Guid id,
        Guid financiamientoId,
        int numeroPeriodo,
        DateTimeOffset fechaVencimiento,
        decimal montoCuota,
        decimal montoCapital,
        decimal montoInteres,
        decimal saldoInsolutoRestante,
        EstadoMensualidad estado)
    {
        Id = id;
        FinanciamientoId = financiamientoId;
        NumeroPeriodo = numeroPeriodo;
        FechaVencimiento = fechaVencimiento;
        MontoCuota = montoCuota;
        MontoCapital = montoCapital;
        MontoInteres = montoInteres;
        SaldoInsolutoRestante = saldoInsolutoRestante;
        Estado = estado;
    }

    /// <summary>
    /// Factory Method canónico para crear una cuota de mensualidad con validación de invariantes.
    /// </summary>
    public static Mensualidad Crear(
        int numeroPeriodo,
        DateTimeOffset fechaVencimiento,
        decimal montoCuota,
        decimal montoCapital,
        decimal montoInteres,
        decimal saldoInsolutoRestante)
    {
        if (numeroPeriodo <= 0)
        {
            throw new ReglaNegocioException("El número de periodo de la mensualidad debe ser mayor o igual a 1.");
        }

        if (montoCuota <= 0m)
        {
            throw new ReglaNegocioException("El monto de la cuota mensual debe ser estrictamente mayor a cero.");
        }

        if (montoCapital < 0m)
        {
            throw new ReglaNegocioException("El monto de capital amortizado no puede ser negativo.");
        }

        if (montoInteres < 0m)
        {
            throw new ReglaNegocioException("El monto de interés ordinario no puede ser negativo.");
        }

        if (saldoInsolutoRestante < 0m)
        {
            throw new ReglaNegocioException("El saldo insoluto restante no puede ser negativo.");
        }

        return new Mensualidad(
            Guid.NewGuid(),
            Guid.Empty,
            numeroPeriodo,
            fechaVencimiento,
            montoCuota,
            montoCapital,
            montoInteres,
            saldoInsolutoRestante,
            EstadoMensualidad.Pendiente);
    }

    /// <summary>
    /// Registra el pago formal de la mensualidad en caja y transiciona su estado a Pagada.
    /// </summary>
    public void RegistrarPago()
    {
        if (Estado == EstadoMensualidad.Pagada)
        {
            throw new ReglaNegocioException($"La mensualidad del periodo {NumeroPeriodo} ya se encuentra pagada.");
        }

        Estado = EstadoMensualidad.Pagada;
    }

    internal void AsignarFinanciamientoId(Guid financiamientoId)
    {
        FinanciamientoId = financiamientoId;
    }
}
