using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Simulador;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Application.Servicios.Simulador;

/// <summary>
/// Implementación en memoria del servicio de simulación financiera de lotes funerarios (SF-013, SF-027, SF-048).
/// Valida parámetros contra el catálogo oficial y orquesta el cálculo de amortización francesa bajo CP-10.
/// </summary>
public class SimuladorService(TimeProvider? timeProvider = null) : ISimuladorService
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
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

    /// <inheritdoc />
    public Resultado<SimulacionCotizacionDto> Cotizar(SimularCotizacionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        decimal precioLista = request.Gavetas switch
        {
            2 => 120_000.00m,
            3 => 150_000.00m,
            4 => 180_000.00m,
            _ => 0m
        };

        if (precioLista == 0m)
        {
            return Resultado<SimulacionCotizacionDto>.Fallo("La capacidad de gavetas debe ser 2, 3 o 4.");
        }

        string modalidad = request.Modalidad?.Trim() ?? string.Empty;
        DateOnly fechaPrimerPago = request.FechaPrimerPago
            ?? DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime).AddMonths(1);

        if (string.Equals(modalidad, "Contado", StringComparison.OrdinalIgnoreCase))
        {
            return Resultado<SimulacionCotizacionDto>.Exito(new SimulacionCotizacionDto(
                PrecioLista: precioLista,
                Modalidad: "Contado",
                PorcentajeEnganche: 0m,
                MontoEnganche: precioLista,
                SaldoAFinanciar: 0m,
                PlazoMeses: 0,
                TasaAnualPorcentaje: 0m,
                MensualidadEstimada: 0m,
                FechaPrimerPago: fechaPrimerPago,
                TotalAPagar: precioLista,
                TablaAmortizacion: Array.Empty<FilaAmortizacionDto>()));
        }

        if (!string.Equals(modalidad, "Credito", StringComparison.OrdinalIgnoreCase))
        {
            return Resultado<SimulacionCotizacionDto>.Fallo("La modalidad debe ser 'Contado' o 'Credito'.");
        }

        var errores = new List<string>();
        if (request.PorcentajeEnganche != Math.Truncate(request.PorcentajeEnganche)
            || !ParametrosNegocio.EsEngancheAutorizado((int)request.PorcentajeEnganche))
        {
            errores.Add("Enganche no permitido. Solo se acepta 10%, 20% o 30% (RN-04).");
        }

        if (!ParametrosNegocio.EsPlazoAutorizado(request.PlazoMeses))
        {
            errores.Add("Plazo inválido. Solo se admiten plazos de 12, 18, 24 o 48 meses (RN-05).");
        }

        if (errores.Count > 0)
        {
            return Resultado<SimulacionCotizacionDto>.Fallo(errores);
        }

        int engancheEntero = (int)request.PorcentajeEnganche;
        var (montoEnganche, saldoAFinanciar) = CalculadoraFinanciamiento.CalcularEnganche(precioLista, engancheEntero);
        var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(request.PlazoMeses);
        decimal mensualidadEstimada = CalculadoraFinanciamiento.CalcularMensualidadOrdinaria(saldoAFinanciar, tasaMensual, request.PlazoMeses);
        var renglones = GeneradorAmortizacion.Generar(saldoAFinanciar, tasaMensual, request.PlazoMeses);

        var tabla = renglones
            .Select(r => new FilaAmortizacionDto(
                r.Periodo,
                r.SaldoInicial,
                r.CapitalAmortizado,
                r.Interes,
                r.PagoMensual,
                r.SaldoFinal,
                fechaPrimerPago.AddMonths(r.Periodo - 1)))
            .ToList();

        decimal totalAPagar = montoEnganche + tabla.Sum(f => f.CuotaTotal);

        return Resultado<SimulacionCotizacionDto>.Exito(new SimulacionCotizacionDto(
            PrecioLista: precioLista,
            Modalidad: "Credito",
            PorcentajeEnganche: request.PorcentajeEnganche,
            MontoEnganche: montoEnganche,
            SaldoAFinanciar: saldoAFinanciar,
            PlazoMeses: request.PlazoMeses,
            TasaAnualPorcentaje: tasaAnual * 100m,
            MensualidadEstimada: mensualidadEstimada,
            FechaPrimerPago: fechaPrimerPago,
            TotalAPagar: totalAPagar,
            TablaAmortizacion: tabla));
    }

    /// <inheritdoc />
    public PlanesFinanciamientoDto ObtenerPlanes()
    {
        var plazos = ParametrosNegocio.TasasAnualesPorPlazo
            .OrderBy(kv => kv.Key)
            .Select(kv => new OpcionPlazoDto(
                kv.Key,
                kv.Value * 100m,
                $"{kv.Key} meses · {kv.Value * 100m:0}% anual"))
            .ToList();

        return new PlanesFinanciamientoDto(
            ParametrosNegocio.EnganchesAutorizados,
            plazos);
    }
}
