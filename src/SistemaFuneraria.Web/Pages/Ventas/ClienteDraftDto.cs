using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SistemaFuneraria.Web.Pages.Ventas;

public class ClienteDraftDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Curp { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string NombreCompleto => $"{Nombre} {Apellidos}".Trim();

    public bool EsValido(out List<string> errores)
    {
        errores = new List<string>();

        if (string.IsNullOrWhiteSpace(Nombre))
        {
            errores.Add("El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(Apellidos))
        {
            errores.Add("Los apellidos son obligatorios.");
        }

        if (!string.IsNullOrWhiteSpace(Telefono))
        {
            var soloDigitos = Regex.Replace(Telefono, @"\D", "");
            if (soloDigitos.Length != 10)
            {
                errores.Add("El teléfono debe contener exactamente 10 dígitos numéricos.");
            }
        }

        if (!string.IsNullOrWhiteSpace(Email) && (!Email.Contains("@") || !Email.Contains(".")))
        {
            errores.Add("El correo electrónico no tiene un formato válido.");
        }

        return errores.Count == 0;
    }
}
