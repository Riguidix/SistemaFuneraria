namespace SistemaFuneraria.Application.DTOs.Simulador;

/// <summary>
/// Opción descriptiva de plazo y tasa de interés anual para planes de financiamiento (SF-049).
/// </summary>
public sealed record OpcionPlazoDto(
    int Meses,
    decimal TasaAnualPorcentaje,
    string Descripcion);

/// <summary>
/// Catálogo oficial de opciones de enganche y plazos autorizados para cotizaciones (SF-049, Captura 7 Figma).
/// </summary>
public sealed record PlanesFinanciamientoDto(
    IReadOnlyList<int> PorcentajesEnganche,
    IReadOnlyList<OpcionPlazoDto> Plazos);
