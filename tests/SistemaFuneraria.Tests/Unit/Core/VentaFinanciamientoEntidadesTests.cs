using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Suite de pruebas unitarias para las entidades de dominio Venta, Financiamiento y Mensualidad (SF-030).
/// Valida el encapsulamiento DDD, ciclo de vida del crédito, integridad referencial en memoria e invariantes ERD v1.1.
/// </summary>
public class VentaFinanciamientoEntidadesTests
{
    private static readonly Guid ClienteIdValido = Guid.NewGuid();
    private const string LoteIdValido = "LOT-02";
    private const string VendedorIdValido = "VEND-001";
    private const decimal PrecioLoteValido = 120000.00m;
    private const decimal EngancheValido = 24000.00m;
    private const decimal CapitalValido = 96000.00m;
    private const int PlazoValido = 12;
    private const decimal TasaAnualValida = 0.16m;

    private static List<Mensualidad> GenerarCuotasValidas(decimal capital, int plazo, decimal tasaAnual, DateTimeOffset fechaInicio)
    {
        var renglones = GeneradorAmortizacion.Generar(capital, tasaAnual / 12m, plazo);
        var cuotas = new List<Mensualidad>();

        foreach (var renglon in renglones)
        {
            var fechaVencimiento = fechaInicio.AddMonths(renglon.Periodo);
            var cuota = Mensualidad.Crear(
                renglon.Periodo,
                fechaVencimiento,
                renglon.PagoMensual,
                renglon.CapitalAmortizado,
                renglon.Interes,
                renglon.SaldoFinal);
            cuotas.Add(cuota);
        }

        return cuotas;
    }

    #region Happy Path: Venta de Contado

    [Fact]
    [Trait("Ticket", "SF-030")]
    public void CrearContado_ConDatosValidos_DebeCrearVentaCompletadaSinFinanciamiento()
    {
        // GIVEN: Datos válidos de una venta de contado
        var fechaAntes = DateTimeOffset.UtcNow.AddSeconds(-1);

        // WHEN: Se instancia la venta de contado mediante factory method
        var venta = Venta.CrearContado(ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioLoteValido);
        var fechaDespues = DateTimeOffset.UtcNow.AddSeconds(1);

        // THEN: La venta debe registrarse en estado completada y sin crédito asociado
        venta.Should().NotBeNull();
        venta.Id.Should().NotBeEmpty();
        venta.ClienteId.Should().Be(ClienteIdValido);
        venta.LoteId.Should().Be(LoteIdValido);
        venta.VendedorId.Should().Be(VendedorIdValido);
        venta.MontoTotal.Should().Be(PrecioLoteValido);
        venta.TipoVenta.Should().Be(TipoVenta.Contado);
        venta.Estado.Should().Be(EstadoVenta.Completada);
        venta.Financiamiento.Should().BeNull("una venta de contado no genera contrato de financiamiento");
        venta.FechaVenta.Should().BeOnOrAfter(fechaAntes).And.BeOnOrBefore(fechaDespues);
    }

    #endregion

    #region Happy Path: Venta Financiada (Escenario Canónico CP-05 / CP-06)

