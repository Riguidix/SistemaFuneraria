using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Lotes;
using SistemaFuneraria.Core.Enums;

namespace SistemaFuneraria.Application.Servicios.Lotes;

/// <summary>
/// Contrato de aplicación para el catálogo paginado, KPIs y operaciones CRUD de lotes (SF-041).
/// </summary>
public interface ILoteService
{
    /// <summary>
    /// Obtiene el catálogo paginado de lotes con filtros opcionales y los 3 KPIs consolidados.
    /// </summary>
    Task<Resultado<ResumenCatalogoLotesDto>> ObtenerCatalogoAsync(
        string? buscar = null,
        string? seccion = null,
        string? estado = null,
        int? gavetas = null,
        int pagina = 1,
        int tamanoPagina = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta los detalles de un lote por su ID único.
    /// </summary>
    Task<Resultado<LoteItemDto>> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta los detalles de un lote por su clave comercial (ej. JN-A-042).
    /// </summary>
    Task<Resultado<LoteItemDto>> ObtenerPorClaveAsync(
        string clave,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un nuevo lote en el catálogo validando unicidad de clave (exclusivo Administrador).
    /// </summary>
    Task<Resultado<Guid>> CrearLoteAsync(
        CrearLoteCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza los datos administrativos y estado de un lote (exclusivo Administrador).
    /// </summary>
    Task<Resultado> ActualizarLoteAsync(
        Guid id,
        ActualizarLoteCommand command,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia el estado operativo de un lote validando transiciones permitidas (exclusivo Administrador).
    /// </summary>
    Task<Resultado> CambiarEstadoLoteAsync(
        Guid id,
        EstadoLote nuevoEstado,
        CancellationToken cancellationToken = default);
}
