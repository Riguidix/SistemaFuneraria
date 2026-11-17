namespace SistemaFuneraria.Application.Servicios.Clientes;

public record ActualizarClienteRequest(
    string Nombre,
    string Apellidos,
    string Curp,
    string Telefono,
    string Correo,
    string Direccion
);
