using System.Text.RegularExpressions;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Core.Entidades;

/// <summary>
/// Entidad de dominio Venta que encapsula el contrato comercial, su modalidad, estado y auditoría (SF-057).
/// </summary>
public class Venta
{
    private static readonly Regex RegexFolioContrato = new(@"^CTR-\d{4}-\d{4,6}$", RegexOptions.Compiled);

    public Guid Id { get; private set; }
    public string FolioContrato { get; private set; } = string.Empty;
    public Guid ClienteId { get; private set; }
    public Guid LoteId { get; private set; }
    public string VendedorId { get; private set; } = string.Empty;
    public ModalidadVenta Modalidad { get; private set; }
    public Dinero PrecioPactado { get; private set; }
    public EstadoVenta Estado { get; private set; }
    public DateTimeOffset FechaVenta { get; private set; }
    public string? MotivoCancelacion { get; private set; }
    public string? FirmaClienteBase64 { get; private set; }
    public string? VendedorNombre { get; private set; }
    public DateTimeOffset? FechaFirma { get; private set; }

    private const int LongitudMaximaFirmaBase64 = 300_000;
    private const int LongitudMaximaVendedorNombre = 150;

    /// <summary>
    /// Constructor protegido sin parámetros requerido por EF Core para la materialización de la entidad.
    /// </summary>
    protected Venta() { }

    private Venta(
        Guid id,
        string folioContrato,
        Guid clienteId,
        Guid loteId,
        string vendedorId,
        ModalidadVenta modalidad,
        Dinero precioPactado,
        EstadoVenta estado,
        DateTimeOffset fechaVenta,
        string? firmaClienteBase64,
        string? vendedorNombre,
        DateTimeOffset? fechaFirma)
    {
        Id = id;
        FolioContrato = folioContrato;
        ClienteId = clienteId;
        LoteId = loteId;
        VendedorId = vendedorId;
        Modalidad = modalidad;
        PrecioPactado = precioPactado;
        Estado = estado;
        FechaVenta = fechaVenta;
        FirmaClienteBase64 = firmaClienteBase64;
        VendedorNombre = vendedorNombre;
        FechaFirma = fechaFirma;
    }

    /// <summary>
    /// Crea una venta en modalidad de Contado que nace directamente en estado Liquidado.
    /// </summary>
    public static Venta CrearContado(
        string folioContrato,
        Guid clienteId,
        Guid loteId,
        string vendedorId,
        Dinero precioPactado,
        DateTimeOffset fechaVenta,
        string? firmaClienteBase64 = null,
        string? vendedorNombre = null) =>
        CrearInterno(folioContrato, clienteId, loteId, vendedorId, ModalidadVenta.Contado, precioPactado, EstadoVenta.Liquidado, fechaVenta, firmaClienteBase64, vendedorNombre);

    /// <summary>
    /// Crea una venta en modalidad Financiado que nace en estado Activo.
    /// </summary>
    public static Venta CrearFinanciada(
        string folioContrato,
        Guid clienteId,
        Guid loteId,
        string vendedorId,
        Dinero precioPactado,
        DateTimeOffset fechaVenta,
        string? firmaClienteBase64 = null,
        string? vendedorNombre = null) =>
        CrearInterno(folioContrato, clienteId, loteId, vendedorId, ModalidadVenta.Financiado, precioPactado, EstadoVenta.Activo, fechaVenta, firmaClienteBase64, vendedorNombre);

