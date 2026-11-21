using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;

namespace SistemaFuneraria.Core.Calculos;

/// <summary>
/// Evaluador de vencimientos para créditos y mensualidades utilizando abstracción temporal nativa TimeProvider (RF-13, RF-14, RN-10, CP-09).
/// Aplica estrictamente una política de $0.00 de interés moratorio o penalización por mora.
/// </summary>
public static class EvaluadorVencimientos
{
    /// <summary>
    /// Evalúa las cuotas pendientes de un financiamiento contra la fecha actual del TimeProvider, transicionando a Vencida y EnMora sin recargos.
    /// </summary>
    /// <param name="financiamiento">Entidad de financiamiento a evaluar.</param>
    /// <param name="timeProvider">Proveedor de tiempo opcional. Si es nulo se utiliza TimeProvider.System.</param>
    /// <returns>Número de mensualidades que transicionaron de Pendiente a Vencida en esta evaluación.</returns>
    public static int Evaluar(Financiamiento financiamiento, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(financiamiento);

        if (financiamiento.Estado == EstadoFinanciamiento.Liquidado)
            return 0;

        var provider = timeProvider ?? TimeProvider.System;
        var fechaActual = DateOnly.FromDateTime(provider.GetUtcNow().DateTime);

        int cuotasTransicionadas = 0;

        foreach (var mensualidad in financiamiento.Mensualidades)
        {
            if (mensualidad.Estado == EstadoMensualidad.Pendiente && mensualidad.FechaVencimiento < fechaActual)
            {
                mensualidad.MarcarVencida();
                cuotasTransicionadas++;
            }
        }

        bool tieneCuotasVencidas = financiamiento.Mensualidades.Any(m => m.Estado == EstadoMensualidad.Vencida);
        if (tieneCuotasVencidas && financiamiento.Estado == EstadoFinanciamiento.Activo)
        {
            financiamiento.MarcarEnMora();
        }

        return cuotasTransicionadas;
    }
}
