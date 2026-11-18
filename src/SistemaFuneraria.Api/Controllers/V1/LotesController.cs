using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.DTOs.Lotes;
using SistemaFuneraria.Application.Servicios.Lotes;

namespace SistemaFuneraria.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/lotes")]
public class LotesController(ILoteService loteService) : ApiControllerBase
{
    private readonly ILoteService _loteService = loteService;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? buscar,
        [FromQuery] string? seccion,
        [FromQuery] string? estado,
        [FromQuery] int? gavetas,
        [FromQuery] int pagina = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var resultado = await _loteService.ObtenerCatalogoAsync(buscar, seccion, estado, gavetas, pagina, pageSize, ct);
        if (!resultado.EsExitoso)
        {
            return FromResult(resultado);
        }

        return Ok(resultado.Valor);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var resultado = await _loteService.ObtenerPorIdAsync(id, ct);
        if (!resultado.EsExitoso)
        {
            return NotFound();
        }

        return Ok(resultado.Valor);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Crear([FromBody] CrearLoteRequest request, CancellationToken ct)
    {
        var resultado = await _loteService.CrearLoteAsync(request, ct);
        if (!resultado.EsExitoso)
        {
            return Problem(
                detail: resultado.Error,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Error al crear lote");
        }

        return StatusCode(StatusCodes.Status201Created, resultado.Valor);
    }
}
