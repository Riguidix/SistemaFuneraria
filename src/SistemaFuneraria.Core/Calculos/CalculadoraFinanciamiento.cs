using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Core.Calculos;

/// <summary>
/// Motor de cálculos financieros de dominio puro para el Sistema Funerario.
/// Encapsula las reglas de amortización, cálculo de enganche y asignación de tasas de interés (RN-04 a RN-07).
/// </summary>
public static class CalculadoraFinanciamiento
{
    /// <summary>
    /// Obtiene la tasa de interés anual inmutable y calcula la tasa mensual correspondiente indexada al plazo,
    /// cumpliendo estrictamente con las reglas RN-05 y RN-06 mediante el catálogo centralizado inmutable.
    /// </summary>
    /// <param name="plazoMeses">Plazo del financiamiento en meses (exclusivamente 12, 18, 24 o 48 meses).</param>
    /// <returns>Tupla nombrada con <c>TasaAnual</c> y <c>TasaMensual</c> en precisión decimal.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se lanza cuando el plazo no corresponde a uno de los plazos autorizados.</exception>
    public static (decimal TasaAnual, decimal TasaMensual) ObtenerTasas(int plazoMeses)
    {
        decimal tasaAnual = ParametrosNegocio.ObtenerTasaAnual(plazoMeses);
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

    /// <summary>
    /// Calcula el monto del enganche y el saldo insoluto de capital a financiar (RF-04),
    /// validando rigurosamente que el precio del lote corresponda al catálogo oficial (RN-01)
    /// y que el porcentaje de enganche sea uno de los autorizados (10%, 20%, 30%) (RN-04).
    /// </summary>
    /// <param name="precioLote">Precio de lista oficial del lote en MXN ($120k, $150k, $180k).</param>
    /// <param name="porcentajeEnganche">Porcentaje de enganche autorizado (10, 20 o 30).</param>
    /// <returns>Tupla nombrada con el monto de <c>Enganche</c> y el <c>CapitalFinanciado</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se lanza cuando el precio del lote o el porcentaje de enganche no están autorizados.</exception>
    public static (decimal Enganche, decimal CapitalFinanciado) CalcularEnganche(decimal precioLote, int porcentajeEnganche)
    {
        // 1. Validación de precio de lote autorizado (RN-01)
        if (!ParametrosNegocio.EsPrecioAutorizado(precioLote))
        {
            throw new ArgumentOutOfRangeException(
                nameof(precioLote),
                precioLote,
                $"El precio de {precioLote:C2} no está autorizado en el catálogo oficial (RN-01). Los precios permitidos son exclusivamente $120,000.00, $150,000.00 y $180,000.00.");
        }

        // 2. Validación de porcentaje de enganche autorizado (RN-04)
        if (!ParametrosNegocio.EsEngancheAutorizado(porcentajeEnganche))
        {
            throw new ArgumentOutOfRangeException(
                nameof(porcentajeEnganche),
                porcentajeEnganche,
                $"El porcentaje de enganche de {porcentajeEnganche}% no está autorizado (RN-04). Los permitidos son exclusivamente 10%, 20% y 30%.");
        }

        // 3. Cálculo aritmético O(1) con precisión decimal y redondeo bancario (RNF-02)
        decimal factorPorcentaje = porcentajeEnganche / 100m;
        decimal enganche = Math.Round(precioLote * factorPorcentaje, 2, MidpointRounding.ToEven);
        decimal capitalFinanciado = precioLote - enganche;

        return (enganche, capitalFinanciado);
    }
}
