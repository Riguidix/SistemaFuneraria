namespace SistemaFuneraria.Application.Comun;

/// <summary>
/// Representa el resultado no genérico de una operación de negocio.
/// Permite comunicar éxito o falla con mensajes y colecciones de errores sin recurrir a excepciones.
/// </summary>
public class Resultado
{
    private static readonly string[] ErroresVacios = Array.Empty<string>();

    protected Resultado(bool esExitoso, IEnumerable<string>? errores = null, TipoError tipoError = TipoError.Ninguno)
    {
        EsExitoso = esExitoso;
        Errores = errores?.ToArray() ?? ErroresVacios;
        TipoError = esExitoso
            ? TipoError.Ninguno
            : ResolverTipoError(tipoError, Errores);
    }

    /// <summary>
    /// Categoría semántica del fallo (o <see cref="TipoError.Ninguno"/> si la operación fue exitosa).
    /// </summary>
    public TipoError TipoError { get; }

    /// <summary>
    /// Indica si la operación culminó exitosamente.
    /// </summary>
    public bool EsExitoso { get; }

    /// <summary>
    /// Indica si la operación resultó en fallo.
    /// </summary>
    public bool EsFallo => !EsExitoso;

    /// <summary>
    /// Mensaje descriptivo del error o errores. Si la operación fue exitosa, retorna cadena vacía.
    /// </summary>
    public string Error => Errores.Count switch
    {
        0 => string.Empty,
        1 => Errores[0],
        _ => string.Join("; ", Errores)
    };

    /// <summary>
    /// Colección de sólo lectura con todos los mensajes de error asociados al fallo.
    /// </summary>
    public IReadOnlyList<string> Errores { get; }

    /// <summary>
    /// Crea una instancia exitosa sin valor de retorno.
    /// </summary>
    public static Resultado Exito() => new(true);

    /// <summary>
    /// Crea una instancia fallida con un único mensaje de error descriptivo.
    /// </summary>
    public static Resultado Fallo(string error, TipoError tipoError = TipoError.Ninguno)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("El mensaje de error no puede ser nulo ni estar vacío.", nameof(error));
        }

        return new(false, new[] { error.Trim() }, tipoError);
    }

    /// <summary>
    /// Crea una instancia fallida a partir de una colección de mensajes de error.
    /// </summary>
    public static Resultado Fallo(IEnumerable<string> errores, TipoError tipoError = TipoError.Ninguno)
    {
        ArgumentNullException.ThrowIfNull(errores);

        var listaFiltrada = NormalizarErrores(errores);
        return new(false, listaFiltrada, tipoError);
    }

    public static Resultado FalloNoEncontrado(string error) => Fallo(error, TipoError.NoEncontrado);
    public static Resultado FalloConflicto(string error) => Fallo(error, TipoError.Conflicto);
    public static Resultado FalloNoAutorizado(string error) => Fallo(error, TipoError.NoAutorizado);
    public static Resultado FalloProhibido(string error) => Fallo(error, TipoError.Prohibido);

    protected static string[] NormalizarErrores(IEnumerable<string> errores)
    {
        var listaFiltrada = errores
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim())
            .ToArray();

        if (listaFiltrada.Length == 0)
        {
            throw new ArgumentException("La colección de errores no puede estar vacía ni contener solo elementos en blanco.", nameof(errores));
        }

        return listaFiltrada;
    }

    private static TipoError ResolverTipoError(TipoError tipoExplicito, IReadOnlyList<string> errores)
    {
        if (tipoExplicito != TipoError.Ninguno)
        {
            return tipoExplicito;
        }

        var primero = errores.Count > 0 ? errores[0] : string.Empty;
        if (primero.StartsWith("[CONFLICTO]", StringComparison.Ordinal))
        {
            return TipoError.Conflicto;
        }

        if (primero.StartsWith("[NO_ENCONTRADO]", StringComparison.Ordinal))
        {
            return TipoError.NoEncontrado;
        }

        return TipoError.Validacion;
    }
}

/// <summary>
/// Representa el resultado tipado de una operación de negocio que produce un valor <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">Tipo del valor transportado en caso de éxito.</typeparam>
public class Resultado<T> : Resultado
{
    private readonly T? _valor;

    protected Resultado(bool esExitoso, T? valor, IEnumerable<string>? errores = null, TipoError tipoError = TipoError.Ninguno)
        : base(esExitoso, errores, tipoError)
    {
        _valor = valor;
    }

    /// <summary>
    /// Obtiene el valor resultante de la operación.
    /// </summary>
    /// <exception cref="InvalidOperationException">Se lanza si la operación es un fallo.</exception>
    public T Valor => EsExitoso
        ? _valor!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    /// <summary>
    /// Crea una instancia exitosa que transporta el valor indicado.
    /// </summary>
    public static Resultado<T> Exito(T valor) => new(true, valor);

    /// <summary>
    /// Crea una instancia fallida tipada con un único mensaje de error.
    /// </summary>
    public static new Resultado<T> Fallo(string error, TipoError tipoError = TipoError.Ninguno)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("El mensaje de error no puede ser nulo ni estar vacío.", nameof(error));
        }

        return new(false, default, new[] { error.Trim() }, tipoError);
    }

    /// <summary>
    /// Crea una instancia fallida tipada a partir de una colección de errores.
    /// </summary>
    public static new Resultado<T> Fallo(IEnumerable<string> errores, TipoError tipoError = TipoError.Ninguno)
    {
        ArgumentNullException.ThrowIfNull(errores);

        var listaFiltrada = NormalizarErrores(errores);
        return new(false, default, listaFiltrada, tipoError);
    }

    public static new Resultado<T> FalloNoEncontrado(string error) => Fallo(error, TipoError.NoEncontrado);
    public static new Resultado<T> FalloConflicto(string error) => Fallo(error, TipoError.Conflicto);
    public static new Resultado<T> FalloNoAutorizado(string error) => Fallo(error, TipoError.NoAutorizado);
    public static new Resultado<T> FalloProhibido(string error) => Fallo(error, TipoError.Prohibido);

    /// <summary>
    /// Operador de conversión implícita para simplificar el retorno directo de instancias de <typeparamref name="T"/>.
    /// </summary>
    public static implicit operator Resultado<T>(T valor) => Exito(valor);
}
