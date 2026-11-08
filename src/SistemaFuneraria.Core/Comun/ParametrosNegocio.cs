using System.Collections.ObjectModel;

namespace SistemaFuneraria.Core.Comun;

/// <summary>
/// Catálogo inmutable centralizado de parámetros de negocio del Sistema Funerario (RNF-05).
/// Centraliza reglas de precios (RN-01), enganches (RN-04) y tasas indexadas al plazo (RN-05, RN-06).
/// </summary>
public static class ParametrosNegocio
{
    public static readonly IReadOnlyList<int> PlazosAutorizados = new ReadOnlyCollection<int>([12, 18, 24, 48]);
    public static readonly IReadOnlyList<int> EnganchesAutorizados = new ReadOnlyCollection<int>([10, 20, 30]);
    public static readonly IReadOnlyList<decimal> PreciosLotesAutorizados = new ReadOnlyCollection<decimal>([120000.00m, 150000.00m, 180000.00m]);

    public static readonly IReadOnlyDictionary<int, decimal> TasasAnualesPorPlazo =
        new ReadOnlyDictionary<int, decimal>(new Dictionary<int, decimal>
        {
            [12] = 0.16m, [18] = 0.18m, [24] = 0.20m, [48] = 0.24m
        });

    public static bool EsPlazoAutorizado(int plazoMeses) => TasasAnualesPorPlazo.ContainsKey(plazoMeses);

    public static decimal ObtenerTasaAnual(int plazoMeses)
    {
        if (!TasasAnualesPorPlazo.TryGetValue(plazoMeses, out decimal tasaAnual))
        {
            throw new ArgumentOutOfRangeException(
                nameof(plazoMeses),
                plazoMeses,
                $"El plazo de {plazoMeses} meses no está autorizado. Los plazos permitidos son exclusivamente 12, 18, 24 y 48 meses (RN-05).");
        }
        return tasaAnual;
    }
}
