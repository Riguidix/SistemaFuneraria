using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.DTOs.Simulador;

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

    /// <summary>
    /// Genera una cotización comercial completa en modalidad Contado o Credito a partir de gavetas, enganche y plazo.
    /// </summary>
    /// <param name="request">Parámetros de lote/gavetas, modalidad, enganche, plazo y fecha de primer pago.</param>
    /// <returns>Resultado tipado con el desglose de cotización o errores de validación.</returns>
    Resultado<SimulacionCotizacionDto> Cotizar(SimularCotizacionRequest request);

    /// <summary>
    /// Consulta el catálogo oficial de porcentajes de enganche y opciones de plazos autorizados (SF-049).
    /// </summary>
    PlanesFinanciamientoDto ObtenerPlanes();
}
