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

        // 2. Orquestación del motor financiero de dominio puro (Core)
        var (montoEnganche, capitalFinanciado) = CalculadoraFinanciamiento.CalcularEnganche(request.PrecioLote, request.PorcentajeEnganche);
        var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(request.PlazoMeses);
        decimal mensualidadOrdinaria = CalculadoraFinanciamiento.CalcularMensualidadOrdinaria(capitalFinanciado, tasaMensual, request.PlazoMeses);
        var renglones = GeneradorAmortizacion.Generar(capitalFinanciado, tasaMensual, request.PlazoMeses);

        // 3. Mapeo de entidades de dominio a DTOs para la capa Web
        var tabla = renglones
            .Select(r => new CuotaAmortizacionDto(
                r.Periodo,
                r.SaldoInicial,
                r.PagoMensual,
                r.Interes,
                r.CapitalAmortizado,
                r.SaldoFinal))
            .ToList();

        decimal totalIntereses = renglones.Sum(r => r.Interes);
        decimal totalPagar = montoEnganche + renglones.Sum(r => r.PagoMensual);

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
