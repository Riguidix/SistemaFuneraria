using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Ventas;

namespace SistemaFuneraria.Application.Servicios.Ventas;

/// <summary>
/// Contrato del servicio de aplicación para la gestión del ciclo de vida de ventas funerarias (SF-060).
/// </summary>
public interface IVentaService
{
    /// <summary>
    /// Registra atómicamente una venta de contado, cambia el lote a Vendido y asienta el comprobante de pago único (CP-02).
    /// </summary>
    Task<Resultado<ContratoResumenDto>> RegistrarVentaContadoAsync(
        RegistrarVentaContadoCommand command,
        CancellationToken cancellationToken = default);
}
