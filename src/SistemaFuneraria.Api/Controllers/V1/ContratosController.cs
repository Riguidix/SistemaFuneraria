using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.DTOs.Contratos;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Contratos;
using SistemaFuneraria.Application.Servicios.Ventas;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador REST para la consulta de cartera de contratos, detalle por folio/UUID y amortización (SF-069).
/// </summary>
[ApiController]
[Route("api/v1/contratos")]
[Authorize]
public class ContratosController(
    ICarteraContratosService carteraContratosService,
    IVentaService ventaService) : ApiControllerBase
{
    private readonly ICarteraContratosService _carteraContratosService = carteraContratosService;
    private readonly IVentaService _ventaService = ventaService;

    /// <summary>
    /// Obtiene los 4 KPIs agregados y la tabla filtrada/paginada de contratos (SF-068 / SF-069).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CarteraContratosResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerCartera(
        [FromQuery] string? buscar = null,
        [FromQuery] string? tab = "Todos",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var resultado = await _carteraContratosService.ObtenerCarteraAsync(buscar, tab, page, pageSize, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Localiza un contrato comercial por su identificador UUID o su folio comercial CTR-YYYY-XXXX (SF-069).
    /// </summary>
    [HttpGet("{folioOrId}")]
    [ProducesResponseType(typeof(ContratoDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerDetalle(string folioOrId, CancellationToken ct = default)
    {
        var resultado = await _carteraContratosService.ObtenerDetalleAsync(folioOrId, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Obtiene las N cuotas persistidas de amortización para un contrato financiado por su UUID (SF-067 / SF-069).
    /// </summary>
    [HttpGet("{id:guid}/amortizacion")]
    [ProducesResponseType(typeof(IReadOnlyList<CuotaAmortizacionContratoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerAmortizacion(Guid id, CancellationToken ct = default)
    {
        var resultado = await _ventaService.ObtenerAmortizacionPorVentaIdAsync(id, ct);
        return FromResult(resultado);
    }
}
