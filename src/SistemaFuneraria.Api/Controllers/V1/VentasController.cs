using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Ventas;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador REST para el registro de ventas y consulta de contratos comerciales (SF-061).
/// </summary>
[ApiController]
[Route("api/v1/ventas")]
[Authorize]
public class VentasController(IVentaService ventaService) : ApiControllerBase
{
    private readonly IVentaService _ventaService = ventaService;

    /// <summary>
    /// Registra una venta en modalidad de contado con liquidación inmediata (CP-02 / ST-01).
    /// </summary>
    [HttpPost("contado")]
    [ProducesResponseType(typeof(ContratoResumenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarContado([FromBody] RegistrarVentaContadoRequest request, CancellationToken ct = default)
    {
        if (request is null || request.ClienteId == Guid.Empty || request.LoteId == Guid.Empty)
        {
            return Problem(
                detail: "El ClienteId y LoteId son obligatorios y no pueden ser vacíos.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Datos de entrada inválidos");
        }

        var vendedorId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.Identity?.Name
            ?? "sistema";

        var command = new RegistrarVentaContadoCommand(
            ClienteId: request.ClienteId,
            LoteId: request.LoteId,
            VendedorId: vendedorId,
            MetodoPago: request.MetodoPago,
            ReferenciaPago: request.ReferenciaPago);

        var resultado = await _ventaService.RegistrarVentaContadoAsync(command, ct);
        if (resultado.EsFallo)
        {
            return FromResult(resultado);
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor.VentaId }, resultado.Valor);
    }

    /// <summary>
    /// Obtiene el detalle de un contrato comercial y comprobante de venta por su identificador UUID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContratoResumenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct = default)
    {
        var resultado = await _ventaService.ObtenerPorIdAsync(id, ct);
        return FromResult(resultado);
    }
}
