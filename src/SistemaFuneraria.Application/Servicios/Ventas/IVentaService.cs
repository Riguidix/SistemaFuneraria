using SistemaFuneraria.Application.Comun;

namespace SistemaFuneraria.Application.Servicios.Ventas;

/// <summary>
/// Contrato de aplicación para el registro y orquestación de ventas de lotes funerarios (SF-024).
/// </summary>
public interface IVentaService
{
    /// <summary>
    /// Registra la compra de contado de un lote funerario en una sola exhibición (RF-03, RN-03).
    /// Emite folio único canónico VTA-2026-XXXXXX y liquida la transacción al 100% sin generar intereses ni cuotas.
    /// </summary>
    /// <param name="request">Datos requeridos de la venta de contado.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Resultado exitoso con el folio generado o fallo con los errores de negocio.</returns>
    Task<Resultado<string>> RegistrarVentaContadoAsync(RegistrarVentaContadoRequest request, CancellationToken cancellationToken = default);
}
