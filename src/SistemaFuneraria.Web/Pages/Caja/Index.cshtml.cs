using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Caja;

public class IndexModel : PageModel
{
    public string FolioContrato { get; set; } = "CTR-2024-0118";
    public string ClienteNombre { get; set; } = "Elena Vargas";
    public string InicialesCliente { get; set; } = "EV";
    public string UbicacionLote { get; set; } = "Lote RO-B-027";
    public string EstadoCuota { get; set; } = "1 vencido";
    public decimal MontoMensualidad { get; set; } = 6248.50m;
    public decimal MontoLiquidacion { get; set; } = 77893.39m;

    public void OnGet(string? contrato)
    {
        if (!string.IsNullOrWhiteSpace(contrato))
        {
            FolioContrato = contrato.Trim();
        }
    }
}
