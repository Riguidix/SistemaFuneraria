using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Cuenta;

public class OlvideContrasenaModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido")]
    public string Email { get; set; } = string.Empty;

    public string? MensajeConfirmacion { get; set; }
    public bool SolicitudEnviada { get; set; } = false;

    public void OnGet()
    {
        SolicitudEnviada = false;
        MensajeConfirmacion = null;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Mensaje uniforme seguro contra enumeración de usuarios (RNF-01, RNF-03)
        SolicitudEnviada = true;
        MensajeConfirmacion = "Si el correo ingresado se encuentra registrado, recibirás un enlace de restablecimiento en breve";

        return Page();
    }
}
