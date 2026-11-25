using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SistemaFuneraria.Web.Pages.Contratos;

public class DetalleModel : PageModel
{
    public record CuotaAmortizacionDto(
        int Numero,
        string Vencimiento,
        decimal SaldoInicial,
        decimal Capital,
        decimal Interes,
        decimal CuotaTotal,
        decimal SaldoFinal,
        string Estado,
        string EstadoBadgeClass
    );

    public string? FolioParam { get; set; }
    public string? IdParam { get; set; }
    public string? LoteClaveParam { get; set; }

    public string Folio { get; set; } = "Cargando...";
    public string Titular { get; set; } = "Cargando expediente...";
    public string Telefono { get; set; } = "—";
    public string Email { get; set; } = "—";
    public string Lote { get; set; } = "—";
    public string Estado { get; set; } = "—";
    public string EstadoBadgeClass { get; set; } = "bg-light text-muted border";

    public decimal PrecioPactado { get; set; }
    public decimal Enganche { get; set; }
    public decimal EnganchePorcentaje { get; set; }
    public decimal SaldoInsoluto { get; set; }
    public decimal MensualidadRegular { get; set; }
    public string ProximoVencimiento { get; set; } = "—";
    public int PlazoMeses { get; set; }
    public decimal TasaAnual { get; set; }

    public List<CuotaAmortizacionDto> Amortizacion { get; set; } = new();

    public void OnGet(string? folio = null, string? id = null, string? loteClave = null)
    {
        FolioParam = string.IsNullOrWhiteSpace(folio) ? null : folio.Trim();
        IdParam = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
        LoteClaveParam = string.IsNullOrWhiteSpace(loteClave) ? null : loteClave.Trim();

        if (!string.IsNullOrWhiteSpace(FolioParam))
        {
            Folio = FolioParam;
        }
        else if (!string.IsNullOrWhiteSpace(LoteClaveParam))
        {
            Folio = $"Lote {LoteClaveParam}";
            Lote = LoteClaveParam;
        }

        Amortizacion = new List<CuotaAmortizacionDto>();
    }
}
