using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Core.Enums;

namespace SistemaFuneraria.Application.Servicios.Ventas;

/// <summary>
/// Contrato del servicio de aplicación para la gestión del ciclo de vida de ventas funerarias (SF-060, SF-061, SF-063).
/// </summary>
public interface IVentaService
{
    /// <summary>
    /// Registra atómicamente una venta de contado, cambia el lote a Vendido y asienta el comprobante de pago único (CP-02).
    /// </summary>
    Task<Resultado<ContratoResumenDto>> RegistrarVentaContadoAsync(
        RegistrarVentaContadoCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el resumen detallado de una venta y su comprobante a partir de su identificador UUID (SF-061).
    /// </summary>
    Task<Resultado<ContratoResumenDto>> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Modifica administrativamente el estado de un contrato comercial; si es cancelación, libera el lote a Disponible y cancela financiamientos (SF-063).
    /// </summary>
    Task<Resultado> CambiarEstadoContratoAdminAsync(
        Guid ventaId,
        EstadoVenta nuevoEstado,
        string motivo,
        CancellationToken cancellationToken = default);
}
