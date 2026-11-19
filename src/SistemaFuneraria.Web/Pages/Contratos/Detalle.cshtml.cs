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

    public string Folio { get; set; } = "CTR-2025-0189";
    public string Titular { get; set; } = "Laura Castañeda";
    public string Telefono { get; set; } = "55 1284 9021";
    public string Email { get; set; } = "laura.c@email.mx";
    public string Lote { get; set; } = "JC-C-118 (Jardín Central, 3 gavetas)";
    public string Estado { get; set; } = "Al corriente";
    public string EstadoBadgeClass { get; set; } = "bg-success-subtle text-success border border-success-subtle";

    public decimal PrecioPactado { get; set; } = 150000.00m;
    public decimal Enganche { get; set; } = 22500.00m;
    public decimal EnganchePorcentaje { get; set; } = 15.0m;
    public decimal SaldoInsoluto { get; set; } = 127500.00m;
    public decimal MensualidadRegular { get; set; } = 6187.50m;
    public string ProximoVencimiento { get; set; } = "02 dic. 2025";
    public int PlazoMeses { get; set; } = 24;
    public decimal TasaAnual { get; set; } = 20.0m;

    public List<CuotaAmortizacionDto> Amortizacion { get; set; } = new();

    public void OnGet(string? folio)
    {
        if (!string.IsNullOrWhiteSpace(folio))
        {
            Folio = folio.Trim();
        }

        // Cargar datos contextuales según folio
        if (Folio == "CTR-2024-0118")
        {
            Titular = "Elena Vargas";
            Telefono = "55 9021 3364";
            Email = "elena.v@email.mx";
            Lote = "RO-B-027 (Rosales, 2 gavetas)";
            Estado = "Vencido";
            EstadoBadgeClass = "bg-danger-subtle text-danger border border-danger-subtle";
            PrecioPactado = 120000.00m;
            Enganche = 24000.00m;
            SaldoInsoluto = 76180.40m;
            ProximoVencimiento = "02 oct. 2025";
        }
        else if (Folio == "CTR-2025-0182")
        {
            Titular = "Roberto Méndez";
            Telefono = "55 4390 1178";
            Email = "roberto.m@email.mx";
            Lote = "JC-C-118 (Jardín Central, 3 gavetas)";
            Estado = "Al corriente";
            EstadoBadgeClass = "bg-success-subtle text-success border border-success-subtle";
            PrecioPactado = 120000.00m;
            Enganche = 24000.00m;
            SaldoInsoluto = 58743.20m;
            ProximoVencimiento = "18 nov. 2025";
        }
        else if (Folio == "CTR-2025-0144")
        {
            Titular = "Jorge Salinas";
            Telefono = "55 6653 0871";
            Email = "jorge.s@email.mx";
            Lote = "RO-E-081 (Rosales, 4 gavetas)";
            Estado = "En revisión";
            EstadoBadgeClass = "bg-warning-subtle text-warning-emphasis border border-warning-subtle";
            PrecioPactado = 150000.00m;
            Enganche = 30000.00m;
            SaldoInsoluto = 94265.80m;
            ProximoVencimiento = "17 nov. 2025";
        }

        GenerarAmortizacion();
    }

    private void GenerarAmortizacion()
    {
        Amortizacion.Clear();
        decimal montoFinanciado = PrecioPactado - Enganche;
        decimal tasaMensual = (TasaAnual / 100m) / 12m;

        // Cuota fija mensual correspondiente al plan del contrato
        decimal cuotaFija = MensualidadRegular > 0 ? MensualidadRegular : Math.Round(montoFinanciado * (tasaMensual / (decimal)(1.0 - Math.Pow(1.0 + (double)tasaMensual, -PlazoMeses))), 2);

        decimal saldo = montoFinanciado;
        var fechaInicio = new DateTime(2025, 9, 2);

        for (int i = 1; i <= PlazoMeses; i++)
        {
            decimal saldoInicial = saldo;
            decimal interes = Math.Round(saldoInicial * tasaMensual, 2);
            decimal capital = (i == PlazoMeses) ? saldoInicial : Math.Round(cuotaFija - interes, 2);
            decimal cuotaTotal = (i == PlazoMeses) ? capital + interes : cuotaFija;
            decimal saldoFinal = (i == PlazoMeses) ? 0.00m : Math.Round(saldoInicial - capital, 2);

            saldo = saldoFinal;
            var fechaCuota = fechaInicio.AddMonths(i - 1);
            string vencimiento = fechaCuota.ToString("dd MMM. yyyy");

            string estadoCuota;
            string badgeClass;

            if (i <= 3)
            {
                estadoCuota = "Pagada";
                badgeClass = "badge bg-success-subtle text-success border border-success-subtle";
            }
            else
            {
                estadoCuota = "Pendiente";
                badgeClass = "badge bg-light text-muted border";
            }

            Amortizacion.Add(new CuotaAmortizacionDto(
                i,
                vencimiento,
                saldoInicial,
                capital,
                interes,
                cuotaTotal,
                saldoFinal,
                estadoCuota,
                badgeClass
            ));
        }
    }
}
