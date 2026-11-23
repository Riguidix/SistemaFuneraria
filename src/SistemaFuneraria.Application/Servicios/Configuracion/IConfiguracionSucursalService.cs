using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Configuracion;

namespace SistemaFuneraria.Application.Servicios.Configuracion;

/// <summary>
/// Servicio de configuración operativa de sucursal (SF-091).
/// </summary>
public interface IConfiguracionSucursalService
{
    Task<Resultado<ConfiguracionSucursalDto>> ObtenerSucursalAsync(CancellationToken cancellationToken = default);

    Task<Resultado<ConfiguracionSucursalDto>> ActualizarSucursalAsync(
        ActualizarConfiguracionSucursalRequest request,
        CancellationToken cancellationToken = default);
}
