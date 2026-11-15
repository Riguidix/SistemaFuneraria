using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Api.Controllers.V1;

/// <summary>
/// Controlador de autenticación con máquina de estados, JWT firmado, bloqueo temporal y mitigación anti-enumeración (SF-023).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration) : ApiControllerBase
{
    /// <summary>
    /// Inicia sesión de usuario con control de lockout (5 intentos / 15 minutos) y advertencia preventiva al 3.er intento.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Problem(
                detail: "El correo y la contraseña son requeridos.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros inválidos");
        }

        var user = await userManager.FindByEmailAsync(request.Correo);
        if (user == null)
        {
            return Unauthorized(CrearRespuestaFallida(
                usuarioId: string.Empty,
                correo: request.Correo,
                intentosFallidos: 1));
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            int minutosRestantes = CalcularMinutosBloqueoRestantes(user);
            return StatusCode(StatusCodes.Status423Locked, CrearRespuestaBloqueada(
                user,
                request.Correo,
                user.AccessFailedCount,
                minutosRestantes,
                $"Cuenta bloqueada temporalmente. Intenta nuevamente en {minutosRestantes} minutos."));
        }

        bool esPasswordValido = await userManager.CheckPasswordAsync(user, request.Password);
        if (!esPasswordValido)
        {
            await userManager.AccessFailedAsync(user);
            var estaBloqueado = await userManager.IsLockedOutAsync(user);
            var intentos = await userManager.GetAccessFailedCountAsync(user);

            if (estaBloqueado || intentos >= 5)
            {
                await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15));
                return StatusCode(StatusCodes.Status423Locked, CrearRespuestaBloqueada(
                    user,
                    request.Correo,
                    5,
                    15,
                    "Cuenta bloqueada temporalmente. Intenta nuevamente en 15 minutos."));
            }

            var advertencia = intentos == 3
                ? "Advertencia: Si alcanzas 5 intentos fallidos tu cuenta será bloqueada temporalmente por seguridad"
                : null;

            return Unauthorized(CrearRespuestaFallida(
                usuarioId: user.Id,
                correo: user.Email ?? request.Correo,
                intentosFallidos: intentos,
                advertenciaSeguridad: advertencia));
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var perfil = await ResolverPerfilAsync(user, request.Correo);
        if (perfil == null)
        {
            return Problem(
                detail: "El usuario no tiene un rol asignado en el sistema.",
                statusCode: StatusCodes.Status403Forbidden,
                title: "Acceso denegado");
        }

        var token = GenerarTokenJwt(user.Id, perfil.Correo, perfil.Rol);

        return Ok(new LoginResponseDto(
            Token: token,
            UsuarioId: perfil.UsuarioId,
            NombreCompleto: perfil.NombreCompleto,
            Iniciales: perfil.Iniciales,
            Correo: perfil.Correo,
            Rol: perfil.Rol,
            Sucursal: perfil.Sucursal,
            IntentosFallidos: 0,
            MinutosBloqueoRestantes: null,
            AdvertenciaSeguridad: null));
    }

    /// <summary>
    /// Recuperación de contraseña con respuesta constante y jitter pseudoaleatorio anti-timing attack (200ms - 600ms).
    /// </summary>
    [HttpPost("olvide-contrasena")]
    public async Task<IActionResult> OlvideContrasena([FromBody] OlvideContrasenaRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Correo))
        {
            return Problem(
                detail: "El correo electrónico es requerido.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Parámetros inválidos");
        }

        var jitterMs = Random.Shared.Next(200, 601);
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
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Problem(
                detail: "No autorizado.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "No autorizado");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Problem(
                detail: "Usuario no encontrado.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Recurso no encontrado");
        }

        var perfil = await ResolverPerfilAsync(user, user.Email ?? string.Empty);
        if (perfil == null)
        {
            return Problem(
                detail: "El usuario no tiene un rol asignado en el sistema.",
                statusCode: StatusCodes.Status403Forbidden,
                title: "Acceso denegado");
        }

        return Ok(perfil);
    }

    private async Task<UsuarioPerfilDto?> ResolverPerfilAsync(IdentityUser user, string correoFallback)
    {
        var roles = await userManager.GetRolesAsync(user);
        var rol = roles.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rol))
        {
            return null;
        }

        var claims = await userManager.GetClaimsAsync(user);
        var nombreCompleto = claims.FirstOrDefault(c => c.Type == "NombreCompleto")?.Value
                             ?? user.UserName
                             ?? "Usuario";
        var iniciales = claims.FirstOrDefault(c => c.Type == "Iniciales")?.Value
                        ?? (nombreCompleto.Length >= 2 ? nombreCompleto[..2].ToUpperInvariant() : "US");
        var sucursal = claims.FirstOrDefault(c => c.Type == "Sucursal")?.Value
                       ?? ParametrosNegocio.SucursalPorDefecto;

        return new UsuarioPerfilDto(
            UsuarioId: user.Id,
            NombreCompleto: nombreCompleto,
            Iniciales: iniciales,
            Correo: user.Email ?? correoFallback,
            Rol: rol,
            Sucursal: sucursal);
    }

    private string GenerarTokenJwt(string userId, string email, string rol)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException("La clave de firma 'Jwt:Key' no está configurada.");
        var issuer = jwtSection["Issuer"] ?? "SistemaFuneraria.Api";
        var audience = jwtSection["Audience"] ?? "SistemaFuneraria.Web";
        var expirationMinutes = int.TryParse(jwtSection["ExpirationMinutes"], out var mins) && mins > 0
            ? mins
            : 480;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static int CalcularMinutosBloqueoRestantes(IdentityUser user)
    {
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            return Math.Max(1, (int)Math.Ceiling((user.LockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes));
        }

        return 15;
    }

    private static LoginResponseDto CrearRespuestaFallida(
        string usuarioId,
        string correo,
        int intentosFallidos,
        string? advertenciaSeguridad = null) =>
        new(
            Token: string.Empty,
            UsuarioId: usuarioId,
            NombreCompleto: string.Empty,
            Iniciales: string.Empty,
            Correo: correo,
            Rol: string.Empty,
            Sucursal: ParametrosNegocio.SucursalPorDefecto,
            IntentosFallidos: intentosFallidos,
            MinutosBloqueoRestantes: null,
            AdvertenciaSeguridad: advertenciaSeguridad);

    private static LoginResponseDto CrearRespuestaBloqueada(
        IdentityUser user,
        string correoFallback,
        int intentosFallidos,
        int minutosRestantes,
        string mensajeBloqueo) =>
        new(
            Token: string.Empty,
            UsuarioId: user.Id,
            NombreCompleto: user.UserName ?? string.Empty,
            Iniciales: string.Empty,
            Correo: user.Email ?? correoFallback,
            Rol: string.Empty,
            Sucursal: ParametrosNegocio.SucursalPorDefecto,
            IntentosFallidos: intentosFallidos,
            MinutosBloqueoRestantes: minutosRestantes,
            AdvertenciaSeguridad: mensajeBloqueo);
}
