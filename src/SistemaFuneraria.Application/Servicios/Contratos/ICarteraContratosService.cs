using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Contratos;

namespace SistemaFuneraria.Application.Servicios.Contratos;

/// <summary>
/// Servicio de consultas de cartera de contratos, KPIs y detalle por folio o UUID (SF-068 / SF-069).
/// </summary>
public interface ICarteraContratosService
{
    /// <summary>
    /// Obtiene los 4 KPIs agregados y la lista filtrada/paginada de contratos por búsqueda y pestaña.
    /// </summary>
    Task<Resultado<CarteraContratosResponseDto>> ObtenerCarteraAsync(
        string? buscar = null,
        string? tab = "Todos",
        int pagina = 1,
        int tamanoPagina = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Localiza el expediente completo de un contrato por su UUID o folio comercial (CTR-YYYY-XXXX).
    /// </summary>
    Task<Resultado<ContratoDetalleDto>> ObtenerDetalleAsync(
        string folioOrId,
        CancellationToken cancellationToken = default);
}
