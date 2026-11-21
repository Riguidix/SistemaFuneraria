using System.Text;

namespace SistemaFuneraria.Web.Pages.Clientes;

/// <summary>
/// Utilidad para la exportación del directorio de clientes a formato CSV con BOM UTF-8 (Ticket SF-037).
/// </summary>
public static class ClientesExportadorCsv
{
    public const string CabeceraCsv = "Cliente,Teléfono,CURP,Contratos,Saldo,Último Pago,Estado";

    public static string GenerarNombreArchivo(DateTime fecha)
    {
        return $"directorio_clientes_{fecha:yyyyMMdd}.csv";
    }

    public static byte[] GenerarCsvBytes(IEnumerable<ClienteExportDto> clientes)
    {
        var sb = new StringBuilder();
        sb.Append('\uFEFF'); // BOM UTF-8 para Excel en español
        sb.AppendLine(CabeceraCsv);

        foreach (var c in clientes)
        {
            var fila = $"{Escapar(c.Nombre)},{Escapar(c.Telefono)},{Escapar(c.Curp)},{Escapar(c.Contratos)},{Escapar(c.Saldo)},{Escapar(c.UltimoPago)},{Escapar(c.Estado)}";
            sb.AppendLine(fila);
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escapar(string valor)
    {
        if (string.IsNullOrEmpty(valor)) return string.Empty;
        if (valor.Contains(',') || valor.Contains('"') || valor.Contains('\n') || valor.Contains('\r'))
        {
            return $"\"{valor.Replace("\"", "\"\"")}\"";
        }
        return valor;
    }
}
