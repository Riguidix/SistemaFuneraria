using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Core.Calculos;

/// <summary>
/// Motor de generación de tablas de amortización bajo el sistema francés sobre saldos insolutos (RF-08, CP-06, RNF-02).
/// </summary>
public static class GeneradorAmortizacion
{
    /// <summary>
    /// Genera la tabla completa de amortización para un capital financiado, tasa mensual y plazo autorizado.
    /// </summary>
    /// <param name="capital">Capital neto financiado en MXN (> 0).</param>
    /// <param name="tasaMensual">Tasa de interés mensual decimal (> 0).</param>
    /// <param name="plazo">Plazo del financiamiento en meses (12, 18, 24 o 48).</param>
    /// <returns>Lista inmutable con cada uno de los periodos de amortización.</returns>
    public static IReadOnlyList<RenglonAmortizacion> Generar(decimal capital, decimal tasaMensual, int plazo)
    {
        if (capital <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capital),
                capital,
                "El capital financiado debe ser estrictamente mayor a cero (RN-04, RNF-02).");
        }

        if (tasaMensual <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tasaMensual),
                tasaMensual,
                "La tasa de interés mensual debe ser mayor a cero.");
        }

        if (!ParametrosNegocio.EsPlazoAutorizado(plazo))
        {
            throw new ArgumentOutOfRangeException(
                nameof(plazo),
                plazo,
                $"El plazo de {plazo} meses no está autorizado (RN-05). Los plazos permitidos son exclusivamente 12, 18, 24 y 48 meses.");
        }

        var cuotaFija = CalculadoraFinanciamiento.CalcularMensualidadOrdinaria(capital, tasaMensual, plazo);
        var renglones = new List<RenglonAmortizacion>(plazo);
        var saldoActual = capital;

        for (int periodo = 1; periodo <= plazo; periodo++)
        {
            var saldoInicial = saldoActual;
            var interes = RedondeoBancario.Redondear(saldoInicial * tasaMensual);

            decimal capitalAmortizado;
            decimal pagoMensual;
            decimal saldoFinal;

            if (periodo == plazo)
            {
                capitalAmortizado = saldoInicial;
                pagoMensual = capitalAmortizado + interes;
                saldoFinal = 0.00m;
            }
            else
            {
                capitalAmortizado = cuotaFija - interes;
                pagoMensual = cuotaFija;
                saldoFinal = saldoInicial - capitalAmortizado;
            }

            saldoActual = saldoFinal;

            renglones.Add(new RenglonAmortizacion(
                periodo,
                saldoInicial,
                interes,
                capitalAmortizado,
                pagoMensual,
                saldoFinal));
        }

        return renglones.AsReadOnly();
    }

    /// <summary>
    /// Sobrecarga de conveniencia que resuelve automáticamente la tasa mensual oficial indexada al plazo.
    /// </summary>
    /// <param name="capital">Capital neto financiado en MXN (> 0).</param>
    /// <param name="plazoMeses">Plazo autorizado en meses (12, 18, 24 o 48).</param>
    /// <returns>Lista inmutable con la tabla de amortización generada.</returns>
    public static IReadOnlyList<RenglonAmortizacion> Generar(decimal capital, int plazoMeses)
    {
        var tasaMensual = CalculadoraFinanciamiento.ObtenerTasaMensual(plazoMeses);
        return Generar(capital, tasaMensual, plazoMeses);
    }
}
