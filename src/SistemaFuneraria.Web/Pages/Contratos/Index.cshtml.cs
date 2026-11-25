using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Contratos;

public class IndexModel : PageModel
{
    public record ContratoItemDto(
        string Folio,
        string Cliente,
        string Lote,
        decimal Saldo,
        string ProximoPago,
        string Estado,
        string EstadoBadgeClass,
        bool RequiereCobro
    );

    public int TotalActivos { get; set; }
    public int TotalAlCorriente { get; set; }
    public int TotalConAtraso { get; set; }
    public int TotalLiquidados { get; set; }
    public string TabInicial { get; set; } = "Todos";
    public string BusquedaInicial { get; set; } = string.Empty;

    public List<ContratoItemDto> Contratos { get; set; } = new();

    public void OnGet(string? tab = null, string? buscar = null)
    {
        TabInicial = string.IsNullOrWhiteSpace(tab) ? "Todos" : tab.Trim();
        BusquedaInicial = string.IsNullOrWhiteSpace(buscar) ? string.Empty : buscar.Trim();
        Contratos = new List<ContratoItemDto>();
    }
}
