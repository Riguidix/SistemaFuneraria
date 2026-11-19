using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.Servicios.Clientes;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador REST para la gestión del ciclo de vida de clientes, consultas y exportación (SF-032).
/// Conforme a especificaciones de API v1 y vistas Figma (Capturas 3 y 4).
/// </summary>
[ApiController]
[Route("api/v1/clientes")]
[Authorize(Roles = "Administrador,Vendedor")]
public class ClientesController(IClienteService clienteService) : ApiControllerBase
{
    private readonly IClienteService _clienteService = clienteService;

    /// <summary>
    /// Consulta y busca clientes aplicando filtros reactivos por nombre, apellidos, CURP o teléfono.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] string? filtro, CancellationToken ct)
    {
        var resultado = await _clienteService.BuscarClientesAsync(filtro, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Consulta los detalles de un cliente por su identificador único.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(Guid id, CancellationToken ct)
    {
        var resultado = await _clienteService.ObtenerPorIdAsync(id, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Registra un nuevo cliente en el padrón con validación estricta de invariantes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarClienteRequest request, CancellationToken ct)
    {
        var resultado = await _clienteService.RegistrarClienteAsync(request, ct);
        if (resultado.EsFallo)
        {
            return FromResult(resultado);
        }

        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Valor }, new { id = resultado.Valor });
    }

    /// <summary>
    /// Actualiza los datos de un cliente existente con validación de unicidad de CURP y correo.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(Guid id, [FromBody] ActualizarClienteRequest request, CancellationToken ct)
    {
        var resultado = await _clienteService.ActualizarClienteAsync(id, request, ct);
        return FromResult(resultado);
    }

    /// <summary>
    /// Elimina a un cliente del padrón. Exclusivo para el rol Administrador.
    /// Falla si el cliente posee contratos asociados.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        var resultado = await _clienteService.EliminarClienteAsync(id, ct);
        return resultado.EsExitoso ? NoContent() : FromResult(resultado);
    }

    /// <summary>
    /// Exporta el directorio de clientes en formato CSV UTF-8 para su descarga.
    /// </summary>
    [HttpGet("exportar")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportarDirectorio(CancellationToken ct)
    {
        var resultado = await _clienteService.BuscarClientesAsync(null, ct);
        if (resultado.EsFallo)
        {
            return FromResult(resultado);
        }

        var sb = new StringBuilder();
        sb.AppendLine("Id,Nombre,Apellidos,CURP,Telefono,Correo,Direccion,ContratosActivos,SaldoActual,UltimoPago,EstadoCartera,FechaRegistro");

        foreach (var c in resultado.Valor)
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4},{5},{6},{7},{8:F2},{9},{10},{11:yyyy-MM-dd HH:mm:ss}",
                c.Id,
                EscaparCsv(c.Nombre),
                EscaparCsv(c.Apellidos),
                EscaparCsv(c.Curp),
                EscaparCsv(c.Telefono),
                EscaparCsv(c.Correo),
                EscaparCsv(c.Direccion),
                c.ContratosActivos,
                c.SaldoActual,
                c.UltimoPago?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                EscaparCsv(c.EstadoCartera),
                c.CreatedAt
            ));
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fullBytes = new byte[preamble.Length + bytes.Length];
        Buffer.BlockCopy(preamble, 0, fullBytes, 0, preamble.Length);
        Buffer.BlockCopy(bytes, 0, fullBytes, preamble.Length, bytes.Length);

        return File(fullBytes, "text/csv; charset=utf-8", "directorio-clientes.csv");
    }

    private static string EscaparCsv(string? texto)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;
        if (texto.Contains(',') || texto.Contains('"') || texto.Contains('\n') || texto.Contains('\r'))
        {
            return $"\"{texto.Replace("\"", "\"\"")}\"";
        }
        return texto;
    }
}
