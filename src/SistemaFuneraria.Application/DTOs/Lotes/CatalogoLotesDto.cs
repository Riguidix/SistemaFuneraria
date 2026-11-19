namespace SistemaFuneraria.Application.DTOs.Lotes;

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

public record ResumenCatalogoLotesDto(
    int TotalDisponibles,
    int TotalLotes,
    decimal PrecioDesde,
    string SeccionMasEspacio,
    int LotesLibresSeccionMasEspacio,
    IReadOnlyList<LoteItemDto> Items,
    int PaginaActual = 1,
    int TotalPaginas = 1
);

public record CrearLoteRequest(
    string Clave,
    string Seccion,
    string AndadorFila,
    int Gavetas,
    decimal PrecioContado
);
