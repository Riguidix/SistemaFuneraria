namespace SistemaFuneraria.Core.Excepciones;

/// <summary>
/// Excepción de dominio lanzada cuando una operación viola una regla o invariante de negocio.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) {}

    public ReglaNegocioException(string mensaje, Exception innerException) : base(mensaje, innerException) {}
}
