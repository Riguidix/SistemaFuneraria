using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Resumen;

public class IndexModel : PageModel
{
    // Metadatos y Saludo
    public string FechaKicker { get; set; } = "MARTES, 18 DE NOVIEMBRE";
    public string Saludo { get; set; } = "Andrea";
    public string Subtitulo { get; set; } = "Este es el estado de la operación en Jardines del Valle.";
    public string SucursalNombre { get; set; } = "Jardines del Valle";

    // 4 KPI Cards Superiores (Captura 1 de Figma · SF-089)
    public decimal VentasMes { get; set; } = 486500m;
    public string VentasMesComparativa { get; set; } = "+12.4% vs. mes anterior";

    public decimal CobranzaMensual { get; set; } = 238240m;
    public string CobranzaMensualComparativa { get; set; } = "+8.1% vs. mes anterior";

    public int ContratosActivos { get; set; } = 128;
    public string ContratosNuevos { get; set; } = "6 nuevos este mes";

    public int PagosVencidos { get; set; } = 8;
    public decimal MontoPorRecuperar { get; set; } = 24860m;

    // Disponibilidad de Lotes
    public int LotesDisponibles { get; set; } = 64;
    public int Lotes2Gavetas { get; set; } = 28;
    public int Lotes3Gavetas { get; set; } = 21;
    public int Lotes4Gavetas { get; set; } = 15;
    public int OcupacionPorcentaje { get; set; } = 64;

    // Gráfica de Ventas y Cobranza (Últimos 6 meses)
    public List<VentasCobranzaMesDto> DatosGrafica { get; set; } = new();

    // Actividad Reciente
    public List<MovimientoRecienteDto> ActividadReciente { get; set; } = new();

    public void OnGet()
    {
        DatosGrafica = new List<VentasCobranzaMesDto>
        {
            new("Jun", 320000m, 180000m, 64, 36),
            new("Jul", 380000m, 210000m, 76, 42),
            new("Ago", 340000m, 195000m, 68, 39),
            new("Sep", 420000m, 230000m, 84, 46),
            new("Oct", 450000m, 225000m, 90, 45),
            new("Nov", 486500m, 238240m, 97, 48)
        };

        ActividadReciente = new List<MovimientoRecienteDto>
        {
            new("Roberto Méndez", "roberto.m@email.mx", "RM", "Mensualidad", "CTR-2025-0182", 6248.50m, "Aplicado", "Hoy, 10:42"),
            new("Elena Vargas", "elena.vargas@email.mx", "EV", "Mensualidad", "CTR-2024-0118", 6248.50m, "Pendiente", "Hoy, 09:15"),
            new("Laura Castañeda", "laura.c@email.mx", "LC", "Abono a capital", "CTR-2024-0095", 15000.00m, "Aplicado", "Ayer, 16:30"),
            new("Jorge Salinas", "jorge.s@email.mx", "JS", "Liquidación anticipada", "CTR-2023-0042", 42800.00m, "Aplicado", "Ayer, 14:10")
        };
    }
}

public record VentasCobranzaMesDto(string Mes, decimal Ventas, decimal Cobranza, int PorcentajeVentas, int PorcentajeCobranza);

public record MovimientoRecienteDto(string Cliente, string Correo, string Iniciales, string Movimiento, string Referencia, decimal Importe, string Estado, string Fecha);
