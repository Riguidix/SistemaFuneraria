namespace SistemaFuneraria.Core.Calculos;

/// <summary>
/// Representa el resultado inmutable del recálculo de amortización tras aplicar un abono extraordinario a capital.
/// </summary>
/// <param name="SaldoAnterior">Saldo insoluto previo a la aplicación del abono.</param>
/// <param name="MontoAbono">Monto del abono descontado 100% al capital.</param>
/// <param name="NuevoSaldoInsoluto">Nuevo saldo insoluto de capital tras el abono.</param>
/// <param name="NuevaCuota">Nueva mensualidad fija recalculada para las cuotas restantes.</param>
/// <param name="CuotasRestantes">Número de mensualidades pendientes de amortizar.</param>
/// <param name="TablaRecalculada">Calendario detallado de amortización recalculado con ajuste al centavo.</param>
public sealed record ResultadoAbonoCapital(
    decimal SaldoAnterior,
    decimal MontoAbono,
    decimal NuevoSaldoInsoluto,
    decimal NuevaCuota,
    int CuotasRestantes,
    IReadOnlyList<RenglonAmortizacion> TablaRecalculada);
