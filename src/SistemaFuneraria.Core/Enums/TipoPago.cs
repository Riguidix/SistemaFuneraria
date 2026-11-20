namespace SistemaFuneraria.Core.Enums;

/// <summary>
/// Especifica el concepto comercial y contable por el que ingresa un pago en caja (SF-075).
/// </summary>
public enum TipoPago
{
    Enganche = 1,
    Contado = 2,
    Mensualidad = 3,
    AbonoCapital = 4,
    Liquidacion = 5
}
