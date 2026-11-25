namespace SistemaFuneraria.Core.Comun;

/// <summary>
/// Utilidades puras de formato y derivación de texto compartidas en el dominio y aplicación.
/// </summary>
public static class TextoHelper
{
    public const string FallbackInicialesDefecto = "US";

    /// <summary>
    /// Obtiene hasta 2 iniciales en mayúsculas a partir de un nombre completo.
    /// </summary>
    public static string ObtenerIniciales(string? nombreCompleto) =>
        ObtenerInicialesConFallback(nombreCompleto, FallbackInicialesDefecto);

    /// <summary>
    /// Obtiene hasta 2 iniciales en mayúsculas combinando nombre y apellidos.
    /// </summary>
    public static string ObtenerIniciales(string? nombre, string? apellidos) =>
        ObtenerInicialesConFallback($"{nombre ?? string.Empty} {apellidos ?? string.Empty}".Trim(), FallbackInicialesDefecto);

    /// <summary>
    /// Obtiene hasta 2 iniciales en mayúsculas combinando nombre y apellidos con un valor de respaldo personalizado.
    /// </summary>
    public static string ObtenerIniciales(string? nombre, string? apellidos, string fallback) =>
        ObtenerInicialesConFallback($"{nombre ?? string.Empty} {apellidos ?? string.Empty}".Trim(), fallback);

    /// <summary>
    /// Obtiene hasta 2 iniciales en mayúsculas a partir de un nombre completo con un valor de respaldo personalizado.
    /// </summary>
    public static string ObtenerInicialesConFallback(string? nombreCompleto, string fallback)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            return fallback;
        }

        var partes = nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (partes.Length >= 2)
        {
            return $"{char.ToUpperInvariant(partes[0][0])}{char.ToUpperInvariant(partes[1][0])}";
        }

        var unica = partes[0];
        if (unica.Length >= 2)
        {
            return unica[..2].ToUpperInvariant();
        }

        return unica.ToUpperInvariant();
    }
}
