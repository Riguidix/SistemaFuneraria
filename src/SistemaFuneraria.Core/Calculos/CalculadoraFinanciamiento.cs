namespace SistemaFuneraria.Core.Calculos;

/// <summary>
/// Motor de cálculos financieros de dominio puro para el Sistema Funerario.
/// Encapsula las reglas de amortización, cálculo de enganche y asignación de tasas de interés (RN-04 a RN-07).
/// </summary>
public static class CalculadoraFinanciamiento
{
    /// <summary>
    /// Obtiene la tasa de interés anual inmutable y calcula la tasa mensual correspondiente indexada al plazo,
    /// cumpliendo estrictamente con las reglas RN-05 y RN-06.
    /// </summary>
    /// <param name="plazoMeses">Plazo del financiamiento en meses (exclusivamente 12, 18, 24 o 48 meses).</param>
    /// <returns>Tupla nombrada con <c>TasaAnual</c> y <c>TasaMensual</c> en precisión decimal.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se lanza cuando el plazo no corresponde a uno de los 4 plazos autorizados.</exception>
    public static (decimal TasaAnual, decimal TasaMensual) ObtenerTasas(int plazoMeses)
    {
        decimal tasaAnual = plazoMeses switch
        {
            12 => 0.16m,
            18 => 0.18m,
            24 => 0.20m,
            48 => 0.24m,
            _ => throw new ArgumentOutOfRangeException(
                nameof(plazoMeses),
                plazoMeses,
                $"El plazo de {plazoMeses} meses no está autorizado. Los plazos permitidos son exclusivamente 12, 18, 24 y 48 meses (RN-05).")
        };

        decimal tasaMensual = tasaAnual / 12m;
        return (tasaAnual, tasaMensual);
    }

    /// <summary>
    /// Método de conveniencia para consultar directamente la tasa de interés mensual aplicable para un plazo autorizado.
    /// </summary>
    /// <param name="plazoMeses">Plazo del financiamiento en meses (12, 18, 24 o 48).</param>
    /// <returns>Tasa mensual aplicable (<c>tasaAnual / 12</c>) en precisión decimal de punto fijo sin truncamiento.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se lanza cuando el plazo no está autorizado.</exception>
    public static decimal ObtenerTasaMensual(int plazoMeses) => ObtenerTasas(plazoMeses).TasaMensual;
}
