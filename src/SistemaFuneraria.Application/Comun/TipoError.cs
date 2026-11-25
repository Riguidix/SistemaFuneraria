namespace SistemaFuneraria.Application.Comun;

/// <summary>
/// Categoría semántica del error de negocio o aplicación para mapear respuestas HTTP sin depender de prefijos en cadenas.
/// </summary>
public enum TipoError
{
    Ninguno = 0,
    Validacion = 1,
    NoEncontrado = 2,
    Conflicto = 3,
    NoAutorizado = 4,
    Prohibido = 5
}
