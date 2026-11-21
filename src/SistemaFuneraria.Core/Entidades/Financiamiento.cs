using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Entidad raíz Financiamiento que gobierna el crédito, saldo insoluto y calendario de mensualidades (SF-058).
/// </summary>
public class Financiamiento
{
    private readonly List<Mensualidad> _mensualidades = [];

    public Guid Id { get; private set; }
    public Guid VentaId { get; private set; }
    public decimal PorcentajeEnganche { get; private set; }
    public Dinero MontoEnganche { get; private set; }
    public Dinero CapitalFinanciado { get; private set; }
    public int PlazoMeses { get; private set; }
    public decimal TasaInteresAnual { get; private set; }
    public Dinero CuotaMensualActual { get; private set; }
    public Dinero SaldoInsoluto { get; private set; }
    public EstadoFinanciamiento Estado { get; private set; }
    public uint VersionFila { get; private set; }
    public IReadOnlyCollection<Mensualidad> Mensualidades => _mensualidades.AsReadOnly();

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core.
    /// </summary>
    protected Financiamiento() { }

    private Financiamiento(Guid id, Guid ventaId, decimal porcentajeEnganche, Dinero montoEnganche, Dinero capitalFinanciado, int plazoMeses, decimal tasaInteresAnual, Dinero cuotaMensualActual, IEnumerable<Mensualidad> mensualidades)
    {
        Id = id;
        VentaId = ventaId;
        PorcentajeEnganche = porcentajeEnganche;
        MontoEnganche = montoEnganche;
        CapitalFinanciado = capitalFinanciado;
        PlazoMeses = plazoMeses;
        TasaInteresAnual = tasaInteresAnual;
        CuotaMensualActual = cuotaMensualActual;
        SaldoInsoluto = capitalFinanciado;
        Estado = EstadoFinanciamiento.Activo;
        _mensualidades.AddRange(mensualidades);
    }

    /// <summary>
    /// Crea un financiamiento y genera sus N mensualidades validando parámetros de negocio y cuadre de capital.
    /// </summary>
    public static Financiamiento Crear(Guid ventaId, decimal porcentajeEnganche, Dinero montoEnganche, Dinero capitalFinanciado, int plazoMeses, decimal tasaAnual, IReadOnlyList<RenglonAmortizacion> filasAmortizacion, DateOnly fechaPrimerPago)
    {
        if (ventaId == Guid.Empty)
            throw new ReglaNegocioException("El identificador de la venta es obligatorio.");

        if (porcentajeEnganche != Math.Truncate(porcentajeEnganche) || !ParametrosNegocio.EsEngancheAutorizado((int)porcentajeEnganche))
            throw new ReglaNegocioException("El porcentaje de enganche debe ser 10%, 20% o 30%.");

        if (montoEnganche.Monto <= 0m)
            throw new ReglaNegocioException("El monto de enganche debe ser mayor a cero.");

        if (capitalFinanciado.Monto <= 0m)
            throw new ReglaNegocioException("El capital financiado debe ser mayor a cero.");

        if (!ParametrosNegocio.EsPlazoAutorizado(plazoMeses))
            throw new ReglaNegocioException("El plazo autorizado debe ser 12, 18, 24 o 48 meses.");

        if (tasaAnual != ParametrosNegocio.ObtenerTasaAnual(plazoMeses))
            throw new ReglaNegocioException("La tasa anual no corresponde al plazo autorizado.");

        if (filasAmortizacion is null || filasAmortizacion.Count != plazoMeses)
            throw new ReglaNegocioException("La tabla de amortización debe contener exactamente tantas filas como el plazo en meses.");

        if (filasAmortizacion.Sum(f => f.CapitalAmortizado) != capitalFinanciado.Monto)
            throw new ReglaNegocioException("La suma del capital programado debe cuadrar exactamente con el capital financiado.");

        var cuotaMensualActual = new Dinero(filasAmortizacion[0].PagoMensual);
        if (cuotaMensualActual.Monto <= 0m)
            throw new ReglaNegocioException("La cuota mensual debe ser mayor a cero.");

        var financiamientoId = Guid.NewGuid();
        var mensualidades = filasAmortizacion
            .OrderBy(f => f.Periodo)
            .Select((fila, indice) =>
            {
                if (fila.Periodo != indice + 1)
                    throw new ReglaNegocioException("Los periodos de amortización deben ser consecutivos desde 1.");

                return Mensualidad.Crear(financiamientoId, fila, fechaPrimerPago.AddMonths(indice));
            })
            .ToList();

        return new Financiamiento(financiamientoId, ventaId, porcentajeEnganche, montoEnganche, capitalFinanciado, plazoMeses, tasaAnual, cuotaMensualActual, mensualidades);
    }

    /// <summary>
    /// Aplica el pago de la mensualidad exigible más antigua respetando el orden secuencial de cobro.
    /// </summary>
    public void AplicarPagoCuota(int numeroCuota, DateTimeOffset fechaPago)
    {
        if (Estado is not (EstadoFinanciamiento.Activo or EstadoFinanciamiento.EnMora))
            throw new ReglaNegocioException("Solo es posible aplicar pagos a un financiamiento Activo o EnMora.");

        var siguienteExigible = _mensualidades
            .Where(m => m.Estado is EstadoMensualidad.Pendiente or EstadoMensualidad.Vencida)
            .OrderBy(m => m.NumeroCuota)
            .FirstOrDefault()
            ?? throw new ReglaNegocioException("El financiamiento no tiene mensualidades pendientes por cobrar.");

        if (siguienteExigible.NumeroCuota != numeroCuota)
            throw new ReglaNegocioException($"Debe cubrir primero la mensualidad {siguienteExigible.NumeroCuota} antes de pagar la cuota {numeroCuota}.");

        siguienteExigible.MarcarPagada(fechaPago);

        decimal nuevoSaldo = SaldoInsoluto.Monto - siguienteExigible.CapitalProgramado.Monto;
        SaldoInsoluto = new Dinero(Math.Max(0m, nuevoSaldo));

        bool quedanPendientes = _mensualidades.Any(m => m.Estado is EstadoMensualidad.Pendiente or EstadoMensualidad.Vencida);
        if (SaldoInsoluto.Monto == 0m || !quedanPendientes)
        {
            SaldoInsoluto = Dinero.Zero;
            Estado = EstadoFinanciamiento.Liquidado;
        }
    }
}
