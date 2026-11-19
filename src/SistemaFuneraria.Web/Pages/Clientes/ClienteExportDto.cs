namespace SistemaFuneraria.Web.Pages.Clientes;

/// <summary>
/// DTO con los datos del directorio de clientes listos para exportación.
/// </summary>
public record ClienteExportDto(
    string Nombre,
    string Telefono,
    string Curp,
    string Contratos,
    string Saldo,
    string UltimoPago,
    string Estado
);
