namespace SistemaFuneraria.Application.DTOs.Auth;

/// <summary>
/// Petición de autenticación de usuario (SF-023).
/// </summary>
public record LoginRequest(string Correo, string Password);

/// <summary>
/// Respuesta del endpoint de autenticación con metadatos de seguridad y control de lockout (SF-023).
/// </summary>
public record LoginResponseDto(
    string Token,
    string UsuarioId,
    string NombreCompleto,
    string Iniciales,
    string Correo,
    string Rol,
    string Sucursal,
    int IntentosFallidos,
    int? MinutosBloqueoRestantes,
    string? AdvertenciaSeguridad
);

/// <summary>
/// Petición para recuperación de contraseña anti-enumeración (SF-023).
/// </summary>
public record OlvideContrasenaRequest(string Correo);

/// <summary>
/// Respuesta informativa estándar para recuperación de contraseña (SF-023).
/// </summary>
public record OlvideContrasenaResponseDto(string Mensaje);

/// <summary>
/// Perfil del usuario autenticado en sesión (SF-023).
/// </summary>
public record UsuarioPerfilDto(
    string UsuarioId,
    string NombreCompleto,
    string Iniciales,
    string Correo,
    string Rol,
    string Sucursal
);
