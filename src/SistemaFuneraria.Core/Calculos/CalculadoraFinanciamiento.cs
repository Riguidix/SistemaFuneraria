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

    /// <summary>
    /// Calcula la cuota ordinaria fija bajo el sistema de amortización francesa sobre saldos insolutos (CP-05, RF-07, RN-07).
    /// M = P * [ i * (1 + i)^n ] / [ (1 + i)^n - 1 ]
    /// </summary>
    /// <param name="capital">Capital neto financiado en MXN (> 0) (P).</param>
    /// <param name="tasaMensual">Tasa de interés mensual decimal (> 0) (i).</param>
    /// <param name="plazo">Plazo del financiamiento en meses (12, 18, 24 o 48) (n).</param>
    /// <returns>Monto de la mensualidad fija ordinaria redondeada con redondeo bancario.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se lanza cuando el capital, la tasa mensual o el plazo no son válidos.</exception>
    public static decimal CalcularMensualidadOrdinaria(decimal capital, decimal tasaMensual, int plazo)
    {
        // 1. Validación de capital neto financiado (RN-04, RNF-02)
        if (capital <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capital),
                capital,
                $"El capital financiado debe ser estrictamente mayor a cero (RN-04, RNF-02). Valor recibido: {capital:C2}.");
        }

        // 2. Validación de plazo autorizado (RN-05)
        if (!ParametrosNegocio.EsPlazoAutorizado(plazo))
        {
            throw new ArgumentOutOfRangeException(
                nameof(plazo),
                plazo,
                $"El plazo de {plazo} meses no está autorizado (RN-05). Los plazos permitidos son exclusivamente 12, 18, 24 y 48 meses.");
        }

        // 3. Validación de tasa de interés mensual
        if (tasaMensual <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tasaMensual),
                tasaMensual,
                $"La tasa de interés mensual debe ser mayor a cero. Valor recibido: {tasaMensual}.");
        }

        // 4. Cálculo de (1 + i)^n con precisión decimal de 128 bits sin float/double (RNF-02)
        decimal factor = 1m + tasaMensual;
        decimal factorPotencia = 1m;
        for (int k = 0; k < plazo; k++)
        {
            factorPotencia *= factor;
        }

        // 5. Aplicación de la fórmula francesa: M = P * [ i * (1 + i)^n ] / [ (1 + i)^n - 1 ]
        decimal cuota = capital * (tasaMensual * factorPotencia) / (factorPotencia - 1m);

        // 6. Redondeo bancario a 2 decimales (RNF-02)
        return RedondeoBancario.Redondear(cuota);
    }

    /// <summary>
    /// Sobrecarga de conveniencia que calcula la cuota ordinaria fija a partir del capital y plazo,
    /// obteniendo la tasa mensual oficial indexada del catálogo de negocio (RN-05, RN-06).
    /// </summary>
    /// <param name="capital">Capital neto financiado en MXN (> 0).</param>
    /// <param name="plazoMeses">Plazo autorizado en meses (12, 18, 24 o 48).</param>
    /// <returns>Monto de la mensualidad fija ordinaria.</returns>
    public static decimal CalcularCuota(decimal capital, int plazoMeses)
    {
        decimal tasaMensual = ObtenerTasaMensual(plazoMeses);
        return CalcularMensualidadOrdinaria(capital, tasaMensual, plazoMeses);
    }
}
