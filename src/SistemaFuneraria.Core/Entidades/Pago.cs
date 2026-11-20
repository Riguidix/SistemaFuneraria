using System.Text.RegularExpressions;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Entidad de dominio Pago que representa un ingreso monetario registrado en la ventanilla de caja (SF-075).
/// </summary>
public class Pago
{
    private static readonly Regex RegexFolioPago = new(@"^PAG-\d{4}-\d{6}$", RegexOptions.Compiled);

    public Guid Id { get; private set; }
    public string Folio { get; private set; } = string.Empty;
    public Guid VentaId { get; private set; }
    public Guid? FinanciamientoId { get; private set; }
    public Guid? MensualidadId { get; private set; }
    public string CajeroId { get; private set; } = string.Empty;
    public TipoPago Tipo { get; private set; }
    public MetodoPago Metodo { get; private set; }
    public string? Referencia { get; private set; }
    public Dinero MontoTotal { get; private set; }
    public Dinero MontoCapital { get; private set; }
    public Dinero MontoInteres { get; private set; }
    public Dinero SaldoAnterior { get; private set; }
    public Dinero SaldoPosterior { get; private set; }
    public string ClaveIdempotencia { get; private set; } = string.Empty;
    public DateTimeOffset FechaPago { get; private set; }

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core para la materialización de entidades.
    /// </summary>
    protected Pago() { }

    private Pago(
        Guid id,
        string folio,
        Guid ventaId,
        Guid? financiamientoId,
        Guid? mensualidadId,
        string cajeroId,
        TipoPago tipo,
        MetodoPago metodo,
        string? referencia,
        Dinero montoTotal,
        Dinero montoCapital,
        Dinero montoInteres,
        Dinero saldoAnterior,
        Dinero saldoPosterior,
        string claveIdempotencia,
        DateTimeOffset fechaPago)
    {
        Id = id;
        Folio = folio;
        VentaId = ventaId;
        FinanciamientoId = financiamientoId;
        MensualidadId = mensualidadId;
        CajeroId = cajeroId;
        Tipo = tipo;
        Metodo = metodo;
        Referencia = referencia;
        MontoTotal = montoTotal;
        MontoCapital = montoCapital;
        MontoInteres = montoInteres;
        SaldoAnterior = saldoAnterior;
        SaldoPosterior = saldoPosterior;
        ClaveIdempotencia = claveIdempotencia;
        FechaPago = fechaPago;
    }

    /// <summary>
    /// Factory Method canónico para crear un registro inmutable de pago con validaciones de invariantes de negocio.
    /// </summary>
    public static Pago Crear(
        string folio,
        Guid ventaId,
        Guid? financiamientoId,
        Guid? mensualidadId,
        string cajeroId,
        TipoPago tipo,
        MetodoPago metodo,
        string? referencia,
        Dinero montoTotal,
        Dinero montoCapital,
        Dinero montoInteres,
        Dinero saldoAnterior,
        Dinero saldoPosterior,
        string claveIdempotencia,
        DateTimeOffset fechaPago)
    {
        if (string.IsNullOrWhiteSpace(folio) || !RegexFolioPago.IsMatch(folio.Trim()))
            throw new ReglaNegocioException($"El formato del folio '{folio}' es inválido. Debe cumplir con el patrón PAG-YYYY-XXXXXX.");

        if (ventaId == Guid.Empty)
            throw new ReglaNegocioException("El identificador de la venta es obligatorio.");

        if (string.IsNullOrWhiteSpace(cajeroId))
            throw new ReglaNegocioException("El identificador del cajero es obligatorio.");

        if (!Enum.IsDefined(tipo))
            throw new ReglaNegocioException("El tipo de pago especificado no es válido.");

        if (!Enum.IsDefined(metodo))
            throw new ReglaNegocioException("El método de pago especificado no es válido.");

        if (string.IsNullOrWhiteSpace(claveIdempotencia))
            throw new ReglaNegocioException("La clave de idempotencia es obligatoria.");

        if (montoTotal.Monto <= 0m)
            throw new ReglaNegocioException("El monto total del pago debe ser mayor a cero.");

        if (montoCapital.Monto < 0m || montoInteres.Monto < 0m)
            throw new ReglaNegocioException("Los desgloses de capital e interés no pueden ser negativos.");

        if (montoCapital.Monto + montoInteres.Monto != montoTotal.Monto)
            throw new ReglaNegocioException("La suma del capital y el interés debe coincidir exactamente con el monto total del pago.");

        if (saldoAnterior.Monto < 0m || saldoPosterior.Monto < 0m)
            throw new ReglaNegocioException("Los saldos anterior y posterior no pueden ser negativos.");

        string? referenciaNormalizada = referencia?.Trim();
        if (metodo is MetodoPago.TransferenciaBancaria or MetodoPago.Tarjeta)
        {
            if (string.IsNullOrWhiteSpace(referenciaNormalizada) || referenciaNormalizada.Length < 4)
                throw new ReglaNegocioException("La referencia (folio o últimos 4 dígitos) es obligatoria para transferencias y tarjetas");
        }

        return new Pago(
            Guid.NewGuid(),
            folio.Trim().ToUpperInvariant(),
            ventaId,
            financiamientoId,
            mensualidadId,
            cajeroId.Trim(),
            tipo,
            metodo,
            referenciaNormalizada,
            montoTotal,
            montoCapital,
            montoInteres,
            saldoAnterior,
            saldoPosterior,
            claveIdempotencia.Trim(),
            fechaPago);
    }
}
