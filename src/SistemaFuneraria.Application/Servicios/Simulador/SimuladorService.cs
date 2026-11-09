using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Application.Servicios.Simulador;

/// <summary>
/// Implementación en memoria del servicio de simulación financiera de lotes funerarios (SF-013).
/// Valida parámetros contra el catálogo oficial y orquesta el cálculo de amortización francesa.
/// </summary>
public class SimuladorService : ISimuladorService
{
    /// <inheritdoc />
    public Resultado<SimulacionFinanciamientoDto> Simular(SimularFinanciamientoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Validaciones de catálogo y reglas de negocio
        var errores = new List<string>();

        if (!ParametrosNegocio.EsPrecioAutorizado(request.PrecioLote))
        {
            errores.Add($"El precio de {request.PrecioLote:C2} no está autorizado (RN-01).");
        }

        if (!ParametrosNegocio.EsEngancheAutorizado(request.PorcentajeEnganche))
        {
            errores.Add($"El porcentaje de enganche de {request.PorcentajeEnganche}% no está autorizado (RN-04).");
        }

        if (!ParametrosNegocio.EsPlazoAutorizado(request.PlazoMeses))
        {
            errores.Add($"El plazo de {request.PlazoMeses} meses no está autorizado (RN-05).");
        }

        if (errores.Count > 0)
        {
            return Resultado<SimulacionFinanciamientoDto>.Fallo(errores);
        }

        // 2. Cálculos financieros de enganche y tasas
        var (montoEnganche, capitalFinanciado) = CalculadoraFinanciamiento.CalcularEnganche(request.PrecioLote, request.PorcentajeEnganche);
        var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(request.PlazoMeses);

        // 3. Fórmula de amortización francesa para mensualidad ordinaria
        double iDouble = (double)tasaMensual;
        double factor = Math.Pow(1.0 + iDouble, request.PlazoMeses);
        decimal mensualidadOrdinaria = Math.Round((decimal)((double)capitalFinanciado * (iDouble * factor) / (factor - 1.0)), 2, MidpointRounding.ToEven);

        // 4. Generación de tabla de amortización con absorción al centavo en cuota final
        var tabla = new List<CuotaAmortizacionDto>(request.PlazoMeses);
        decimal saldo = capitalFinanciado;
        decimal totalIntereses = 0m;
        decimal totalPagosCuotas = 0m;

        for (int k = 1; k <= request.PlazoMeses; k++)
        {
            decimal interes = Math.Round(saldo * tasaMensual, 2, MidpointRounding.ToEven);
            decimal cuota = mensualidadOrdinaria;
            decimal amortizacion;
            decimal saldoFinal;

            if (k == request.PlazoMeses)
            {
                // La última cuota absorbe el saldo residual para garantizar saldo final de $0.00
                amortizacion = saldo;
                cuota = amortizacion + interes;
                saldoFinal = 0.00m;
            }
            else
            {
                amortizacion = cuota - interes;
                saldoFinal = saldo - amortizacion;
            }

            totalIntereses += interes;
            totalPagosCuotas += cuota;

            tabla.Add(new CuotaAmortizacionDto(k, saldo, cuota, interes, amortizacion, saldoFinal));
            saldo = saldoFinal;
        }

        decimal totalPagar = montoEnganche + totalPagosCuotas;

        var dto = new SimulacionFinanciamientoDto(
            PrecioLote: request.PrecioLote,
            PorcentajeEnganche: request.PorcentajeEnganche,
            MontoEnganche: montoEnganche,
            CapitalFinanciado: capitalFinanciado,
            PlazoMeses: request.PlazoMeses,
            TasaAnual: tasaAnual,
            TasaMensual: tasaMensual,
            MensualidadOrdinaria: mensualidadOrdinaria,
            TotalIntereses: totalIntereses,
            TotalPagar: totalPagar,
            TablaAmortizacion: tabla
        );

        return Resultado<SimulacionFinanciamientoDto>.Exito(dto);
    }
}
