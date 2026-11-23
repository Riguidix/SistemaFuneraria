using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.DTOs.Cobranza;
using SistemaFuneraria.Application.Servicios.Cobranza;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador REST de Caja y Cobranza para pagos ordinarios, corte del día y recibos oficiales (SF-078).
/// </summary>
[ApiController]
[Route("api/v1/cobranza")]
[Authorize(Roles = "Administrador,Cajero")]
public class CobranzaController(ICobranzaService cobranzaService) : ApiControllerBase
{
    private readonly ICobranzaService _cobranzaService = cobranzaService;

    /// <summary>
    /// Devuelve el corte del día en vivo y el conteo de mensualidades vencidas (SF-078).
    /// </summary>
    [HttpGet("resumen-caja")]
    [ProducesResponseType(typeof(CorteDelDiaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerResumenCaja(CancellationToken ct = default)
    {
        var resultado = await _cobranzaService.ObtenerResumenCajaAsync(ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Localiza un contrato en Caja por folio o UUID devolviendo cuota vigente e importes estimados (SF-078).
    /// </summary>
    [HttpGet("contrato/{folio}")]
    [ProducesResponseType(typeof(ContratoCajaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ObtenerContrato(string folio, CancellationToken ct = default)
    {
        var resultado = await _cobranzaService.ObtenerContratoParaCajaAsync(folio, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Aplica el cobro de la siguiente mensualidad exigible con idempotencia (X-Idempotency-Key) y control de concurrencia.
    /// </summary>
    [HttpPost("mensualidad")]
    [ProducesResponseType(typeof(ReciboPagoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AplicarMensualidad(
        [FromBody] AplicarPagoMensualidadRequest request,
        CancellationToken ct = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.FolioContrato))
        {
            return Problem(
                detail: "El folio del contrato es obligatorio.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Datos de entrada inválidos");
        }

        var headerKey = Request.Headers["X-Idempotency-Key"].FirstOrDefault();
        var claveIdempotencia = !string.IsNullOrWhiteSpace(headerKey)
            ? headerKey.Trim()
            : request.ClaveIdempotencia?.Trim();

        if (string.IsNullOrWhiteSpace(claveIdempotencia))
        {
            return Problem(
                detail: "La clave de idempotencia (X-Idempotency-Key o ClaveIdempotencia) es obligatoria.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Clave de idempotencia faltante");
        }

        var cajeroId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.Identity?.Name
            ?? "cajero";

        var command = new AplicarPagoMensualidadCommand(
            FolioContrato: request.FolioContrato,
            ImporteRecibido: request.ImporteRecibido,
            MetodoPago: request.MetodoPago,
            Referencia: request.Referencia,
            ClaveIdempotencia: claveIdempotencia,
            CajeroId: cajeroId,
            NumeroCuota: request.NumeroCuota);

        var resultado = await _cobranzaService.AplicarPagoMensualidadAsync(command, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Consulta el recibo oficial PAG-YYYY-XXXXXX por su folio (SF-078 / SF-082).
    /// </summary>
    [HttpGet("recibos/{folio}")]
    [ProducesResponseType(typeof(ReciboPagoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerRecibo(string folio, CancellationToken ct = default)
    {
        var resultado = await _cobranzaService.ObtenerReciboPorFolioAsync(folio, ct);
        return FromResult(resultado);
    }
}
