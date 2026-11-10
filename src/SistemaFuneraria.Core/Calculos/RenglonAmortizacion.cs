namespace SistemaFuneraria.Core.Calculos;

/// <summary>
/// Representa un renglón o periodo individual dentro de la tabla de amortización francesa (RF-08, CP-06).
/// Inmutable y modelado con precisión decimal de punto fijo conforme a la regla de integridad financiera RNF-02.
/// </summary>
/// <param name="Periodo">Número secuencial de la cuota (1 a n).</param>
/// <param name="SaldoInicial">Saldo de capital insoluto al inicio del periodo.</param>
/// <param name="Interes">Interés ordinario devengado en el periodo sobre saldo insoluto.</param>
/// <param name="CapitalAmortizado">Porción de capital neto abonada a la deuda en el periodo.</param>
/// <param name="PagoMensual">Monto total a pagar en el periodo (Capital + Interés).</param>
/// <param name="SaldoFinal">Saldo de capital insoluto remanente tras aplicar el pago.</param>
public sealed record RenglonAmortizacion(
    int Periodo,
    decimal SaldoInicial,
    decimal Interes,
    decimal CapitalAmortizado,
    decimal PagoMensual,
    decimal SaldoFinal);
