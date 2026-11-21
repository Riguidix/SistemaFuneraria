using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Configuracion;

public class IndexModel : PageModel
{
    // 1. Datos de la Sucursal (Captura 12 de Figma · SF-092)
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

    // 2. Usuarios y Roles
    public List<UsuarioConfigDto> Usuarios { get; set; } = new();

    // 3. Planes y Tasas
    public List<PlanLoteConfigDto> PlanesLotes { get; set; } = new();
    public List<TasaPlazoConfigDto> TasasPlazos { get; set; } = new();

    public void OnGet()
    {
        Usuarios = new List<UsuarioConfigDto>
        {
            new("Andrea Morales", "admin@funeraria.com", "AM", "Administradora", "Activo", "Hoy, 08:02"),
            new("Carlos Mendoza", "vendedor@funeraria.com", "CM", "Vendedor", "Activo", "Ayer, 18:24"),
            new("Guadalupe Ramos", "cajero@funeraria.com", "GR", "Cajera", "Activo", "Hoy, 07:55")
        };

        PlanesLotes = new List<PlanLoteConfigDto>
        {
            new("Lote Básico", 2, 120000m, "10% a 30%"),
            new("Lote Familiar", 3, 150000m, "10% a 30%"),
            new("Lote Preferencial", 4, 180000m, "10% a 30%")
        };

        TasasPlazos = new List<TasaPlazoConfigDto>
        {
            new("12 meses", 16.0m, "Fija anual"),
            new("18 meses", 18.0m, "Fija anual"),
            new("24 meses", 20.0m, "Fija anual"),
            new("48 meses", 24.0m, "Fija anual")
        };
    }
}

public record UsuarioConfigDto(string Nombre, string Correo, string Iniciales, string Rol, string Estado, string UltimoAcceso);

public record PlanLoteConfigDto(string Nombre, int Gavetas, decimal PrecioLista, string EngancheSugerido);

public record TasaPlazoConfigDto(string Plazo, decimal TasaAnual, string Tipo);
