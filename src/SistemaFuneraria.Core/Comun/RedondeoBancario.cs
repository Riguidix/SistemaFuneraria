namespace SistemaFuneraria.Core.Comun;

/// <summary>
/// Proporciona utilidades centralizadas de redondeo financiero basadas en la regla RNF-02
/// utilizando la estrategia bancaria (Banker's Rounding / MidpointRounding.ToEven).
/// </summary>
public static class RedondeoBancario
{
    private const int DecimalesPorDefecto = 2;

    /// <summary>
    /// Aplica redondeo bancario (al número par más cercano) a una cantidad decimal.
    /// </summary>
    /// <param name="valor">El valor decimal a redondear.</param>
    /// <param name="decimales">Número de cifras decimales requeridas (por defecto 2).</param>
    /// <returns>Valor redondeado según la regla bancaria ToEven.</returns>
    public static decimal Redondear(decimal valor) => RedondearConNDecimales(valor, DecimalesPorDefecto);
    
    /// <summary>
    /// Aplica redondeo bancario (al número par más cercano) a una cantidad decimal.
    /// </summary>
    /// <param name="valor">El valor decimal a redondear.</param>
    /// <param name="decimales">Número de cifras decimales requeridas (por defecto 2).</param>
    /// <returns>Valor redondeado según la regla bancaria ToEven.</returns>
    public static decimal RedondearConNDecimales(decimal valor, int decimales)
    {
        return Math.Round(valor, decimales, MidpointRounding.ToEven);
    }
}
