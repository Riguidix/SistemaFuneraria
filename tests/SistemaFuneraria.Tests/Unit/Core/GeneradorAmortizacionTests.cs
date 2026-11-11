using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Core;

/// <summary>
/// Suite de pruebas unitarias para el generador de amortización francesa (SF-011).
/// Valida el cumplimiento estricto del caso de prueba CP-06, requisitos RF-08, RNF-02 y límite BVA-11.
/// </summary>
public class GeneradorAmortizacionTests
{
    private const decimal CapitalCP06 = 96000.00m;
    private const int PlazoCP06 = 12;
    private static readonly decimal TasaMensualCP06 = 0.16m / 12m;

    #region Escenario Canónico Gherkin CP-06 / BVA-11

    [Fact]
    [Trait("Ticket", "SF-011")]
    [Trait("CP", "CP-06")]
    [Trait("Regla", "RF-08")]
    [Trait("Regla", "RNF-02")]
    public void Generar_EscenarioCanonicoCP06_DebeGenerarExactamente12Periodos()
    {
        // GIVEN: Un capital de $96,000.00 y plazo de 12 meses al 16% anual
        // WHEN: Se genera la tabla de amortización
        var tabla = GeneradorAmortizacion.Generar(CapitalCP06, TasaMensualCP06, PlazoCP06);

        // THEN: Debe contener exactamente 12 renglones numerados secuencialmente
        tabla.Should().HaveCount(12);
        tabla.Select(r => r.Periodo).Should().Equal(Enumerable.Range(1, 12));
    }

    [Fact]
    [Trait("Ticket", "SF-011")]
    [Trait("CP", "CP-06")]
    [Trait("Regla", "RF-08")]
    [Trait("Regla", "RNF-02")]
    public void Generar_EscenarioCanonicoCP06_Periodo12_DebeAbsorberAjusteCentavosYSaldoFinalCero()
    {
        // GIVEN: Financiamiento de Lote Básico ($120k con 20% enganche = $96,000.00 capital, 12 meses, 16% anual)
        // WHEN: Se inspecciona detalladamente la última fila (Periodo 12)
        var tabla = GeneradorAmortizacion.Generar(CapitalCP06, TasaMensualCP06, PlazoCP06);
        var renglon12 = tabla[11]; // Índice 11 = Periodo 12

        // THEN: La cuota 12 debe absorber los 3 centavos de ajuste ($8,710.19) y el saldo final debe ser exactamente $0.00 (CP-06, BVA-11)
        renglon12.Periodo.Should().Be(12);
        renglon12.SaldoInicial.Should().Be(8595.58m, "el saldo inicial del periodo 12 según la tabla oficial CP-06 es $8,595.58");
        renglon12.Interes.Should().Be(114.61m, "el interés devengado del periodo 12 con redondeo bancario es $114.61");
        renglon12.CapitalAmortizado.Should().Be(8595.58m, "el capital amortizado debe cubrir el 100% del saldo inicial remanente");
        renglon12.PagoMensual.Should().Be(8710.19m, "el pago mensual del periodo 12 debe absorber +0.03 centavos de diferencia ($8,710.19)");
        renglon12.SaldoFinal.Should().Be(0.00m, "el saldo final del periodo 12 debe cerrar en exactamente $0.00");
    }

    [Fact]
    [Trait("Ticket", "SF-011")]
    [Trait("CP", "CP-06")]
    [Trait("Regla", "RF-08")]
    public void Generar_EscenarioCanonicoCP06_Periodos1A11_DebenMantenerCuotaOrdinariaFija()
    {
        // GIVEN: Financiamiento CP-06
        // WHEN: Se consultan las cuotas ordinarias de los periodos 1 al 11
        var tabla = GeneradorAmortizacion.Generar(CapitalCP06, TasaMensualCP06, PlazoCP06);

        // THEN: Cada periodo del 1 al 11 debe tener exactamente la cuota ordinaria de $8,710.16
        for (int i = 0; i < 11; i++)
        {
            tabla[i].PagoMensual.Should().Be(8710.16m, $"el periodo {i + 1} debe mantener la cuota ordinaria fija de $8,710.16");
        }
    }

    [Fact]
    [Trait("Ticket", "SF-011")]
    [Trait("CP", "CP-06")]
    [Trait("Regla", "RNF-02")]
    public void Generar_EscenarioCanonicoCP06_SumaCapitalAmortizado_DebeSerExactamenteCapitalInicial()
    {
        // GIVEN: Capital de $96,000.00
        // WHEN: Se genera la tabla
        var tabla = GeneradorAmortizacion.Generar(CapitalCP06, TasaMensualCP06, PlazoCP06);

        // THEN: La suma de capital amortizado de todas las cuotas debe sumar exactamente $96,000.00
        decimal totalAmortizado = tabla.Sum(r => r.CapitalAmortizado);
        totalAmortizado.Should().Be(CapitalCP06, "la suma de capital amortizado no puede diferir por centavos del capital inicial financiado");
    }

