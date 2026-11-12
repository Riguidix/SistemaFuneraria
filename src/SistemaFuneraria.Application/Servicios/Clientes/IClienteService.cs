using SistemaFuneraria.Application.Comun;

namespace SistemaFuneraria.Application.Servicios.Clientes;

/// <summary>
/// Contrato de aplicación para la gestión y consulta de clientes.
/// Expone operaciones asíncronas desacopladas de la persistencia mediante el patrón Resultado (SF-017).
/// </summary>
public interface IClienteService
{
    /// <summary>
    /// Registra un nuevo cliente en el sistema.
    /// </summary>
    /// <param name="request">Datos requeridos para el alta del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Resultado exitoso con el ID único asignado o resultado fallido con errores de validación.</returns>
    Task<Resultado<Guid>> RegistrarClienteAsync(RegistrarClienteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta los datos de un cliente a partir de su identificador único.
    /// </summary>
    /// <param name="id">Identificador único del cliente.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Resultado exitoso con el DTO del cliente o fallo si no existe.</returns>
    Task<Resultado<ClienteDto>> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta los datos de un cliente a partir de su clave CURP.
    /// </summary>
    /// <param name="curp">Clave CURP de 18 caracteres.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Resultado exitoso con el DTO del cliente o fallo si no existe.</returns>
    Task<Resultado<ClienteDto>> ObtenerPorCurpAsync(string curp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la lista completa de clientes registrados en el sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Colección de sólo lectura de clientes.</returns>
    Task<Resultado<IReadOnlyList<ClienteDto>>> ListarClientesAsync(CancellationToken cancellationToken = default);
}
