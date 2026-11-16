namespace SistemaFuneraria.Application.Servicios.Clientes;

/// <summary>
/// DTO de entrada inmutable para el registro de un nuevo cliente en el sistema funerario.
/// </summary>
/// <param name="Nombre">Nombre de pila del cliente (máx 100 caracteres).</param>
/// <param name="Apellidos">Apellidos paterno y materno del cliente (máx 100 caracteres).</param>
/// <param name="Curp">Clave Única de Registro de Población (exactamente 18 caracteres alfanuméricos).</param>
/// <param name="Telefono">Número telefónico de contacto (10 dígitos numéricos).</param>
/// <param name="Correo">Dirección de correo electrónico válida (máx 255 caracteres).</param>
/// <param name="Direccion">Domicilio residencial completo del cliente.</param>
public record RegistrarClienteRequest(
    string Nombre,
    string Apellidos,
    string Curp,
    string Telefono,
    string Correo,
    string Direccion
);