    #endregion

    #region Validación de Integridad Financiera en Otros Plazos Autorizados (18, 24 y 48 Meses)

    [Theory]
    [Trait("Ticket", "SF-011")]
    [Trait("CP", "CP-06")]
    [Trait("Regla", "RNF-02")]
    [InlineData(96000.00, 18)]
    [InlineData(96000.00, 24)]
    [InlineData(96000.00, 48)]
    [InlineData(108000.00, 12)]
    [InlineData(108000.00, 24)]
    [InlineData(126000.00, 48)]
    public void Generar_EnCualquierPlazoAutorizado_UltimoPeriodoDebeCerrarSaldoEnCeroExacto(decimal capital, int plazoMeses)
    {
        // Act
        var tabla = GeneradorAmortizacion.Generar(capital, plazoMeses);

        // Assert
        var ultimoRenglon = tabla.Last();
        ultimoRenglon.SaldoFinal.Should().Be(0.00m, $"en el plazo de {plazoMeses} meses el saldo final debe cerrar exactamente en $0.00");
        tabla.Sum(r => r.CapitalAmortizado).Should().Be(capital, "la suma total amortizada debe igualar el capital financiado");
    }

    #endregion

    #region Consistencia Contable e Invariantes por Periodo

    [Fact]
    [Trait("Ticket", "SF-011")]
    public void Generar_ConsistenciaInterna_CadaRenglonDebeSatisfacerIdentidadesContables()
    {
        // Arrange & Act
        var tabla = GeneradorAmortizacion.Generar(CapitalCP06, PlazoCP06);

        // Assert
        for (int i = 0; i < tabla.Count; i++)
        {
            var renglon = tabla[i];

            // 1. SaldoInicial - CapitalAmortizado = SaldoFinal
            (renglon.SaldoInicial - renglon.CapitalAmortizado).Should().Be(renglon.SaldoFinal);

            // 2. CapitalAmortizado + Interes = PagoMensual
            (renglon.CapitalAmortizado + renglon.Interes).Should().Be(renglon.PagoMensual);

            // 3. Encadenamiento continuo de saldos (SaldoInicial[k] = SaldoFinal[k-1])
            if (i > 0)
            {
                renglon.SaldoInicial.Should().Be(tabla[i - 1].SaldoFinal);
            }
        }
    }

    [Fact]
    [Trait("Ticket", "SF-011")]
    public void Generar_SobrecargaConPlazo_DebeProducirMismaTablaQueSobrecargaConTasaMensual()
    {
        // Arrange
        decimal capital = 96000.00m;
        int plazo = 12;

        // Act
        var tablaSobrecarga = GeneradorAmortizacion.Generar(capital, plazo);
        var tablaExplicita = GeneradorAmortizacion.Generar(capital, TasaMensualCP06, plazo);

        // Assert
        tablaSobrecarga.Should().BeEquivalentTo(tablaExplicita);
    }

    #endregion

    #region Validaciones Defensivas de Argumentos

    [Theory]
    [Trait("Ticket", "SF-011")]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Generar_ConCapitalInvalido_DebeLanzarArgumentOutOfRangeException(decimal capitalInvalido)
    {
        // Act
        Action accion = () => GeneradorAmortizacion.Generar(capitalInvalido, TasaMensualCP06, 12);

        // Assert
        accion.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("capital");
    }

    [Theory]
    [Trait("Ticket", "SF-011")]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Generar_ConTasaInvalida_DebeLanzarArgumentOutOfRangeException(decimal tasaInvalida)
    {
        // Act
        Action accion = () => GeneradorAmortizacion.Generar(96000.00m, tasaInvalida, 12);

        // Assert
        accion.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("tasaMensual");
    }

    [Theory]
    [Trait("Ticket", "SF-011")]
    [InlineData(6)]
    [InlineData(15)]
    [InlineData(36)]
    public void Generar_ConPlazoNoAutorizado_DebeLanzarArgumentOutOfRangeException(int plazoNoAutorizado)
    {
        // Act
        Action accion = () => GeneradorAmortizacion.Generar(96000.00m, TasaMensualCP06, plazoNoAutorizado);

        // Assert
        accion.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("plazo");
    }

    #endregion
}
