using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Core.Calculos;

/// <summary>
/// Representa el desglose financiero inmutable para la liquidación anticipada de un crédito (RF-12, RN-09, CP-08).
/// </summary>
/// <param name="CapitalPendiente">Monto del saldo insoluto de capital pendiente.</param>
/// <param name="InteresesFuturosProgramados">Suma de intereses ordinarios futuros programados no devengados.</param>
/// <param name="PorcentajeDescuentoIntereses">Porcentaje de descuento comercial aplicado sobre los intereses futuros (ej. 0.10 para 10%).</param>
/// <param name="DescuentoIntereses">Monto bonificado al cliente correspondiente al porcentaje de descuento sobre intereses.</param>
/// <param name="InteresesNetosAPagar">Intereses efectivamente exigibles a pagar tras aplicar la bonificación comercial.</param>
/// <param name="Penalizacion">Cargo por penalización por terminación o pago anticipado (siempre $0.00 según RN-09).</param>
/// <param name="MontoTotalLiquidacion">Monto total exigible para saldar y finiquitar el contrato al 100%.</param>
public sealed record ResultadoLiquidacion(
    Dinero CapitalPendiente,
    Dinero InteresesFuturosProgramados,
    decimal PorcentajeDescuentoIntereses,
    Dinero DescuentoIntereses,
    Dinero InteresesNetosAPagar,
    Dinero Penalizacion,
    Dinero MontoTotalLiquidacion);
