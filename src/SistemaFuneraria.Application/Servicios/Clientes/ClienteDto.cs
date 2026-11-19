namespace SistemaFuneraria.Application.Servicios.Clientes;

/// <summary>
/// DTO plano e inmutable que expone la información pública del cliente y su resumen de cartera (SF-017, SF-031).
/// </summary>
/// <param name="Id">Identificador único del cliente.</param>
/// <param name="Nombre">Nombre del cliente.</param>
/// <param name="Apellidos">Apellidos del cliente.</param>
/// <param name="Curp">Clave CURP del cliente.</param>
/// <param name="Telefono">Teléfono de 10 dígitos.</param>
/// <param name="Correo">Correo electrónico.</param>
/// <param name="Direccion">Dirección del cliente.</param>
/// <param name="CreatedAt">Marca temporal de registro en formato UTC.</param>
/// <param name="Iniciales">Iniciales calculadas (ej. "RM", "LC").</param>
/// <param name="FechaAltaFormateada">Texto formateado de alta (ej. "Cliente desde nov. 2025").</param>
/// <param name="ContratosActivos">Número de contratos activos del cliente.</param>
/// <param name="SaldoActual">Saldo insoluto total consolidado.</param>
/// <param name="UltimoPago">Fecha del último pago registrado.</param>
/// <param name="EstadoCartera">Estado de cartera ("Al corriente", "Pago vencido", "En revisión").</param>
public record ClienteDto(
    Guid Id,
    string Nombre,
    string Apellidos,
    string Curp,
    string Telefono,
    string Correo,
    string Direccion,
    DateTimeOffset CreatedAt,
    string Iniciales = "",
    string FechaAltaFormateada = "",
    int ContratosActivos = 0,
    decimal SaldoActual = 0m,
    DateTime? UltimoPago = null,
    string EstadoCartera = "Al corriente"
);
