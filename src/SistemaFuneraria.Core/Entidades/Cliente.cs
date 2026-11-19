using System.Text.RegularExpressions;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Entidad de dominio Cliente que encapsula los datos del comprador y sus invariantes de negocio (SF-015, SF-031).
/// Cumple con las especificaciones del ERD v1.1 (Sección 3.2).
/// </summary>
public class Cliente
{
    private static readonly Regex RegexCurp = new(@"^[A-Z0-9]{18}$", RegexOptions.Compiled);
    private static readonly Regex RegexTelefono = new(@"^[0-9]{10}$", RegexOptions.Compiled);
    private static readonly Regex RegexCorreo = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public Guid Id { get; private set; }
    public string Curp { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Apellidos { get; private set; } = string.Empty;
    public string Telefono { get; private set; } = string.Empty;
    public string Direccion { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core para la materialización de entidades.
    /// </summary>
    protected Cliente() { }

    private Cliente(
        Guid id,
        string nombre,
        string apellidos,
        string curp,
        string telefono,
        string correo,
        string direccion,
        DateTimeOffset createdAt)
    {
        Id = id;
        Nombre = nombre;
        Apellidos = apellidos;
        Curp = curp;
        Telefono = telefono;
        Correo = correo;
        Direccion = direccion;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Factory Method canónico para la creación controlada de instancias de Cliente con validación de invariantes.
    /// </summary>
    public static Cliente Crear(
        string nombre,
        string apellidos,
        string curp,
        string telefono,
        string correo,
        string direccion)
    {
        var (nombreNorm, apellidosNorm, curpNorm, telefonoNorm, correoNorm) =
            NormalizarYValidarDatosBasicos(nombre, apellidos, curp, telefono, correo);

        if (string.IsNullOrWhiteSpace(direccion))
        {
            throw new ReglaNegocioException("La dirección del cliente es obligatoria.");
        }

        return new Cliente(
            Guid.NewGuid(),
            nombreNorm,
            apellidosNorm,
            curpNorm,
            telefonoNorm,
            correoNorm,
            direccion.Trim(),
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Actualiza los datos editables del cliente aplicando las mismas invariantes de dominio (SF-031).
    /// </summary>
    public void Actualizar(
        string nombre,
        string apellidos,
        string curp,
        string telefono,
        string correo,
        string? direccion = null)
    {
        var (nombreNorm, apellidosNorm, curpNorm, telefonoNorm, correoNorm) =
            NormalizarYValidarDatosBasicos(nombre, apellidos, curp, telefono, correo);

        string direccionNormalizada = string.IsNullOrWhiteSpace(direccion)
            ? (string.IsNullOrWhiteSpace(Direccion) ? "Sin dirección registrada" : Direccion)
            : direccion.Trim();

        Nombre = nombreNorm;
        Apellidos = apellidosNorm;
        Curp = curpNorm;
        Telefono = telefonoNorm;
        Correo = correoNorm;
        Direccion = direccionNormalizada;
    }

    /// <summary>
    /// Calcula las iniciales del cliente a partir de su nombre y apellidos (ej. "LC", "RM").
    /// </summary>
    public string ObtenerIniciales()
    {
        char c1 = !string.IsNullOrWhiteSpace(Nombre) ? char.ToUpperInvariant(Nombre.Trim()[0]) : 'C';
        char c2 = !string.IsNullOrWhiteSpace(Apellidos) ? char.ToUpperInvariant(Apellidos.Trim()[0]) : 'L';
        return $"{c1}{c2}";
    }

    private static (string Nombre, string Apellidos, string Curp, string Telefono, string Correo)
        NormalizarYValidarDatosBasicos(string nombre, string apellidos, string curp, string telefono, string correo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaNegocioException("El nombre del cliente es obligatorio.");
        string nombreNorm = nombre.Trim();
        if (nombreNorm.Length > 100)
            throw new ReglaNegocioException("El nombre del cliente no puede exceder los 100 caracteres.");

        if (string.IsNullOrWhiteSpace(apellidos))
            throw new ReglaNegocioException("Los apellidos del cliente son obligatorios.");
        string apellidosNorm = apellidos.Trim();
        if (apellidosNorm.Length > 100)
            throw new ReglaNegocioException("Los apellidos del cliente no pueden exceder los 100 caracteres.");

        if (string.IsNullOrWhiteSpace(curp))
            throw new ReglaNegocioException("El CURP del cliente es obligatorio.");
        string curpNorm = curp.Trim().ToUpperInvariant();
        if (!RegexCurp.IsMatch(curpNorm))
            throw new ReglaNegocioException($"El CURP '{curp}' es inválido. Debe contener exactamente 18 caracteres alfanuméricos.");

        if (string.IsNullOrWhiteSpace(telefono))
            throw new ReglaNegocioException("El teléfono del cliente es obligatorio.");
        string telefonoNorm = telefono.Trim();
        if (!RegexTelefono.IsMatch(telefonoNorm))
            throw new ReglaNegocioException($"El teléfono '{telefono}' es inválido. Debe contener exactamente 10 dígitos numéricos.");

        if (string.IsNullOrWhiteSpace(correo))
            throw new ReglaNegocioException("El correo electrónico del cliente es obligatorio.");
        string correoNorm = correo.Trim().ToLowerInvariant();
        if (correoNorm.Length > 255)
            throw new ReglaNegocioException("El correo electrónico no puede exceder los 255 caracteres.");
        if (!RegexCorreo.IsMatch(correoNorm))
            throw new ReglaNegocioException($"El correo electrónico '{correo}' tiene un formato inválido.");

        return (nombreNorm, apellidosNorm, curpNorm, telefonoNorm, correoNorm);
    }
}
