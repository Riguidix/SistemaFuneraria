using System.Text.RegularExpressions;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Entidad de dominio Lote que representa una unidad física del inventario memorial y sus transiciones de estado.
/// </summary>
public class Lote
{
    private static readonly Regex RegexClave = new(@"^[A-Z]{2,3}-[A-Z]-\d{3}$", RegexOptions.Compiled);

    public Guid Id { get; private set; }
    public string Clave { get; private set; } = string.Empty;
    public string Seccion { get; private set; } = string.Empty;
    public string AndadorFila { get; private set; } = string.Empty;
    public int Gavetas { get; private set; }
    public Dinero PrecioContado { get; private set; }
    public EstadoLote Estado { get; private set; }

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core para la materialización de la entidad.
    /// </summary>
    protected Lote() { }

    private Lote(Guid id, string clave, string seccion, string andadorFila, int gavetas, Dinero precioContado, EstadoLote estado)
    {
        Id = id;
        Clave = clave;
        Seccion = seccion;
        AndadorFila = andadorFila;
        Gavetas = gavetas;
        PrecioContado = precioContado;
        Estado = estado;
    }

    /// <summary>
    /// Devuelve el precio oficial de contado según la capacidad en gavetas (RN-01).
    /// </summary>
    public static Dinero ObtenerPrecioOficial(int gavetas) => gavetas switch
    {
        2 => new Dinero(120000m),
        3 => new Dinero(150000m),
        4 => new Dinero(180000m),
        _ => throw new ReglaNegocioException("La capacidad permitida es de 2, 3 o 4 gavetas.")
    };

    /// <summary>
    /// Crea una nueva instancia de Lote en estado Disponible validando clave, ubicación, gavetas y precio oficial RN-01.
    /// </summary>
    public static Lote Crear(string clave, string seccion, string andadorFila, int gavetas, Dinero precioContado)
    {
        var claveNormalizada = ValidarClave(clave);
        var seccionNormalizada = ValidarTextoObligatorio(seccion, "La sección del lote es obligatoria.");
        var andadorNormalizado = ValidarTextoObligatorio(andadorFila, "El andador o fila del lote es obligatorio.");
        
        ValidarGavetas(gavetas);
        ValidarPrecio(gavetas, precioContado);

        return new Lote(Guid.NewGuid(), claveNormalizada, seccionNormalizada, andadorNormalizado, gavetas, precioContado, EstadoLote.Disponible);
    }

    /// <summary>
    /// Cambia el estado del lote de Disponible a Reservado.
    /// </summary>
    public void Reservar()
    {
        if (Estado != EstadoLote.Disponible)
            throw new ReglaNegocioException("Solo es posible reservar un lote en estado Disponible.");

        Estado = EstadoLote.Reservado;
    }

    /// <summary>
    /// Libera un lote en estado Reservado devolviéndolo a Disponible.
    /// </summary>
    public void Liberar()
    {
        if (Estado != EstadoLote.Reservado)
            throw new ReglaNegocioException("Solo es posible liberar un lote en estado Reservado.");

        Estado = EstadoLote.Disponible;
    }

    /// <summary>
    /// Restablece el lote a estado Disponible tras la cancelación administrativa de su contrato de venta (SF-063).
    /// </summary>
    public void LiberarPorCancelacionVenta()
    {
        if (Estado == EstadoLote.Mantenimiento)
            throw new ReglaNegocioException("No es posible liberar un lote en estado Mantenimiento.");

        Estado = EstadoLote.Disponible;
    }

    /// <summary>
    /// Marca el lote como Vendido cuando se encuentra Disponible o Reservado.
    /// </summary>
    public void MarcarVendido()
    {
        if (Estado == EstadoLote.Vendido)
            throw new ReglaNegocioException("El lote ya se encuentra vendido");

        if (Estado == EstadoLote.Mantenimiento)
            throw new ReglaNegocioException("No es posible vender un lote en estado Mantenimiento.");

        Estado = EstadoLote.Vendido;
    }

    /// <summary>
    /// Actualiza la capacidad (con precio oficial RN-01) y el estado operativo de un lote que no se encuentra Vendido.
    /// La ubicación física (Sección y AndadorFila) permanece inmutable una vez registrado el lote.
    /// </summary>
    public void ActualizarDatosAdmin(string? seccion, string? andadorFila, int gavetas, Dinero precioContado, EstadoLote nuevoEstado)
    {
        if (Estado == EstadoLote.Vendido)
            throw new ReglaNegocioException("No es posible modificar un lote que ya se encuentra vendido.");

        if (!Enum.IsDefined(nuevoEstado))
            throw new ReglaNegocioException("El estado del lote indicado no es válido.");

        ValidarGavetas(gavetas);
        ValidarPrecio(gavetas, precioContado);

        Gavetas = gavetas;
        PrecioContado = precioContado;
        Estado = nuevoEstado;
    }

    private static string ValidarClave(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
            throw new ReglaNegocioException("La clave del lote es obligatoria.");

        string claveNormalizada = clave.Trim().ToUpperInvariant();
        if (!RegexClave.IsMatch(claveNormalizada))
            throw new ReglaNegocioException($"La clave '{clave}' no cumple con el formato requerido (ej. JN-A-042).");

        return claveNormalizada;
    }

    private static string ValidarTextoObligatorio(string valor, string mensajeError)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ReglaNegocioException(mensajeError);

        return valor.Trim();
    }

    private static void ValidarGavetas(int gavetas)
    {
        if (gavetas is not (2 or 3 or 4))
            throw new ReglaNegocioException("La capacidad permitida es de 2, 3 o 4 gavetas.");
    }

    private static void ValidarPrecio(int gavetas, Dinero precioContado)
    {
        if (precioContado.Monto <= 0m)
            throw new ReglaNegocioException("El precio de contado debe ser mayor a cero.");

        var precioOficial = ObtenerPrecioOficial(gavetas);
        if (precioContado.Monto != precioOficial.Monto)
        {
            throw new ReglaNegocioException(
                $"El precio de contado para {gavetas} gavetas debe ser exactamente {precioOficial.Monto:C2} (RN-01).");
        }
    }
}
