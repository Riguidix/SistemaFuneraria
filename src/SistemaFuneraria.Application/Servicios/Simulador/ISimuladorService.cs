using SistemaFuneraria.Application.Comun;

namespace SistemaFuneraria.Application.Servicios.Simulador;

/// <summary>
/// Contrato del servicio de aplicación para simular esquemas de financiamiento funerario.
/// </summary>
public interface ISimuladorService
{
    /// <summary>
    /// Ejecuta la simulación matemática de financiamiento y genera la tabla de amortización preliminar.
    /// </summary>
    /// <param name="request">Datos requeridos de precio, enganche y plazo.</param>
    /// <returns>Resultado exitoso con los DTOs calculados o resultado fallido con el detalle de errores de validación.</returns>
    Resultado<SimulacionFinanciamientoDto> Simular(SimularFinanciamientoRequest request);
}
