using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.Servicios.Clientes;

namespace SistemaFuneraria.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/clientes")]
public class ClientesController(IClienteService clienteService) : ApiControllerBase
{
    private readonly IClienteService _clienteService = clienteService;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? filtro, CancellationToken ct)
    {
        var resultado = await _clienteService.ListarClientesAsync(ct);
        if (!resultado.EsExitoso)
        {
            return FromResult(resultado);
        }

        var clientes = resultado.Valor;
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var f = filtro.Trim();
            clientes = clientes.Where(c =>
                c.Nombre.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                c.Apellidos.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                c.Curp.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                c.Telefono.Contains(f, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var resultado = await _clienteService.ObtenerPorIdAsync(id, ct);
        if (!resultado.EsExitoso)
        {
            return NotFound();
        }

        return Ok(resultado.Valor);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarClienteRequest request, CancellationToken ct)
    {
        var resultado = await _clienteService.RegistrarClienteAsync(request, ct);
        if (!resultado.EsExitoso)
        {
            return Problem(
                detail: resultado.Error,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Error al registrar cliente");
        }

        return Ok(resultado.Valor);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarClienteRequest request, CancellationToken ct)
    {
        var resultado = await _clienteService.ActualizarClienteAsync(id, request, ct);
        if (!resultado.EsExitoso)
        {
            if (resultado.Error.Contains("[NO_ENCONTRADO]", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            return Problem(
                detail: resultado.Error,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Error al actualizar cliente");
        }

        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var resultado = await _clienteService.EliminarClienteAsync(id, ct);
        if (!resultado.EsExitoso)
        {
            if (resultado.Error.Contains("[NO_ENCONTRADO]", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            return Problem(
                detail: resultado.Error,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Error al eliminar cliente");
        }

        return Ok();
    }

    [HttpGet("exportar")]
    public async Task<IActionResult> Exportar(CancellationToken ct)
    {
        var resultado = await _clienteService.ListarClientesAsync(ct);
        if (!resultado.EsExitoso)
        {
            return FromResult(resultado);
        }

        var sb = new StringBuilder();
        sb.AppendLine("Id,Nombre,Apellidos,CURP,Telefono,Correo,Direccion,FechaRegistro");
        foreach (var c in resultado.Valor)
        {
            sb.AppendLine($"\"{c.Id}\",\"{c.Nombre}\",\"{c.Apellidos}\",\"{c.Curp}\",\"{c.Telefono}\",\"{c.Correo}\",\"{c.Direccion}\",\"{c.CreatedAt:O}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "directorio-clientes.csv");
    }
}
