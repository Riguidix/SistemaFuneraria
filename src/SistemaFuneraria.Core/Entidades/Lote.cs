using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Entidad de dominio Lote que representa un producto del catálogo inmutable funerario (RN-01, SF-022).
/// Cumple estrictamente con la especificación del ERD v1.1 (Sección 3.1).
/// </summary>
public class Lote
{
    private static readonly HashSet<string> IdsValidos = ["LOT-02", "LOT-03", "LOT-04"];
    private static readonly HashSet<int> GavetasValidas = [2, 3, 4];

    public string Id { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int Gavetas { get; private set; }
    public decimal PrecioContado { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core para la materialización de entidades.
    /// </summary>
    protected Lote() { }

    private Lote(string id, string nombre, int gavetas, decimal precioContado, bool isActive)
    {
        Id = id;
        Nombre = nombre;
        Gavetas = gavetas;
        PrecioContado = precioContado;
        IsActive = isActive;
    }

    /// <summary>
    /// Factory Method canónico para la instanciación de un lote con validación de invariantes de negocio.
    /// </summary>
    public static Lote Crear(string id, string nombre, int gavetas, decimal precioContado, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(id) || !IdsValidos.Contains(id.Trim().ToUpperInvariant()))
        {
            throw new ReglaNegocioException(
                $"El identificador de lote '{id}' no es válido. Los identificadores autorizados son exclusivamente LOT-02, LOT-03 y LOT-04 (RN-01).");
        }

        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 50)
        {
            throw new ReglaNegocioException(
                "El nombre del lote no puede ser vacío ni exceder los 50 caracteres.");
        }

        if (!GavetasValidas.Contains(gavetas))
        {
            throw new ReglaNegocioException(
                $"La capacidad de {gavetas} gavetas no está autorizada. Los lotes deben contar con 2, 3 o 4 gavetas.");
        }

        if (precioContado <= 0 || !ParametrosNegocio.EsPrecioAutorizado(precioContado))
        {
            throw new ReglaNegocioException(
                $"El precio de contado ${precioContado:N2} no está autorizado. Los precios oficiales son exclusivamente $120,000.00, $150,000.00 y $180,000.00 (RN-01).");
        }

        return new Lote(id.Trim().ToUpperInvariant(), nombre.Trim(), gavetas, precioContado, isActive);
    }

    public void Desactivar() => IsActive = false;
    public void Activar() => IsActive = true;
}
