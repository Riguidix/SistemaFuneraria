using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Auth;
using SistemaFuneraria.Application.Servicios.Auth;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Api.Servicios;

/// <summary>
/// Servicio de autenticación con Identity, emisión JWT, bloqueo temporal (5 intentos / 15 min) y mitigación anti-enumeración.
/// </summary>
public class AuthService(
    UserManager<IdentityUser> userManager,
    IConfiguration configuration,
    TimeProvider? timeProvider = null) : IAuthService
{
    private const int MaxIntentosFallidos = 5;
    private const int IntentoAdvertencia = 3;
    private const int MinutosBloqueo = 15;

    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<ResultadoLoginAuth> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Correo);
        if (user is null)
        {
            return new ResultadoLoginAuth(
                EstadoLogin.CredencialesInvalidas,
                CrearRespuestaFallida(string.Empty, request.Correo, intentosFallidos: 1));
        }

        var ahoraUtc = _timeProvider.GetUtcNow();
        if (await userManager.IsLockedOutAsync(user))   
        {
            int minutosRestantes = CalcularMinutosBloqueoRestantes(user, ahoraUtc);
            return new ResultadoLoginAuth(
                EstadoLogin.Bloqueado,
                CrearRespuestaBloqueada(
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

            if (estaBloqueado || intentos >= MaxIntentosFallidos)
            {
                await userManager.SetLockoutEndDateAsync(user, ahoraUtc.AddMinutes(MinutosBloqueo));
                return new ResultadoLoginAuth(
                    EstadoLogin.Bloqueado,
                    CrearRespuestaBloqueada(
                        user,
                        request.Correo,
                        MaxIntentosFallidos,
                        MinutosBloqueo,
                        $"Cuenta bloqueada temporalmente. Intenta nuevamente en {MinutosBloqueo} minutos."));
            }

            var advertencia = intentos == IntentoAdvertencia
                ? "Advertencia: Si alcanzas 5 intentos fallidos tu cuenta será bloqueada temporalmente por seguridad"
                : null;

            return new ResultadoLoginAuth(
                EstadoLogin.CredencialesInvalidas,
                CrearRespuestaFallida(user.Id, user.Email ?? request.Correo, intentos, advertencia));
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var perfil = await ResolverPerfilAsync(user, request.Correo);
        if (perfil is null)
        {
            return new ResultadoLoginAuth(
                EstadoLogin.SinRol,
                Payload: null,
                ErrorDetalle: "El usuario no tiene un rol asignado en el sistema.");
        }

        var token = GenerarTokenJwt(user.Id, perfil.Correo, perfil.Rol, ahoraUtc);

        return new ResultadoLoginAuth(
            EstadoLogin.Exitoso,
            new LoginResponseDto(
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

    public async Task<OlvideContrasenaResponseDto> OlvideContrasenaAsync(
        OlvideContrasenaRequest request,
        CancellationToken cancellationToken = default)
    {
        var jitterMs = Random.Shared.Next(200, 601);
        await Task.Delay(jitterMs, cancellationToken);

        return new OlvideContrasenaResponseDto(
            "Si el correo ingresado se encuentra registrado, recibirás un enlace de restablecimiento en breve");
    }

    public async Task<Resultado<UsuarioPerfilDto>> ObtenerPerfilPorIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Resultado<UsuarioPerfilDto>.FalloNoAutorizado("No autorizado.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Resultado<UsuarioPerfilDto>.FalloNoEncontrado("Usuario no encontrado.");
        }

        var perfil = await ResolverPerfilAsync(user, user.Email ?? string.Empty);
        if (perfil is null)
        {
            return Resultado<UsuarioPerfilDto>.FalloProhibido("El usuario no tiene un rol asignado en el sistema.");
        }

        return Resultado<UsuarioPerfilDto>.Exito(perfil);
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
        var nombreCompleto = claims.FirstOrDefault(c => c.Type == ParametrosNegocio.ClaimNombreCompleto)?.Value
            ?? user.UserName
            ?? "Usuario";
        var iniciales = claims.FirstOrDefault(c => c.Type == ParametrosNegocio.ClaimIniciales)?.Value
            ?? TextoHelper.ObtenerIniciales(nombreCompleto);
        var sucursal = claims.FirstOrDefault(c => c.Type == ParametrosNegocio.ClaimSucursal)?.Value
            ?? ParametrosNegocio.SucursalPorDefecto;

        return new UsuarioPerfilDto(
            UsuarioId: user.Id,
            NombreCompleto: nombreCompleto,
            Iniciales: iniciales,
            Correo: user.Email ?? correoFallback,
            Rol: rol,
            Sucursal: sucursal);
    }

    private string GenerarTokenJwt(string userId, string email, string rol, DateTimeOffset ahoraUtc)
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
            expires: ahoraUtc.UtcDateTime.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static int CalcularMinutosBloqueoRestantes(IdentityUser user, DateTimeOffset ahoraUtc)
    {
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > ahoraUtc)
        {
            return Math.Max(1, (int)Math.Ceiling((user.LockoutEnd.Value - ahoraUtc).TotalMinutes));
        }

        return MinutosBloqueo;
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
