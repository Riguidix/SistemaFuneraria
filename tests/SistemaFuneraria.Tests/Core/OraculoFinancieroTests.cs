using System.Globalization;
using FluentAssertions;
using SistemaFuneraria.Core.Calculos;
using Xunit;

namespace SistemaFuneraria.Tests.Core;

/// <summary>
/// Suite de pruebas parametrizadas contra el Oráculo Oficial de 36 Combinaciones Cartesianas (SF-012).
/// Valida al centavo exacto ($0.00 de discrepancia) enganche, capital financiado, tasa indexada,
/// mensualidad ordinaria, ajuste de cuota final, suma de intereses y total pagado (RNF-02, DT-01..36).
/// </summary>
[Trait("Ticket", "SF-012")]
[Trait("Capa", "Core")]
[Trait("Regla", "RNF-02")]
public class OraculoFinancieroTests
{
    private const int TotalCombinacionesOraculo = 36;

    private static string ObtenerRutaAbsolutaCsv()
    {
        var dirActual = new DirectoryInfo(AppContext.BaseDirectory);

        while (dirActual != null && !Directory.Exists(Path.Combine(dirActual.FullName, "docs")))
        {
            dirActual = dirActual.Parent;
        }

        if (dirActual == null)
        {
            throw new DirectoryNotFoundException("No se encontró la carpeta raíz del repositorio con el directorio 'docs'.");
        }

        return Path.Combine(dirActual.FullName, "docs", "anexos", "oraculo-36-combinaciones.csv");
    }

    public static IEnumerable<object[]> ObtenerDatosOraculo()
    {
        var rutaCsv = ObtenerRutaAbsolutaCsv();

        if (!File.Exists(rutaCsv))
        {
            throw new FileNotFoundException($"No se encontró el archivo del oráculo en la ruta: {rutaCsv}");
        }

        var lineas = File.ReadAllLines(rutaCsv);
        var filas = new List<object[]>(TotalCombinacionesOraculo);

        foreach (var linea in lineas.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;

            var col = linea.Split(',');

            filas.Add(new object[]
            {
                col[0].Trim(),
                int.Parse(col[1], CultureInfo.InvariantCulture),
                decimal.Parse(col[2], CultureInfo.InvariantCulture),
                int.Parse(col[3], CultureInfo.InvariantCulture),
                decimal.Parse(col[4], CultureInfo.InvariantCulture),
                decimal.Parse(col[5], CultureInfo.InvariantCulture),
                int.Parse(col[6], CultureInfo.InvariantCulture),
                decimal.Parse(col[7], CultureInfo.InvariantCulture),
                decimal.Parse(col[8], CultureInfo.InvariantCulture),
                decimal.Parse(col[9], CultureInfo.InvariantCulture),
                decimal.Parse(col[10], CultureInfo.InvariantCulture),
                decimal.Parse(col[11], CultureInfo.InvariantCulture)
            });
        }

        if (filas.Count != TotalCombinacionesOraculo)
        {
            throw new InvalidOperationException(
                $"El archivo del oráculo debe contener exactamente {TotalCombinacionesOraculo} combinaciones, pero se encontraron {filas.Count}.");
        }

        return filas;
    }

    [Theory]
    [MemberData(nameof(ObtenerDatosOraculo))]
    public void ValidarCombinacionOraculoFinanciero(
        string lote,
        int gavetas,
        decimal precio,
        int enganchePct,
        decimal engancheEsperado,
        decimal capitalFinanciadoEsperado,
        int plazoMeses,
        decimal tasaAnualPct,
        decimal mensualidadEsperada,
        decimal ultimaMensualidadEsperada,
        decimal interesesTotalesEsperados,
        decimal totalPagadoEsperado)
    {
        // Arrange
        lote.Should().NotBeNullOrWhiteSpace();
        gavetas.Should().BeGreaterThan(0);

        // Act 1: Cálculo de enganche y capital financiado (RF-04, RN-01, RN-04)
        var (enganche, capital) = CalculadoraFinanciamiento.CalcularEnganche(precio, enganchePct);

        // Act 2: Resolución de tasas indexadas al plazo (RF-05, RF-06, RN-05, RN-06)
        var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(plazoMeses);

        // Act 3: Cálculo de mensualidad ordinaria francesa (RF-07, RN-07, CP-05)
        decimal mensualidad = CalculadoraFinanciamiento.CalcularMensualidadOrdinaria(capital, tasaMensual, plazoMeses);

        // Act 4: Generación de tabla de amortización con absorción al centavo en cuota final (RF-08, CP-06, RNF-02)
        var tabla = GeneradorAmortizacion.Generar(capital, tasaMensual, plazoMeses);

        decimal sumaIntereses = tabla.Sum(r => r.Interes);
        decimal totalPagado = enganche + tabla.Sum(r => r.PagoMensual);
        var ultimoPeriodo = tabla[^1];

        // Assert: Tolerancia de $0.00 (precisión exacta al centavo en los 36 escenarios)
        enganche.Should().Be(engancheEsperado, $"({lote}, {enganchePct}%, {plazoMeses}m) enganche debe ser exacto");
        capital.Should().Be(capitalFinanciadoEsperado, $"({lote}, {enganchePct}%, {plazoMeses}m) capital financiado debe ser exacto");
        tasaAnual.Should().Be(tasaAnualPct / 100m, $"({lote}, {plazoMeses}m) tasa anual debe coincidir con catálogo");
        mensualidad.Should().Be(mensualidadEsperada, $"({lote}, {enganchePct}%, {plazoMeses}m) mensualidad ordinaria debe ser exacta");

        tabla.Should().HaveCount(plazoMeses);
        ultimoPeriodo.Periodo.Should().Be(plazoMeses);
        ultimoPeriodo.PagoMensual.Should().Be(ultimaMensualidadEsperada, $"({lote}, {enganchePct}%, {plazoMeses}m) cuota final ajustada debe coincidir con el oráculo");
        ultimoPeriodo.SaldoFinal.Should().Be(0.00m, $"({lote}, {enganchePct}%, {plazoMeses}m) saldo final debe cerrar en $0.00");

        sumaIntereses.Should().Be(interesesTotalesEsperados, $"({lote}, {enganchePct}%, {plazoMeses}m) suma total de intereses debe ser exacta");
        totalPagado.Should().Be(totalPagadoEsperado, $"({lote}, {enganchePct}%, {plazoMeses}m) total pagado (enganche + cuotas) debe ser exacto");
    }
}
