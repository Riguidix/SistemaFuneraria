using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.DTOs.Simulador;
using SistemaFuneraria.Application.Servicios.Lotes;
using SistemaFuneraria.Application.Servicios.Simulador;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador REST para el simulador y cotizador de esquemas de financiamiento funerario (SF-049).
/// Expone catálogo oficial de planes y cálculo de cotizaciones Contado/Crédito bajo CP-10.
/// </summary>
[ApiController]
[Route("api/v1/simulador")]
[Authorize]
public class SimuladorController(ISimuladorService simuladorService, ILoteService loteService) : ApiControllerBase
{
    private readonly ISimuladorService _simuladorService = simuladorService;
    private readonly ILoteService _loteService = loteService;

    /// <summary>
    /// Consulta el catálogo oficial de porcentajes de enganche (10%, 20%, 30%) y plazos con tasa anual (Captura 7).
    /// </summary>
    [HttpGet("planes")]
    [ProducesResponseType(typeof(PlanesFinanciamientoDto), StatusCodes.Status200OK)]
    public IActionResult ObtenerPlanes()
    {
        var planes = _simuladorService.ObtenerPlanes();
        return Ok(planes);
    }

    /// <summary>
    /// Ejecuta la simulación de cotización comercial para un lote en modalidad Contado o Crédito (CP-10).
    /// </summary>
    [HttpPost("cotizar")]
    [ProducesResponseType(typeof(SimulacionCotizacionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cotizar([FromBody] SimularCotizacionRequest request, CancellationToken ct = default)
    {
        var requestProcesado = request;

        if (request.LoteId.HasValue)
        {
            var loteResult = await _loteService.ObtenerPorIdAsync(request.LoteId.Value, ct);
            if (loteResult.EsFallo)
            {
                return Problem(
                    detail: $"[NO_ENCONTRADO] El lote con id '{request.LoteId.Value}' no existe en inventario.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Recurso no encontrado");
            }

            if (!request.Gavetas.HasValue)
            {
                requestProcesado = request with { Gavetas = loteResult.Valor.Gavetas };
            }
        }

        var resultado = _simuladorService.Cotizar(requestProcesado);
        return FromResult(resultado);
    }
}
