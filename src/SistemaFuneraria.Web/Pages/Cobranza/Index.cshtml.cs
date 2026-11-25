using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Cobranza;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Caja/Index");
}
