using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.DTOs.Lotes;
using SistemaFuneraria.Application.Servicios.Lotes;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador REST para el catálogo y administración de inventario de lotes (SF-042).
/// Expone consultas autorizadas y operaciones de mutación restringidas al rol Administrador.
/// </summary>
[ApiController]
[Route("api/v1/lotes")]
[Authorize]
public class LotesController(ILoteService loteService) : ApiControllerBase
{
    private readonly ILoteService _loteService = loteService;

    /// <summary>
    /// Consulta el catálogo paginado de lotes con filtros opcionales de búsqueda y KPIs consolidados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResumenCatalogoLotesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCatalogo(
        [FromQuery] string? buscar = null,
        [FromQuery] string? seccion = null,
        [FromQuery] string? estado = null,
        [FromQuery] int? gavetas = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var resultado = await _loteService.ObtenerCatalogoAsync(buscar, seccion, estado, gavetas, page, pageSize, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Consulta los datos detallados de un lote a partir de su identificador UUID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoteItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct = default)
    {
        var resultado = await _loteService.ObtenerPorIdAsync(id, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Consulta los datos detallados de un lote a partir de su clave comercial (ej. RO-B-027).
    /// </summary>
    [HttpGet("clave/{clave}")]
    [ProducesResponseType(typeof(LoteItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorClave(string clave, CancellationToken ct = default)
    {
        var resultado = await _loteService.ObtenerPorClaveAsync(clave, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Registra un nuevo lote en el catálogo. Exclusivo para el rol Administrador.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearLoteCommand command, CancellationToken ct = default)
    {
        var resultado = await _loteService.CrearLoteAsync(command, ct);
        if (resultado.EsFallo)
        {
            return FromResult(resultado);
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor }, new { id = resultado.Valor });
    }

    /// <summary>
    /// Actualiza ubicación, capacidad, precio o estado de un lote. Exclusivo para el rol Administrador.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarLoteCommand command, CancellationToken ct = default)
    {
        var resultado = await _loteService.ActualizarLoteAsync(id, command, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Actualiza el estado operativo de un lote (Disponible, Reservado, Vendido). Exclusivo para el rol Administrador.
    /// </summary>
    [HttpPatch("{id:guid}/estado")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoLoteRequest request, CancellationToken ct = default)
    {
        var resultado = await _loteService.CambiarEstadoLoteAsync(id, request.Estado, ct);
        return FromResult(resultado);
    }
}
