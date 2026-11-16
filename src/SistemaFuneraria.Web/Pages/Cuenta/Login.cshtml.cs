using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Cuenta;

public class LoginModel : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public int IntentosFallidos { get; set; } = 0;
    public bool EstaBloqueado { get; set; } = false;
    public int MinutosBloqueoRestantes { get; set; } = 0;
    public string? MensajeAlerta { get; set; }
    public string? MensajeError { get; set; }

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
        IntentosFallidos = 0;
        EstaBloqueado = false;
        MinutosBloqueoRestantes = 0;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Simulación de autenticación en modo frontend
        if (Input.Email.EndsWith("@serenia.mx", StringComparison.OrdinalIgnoreCase) ||
            Input.Email.Contains("@", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Clientes/Index");
        }

        return Page();
    }
}
