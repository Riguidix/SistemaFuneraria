namespace SistemaFuneraria.Application.Servicios.Simulador;

/// <summary>
/// Representa un renglón del desglose de amortización periódica en la simulación.
/// </summary>
public record CuotaAmortizacionDto(
    int Periodo,
    decimal SaldoInicial,
    decimal Cuota,
    decimal Interes,
    decimal Amortizacion,
    decimal SaldoFinal
);

/// <summary>
/// DTO con el resultado integral de la simulación de financiamiento para su consumo en la capa Web.
/// </summary>
public record SimulacionFinanciamientoDto(
    decimal PrecioLote,
    int PorcentajeEnganche,
    decimal MontoEnganche,
    decimal CapitalFinanciado,
    int PlazoMeses,
    decimal TasaAnual,
    decimal TasaMensual,
    decimal MensualidadOrdinaria,
    decimal TotalIntereses,
    decimal TotalPagar,
    IReadOnlyList<CuotaAmortizacionDto> TablaAmortizacion
);
