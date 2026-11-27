using FluentAssertions;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Pruebas unitarias para la entidad de dominio Venta y sus transiciones de estado (SF-057).
/// </summary>
[Trait("Ticket", "SF-057")]
[Trait("Capa", "Core")]
[Trait("Tipo", "Unit")]
public class VentaTests
{
    private const string FolioValido = "CTR-2026-0001";
    private static readonly Guid ClienteIdValido = Guid.NewGuid();
    private static readonly Guid LoteIdValido = Guid.NewGuid();
    private const string VendedorIdValido = "vendedor-user-001";
    private static readonly Dinero PrecioValido = new(120000m);
    private static readonly DateTimeOffset FechaValida = new(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CrearContado_ConDatosValidos_DebeNacerEnEstadoLiquidadoYModalidadContado()
    {
        var venta = Venta.CrearContado("  ctr-2026-0001  ", ClienteIdValido, LoteIdValido, "  vendedor-user-001  ", PrecioValido, FechaValida);

        venta.Should().NotBeNull();
        venta.Id.Should().NotBeEmpty();
        venta.FolioContrato.Should().Be(FolioValido);
        venta.ClienteId.Should().Be(ClienteIdValido);
        venta.LoteId.Should().Be(LoteIdValido);
        venta.VendedorId.Should().Be(VendedorIdValido);
        venta.Modalidad.Should().Be(ModalidadVenta.Contado);
        venta.Estado.Should().Be(EstadoVenta.Liquidado);
        venta.PrecioPactado.Should().Be(PrecioValido);
        venta.FechaVenta.Should().Be(FechaValida);
        venta.MotivoCancelacion.Should().BeNull();
    }

    [Fact]
    public void CrearFinanciada_ConDatosValidos_DebeNacerEnEstadoActivoYModalidadFinanciado()
    {
        var venta = Venta.CrearFinanciada("CTR-2025-0189", ClienteIdValido, LoteIdValido, VendedorIdValido, new Dinero(150000m), FechaValida);

        venta.Id.Should().NotBeEmpty();
        venta.FolioContrato.Should().Be("CTR-2025-0189");
        venta.Modalidad.Should().Be(ModalidadVenta.Financiado);
        venta.Estado.Should().Be(EstadoVenta.Activo);
        venta.PrecioPactado.Should().Be(new Dinero(150000m));
        venta.MotivoCancelacion.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("CTR-2026-001")]
    [InlineData("CTR-26-0001")]
    [InlineData("FAC-2026-0001")]
    [InlineData("CTR-2026-0000001")]
    [InlineData("CTR_2026_0001")]
    public void CrearContado_ConFolioInvalido_DebeLanzarReglaNegocioException(string? folioInvalido)
    {
        Action accion = () => Venta.CrearContado(folioInvalido!, ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void CrearFinanciada_ConIdentificadoresOPrecioInvalidos_DebeLanzarReglaNegocioException()
    {
        Action sinCliente = () => Venta.CrearFinanciada(FolioValido, Guid.Empty, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida);
        Action sinLote = () => Venta.CrearFinanciada(FolioValido, ClienteIdValido, Guid.Empty, VendedorIdValido, PrecioValido, FechaValida);
        Action sinVendedor = () => Venta.CrearFinanciada(FolioValido, ClienteIdValido, LoteIdValido, "   ", PrecioValido, FechaValida);
        Action precioCero = () => Venta.CrearFinanciada(FolioValido, ClienteIdValido, LoteIdValido, VendedorIdValido, Dinero.Zero, FechaValida);

        sinCliente.Should().Throw<ReglaNegocioException>();
        sinLote.Should().Throw<ReglaNegocioException>();
        sinVendedor.Should().Throw<ReglaNegocioException>();
        precioCero.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void CambiarEstadoAdmin_AEnRevisionYCanceladoConMotivo_DebeActualizarEstadoYMotivo()
    {
        var venta = Venta.CrearFinanciada(FolioValido, ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida);

        venta.CambiarEstadoAdmin(EstadoVenta.EnRevision, "  Revisión documental  ");
        venta.Estado.Should().Be(EstadoVenta.EnRevision);
        venta.MotivoCancelacion.Should().Be("Revisión documental");

        venta.CambiarEstadoAdmin(EstadoVenta.Cancelado, "  Solicitud del titular  ");
        venta.Estado.Should().Be(EstadoVenta.Cancelado);
        venta.MotivoCancelacion.Should().Be("Solicitud del titular");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CambiarEstadoAdmin_ACanceladoOEnRevisionSinMotivo_DebeLanzarReglaNegocioException(string? motivoInvalido)
    {
        var venta = Venta.CrearFinanciada(FolioValido, ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida);

        Action aCancelado = () => venta.CambiarEstadoAdmin(EstadoVenta.Cancelado, motivoInvalido!);
        Action aRevision = () => venta.CambiarEstadoAdmin(EstadoVenta.EnRevision, motivoInvalido!);

        aCancelado.Should().Throw<ReglaNegocioException>();
        aRevision.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void CambiarEstadoAdmin_CuandoYaEstaCancelado_DebeLanzarReglaNegocioException()
    {
        var venta = Venta.CrearFinanciada(FolioValido, ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida);
        venta.CambiarEstadoAdmin(EstadoVenta.Cancelado, "Cancelación inicial");

        Action reCancelacion = () => venta.CambiarEstadoAdmin(EstadoVenta.Cancelado, "Segundo intento");
        Action reactivacion = () => venta.CambiarEstadoAdmin(EstadoVenta.Activo, "Reactivar");

        reCancelacion.Should().Throw<ReglaNegocioException>();
        reactivacion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void MarcarLiquidado_DesdeActivoOEnRevision_DebeCambiarEstadoALiquidado()
    {
        var ventaActiva = Venta.CrearFinanciada(FolioValido, ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida);
        ventaActiva.MarcarLiquidado();
        ventaActiva.Estado.Should().Be(EstadoVenta.Liquidado);

        var ventaRevision = Venta.CrearFinanciada("CTR-2026-0002", ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida);
        ventaRevision.CambiarEstadoAdmin(EstadoVenta.EnRevision, "Validación de abono final");
        ventaRevision.MarcarLiquidado();
        ventaRevision.Estado.Should().Be(EstadoVenta.Liquidado);
    }

    [Fact]
    public void MarcarLiquidado_CuandoYaEstaLiquidadoOCancelado_DebeLanzarReglaNegocioException()
    {
        var ventaContado = Venta.CrearContado(FolioValido, ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida);
        Action yaLiquidada = () => ventaContado.MarcarLiquidado();
        yaLiquidada.Should().Throw<ReglaNegocioException>();

        var ventaCancelada = Venta.CrearFinanciada("CTR-2026-0003", ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida);
        ventaCancelada.CambiarEstadoAdmin(EstadoVenta.Cancelado, "Anulada");
        Action cancelada = () => ventaCancelada.MarcarLiquidado();
        cancelada.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void CrearContado_ConFirmaClienteYVendedorNombre_DebePersistirFirmaYFechaFirma()
    {
        const string firmaPng = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

        var venta = Venta.CrearContado(
            FolioValido,
            ClienteIdValido,
            LoteIdValido,
            VendedorIdValido,
            PrecioValido,
            FechaValida,
            firmaClienteBase64: $"  {firmaPng}  ",
            vendedorNombre: "  Ana María López  ");

        venta.FirmaClienteBase64.Should().Be(firmaPng);
        venta.VendedorNombre.Should().Be("Ana María López");
        venta.FechaFirma.Should().Be(FechaValida);
    }

    [Fact]
    public void CrearFinanciada_ConFirmaInvalidaOExcedida_DebeLanzarReglaNegocioException()
    {
        Action formatoInvalido = () => Venta.CrearFinanciada(
            FolioValido, ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida,
            firmaClienteBase64: "https://evil.example/firma.png");

        string firmaGigante = "data:image/png;base64," + new string('A', 300_050);
        Action tamanoExcedido = () => Venta.CrearFinanciada(
            FolioValido, ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioValido, FechaValida,
            firmaClienteBase64: firmaGigante);

        formatoInvalido.Should().Throw<ReglaNegocioException>();
        tamanoExcedido.Should().Throw<ReglaNegocioException>();
    }
}
