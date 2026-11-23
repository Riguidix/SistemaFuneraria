using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Caja;

public class ReciboModel : PageModel
{
    public string Folio { get; set; } = string.Empty;

    public void OnGet(string? folio)
    {
        Folio = folio?.Trim() ?? string.Empty;
    }
}
