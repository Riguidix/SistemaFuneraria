namespace SistemaFuneraria.Application.Servicios.Clientes;

/// <summary>
/// Petición para actualizar los datos personales y de contacto de un cliente (SF-031).
/// </summary>
/// <param name="Nombre">Nombre(s) del cliente.</param>
/// <param name="Apellidos">Apellidos del cliente.</param>
/// <param name="Curp">Clave CURP oficial de 18 caracteres.</param>
/// <param name="Telefono">Teléfono de contacto de 10 dígitos numéricos.</param>
/// <param name="Correo">Correo electrónico del cliente.</param>
/// <param name="Direccion">Dirección domiciliaria opcional.</param>
public record ActualizarClienteRequest(
    string Nombre,
    string Apellidos,
    string Curp,
    string Telefono,
    string Correo,
    string? Direccion = null
);
