using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Caja;

public class IndexModel : PageModel
{
    public string FolioContrato { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string InicialesCliente { get; set; } = string.Empty;
    public string UbicacionLote { get; set; } = string.Empty;
    public string EstadoCuota { get; set; } = string.Empty;
    public decimal MontoMensualidad { get; set; }
    public decimal MontoLiquidacion { get; set; }

    public void OnGet(string? contrato = null)
    {
        FolioContrato = contrato?.Trim() ?? string.Empty;
    }
}
