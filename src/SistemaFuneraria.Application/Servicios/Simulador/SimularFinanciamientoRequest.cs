namespace SistemaFuneraria.Application.Servicios.Simulador;

/// <summary>
/// Parámetros de entrada para la simulación de financiamiento de un lote funerario.
/// </summary>
/// <param name="PrecioLote">Precio de lista del lote funerario (RN-01: $120,000, $150,000, $180,000).</param>
/// <param name="PorcentajeEnganche">Porcentaje de enganche solicitado (RN-04: 10, 20 o 30%).</param>
/// <param name="PlazoMeses">Plazo del financiamiento en meses (RN-05: 12, 18, 24 o 48 meses).</param>
public record SimularFinanciamientoRequest(
    decimal PrecioLote,
    int PorcentajeEnganche,
    int PlazoMeses
);
