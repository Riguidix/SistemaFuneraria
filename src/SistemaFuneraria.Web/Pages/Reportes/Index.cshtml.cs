using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Reportes;

public class IndexModel : PageModel
{
    // Filtros de Periodo y Sucursal
    public string PeriodoSeleccionado { get; set; } = "mes-actual";
    public string SucursalSeleccionada { get; set; } = "Jardines del Valle";

    // 3 KPIs Superiores (SF-090)
    public decimal ConversionCotizacion { get; set; }
    public string ConversionComparativa { get; set; } = string.Empty;

    public decimal TicketPromedio { get; set; }
    public string TicketSubtexto { get; set; } = string.Empty;

    public decimal IndiceMorosidad { get; set; }
    public string MorosidadMeta { get; set; } = "Meta menor a 8.0%";

    // Ventas por tipo de lote (Barras horizontales)
    public List<VentaTipoLoteDto> VentasPorTipoLote { get; set; } = new();

    // Meta Mensual (Gráfica circular)
    public int PorcentajeCumplimientoMeta { get; set; }
    public decimal AcumuladoMes { get; set; }
    public decimal ObjetivoMensual { get; set; }

    public void OnGet()
    {
        VentasPorTipoLote = new List<VentaTipoLoteDto>();
    }
}

public record VentaTipoLoteDto(string TipoLote, decimal Monto, int Porcentaje);
