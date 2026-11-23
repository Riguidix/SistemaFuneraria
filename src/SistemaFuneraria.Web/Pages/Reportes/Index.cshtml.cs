using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Reportes;

public class IndexModel : PageModel
{
    // Filtros de Periodo y Sucursal
    public string PeriodoSeleccionado { get; set; } = "Noviembre 2025";
    public string SucursalSeleccionada { get; set; } = "Jardines del Valle";

    // 3 KPIs Superiores (Figma Captura 11 · SF-090)
    public decimal ConversionCotizacion { get; set; } = 32.8m;
    public string ConversionComparativa { get; set; } = "+4.2 pts vs. octubre";

    public decimal TicketPromedio { get; set; } = 147420m;
    public string TicketSubtexto { get; set; } = "Lotes de 2 y 3 gavetas";

    public decimal IndiceMorosidad { get; set; } = 6.2m;
    public string MorosidadMeta { get; set; } = "Meta menor a 8.0%";

    // Ventas por tipo de lote (Barras horizontales)
    public List<VentaTipoLoteDto> VentasPorTipoLote { get; set; } = new();

    // Meta Mensual (Gráfica circular)
    public int PorcentajeCumplimientoMeta { get; set; } = 81;
    public decimal AcumuladoMes { get; set; } = 486500m;
    public decimal ObjetivoMensual { get; set; } = 600000m;

    public void OnGet()
    {
        VentasPorTipoLote = new List<VentaTipoLoteDto>
        {
            new("Lote 2 gavetas", 198000m, 41),
            new("Lote 3 gavetas", 168500m, 35),
            new("Lote 4 gavetas", 120000m, 24)
        };
    }
}

public record VentaTipoLoteDto(string TipoLote, decimal Monto, int Porcentaje);
