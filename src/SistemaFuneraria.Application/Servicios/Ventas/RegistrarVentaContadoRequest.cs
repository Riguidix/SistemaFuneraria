namespace SistemaFuneraria.Application.Servicios.Ventas;

/// <summary>
/// DTO de entrada inmutable para el registro de una venta de contado en una sola exhibición (SF-024).
/// </summary>
/// <param name="ClienteId">Identificador único del cliente comprador.</param>
/// <param name="PrecioLote">Precio oficial de lista del lote funerario ($120k, $150k o $180k).</param>
/// <param name="DescripcionLote">Descripción del lote seleccionado.</param>
/// <param name="Observaciones">Notas adicionales de la transacción.</param>
public record RegistrarVentaContadoRequest(
    Guid ClienteId,
    decimal PrecioLote,
    string DescripcionLote,
    string? Observaciones = null
);
