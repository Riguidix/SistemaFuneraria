using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Ventas;

public class NuevaModel : PageModel
{
    // Encabezado y Metadatos
    public string Kicker { get; set; } = "PROCESO COMERCIAL";
    public string Titulo { get; set; } = "Nueva venta";
    public string Subtitulo { get; set; } = "Completa los datos para generar la cotización y el contrato.";
    public int PasoActual { get; set; } = 1;

    // Resumen de Cotización (Panel Lateral Derecho · Captura 5 de Figma)
    public string LoteTipo { get; set; } = "Lote Básico";
    public string LoteClave { get; set; } = "JN-A-042";
    public int GavetaSeleccionada { get; set; } = 42;
    public int CapacidadGavetas { get; set; } = 2;
    public decimal PrecioLista { get; set; } = 120000.00m;
    public decimal TotalEstimado { get; set; } = 120000.00m;

    public void OnGet()
    {
        PasoActual = 1;
    }
}
