using SistemaFuneraria.Application.Comun;

namespace SistemaFuneraria.Application.Servicios.Clientes;

/// <summary>
/// Contrato de aplicación para la gestión, consulta y ciclo de vida de clientes (SF-017, SF-031).
/// Expone operaciones asíncronas desacopladas de la persistencia mediante el patrón Resultado.
/// </summary>
public interface IClienteService
{
    /// <summary>
    /// Registra un nuevo cliente en el sistema.
    /// </summary>
    Task<Resultado<Guid>> RegistrarClienteAsync(RegistrarClienteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta los datos de un cliente a partir de su identificador único.
    /// </summary>
    Task<Resultado<ClienteDto>> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta los datos de un cliente a partir de su clave CURP.
    /// </summary>
    Task<Resultado<ClienteDto>> ObtenerPorCurpAsync(string curp, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la lista completa de clientes registrados en el sistema.
    /// </summary>
    Task<Resultado<IReadOnlyList<ClienteDto>>> ListarClientesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca clientes aplicando filtros por nombre, teléfono o CURP (SF-031, Captura 3 Figma).
    /// </summary>
    Task<Resultado<IReadOnlyList<ClienteDto>>> BuscarClientesAsync(string? filtro, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza los datos de un cliente verificando la unicidad de CURP y correo (SF-031).
    /// </summary>
    Task<Resultado> ActualizarClienteAsync(Guid id, ActualizarClienteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina a un cliente del padrón si no posee contratos activos asociados (SF-031).
    /// </summary>
    Task<Resultado> EliminarClienteAsync(Guid id, CancellationToken cancellationToken = default);
}
