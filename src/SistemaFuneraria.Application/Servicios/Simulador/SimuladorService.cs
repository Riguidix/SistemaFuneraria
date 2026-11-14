using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Application.Servicios.Simulador;

/// <summary>
/// Implementación en memoria del servicio de simulación financiera de lotes funerarios (SF-013, SF-027).
/// Valida parámetros contra el catálogo oficial y orquesta el cálculo de amortización francesa bajo CP-10.
/// </summary>
public class SimuladorService : ISimuladorService
{
    /// <inheritdoc />
    public Resultado<SimulacionFinanciamientoDto> Simular(SimularFinanciamientoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Validaciones de catálogo y reglas de negocio (CP-10, RN-01, RN-04, RN-05)
        var errores = new List<string>();

        if (!ParametrosNegocio.EsPrecioAutorizado(request.PrecioLote))
        {
            errores.Add("Precio no permitido. Solo se autorizan lotes de $120,000.00, $150,000.00 y $180,000.00 (RN-01).");
        }

        if (!ParametrosNegocio.EsEngancheAutorizado(request.PorcentajeEnganche))
        {
            errores.Add("Enganche no permitido. Solo se acepta 10%, 20% o 30% (RN-04).");
        }

        if (!ParametrosNegocio.EsPlazoAutorizado(request.PlazoMeses))
        {
            errores.Add("Plazo inválido. Solo se admiten plazos de 12, 18, 24 o 48 meses (RN-05).");
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
