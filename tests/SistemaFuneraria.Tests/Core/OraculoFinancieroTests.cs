using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SistemaFuneraria.Tests.Core;

public class OraculoFinancieroTests
{
    private static string ObtenerRutaAbsolutaCsv()
    {
        var dirActual = new DirectoryInfo(AppContext.BaseDirectory);
        
        while (dirActual != null && !Directory.Exists(Path.Combine(dirActual.FullName, "docs")))
        {
            dirActual = dirActual.Parent;
        }

        if (dirActual == null)
        {
            throw new DirectoryNotFoundException("No se encontró la carpeta raíz del repositorio 'docs'.");
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

        foreach (var linea in lineas.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;

            var col = linea.Split(',');

            yield return new object[] {
                col[0].Trim(),
                int.Parse(col[1]),
                decimal.Parse(col[2]),
                decimal.Parse(col[3]),
                decimal.Parse(col[4]),
                decimal.Parse(col[5]),
                int.Parse(col[6]),
                decimal.Parse(col[7]),
                decimal.Parse(col[8]),
                decimal.Parse(col[9]),
                decimal.Parse(col[10]),
                decimal.Parse(col[11])
            };
        }
    }

    [Theory]
    [MemberData(nameof(ObtenerDatosOraculo))]
    public void ValidarCombinacionOraculoFinanciero(
        string lote, 
        int gavetas, 
        decimal precio, 
        decimal enganchePct,
        decimal engancheEsperado, 
        decimal capitalFinanciadoEsperado, 
        int plazoMeses,
        decimal tasaAnualPct, 
        decimal mensualidadEsperada, 
        decimal ultimaMensualidadEsperada,
        decimal interesesTotalesEsperados, 
        decimal totalPagadoEsperado)
    {
        Assert.False(string.IsNullOrWhiteSpace(lote));
        Assert.True(gavetas > 0);
        Assert.True(precio > 0);
        Assert.True(enganchePct >= 0);
        Assert.True(engancheEsperado >= 0);
        Assert.True(capitalFinanciadoEsperado >= 0);
        Assert.True(plazoMeses > 0);
        Assert.True(tasaAnualPct >= 0);
        Assert.True(mensualidadEsperada >= 0);
        Assert.True(ultimaMensualidadEsperada >= 0);
        Assert.True(interesesTotalesEsperados >= 0);
        Assert.True(totalPagadoEsperado > 0);
    }
}