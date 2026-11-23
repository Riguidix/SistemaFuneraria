using SistemaFuneraria.Core.Enums;

namespace SistemaFuneraria.Application.DTOs.Cobranza;

/// <summary>
/// Resumen operativo del corte del día y alertas de morosidad para /Caja (SF-077 / SF-081).
/// </summary>
public sealed record CorteDelDiaDto(
    DateOnly Fecha,
    decimal SaldoInicial,
    int PagosRecibidosHoy,
    decimal IngresosDelDia,
    decimal TotalEnCaja,
    int PagosVencidosCount
);

/// <summary>
/// Datos del contrato localizado en ventanilla de Caja para las 3 tarjetas de operación (SF-078 / SF-080).
/// </summary>
public sealed record ContratoCajaDto(
    Guid VentaId,
    Guid FinanciamientoId,
    string FolioContrato,
    string ClienteNombre,
    string InicialesCliente,
    string LoteClave,
    string LoteUbicacion,
    string EstadoVisual,
    int CuotasVencidas,
    int? ProximaCuotaNumero,
    DateOnly? ProximaCuotaVencimiento,
    decimal MontoMensualidad,
    decimal SaldoInsoluto,
    decimal MontoLiquidacionEstimado
);

/// <summary>
/// Payload HTTP para aplicar el cobro ordinario de la próxima mensualidad exigible (SF-078).
/// </summary>
public sealed record AplicarPagoMensualidadRequest(
    string FolioContrato,
    decimal ImporteRecibido,
    MetodoPago MetodoPago,
    string? Referencia = null,
    string? ClaveIdempotencia = null,
    int? NumeroCuota = null
);

/// <summary>
/// Comando de aplicación para el cobro ordinario con idempotencia y control optimista de concurrencia (SF-077).
/// </summary>
public sealed record AplicarPagoMensualidadCommand(
    string FolioContrato,
    decimal ImporteRecibido,
    MetodoPago MetodoPago,
    string? Referencia,
    string ClaveIdempotencia,
    string CajeroId,
    int? NumeroCuota = null
);

/// <summary>
/// Recibo oficial foliado PAG-YYYY-XXXXXX con desglose de capital, interés y nuevo saldo (SF-077 / SF-082).
/// </summary>
public sealed record ReciboPagoDto(
    Guid PagoId,
    string FolioPago,
    DateTimeOffset FechaPago,
    string FolioContrato,
    string ClienteNombre,
    string LoteClave,
    string Concepto,
    string TipoPago,
    string MetodoPago,
    string? Referencia,
    decimal SaldoAnterior,
    decimal MontoCapital,
    decimal MontoInteres,
    decimal MontoTotal,
    decimal SaldoPosterior,
    string CajeroId
);
