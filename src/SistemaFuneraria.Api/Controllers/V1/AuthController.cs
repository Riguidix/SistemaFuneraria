using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.Servicios.Auth;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador de autenticación con máquina de estados, JWT firmado, bloqueo temporal y mitigación anti-enumeración (SF-023).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    /// <summary>
    /// Inicia sesión de usuario con control de lockout (5 intentos / 15 minutos) y advertencia preventiva al 3.er intento.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest? request, CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Problem(
                detail: "El correo y la contraseña son requeridos.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros inválidos");
        }

        var resultado = await authService.LoginAsync(request, cancellationToken);
        return resultado.Estado switch
        {
            EstadoLogin.Exitoso => Ok(resultado.Payload),
            EstadoLogin.CredencialesInvalidas => resultado.Payload is not null
                ? Unauthorized(resultado.Payload)
                : Problem(
                    detail: resultado.ErrorDetalle ?? "El correo y / o la contraseña son incorrectos.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Acceso denegado"),
            EstadoLogin.Bloqueado => StatusCode(StatusCodes.Status423Locked, resultado.Payload),
            EstadoLogin.SinRol => Problem(
                detail: resultado.ErrorDetalle ?? "El usuario no tiene un rol asignado en el sistema.",
                statusCode: StatusCodes.Status403Forbidden,
                title: "Acceso denegado"),
            _ => Problem(
                detail: "Estado de autenticación inesperado.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Error interno")
        };
    }

    /// <summary>
    /// Recuperación de contraseña con respuesta constante y jitter pseudoaleatorio anti-timing attack (200ms - 600ms).
    /// </summary>
    [HttpPost("olvide-contrasena")]
    public async Task<IActionResult> OlvideContrasena([FromBody] OlvideContrasenaRequest? request, CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Correo))
        {
            return Problem(
                detail: "El correo electrónico es requerido.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros inválidos");
        }

        return Ok(await authService.OlvideContrasenaAsync(request, cancellationToken));
    }

    /// <summary>
    /// Retorna el perfil y claims del usuario actualmente autenticado en sesión.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Problem(
                detail: "No autorizado.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "No autorizado");
        }

        return FromResult(await authService.ObtenerPerfilPorIdAsync(userId, cancellationToken));
    }
}
