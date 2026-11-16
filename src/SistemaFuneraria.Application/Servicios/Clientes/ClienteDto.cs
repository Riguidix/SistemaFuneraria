namespace SistemaFuneraria.Application.Servicios.Clientes;

/// <summary>
/// DTO plano e inmutable que expone la información pública del cliente hacia la capa de presentación Web.
/// </summary>
/// <param name="Id">Identificador único del cliente.</param>
/// <param name="Nombre">Nombre del cliente.</param>
/// <param name="Apellidos">Apellidos del cliente.</param>
/// <param name="Curp">Clave CURP del cliente.</param>
/// <param name="Telefono">Teléfono de 10 dígitos.</param>
/// <param name="Correo">Correo electrónico.</param>
/// <param name="Direccion">Dirección del cliente.</param>
/// <param name="CreatedAt">Marca temporal de registro en formato UTC.</param>
public record ClienteDto(
    Guid Id,
    string Nombre,
    string Apellidos,
    string Curp,
    string Telefono,
    string Correo,
    string Direccion,
    DateTimeOffset CreatedAt
);
