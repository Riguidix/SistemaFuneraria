using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Configuracion;

public class IndexModel : PageModel
{
    // 1. Datos de la Sucursal (SF-092)
    public string NombreComercial { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Domicilio { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;

    // Switches de Automatización Operativa
    public bool RecordatoriosPagoActivo { get; set; }
    public bool ResumenDiarioCajaActivo { get; set; }

    // Sección activa del submenú lateral
    public string SeccionActiva { get; set; } = "DatosSucursal";

    // 2. Usuarios y Roles
    public List<UsuarioConfigDto> Usuarios { get; set; } = new();

    // 3. Planes y Tasas
    public List<PlanLoteConfigDto> PlanesLotes { get; set; } = new();
    public List<TasaPlazoConfigDto> TasasPlazos { get; set; } = new();

    public void OnGet()
    {
        Usuarios = new List<UsuarioConfigDto>();

        PlanesLotes = new List<PlanLoteConfigDto>
        {
            new("Lote Básico", 2, 120000m, "10%, 20% o 30%"),
            new("Lote Familiar", 3, 150000m, "10%, 20% o 30%"),
            new("Lote Preferencial", 4, 180000m, "10%, 20% o 30%")
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
