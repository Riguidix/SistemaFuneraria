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

    public int TotalActivos { get; set; } = 128;
    public int TotalAlCorriente { get; set; } = 120;
    public int TotalConAtraso { get; set; } = 8;
    public int TotalLiquidados { get; set; } = 32;

    public List<ContratoItemDto> Contratos { get; set; } = new();

    public void OnGet()
    {
        Contratos = new List<ContratoItemDto>
        {
            new("CTR-2024-0118", "Elena Vargas", "RO-B-027", 76180.40m, "02 oct. 2025", "Vencido", "bg-danger-subtle text-danger", true),
            new("CTR-2025-0189", "Laura Castañeda", "JN-A-042", 127500.00m, "18 nov. 2025", "Al corriente", "bg-success-subtle text-success", false),
            new("CTR-2025-0182", "Roberto Méndez", "JC-C-118", 58743.20m, "18 nov. 2025", "Al corriente", "bg-success-subtle text-success", false),
            new("CTR-2025-0144", "Jorge Salinas", "RO-E-081", 94265.80m, "17 nov. 2025", "En revisión", "bg-warning-subtle text-warning-emphasis", false)
        };
    }
}
