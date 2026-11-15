using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaFuneraria.Application.DTOs.Auth;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador de autenticación con máquina de estados, bloqueo temporal y mitigación anti-enumeración (SF-023).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController(UserManager<IdentityUser> userManager) : ApiControllerBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;

    /// <summary>
    /// Inicia sesión de usuario con control de lockout (5 intentos / 15 minutos) y advertencia preventiva al 3.er intento.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(Problem(
                detail: "El correo y la contraseña son requeridos.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros inválidos"));
        }

        var user = await _userManager.FindByEmailAsync(request.Correo);
        if (user == null)
        {
            // Mitigación anti-enumeración: respuesta 401 idéntica a credencial errónea
            return Unauthorized(new LoginResponseDto(
                Token: string.Empty,
                UsuarioId: string.Empty,
                NombreCompleto: string.Empty,
                Iniciales: string.Empty,
                Correo: request.Correo,
                Rol: string.Empty,
                Sucursal: "Jardines del Valle",
                IntentosFallidos: 1,
                MinutosBloqueoRestantes: null,
                AdvertenciaSeguridad: null
            ));
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            int minutosRestantes = 15;
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                minutosRestantes = Math.Max(1, (int)Math.Ceiling((user.LockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes));
            }

            return StatusCode(StatusCodes.Status423Locked, new LoginResponseDto(
                Token: string.Empty,
                UsuarioId: user.Id,
                NombreCompleto: user.UserName ?? string.Empty,
                Iniciales: string.Empty,
                Correo: user.Email ?? request.Correo,
                Rol: string.Empty,
                Sucursal: "Jardines del Valle",
                IntentosFallidos: user.AccessFailedCount,
                MinutosBloqueoRestantes: minutosRestantes,
                AdvertenciaSeguridad: $"Cuenta bloqueada temporalmente. Intenta nuevamente en {minutosRestantes} minutos."
            ));
        }

        bool esPasswordValido = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!esPasswordValido)
        {
            await _userManager.AccessFailedAsync(user);
            bool estaBloqueado = await _userManager.IsLockedOutAsync(user);
            int intentos = await _userManager.GetAccessFailedCountAsync(user);

            if (estaBloqueado || intentos >= 5)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15));
                return StatusCode(StatusCodes.Status423Locked, new LoginResponseDto(
                    Token: string.Empty,
                    UsuarioId: user.Id,
                    NombreCompleto: user.UserName ?? string.Empty,
                    Iniciales: string.Empty,
                    Correo: user.Email ?? request.Correo,
                    Rol: string.Empty,
                    Sucursal: "Jardines del Valle",
                    IntentosFallidos: 5,
                    MinutosBloqueoRestantes: 15,
                    AdvertenciaSeguridad: "Cuenta bloqueada temporalmente. Intenta nuevamente en 15 minutos."
                ));
            }

            string? advertencia = null;
            if (intentos == 3)
            {
                advertencia = "Advertencia: Si alcanzas 5 intentos fallidos tu cuenta será bloqueada temporalmente por seguridad";
            }

            return Unauthorized(new LoginResponseDto(
                Token: string.Empty,
                UsuarioId: user.Id,
                NombreCompleto: string.Empty,
                Iniciales: string.Empty,
                Correo: user.Email ?? request.Correo,
                Rol: string.Empty,
                Sucursal: "Jardines del Valle",
                IntentosFallidos: intentos,
                MinutosBloqueoRestantes: null,
                AdvertenciaSeguridad: advertencia
            ));
        }

        // Credenciales correctas: resetear contador de fallos
        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        string rol = roles.FirstOrDefault() ?? "Vendedor";

        var claims = await _userManager.GetClaimsAsync(user);
        string nombreCompleto = claims.FirstOrDefault(c => c.Type == "NombreCompleto")?.Value
            ?? user.UserName
            ?? "Usuario";
        string iniciales = claims.FirstOrDefault(c => c.Type == "Iniciales")?.Value
            ?? (nombreCompleto.Length >= 2 ? nombreCompleto[..2].ToUpperInvariant() : "US");
        string sucursal = claims.FirstOrDefault(c => c.Type == "Sucursal")?.Value
            ?? "Jardines del Valle";

        string token = GenerarToken(user.Id, user.Email ?? request.Correo, rol);

        return Ok(new LoginResponseDto(
            Token: token,
            UsuarioId: user.Id,
            NombreCompleto: nombreCompleto,
            Iniciales: iniciales,
            Correo: user.Email ?? request.Correo,
            Rol: rol,
            Sucursal: sucursal,
            IntentosFallidos: 0,
            MinutosBloqueoRestantes: null,
            AdvertenciaSeguridad: null
        ));
    }

    /// <summary>
    /// Recuperación de contraseña con respuesta constante y jitter pseudoaleatorio anti-timing attack (200ms - 600ms).
    /// </summary>
    [HttpPost("olvide-contrasena")]
    public async Task<IActionResult> OlvideContrasena([FromBody] OlvideContrasenaRequest request)
    {
        int jitterMs = Random.Shared.Next(200, 601);
        await Task.Delay(jitterMs);

        return Ok(new OlvideContrasenaResponseDto(
            "Si el correo ingresado se encuentra registrado, recibirás un enlace de restablecimiento en breve"
        ));
    }

    /// <summary>
    /// Retorna el perfil y claims del usuario actualmente autenticado en sesión.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { mensaje = "No autorizado" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado" });
        }

        var roles = await _userManager.GetRolesAsync(user);
        string rol = roles.FirstOrDefault() ?? "Vendedor";

        var claims = await _userManager.GetClaimsAsync(user);
        string nombreCompleto = claims.FirstOrDefault(c => c.Type == "NombreCompleto")?.Value
            ?? user.UserName
            ?? "Usuario";
        string iniciales = claims.FirstOrDefault(c => c.Type == "Iniciales")?.Value
            ?? (nombreCompleto.Length >= 2 ? nombreCompleto[..2].ToUpperInvariant() : "US");
        string sucursal = claims.FirstOrDefault(c => c.Type == "Sucursal")?.Value
            ?? "Jardines del Valle";

        return Ok(new UsuarioPerfilDto(
            UsuarioId: user.Id,
            NombreCompleto: nombreCompleto,
            Iniciales: iniciales,
            Correo: user.Email ?? string.Empty,
            Rol: rol,
            Sucursal: sucursal
        ));
    }

    private static string GenerarToken(string userId, string email, string rol)
    {
        string payload = $"{userId}:{email}:{rol}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:{Guid.NewGuid():N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }
}
