namespace SistemaFuneraria.Application.DTOs.Configuracion;

public sealed record ConfiguracionSucursalDto(
    Guid Id,
    string NombreComercial,
    string TelefonoAtencion,
    string CorreoOperativo,
    string Direccion,
    string Ciudad,
    string CodigoPostal,
    bool RecordatoriosPago,
    bool ResumenDiarioCaja,
    DateTimeOffset FechaActualizacion
);

public sealed record ActualizarConfiguracionSucursalRequest(
    string NombreComercial,
    string TelefonoAtencion,
    string CorreoOperativo,
    string Direccion,
    string? Ciudad,
    string? CodigoPostal,
    bool RecordatoriosPago,
    bool ResumenDiarioCaja
);

public sealed record UsuarioSistemaDto(
    string Id,
    string Nombre,
    string Correo,
    string Rol,
    bool Activo,
    bool Bloqueado
);

public sealed record CrearUsuarioConfiguracionRequest(
    string Nombre,
    string Correo,
    string Password,
    string Rol
);

public sealed record ActualizarUsuarioConfiguracionRequest(
    string? Nombre,
    string Rol,
    string? NuevaPassword = null
);

public sealed record CambiarEstadoUsuarioRequest(
    bool Activo,
    bool Desbloquear = true
);