    /// <summary>
    /// Cambia administrativamente el estado del contrato a EnRevision, Cancelado o Activo registrando el motivo obligatorio.
    /// </summary>
    public void CambiarEstadoAdmin(EstadoVenta nuevoEstado, string motivo)
    {
        if (!Enum.IsDefined(nuevoEstado))
            throw new ReglaNegocioException("El estado de venta indicado no es válido.");

        if (Estado == EstadoVenta.Cancelado)
            throw new ReglaNegocioException("No es posible modificar una venta que ya fue cancelada.");

        if (Estado == EstadoVenta.Liquidado && nuevoEstado != EstadoVenta.Cancelado)
            throw new ReglaNegocioException("Una venta liquidada solo puede transicionar a Cancelado mediante autorización administrativa.");

        if (nuevoEstado == EstadoVenta.Liquidado)
            throw new ReglaNegocioException("Para liquidar un contrato utilice el método de dominio MarcarLiquidado.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ReglaNegocioException("El motivo administrativo es obligatorio para cambiar el estado de la venta.");

        Estado = nuevoEstado;
        MotivoCancelacion = motivo.Trim();
    }

    /// <summary>
    /// Marca como Liquidado un contrato que se encuentra Activo o EnRevision.
    /// </summary>
    public void MarcarLiquidado()
    {
        if (Estado is not (EstadoVenta.Activo or EstadoVenta.EnRevision))
            throw new ReglaNegocioException("Solo es posible liquidar una venta en estado Activo o EnRevision.");

        Estado = EstadoVenta.Liquidado;
    }

    private static Venta CrearInterno(
        string folioContrato,
        Guid clienteId,
        Guid loteId,
        string vendedorId,
        ModalidadVenta modalidad,
        Dinero precioPactado,
        EstadoVenta estadoInicial,
        DateTimeOffset fechaVenta,
        string? firmaClienteBase64,
        string? vendedorNombre)
    {
        if (string.IsNullOrWhiteSpace(folioContrato))
            throw new ReglaNegocioException("El folio del contrato es obligatorio.");

        string folioNormalizado = folioContrato.Trim().ToUpperInvariant();
        if (!RegexFolioContrato.IsMatch(folioNormalizado))
            throw new ReglaNegocioException($"El folio '{folioContrato}' no cumple el formato CTR-YYYY-XXXX.");

        if (clienteId == Guid.Empty)
            throw new ReglaNegocioException("El identificador del cliente es obligatorio.");

        if (loteId == Guid.Empty)
            throw new ReglaNegocioException("El identificador del lote es obligatorio.");

        if (string.IsNullOrWhiteSpace(vendedorId))
            throw new ReglaNegocioException("El identificador del vendedor es obligatorio.");

        if (precioPactado.Monto <= 0m)
            throw new ReglaNegocioException("El precio pactado debe ser mayor a cero.");

        string? firmaNormalizada = null;
        DateTimeOffset? fechaFirma = null;
        if (!string.IsNullOrWhiteSpace(firmaClienteBase64))
        {
            firmaNormalizada = firmaClienteBase64.Trim();
            bool esImagenValida =
                firmaNormalizada.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase) ||
                firmaNormalizada.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase) ||
                firmaNormalizada.StartsWith("data:image/webp;base64,", StringComparison.OrdinalIgnoreCase);

            if (!esImagenValida)
                throw new ReglaNegocioException("La firma digital del cliente debe ser una imagen válida en formato PNG, JPEG o WEBP.");

            if (firmaNormalizada.Length > LongitudMaximaFirmaBase64)
                throw new ReglaNegocioException("La firma digital del cliente excede el tamaño máximo permitido.");

            fechaFirma = fechaVenta;
        }

        string? nombreVendedorNormalizado = string.IsNullOrWhiteSpace(vendedorNombre)
            ? null
            : vendedorNombre.Trim();
        if (nombreVendedorNormalizado is not null && nombreVendedorNormalizado.Length > LongitudMaximaVendedorNombre)
        {
            nombreVendedorNormalizado = nombreVendedorNormalizado[..LongitudMaximaVendedorNombre];
        }

        return new Venta(
            Guid.NewGuid(),
            folioNormalizado,
            clienteId,
            loteId,
            vendedorId.Trim(),
            modalidad,
            precioPactado,
            estadoInicial,
            fechaVenta,
            firmaNormalizada,
            nombreVendedorNormalizado,
            fechaFirma);
    }
}
