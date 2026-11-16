namespace SistemaFuneraria.Application.DTOs.Admin;

/// <summary>
/// DTO con los datos de un usuario operativo del sistema para la gestión del Administrador (SF-029).
/// </summary>
public record UsuarioSistemaDto(
    string Id,
    string NombreCompleto,
    string Iniciales,
    string Correo,
    string Rol,
    string Sucursal,
    bool Bloqueado
);

/// <summary>
/// Petición para registrar un nuevo usuario del sistema (SF-029).
/// </summary>
public record CrearUsuarioRequest(
    string NombreCompleto,
    string Correo,
    string Password,
    string Rol,
    string Sucursal
);

/// <summary>
/// Petición para actualizar información de usuario, rol o desbloquear cuenta (SF-029).
/// </summary>
public record ActualizarUsuarioRequest(
    string NombreCompleto,
    string Rol,
    string Sucursal,
    bool Desbloquear
);
