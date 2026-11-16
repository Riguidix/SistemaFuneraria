using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Configuracion;

public class IndexModel : PageModel
{
    // Datos de la Sucursal (Captura 12 de Figma · SF-092)
    public string NombreComercial { get; set; } = "Jardines del Valle";
    public string Telefono { get; set; } = "55 3128 4400";
    public string Correo { get; set; } = "contacto@jardinesdelvalle.mx";
    public string Domicilio { get; set; } = "Av. de la Paz 1840, Col. Del Valle";
    public string Ciudad { get; set; } = "Ciudad de México";
    public string CodigoPostal { get; set; } = "03100";

    // Switches de Automatización Operativa
    public bool RecordatoriosPagoActivo { get; set; } = true;
    public bool ResumenDiarioCajaActivo { get; set; } = true;

    // Sección activa del submenú lateral
    public string SeccionActiva { get; set; } = "DatosSucursal";

    public void OnGet()
    {
    }
}
