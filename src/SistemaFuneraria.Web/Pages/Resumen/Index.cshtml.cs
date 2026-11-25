using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Resumen;

public class IndexModel : PageModel
{
    private static readonly CultureInfo CulturaEsMx = CultureInfo.GetCultureInfo("es-MX");

    // Metadatos y Saludo
    public string FechaKicker { get; set; } = FormatearFechaActual();
    public string Saludo { get; set; } = "Usuario";
    public string Subtitulo { get; set; } = "Este es el estado de la operación en Jardines del Valle.";
    public string SucursalNombre { get; set; } = "Jardines del Valle";
    public int AnioActual { get; set; } = DateTime.Now.Year;

    // 4 KPI Cards Superiores
    public decimal VentasMes { get; set; }
    public string VentasMesComparativa { get; set; } = "Sin comparativa previa";

    public decimal CobranzaMensual { get; set; }
    public string CobranzaMensualComparativa { get; set; } = "Sin comparativa previa";

    public int ContratosActivos { get; set; }
    public string ContratosNuevos { get; set; } = "0 nuevos este mes";

    public int PagosVencidos { get; set; }
    public decimal MontoPorRecuperar { get; set; }

    // Disponibilidad de Lotes
    public int LotesDisponibles { get; set; }
    public int Lotes2Gavetas { get; set; }
    public int Lotes3Gavetas { get; set; }
    public int Lotes4Gavetas { get; set; }
    public decimal OcupacionPorcentaje { get; set; }

    // Gráfica de Ventas y Cobranza
    public List<VentasCobranzaMesDto> DatosGrafica { get; set; } = new();

    // Actividad Reciente
    public List<MovimientoRecienteDto> ActividadReciente { get; set; } = new();

    public void OnGet()
    {
        FechaKicker = FormatearFechaActual();
        AnioActual = DateTime.Now.Year;
        DatosGrafica = new List<VentasCobranzaMesDto>();
        ActividadReciente = new List<MovimientoRecienteDto>();
    }

    private static string FormatearFechaActual() =>
        DateTime.Now.ToString("dddd, d 'de' MMMM", CulturaEsMx).ToUpper(CulturaEsMx);
}

public record VentasCobranzaMesDto(string Mes, decimal Ventas, decimal Cobranza, int PorcentajeVentas, int PorcentajeCobranza);

public record MovimientoRecienteDto(string Cliente, string Correo, string Iniciales, string Movimiento, string Referencia, decimal Importe, string Estado, string Fecha);
