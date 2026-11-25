using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Auth;

namespace SistemaFuneraria.Application.Servicios.Auth;

public enum EstadoLogin
{
    Exitoso = 0,
    CredencialesInvalidas = 1,
    Bloqueado = 2,
    SinRol = 3
}

public record ResultadoLoginAuth(
    EstadoLogin Estado,
    LoginResponseDto? Payload,
    string? ErrorDetalle = null);

/// <summary>
/// Contrato de aplicación para autenticación, control de bloqueo temporal, recuperación y perfil actual.
/// </summary>
public interface IAuthService
{
    Task<ResultadoLoginAuth> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<OlvideContrasenaResponseDto> OlvideContrasenaAsync(OlvideContrasenaRequest request, CancellationToken cancellationToken = default);
    Task<Resultado<UsuarioPerfilDto>> ObtenerPerfilPorIdAsync(string userId, CancellationToken cancellationToken = default);
}
