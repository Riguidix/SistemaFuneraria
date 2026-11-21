using SistemaFuneraria.Core.Enums;

namespace SistemaFuneraria.Application.DTOs.Lotes;

/// <summary>
/// DTO plano que representa un lote dentro del catálogo paginado (SF-041).
/// </summary>
public record LoteItemDto(
    Guid Id,
    string Clave,
    string Seccion,
    string AndadorFila,
    int Gavetas,
    decimal PrecioContado,
    string Estado,
    Guid? ContratoActivoId = null
);

/// <summary>
/// DTO con el resumen de KPIs y la página de lotes del catálogo (SF-041, Captura 2 Figma).
/// </summary>
public record ResumenCatalogoLotesDto(
    int TotalDisponibles,
    int TotalLotes,
    decimal PrecioDesde,
    string SeccionMasEspacio,
    int LotesLibresSeccionMasEspacio,
    IReadOnlyList<LoteItemDto> Items,
    int PaginaActual,
    int TotalPaginas
);

/// <summary>
/// Comando para el registro administrativo de un nuevo lote (SF-041).
/// </summary>
public record CrearLoteCommand(
    string Clave,
    string Seccion,
    string AndadorFila,
    int Gavetas,
    decimal PrecioContado
);

/// <summary>
/// Comando para la edición administrativa de ubicación, capacidad, precio o estado de un lote (SF-041).
/// </summary>
public record ActualizarLoteCommand(
    string Seccion,
    string AndadorFila,
    int Gavetas,
    decimal PrecioContado,
    EstadoLote Estado
);
