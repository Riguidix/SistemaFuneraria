using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Cobranza;

namespace SistemaFuneraria.Application.Servicios.Cobranza;

/// <summary>
/// Servicio de aplicación para la operación de Caja, pagos ordinarios, corte del día y recibos oficiales (SF-077).
/// </summary>
public interface ICobranzaService
{
    /// <summary>
    /// Obtiene el corte del día en vivo (saldo inicial, pagos hoy, ingresos, total en caja y alertas vencidas).
    /// </summary>
    Task<Resultado<CorteDelDiaDto>> ObtenerResumenCajaAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Localiza un contrato financiado en Caja por folio CTR-YYYY-XXXX o UUID con sus montos de cobro.
    /// </summary>
    Task<Resultado<ContratoCajaDto>> ObtenerContratoParaCajaAsync(
        string folioOrId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica atómicamente el cobro de la siguiente mensualidad exigible con idempotencia y control de concurrencia.
    /// </summary>
    Task<Resultado<ReciboPagoDto>> AplicarPagoMensualidadAsync(
        AplicarPagoMensualidadCommand command,
        CancellationToken cancellationToken = default);

    Task<Resultado<ReciboPagoDto>> AplicarAbonoCapitalAsync(
        AplicarAbonoCapitalCommand command,
        CancellationToken cancellationToken = default);

    Task<Resultado<ReciboPagoDto>> AplicarLiquidacionAsync(
        AplicarLiquidacionCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta el detalle completo de un recibo oficial PAG-YYYY-XXXXXX por su folio.
    /// </summary>
    Task<Resultado<ReciboPagoDto>> ObtenerReciboPorFolioAsync(
        string folioPago,
        CancellationToken cancellationToken = default);
}