    [Fact]
    [Trait("Ticket", "SF-030")]
    public void CrearFinanciada_ConAmortizacionCalculada_DebeAsociarCreditoYMensualidadesCoherentes()
    {
        // GIVEN: Tabla de amortización de 12 meses generada con el oráculo financiero
        var fechaBase = DateTimeOffset.UtcNow;
        var cuotas = GenerarCuotasValidas(CapitalValido, PlazoValido, TasaAnualValida, fechaBase);

        // WHEN: Se crea el financiamiento y la venta financiada
        var financiamiento = Financiamiento.Crear(EngancheValido, CapitalValido, PlazoValido, TasaAnualValida, cuotas);
        var venta = Venta.CrearFinanciada(ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioLoteValido, financiamiento);

        // THEN: La venta y el financiamiento deben vincularse mutuamente de forma íntegra
        venta.Should().NotBeNull();
        venta.TipoVenta.Should().Be(TipoVenta.Financiado);
        venta.Estado.Should().Be(EstadoVenta.Completada);
        venta.Financiamiento.Should().NotBeNull();
        venta.Financiamiento.Should().BeSameAs(financiamiento);

        // Verificaciones del Financiamiento
        financiamiento.Id.Should().NotBeEmpty();
        financiamiento.VentaId.Should().Be(venta.Id, "el financiamiento debe referenciar a su venta padre");
        financiamiento.Enganche.Should().Be(EngancheValido);
        financiamiento.CapitalFinanciado.Should().Be(CapitalValido);
        financiamiento.SaldoInsolutoActual.Should().Be(CapitalValido, "al inicio el saldo insoluto es igual al capital neto");
        financiamiento.PlazoMeses.Should().Be(PlazoValido);
        financiamiento.TasaAnual.Should().Be(TasaAnualValida);
        financiamiento.Estado.Should().Be(EstadoFinanciamiento.Activo);

        // Verificaciones del calendario de cuotas
        financiamiento.Mensualidades.Should().HaveCount(12);
        financiamiento.Mensualidades.Should().OnlyContain(m => m.FinanciamientoId == financiamiento.Id);
        financiamiento.Mensualidades.Should().OnlyContain(m => m.Estado == EstadoMensualidad.Pendiente);

        var listaCuotas = financiamiento.Mensualidades.OrderBy(m => m.NumeroPeriodo).ToList();
        for (int i = 0; i < 12; i++)
        {
            listaCuotas[i].NumeroPeriodo.Should().Be(i + 1);
        }

        // Última cuota debe culminar con saldo final $0.00 (CP-06)
        listaCuotas.Last().SaldoInsolutoRestante.Should().Be(0.00m);
    }

    #endregion

    #region Ciclo de Vida de Mensualidad: RegistrarPago

    [Fact]
    [Trait("Ticket", "SF-030")]
    public void RegistrarPago_EnMensualidadPendiente_DebeCambiarEstadoAPagada()
    {
        // Arrange
        var cuota = Mensualidad.Crear(1, DateTimeOffset.UtcNow.AddMonths(1), 8710.16m, 7430.16m, 1280.00m, 88569.84m);

        // Act
        cuota.RegistrarPago();

        // Assert
        cuota.Estado.Should().Be(EstadoMensualidad.Pagada);
    }

    [Fact]
    [Trait("Ticket", "SF-030")]
    public void RegistrarPago_EnMensualidadYaPagada_DebeLanzarReglaNegocioException()
    {
        // Arrange
        var cuota = Mensualidad.Crear(1, DateTimeOffset.UtcNow.AddMonths(1), 8710.16m, 7430.16m, 1280.00m, 88569.84m);
        cuota.RegistrarPago();

        // Act
        Action act = () => cuota.RegistrarPago();

        // Assert
        act.Should().Throw<ReglaNegocioException>()
           .WithMessage("*ya se encuentra pagada*");
    }

    #endregion

    #region Invariantes de Venta

