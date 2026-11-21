using FluentAssertions;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Pruebas unitarias para la entidad de dominio Pago, sus 5 TipoPago y 3 MetodoPago (SF-075).
/// </summary>
[Trait("Ticket", "SF-075")]
[Trait("Capa", "Core")]
[Trait("Tipo", "Unit")]
public class PagoTests
{
    private const string FolioValido = "PAG-2026-000001";
    private static readonly Guid VentaIdValida = Guid.NewGuid();
    private const string CajeroIdValido = "cajero-user-001";
    private const string ClaveIdempotenciaValida = "9f1c2d3e-4a5b-4c6d-8e9f-0123456789ab";
    private static readonly DateTimeOffset FechaValida = new(2026, 11, 19, 11, 20, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(TipoPago.Enganche, MetodoPago.Efectivo, null)]
    [InlineData(TipoPago.Contado, MetodoPago.Efectivo, "   ")]
    [InlineData(TipoPago.Mensualidad, MetodoPago.TransferenciaBancaria, "SPEI-994210")]
    [InlineData(TipoPago.AbonoCapital, MetodoPago.Tarjeta, "4829")]
    [InlineData(TipoPago.Liquidacion, MetodoPago.TransferenciaBancaria, "REF-2026-01")]
    public void Crear_ConTodosLosTiposYMetodosValidos_DebeInstanciarPagoYNormalizarCampos(
        TipoPago tipo,
        MetodoPago metodo,
        string? referencia)
    {
        var pago = Pago.Crear(
            "  pag-2026-000001  ",
            VentaIdValida,
            tipo == TipoPago.Contado ? null : Guid.NewGuid(),
            tipo == TipoPago.Mensualidad ? Guid.NewGuid() : null,
            "  cajero-user-001  ",
            tipo,
            metodo,
            referencia,
            new Dinero(6248.50m),
            new Dinero(4648.50m),
            new Dinero(1600.00m),
            new Dinero(96000.00m),
            new Dinero(91351.50m),
            $"  {ClaveIdempotenciaValida}  ",
            FechaValida);

        pago.Id.Should().NotBeEmpty();
        pago.Folio.Should().Be(FolioValido);
        pago.Tipo.Should().Be(tipo);
        pago.Metodo.Should().Be(metodo);
        pago.CajeroId.Should().Be(CajeroIdValido);
        pago.ClaveIdempotencia.Should().Be(ClaveIdempotenciaValida);
        pago.MontoTotal.Should().Be(new Dinero(6248.50m));
        pago.FechaPago.Should().Be(FechaValida);

        if (metodo == MetodoPago.Efectivo)
            pago.Referencia.Should().BeNull();
        else
            pago.Referencia.Should().Be(referencia!.Trim());
    }

    [Theory]
    [InlineData(MetodoPago.TransferenciaBancaria, null)]
    [InlineData(MetodoPago.TransferenciaBancaria, "")]
    [InlineData(MetodoPago.TransferenciaBancaria, "   ")]
    [InlineData(MetodoPago.TransferenciaBancaria, "123")]
    [InlineData(MetodoPago.Tarjeta, null)]
    [InlineData(MetodoPago.Tarjeta, "")]
    [InlineData(MetodoPago.Tarjeta, "482")]
    public void Crear_ConMetodoElectronicoSinReferenciaMinima_DebeLanzarReglaNegocioException(
        MetodoPago metodo,
        string? referenciaInvalida)
    {
        Action accion = () => Pago.Crear(
            FolioValido,
            VentaIdValida,
            Guid.NewGuid(),
            Guid.NewGuid(),
            CajeroIdValido,
            TipoPago.Mensualidad,
            metodo,
            referenciaInvalida,
            new Dinero(6248.50m),
            new Dinero(4648.50m),
            new Dinero(1600.00m),
            new Dinero(96000.00m),
            new Dinero(91351.50m),
            ClaveIdempotenciaValida,
            FechaValida);

        accion.Should().Throw<ReglaNegocioException>()
            .WithMessage("La referencia (folio o últimos 4 dígitos) es obligatoria para transferencias y tarjetas");
    }

    [Fact]
    public void Crear_CuandoCapitalMasInteresNoCuadraConTotal_DebeLanzarReglaNegocioException()
    {
        Action accion = () => Pago.Crear(
            FolioValido,
            VentaIdValida,
            Guid.NewGuid(),
            null,
            CajeroIdValido,
            TipoPago.AbonoCapital,
            MetodoPago.Efectivo,
            null,
            new Dinero(5000.00m),
            new Dinero(4000.00m),
            new Dinero(500.00m),
            new Dinero(96000.00m),
            new Dinero(92000.00m),
            ClaveIdempotenciaValida,
            FechaValida);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("PAG-2026-0001")]
    [InlineData("FAC-2026-000001")]
    public void Crear_ConFolioInvalido_DebeLanzarReglaNegocioException(string? folioInvalido)
    {
        Action accion = () => Pago.Crear(
            folioInvalido!,
            VentaIdValida,
            null,
            null,
            CajeroIdValido,
            TipoPago.Contado,
            MetodoPago.Efectivo,
            null,
            new Dinero(120000m),
            new Dinero(120000m),
            Dinero.Zero,
            new Dinero(120000m),
            Dinero.Zero,
            ClaveIdempotenciaValida,
            FechaValida);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void Crear_ConIdempotenciaOVentaOCajeroOMontoInvalidos_DebeLanzarReglaNegocioException()
    {
        Action sinIdempotencia = () => Pago.Crear(FolioValido, VentaIdValida, null, null, CajeroIdValido, TipoPago.Contado, MetodoPago.Efectivo, null, new Dinero(1000m), new Dinero(1000m), Dinero.Zero, new Dinero(1000m), Dinero.Zero, "   ", FechaValida);
        Action sinVenta = () => Pago.Crear(FolioValido, Guid.Empty, null, null, CajeroIdValido, TipoPago.Contado, MetodoPago.Efectivo, null, new Dinero(1000m), new Dinero(1000m), Dinero.Zero, new Dinero(1000m), Dinero.Zero, ClaveIdempotenciaValida, FechaValida);
        Action sinCajero = () => Pago.Crear(FolioValido, VentaIdValida, null, null, "", TipoPago.Contado, MetodoPago.Efectivo, null, new Dinero(1000m), new Dinero(1000m), Dinero.Zero, new Dinero(1000m), Dinero.Zero, ClaveIdempotenciaValida, FechaValida);
        Action montoCero = () => Pago.Crear(FolioValido, VentaIdValida, null, null, CajeroIdValido, TipoPago.Contado, MetodoPago.Efectivo, null, Dinero.Zero, Dinero.Zero, Dinero.Zero, Dinero.Zero, Dinero.Zero, ClaveIdempotenciaValida, FechaValida);

        sinIdempotencia.Should().Throw<ReglaNegocioException>();
        sinVenta.Should().Throw<ReglaNegocioException>();
        sinCajero.Should().Throw<ReglaNegocioException>();
        montoCero.Should().Throw<ReglaNegocioException>();
    }
}
