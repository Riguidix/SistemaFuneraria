using SistemaFuneraria.Core.Enums;

namespace SistemaFuneraria.Application.DTOs.Ventas;

/// <summary>
/// Solicitud administrativa para cambiar el estado operativo o legal de una venta/contrato (SF-063).
/// </summary>
public sealed record CambiarEstadoVentaRequest(
    EstadoVenta Estado,
    string Motivo
);

/// <summary>
/// Solicitud administrativa para la baja lógica o cancelación de un contrato (SF-063).
/// </summary>
public sealed record CancelarVentaRequest(
    string? Motivo = null
);
