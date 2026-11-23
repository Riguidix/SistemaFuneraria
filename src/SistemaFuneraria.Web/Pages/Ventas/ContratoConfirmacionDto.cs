namespace SistemaFuneraria.Web.Pages.Ventas;

public class ContratoConfirmacionDto
{
    // 1. Cliente
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;

    // 2. Lote
    public string LoteClave { get; set; } = string.Empty;
    public string LoteJardin { get; set; } = string.Empty;
    public int CapacidadGavetas { get; set; }

    // 3. Precio y Enganche
    public decimal PrecioTotal { get; set; }
    public decimal MontoEnganche { get; set; }
    public string Modalidad { get; set; } = "Credito";

    // 4. Financiamiento
    public int PlazoMeses { get; set; }
    public decimal MensualidadEstimada { get; set; }
    public decimal TasaAnual { get; set; }

    // Checkbox de confirmación
    public bool DatosRevisadosConCliente { get; set; }

    // Propiedades calculadas para presentación
    public string ResumenCliente => ClienteNombre.Trim();
    public string ResumenLote => $"{LoteClave} · {LoteJardin}".Trim();
    public string ResumenPrecio => $"${PrecioTotal:#,##0.00}";

    public string ResumenFinanciamiento => Modalidad.Equals("Contado", System.StringComparison.OrdinalIgnoreCase)
        ? "Liquidación inmediata · 0% interés"
        : $"{PlazoMeses} pagos de ${MensualidadEstimada:#,##0.00}";

    public bool PuedeGenerarContrato()
    {
        return DatosRevisadosConCliente &&
               !string.IsNullOrWhiteSpace(ClienteNombre) &&
               !string.IsNullOrWhiteSpace(LoteClave);
    }
}
