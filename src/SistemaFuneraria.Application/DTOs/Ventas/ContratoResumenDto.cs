namespace SistemaFuneraria.Application.DTOs.Ventas;

/// <summary>
/// Resumen del contrato comercial y comprobante de venta generado (SF-060, CP-02, Figma Capturas 1, 9, 10).
/// </summary>
public sealed record ContratoResumenDto(
    Guid VentaId,
    string FolioContrato,
    Guid ClienteId,
    string ClienteNombre,
    Guid LoteId,
    string LoteClave,
    string Modalidad,
    decimal PrecioPactado,
    string Estado,
    DateTimeOffset FechaVenta,
    string FolioPago
);
