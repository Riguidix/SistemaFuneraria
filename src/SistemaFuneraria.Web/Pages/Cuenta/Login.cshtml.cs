using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Cuenta;

public class LoginModel : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public class LoginInputModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato de correo no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool Recordarme { get; set; } = false;
    }

    public void OnGet()
    {
        Input = new LoginInputModel();
    }

    public IActionResult OnPost()
    {
        // La autenticación real ocurre contra SistemaFuneraria.Api vía authApi.js.
        // Nunca redirigir a rutas protegidas desde este fallback sin validación del servidor.
        return Page();
    }
}