    [Theory]
    [Trait("Ticket", "SF-030")]
    [InlineData(0)]
    [InlineData(-1000)]
    public void CrearContado_ConMontoInvalido_DebeLanzarReglaNegocioException(decimal montoInvalido)
    {
        Action act = () => Venta.CrearContado(ClienteIdValido, LoteIdValido, VendedorIdValido, montoInvalido);
        act.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-030")]
    public void CrearContado_ConClienteIdVacio_DebeLanzarReglaNegocioException()
    {
        Action act = () => Venta.CrearContado(Guid.Empty, LoteIdValido, VendedorIdValido, PrecioLoteValido);
        act.Should().Throw<ReglaNegocioException>();
    }

    [Theory]
    [Trait("Ticket", "SF-030")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CrearContado_ConLoteIdInvalido_DebeLanzarReglaNegocioException(string? loteInvalido)
    {
        Action act = () => Venta.CrearContado(ClienteIdValido, loteInvalido!, VendedorIdValido, PrecioLoteValido);
        act.Should().Throw<ReglaNegocioException>();
    }

    [Theory]
    [Trait("Ticket", "SF-030")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CrearContado_ConVendedorIdInvalido_DebeLanzarReglaNegocioException(string? vendedorInvalido)
    {
        Action act = () => Venta.CrearContado(ClienteIdValido, LoteIdValido, vendedorInvalido!, PrecioLoteValido);
        act.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-030")]
    public void CrearFinanciada_ConFinanciamientoNulo_DebeLanzarReglaNegocioException()
    {
        Action act = () => Venta.CrearFinanciada(ClienteIdValido, LoteIdValido, VendedorIdValido, PrecioLoteValido, null!);
        act.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-030")]
    public void CrearFinanciada_ConMontoIncoherenteConEngancheYCapital_DebeLanzarReglaNegocioException()
    {
        // Arrange: Enganche (24k) + Capital (96k) = 120k, pero pasamos MontoTotal = 150k
        var cuotas = GenerarCuotasValidas(CapitalValido, PlazoValido, TasaAnualValida, DateTimeOffset.UtcNow);
        var financiamiento = Financiamiento.Crear(EngancheValido, CapitalValido, PlazoValido, TasaAnualValida, cuotas);

        // Act
        Action act = () => Venta.CrearFinanciada(ClienteIdValido, LoteIdValido, VendedorIdValido, 150000.00m, financiamiento);

        // Assert
        act.Should().Throw<ReglaNegocioException>()
           .WithMessage("*monto total*coincidir*");
    }

    #endregion

    #region Invariantes de Financiamiento

    [Theory]
    [Trait("Ticket", "SF-030")]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(36)]
    [InlineData(60)]
    public void CrearFinanciamiento_ConPlazoNoAutorizado_DebeLanzarReglaNegocioException(int plazoInvalido)
    {
        var cuotas = GenerarCuotasValidas(CapitalValido, 12, TasaAnualValida, DateTimeOffset.UtcNow);
        Action act = () => Financiamiento.Crear(EngancheValido, CapitalValido, plazoInvalido, TasaAnualValida, cuotas);
        act.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-030")]
    public void CrearFinanciamiento_ConNumeroDeCuotasDiscrepanteDelPlazo_DebeLanzarReglaNegocioException()
    {
        // GIVEN: Plazo de 12 meses pero solo 11 cuotas suministradas
        var cuotas = GenerarCuotasValidas(CapitalValido, 12, TasaAnualValida, DateTimeOffset.UtcNow).Take(11).ToList();

        // Act
        Action act = () => Financiamiento.Crear(EngancheValido, CapitalValido, 12, TasaAnualValida, cuotas);

        // Assert
        act.Should().Throw<ReglaNegocioException>()
           .WithMessage("*exactamente igual al plazo*");
    }

    [Theory]
    [Trait("Ticket", "SF-030")]
    [InlineData(0, 96000)]
    [InlineData(-1000, 96000)]
    [InlineData(24000, 0)]
    [InlineData(24000, -5000)]
    public void CrearFinanciamiento_ConMontosNoPositivos_DebeLanzarReglaNegocioException(decimal enganche, decimal capital)
    {
        var cuotas = GenerarCuotasValidas(96000m, 12, TasaAnualValida, DateTimeOffset.UtcNow);
        Action act = () => Financiamiento.Crear(enganche, capital, 12, TasaAnualValida, cuotas);
        act.Should().Throw<ReglaNegocioException>();
    }

    #endregion

    #region Invariantes de Mensualidad

    [Theory]
    [Trait("Ticket", "SF-030")]
    [InlineData(0)]
    [InlineData(-1)]
    public void CrearMensualidad_ConPeriodoInvalido_DebeLanzarReglaNegocioException(int periodoInvalido)
    {
        Action act = () => Mensualidad.Crear(periodoInvalido, DateTimeOffset.UtcNow, 8710.16m, 7430.16m, 1280.00m, 0m);
        act.Should().Throw<ReglaNegocioException>();
    }

    [Theory]
    [Trait("Ticket", "SF-030")]
    [InlineData(0)]
    [InlineData(-500)]
    public void CrearMensualidad_ConCuotaNoPositiva_DebeLanzarReglaNegocioException(decimal cuotaInvalida)
    {
        Action act = () => Mensualidad.Crear(1, DateTimeOffset.UtcNow, cuotaInvalida, 7430.16m, 1280.00m, 0m);
        act.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    [Trait("Ticket", "SF-030")]
    public void CrearMensualidad_ConSaldosNegativos_DebeLanzarReglaNegocioException()
    {
        Action act = () => Mensualidad.Crear(1, DateTimeOffset.UtcNow, 8710.16m, 7430.16m, 1280.00m, -0.01m);
        act.Should().Throw<ReglaNegocioException>();
    }

    #endregion
}
