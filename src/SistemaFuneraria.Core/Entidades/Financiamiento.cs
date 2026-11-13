using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Agregado que gobierna el crédito otorgado, amortización y saldo deudor (SF-030, ERD 3.4).
/// </summary>
public class Financiamiento
{
    private readonly List<Mensualidad> _mensualidades = [];

    public Guid Id { get; private set; }
    public Guid VentaId { get; private set; }
    public decimal Enganche { get; private set; }
    public decimal CapitalFinanciado { get; private set; }
    public int PlazoMeses { get; private set; }
    public decimal TasaAnual { get; private set; }
    public decimal SaldoInsolutoActual { get; private set; }
    public EstadoFinanciamiento Estado { get; private set; }

    /// <summary>
    /// Calendario inmutable de cuotas periódicas de amortización pertenecientes a este crédito.
    /// </summary>
    public IReadOnlyCollection<Mensualidad> Mensualidades => _mensualidades.AsReadOnly();

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core para materialización.
    /// </summary>
    protected Financiamiento() { }

    private Financiamiento(
        Guid id,
        Guid ventaId,
        decimal enganche,
        decimal capitalFinanciado,
        int plazoMeses,
        decimal tasaAnual,
        decimal saldoInsolutoActual,
        EstadoFinanciamiento estado)
    {
        Id = id;
        VentaId = ventaId;
        Enganche = enganche;
        CapitalFinanciado = capitalFinanciado;
        PlazoMeses = plazoMeses;
        TasaAnual = tasaAnual;
        SaldoInsolutoActual = saldoInsolutoActual;
        Estado = estado;
    }

    /// <summary>
    /// Factory Method canónico para crear un financiamiento con validación estricta de invariantes y cuotas completas.
    /// </summary>
    public static Financiamiento Crear(
        decimal enganche,
        decimal capitalFinanciado,
        int plazoMeses,
        decimal tasaAnual,
        IEnumerable<Mensualidad> cuotas)
    {
        if (!ParametrosNegocio.EsPlazoAutorizado(plazoMeses))
        {
            throw new ReglaNegocioException($"El plazo de {plazoMeses} meses no está autorizado (RN-05). Los plazos permitidos son exclusivamente 12, 18, 24 y 48 meses.");
        }

        if (enganche <= 0m)
        {
            throw new ReglaNegocioException("El enganche del financiamiento debe ser estrictamente mayor a cero (RN-04).");
        }

        if (capitalFinanciado <= 0m)
        {
            throw new ReglaNegocioException("El capital financiado debe ser estrictamente mayor a cero.");
        }

        if (tasaAnual <= 0m)
        {
            throw new ReglaNegocioException("La tasa anual de interés debe ser mayor a cero.");
        }

        if (cuotas == null)
        {
            throw new ReglaNegocioException("La colección de cuotas de amortización no puede ser nula.");
        }

        var listaCuotas = cuotas.ToList();
        if (listaCuotas.Count != plazoMeses)
        {
            throw new ReglaNegocioException($"El número de cuotas suministradas ({listaCuotas.Count}) debe ser exactamente igual al plazo acordado ({plazoMeses} meses).");
        }

        var financiamiento = new Financiamiento(
            Guid.NewGuid(),
            Guid.Empty,
            enganche,
            capitalFinanciado,
            plazoMeses,
            tasaAnual,
            capitalFinanciado,
            EstadoFinanciamiento.Activo);

        foreach (var cuota in listaCuotas)
        {
            cuota.AsignarFinanciamientoId(financiamiento.Id);
            financiamiento._mensualidades.Add(cuota);
        }

        return financiamiento;
    }

    internal void AsignarVentaId(Guid ventaId)
    {
        VentaId = ventaId;
    }
}
