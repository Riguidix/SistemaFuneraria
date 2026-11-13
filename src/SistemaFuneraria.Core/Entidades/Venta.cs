using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Encabeza toda operación comercial cerrada por un vendedor en modalidad contado o financiada (SF-030, ERD 3.3).
/// </summary>
public class Venta
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public string LoteId { get; private set; } = string.Empty;
    public string VendedorId { get; private set; } = string.Empty;
    public TipoVenta TipoVenta { get; private set; }
    public decimal MontoTotal { get; private set; }
    public DateTimeOffset FechaVenta { get; private set; }
    public EstadoVenta Estado { get; private set; }

    /// <summary>
    /// Contrato de financiamiento asociado cuando la venta es a crédito (Relación 1:1, nula en ventas de contado).
    /// </summary>
    public Financiamiento? Financiamiento { get; private set; }

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core para materialización.
    /// </summary>
    protected Venta() { }

    private Venta(
        Guid id,
        Guid clienteId,
        string loteId,
        string vendedorId,
        TipoVenta tipoVenta,
        decimal montoTotal,
        DateTimeOffset fechaVenta,
        EstadoVenta estado,
        Financiamiento? financiamiento)
    {
        Id = id;
        ClienteId = clienteId;
        LoteId = loteId;
        VendedorId = vendedorId;
        TipoVenta = tipoVenta;
        MontoTotal = montoTotal;
        FechaVenta = fechaVenta;
        Estado = estado;
        Financiamiento = financiamiento;
    }

    /// <summary>
    /// Factory Method para crear una venta en modalidad de contado en una sola exhibición.
    /// </summary>
    public static Venta CrearContado(Guid clienteId, string loteId, string vendedorId, decimal montoTotal)
    {
        ValidarParametrosComunes(clienteId, loteId, vendedorId, montoTotal);

        return new Venta(
            Guid.NewGuid(),
            clienteId,
            loteId.Trim(),
            vendedorId.Trim(),
            TipoVenta.Contado,
            montoTotal,
            DateTimeOffset.UtcNow,
            EstadoVenta.Completada,
            null);
    }

    /// <summary>
    /// Factory Method para crear una venta financiada con enganche y calendario de cuotas asociado.
    /// </summary>
    public static Venta CrearFinanciada(
        Guid clienteId,
        string loteId,
        string vendedorId,
        decimal montoTotal,
        Financiamiento financiamiento)
    {
        ValidarParametrosComunes(clienteId, loteId, vendedorId, montoTotal);

        if (financiamiento == null)
        {
            throw new ReglaNegocioException("La venta financiada requiere un contrato de financiamiento válido.");
        }

        if (montoTotal != (financiamiento.Enganche + financiamiento.CapitalFinanciado))
        {
            throw new ReglaNegocioException(
                $"El monto total ({montoTotal:C2}) debe coincidir exactamente con la suma del enganche ({financiamiento.Enganche:C2}) y el capital financiado ({financiamiento.CapitalFinanciado:C2}).");
        }

        var venta = new Venta(
            Guid.NewGuid(),
            clienteId,
            loteId.Trim(),
            vendedorId.Trim(),
            TipoVenta.Financiado,
            montoTotal,
            DateTimeOffset.UtcNow,
            EstadoVenta.Completada,
            financiamiento);

        financiamiento.AsignarVentaId(venta.Id);

        return venta;
    }

    private static void ValidarParametrosComunes(Guid clienteId, string loteId, string vendedorId, decimal montoTotal)
    {
        if (clienteId == Guid.Empty)
        {
            throw new ReglaNegocioException("El ID del cliente es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(loteId))
        {
            throw new ReglaNegocioException("El ID del lote es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(vendedorId))
        {
            throw new ReglaNegocioException("El ID del vendedor es obligatorio.");
        }

        if (montoTotal <= 0m)
        {
            throw new ReglaNegocioException("El monto total de la operación debe ser estrictamente mayor a cero.");
        }
    }
}
