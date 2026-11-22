using SistemaFuneraria.Core.Enums;

namespace SistemaFuneraria.Application.DTOs.Ventas;

/// <summary>
/// Comando para el registro atómico de una venta en modalidad Contado (SF-060, CP-02).
/// </summary>
public sealed record RegistrarVentaContadoCommand(
    Guid ClienteId,
    Guid LoteId,
    string VendedorId,
    MetodoPago MetodoPago = MetodoPago.Efectivo,
    string? ReferenciaPago = null
);
