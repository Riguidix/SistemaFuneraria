namespace SistemaFuneraria.Application.DTOs.Simulador;

/// <summary>
/// Solicitud de cotización comercial para simulación en modalidad Contado o Credito (SF-048).
/// </summary>
public sealed record SimularCotizacionRequest(
    Guid? LoteId,
    int? Gavetas,
    string Modalidad,
    decimal PorcentajeEnganche,
    int PlazoMeses,
    DateOnly? FechaPrimerPago);

/// <summary>
/// Resultado completo de cotización con resumen financiero y tabla de amortización francesa (SF-048).
/// </summary>
public sealed record SimulacionCotizacionDto(
    decimal PrecioLista,
    string Modalidad,
    decimal PorcentajeEnganche,
    decimal MontoEnganche,
    decimal SaldoAFinanciar,
    int PlazoMeses,
    decimal TasaAnualPorcentaje,
    decimal MensualidadEstimada,
    DateOnly FechaPrimerPago,
    decimal TotalAPagar,
    IReadOnlyList<FilaAmortizacionDto> TablaAmortizacion);

/// <summary>
/// Renglón individual proyectado en la tabla de amortización de una cotización (SF-048).
/// </summary>
public sealed record FilaAmortizacionDto(
    int NumeroCuota,
    decimal SaldoInicial,
    decimal Capital,
    decimal Interes,
    decimal CuotaTotal,
    decimal SaldoFinal,
    DateOnly FechaVencimiento);
