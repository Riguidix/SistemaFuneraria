using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Lotes;

namespace SistemaFuneraria.Application.Servicios.Lotes;

public interface ILoteService
{
    Task<Resultado<ResumenCatalogoLotesDto>> ObtenerCatalogoAsync(
        string? buscar = null,
        string? seccion = null,
        string? estado = null,
        int? gavetas = null,
        int pagina = 1,
        int tamanoPagina = 20,
        CancellationToken ct = default);

    Task<Resultado<LoteItemDto>> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<Resultado<Guid>> CrearLoteAsync(CrearLoteRequest request, CancellationToken ct = default);
}
