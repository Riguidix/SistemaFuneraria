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
    Guid? ContratoActivoId = null,
    string? FolioContratoActivo = null
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
    int TotalPaginas,
    int TotalFiltrados = 0
);

/// <summary>
/// Vista previa y resultado de ubicación física autogenerada para un nuevo lote por sección.
/// </summary>
public record SiguienteUbicacionLoteDto(
    string Seccion,
    string Prefijo,
    string Andador,
    int Fila,
    int Consecutivo,
    string Clave,
    string AndadorFila
);

/// <summary>
/// Comando para el registro administrativo de un nuevo lote (SF-041).
/// Si <see cref="Clave"/> o <see cref="AndadorFila"/> se omiten, el backend genera automáticamente
/// la siguiente ubicación secuencial disponible para la sección.
/// </summary>
public record CrearLoteCommand(
    string? Clave,
    string Seccion,
    string? AndadorFila,
    int Gavetas,
    decimal PrecioContado = 0m
);

/// <summary>
/// Comando para la edición administrativa de capacidad, precio oficial RN-01 y estado de un lote (SF-041).
/// La ubicación física del lote permanece inmutable.
/// </summary>
public record ActualizarLoteCommand(
    string? Seccion,
    string? AndadorFila,
    int Gavetas,
    decimal PrecioContado,
    EstadoLote Estado
);

/// <summary>
/// Petición para la actualización del estado operativo de un lote (SF-042).
/// </summary>
public record CambiarEstadoLoteRequest(
    EstadoLote Estado
);
