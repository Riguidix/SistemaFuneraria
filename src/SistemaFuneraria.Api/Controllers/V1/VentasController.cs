using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Ventas;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Enums;

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
        var vendedorNombre = User.FindFirstValue(ParametrosNegocio.ClaimNombreCompleto)
            ?? User.Identity?.Name
            ?? vendedorId;

        var command = new RegistrarVentaContadoCommand(
            ClienteId: request.ClienteId,
            LoteId: request.LoteId,
            VendedorId: vendedorId,
            MetodoPago: request.MetodoPago,
            ReferenciaPago: request.ReferenciaPago,
            FirmaClienteBase64: request.FirmaClienteBase64,
            VendedorNombre: vendedorNombre);

        var resultado = await _ventaService.RegistrarVentaContadoAsync(command, ct);
        if (resultado.EsFallo)
        {
            return FromResult(resultado);
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor.VentaId }, resultado.Valor);
    }

    /// <summary>
    /// Registra una venta en modalidad financiada a crédito creando amortización persistida (SF-067).
    /// </summary>
    [HttpPost("financiada")]
    [ProducesResponseType(typeof(ContratoFinanciadoResumenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarFinanciada([FromBody] RegistrarVentaFinanciadaRequest request, CancellationToken ct = default)
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
        var vendedorNombre = User.FindFirstValue(ParametrosNegocio.ClaimNombreCompleto)
            ?? User.Identity?.Name
            ?? vendedorId;

        var command = new RegistrarVentaFinanciadaCommand(
            ClienteId: request.ClienteId,
            LoteId: request.LoteId,
            VendedorId: vendedorId,
            PorcentajeEnganche: request.PorcentajeEnganche,
            PlazoMeses: request.PlazoMeses,
            FechaPrimerPago: request.FechaPrimerPago,
            FirmaClienteBase64: request.FirmaClienteBase64,
            VendedorNombre: vendedorNombre);

        var resultado = await _ventaService.RegistrarVentaFinanciadaAsync(command, ct);
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

    /// <summary>
    /// Obtiene la tabla de amortización persistida de una venta financiada por su identificador UUID (SF-067).
    /// </summary>
    [HttpGet("{id:guid}/amortizacion")]
    [ProducesResponseType(typeof(IReadOnlyList<CuotaAmortizacionContratoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerAmortizacion(Guid id, CancellationToken ct = default)
    {
        var resultado = await _ventaService.ObtenerAmortizacionPorVentaIdAsync(id, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Cambia administrativamente el estado de un contrato comercial (SF-063). Exclusivo Administrador.
    /// </summary>
    [HttpPatch("{id:guid}/estado")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoVentaRequest request, CancellationToken ct = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length < 10)
        {
            return Problem(
                detail: "El motivo administrativo es obligatorio y debe contener al menos 10 caracteres.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Error de validación");
        }

        var resultado = await _ventaService.CambiarEstadoContratoAdminAsync(id, request.Estado, request.Motivo, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Realiza la baja lógica / cancelación administrativa de una venta liberando su lote asociado (SF-063). Exclusivo Administrador.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelarVenta(Guid id, [FromQuery] string? motivo = null, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CancelarVentaRequest? body = null, CancellationToken ct = default)
    {
        var motivoEfectivo = !string.IsNullOrWhiteSpace(body?.Motivo) ? body.Motivo : motivo;
        if (string.IsNullOrWhiteSpace(motivoEfectivo) || motivoEfectivo.Trim().Length < 10)
        {
            return Problem(
                detail: "El motivo administrativo es obligatorio y debe contener al menos 10 caracteres.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Error de validación");
        }

        var resultado = await _ventaService.CambiarEstadoContratoAdminAsync(id, EstadoVenta.Cancelado, motivoEfectivo, ct);
        return FromResult(resultado);
    }
}
