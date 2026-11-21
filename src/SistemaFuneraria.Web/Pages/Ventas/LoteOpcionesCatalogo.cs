using System.Collections.Generic;
using System.Linq;

namespace SistemaFuneraria.Web.Pages.Ventas;

public class LoteOpcionDto
{
    public string Clave { get; set; } = string.Empty;
    public string Jardin { get; set; } = string.Empty;
    public string Andador { get; set; } = string.Empty;
    public int CapacidadGavetas { get; set; }
    public decimal Precio { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string SlotId { get; set; } = string.Empty;

    public string PrecioFormateado => $"${Precio:#,##0.00}";
    public string Descripcion => $"{CapacidadGavetas} gavetas · {Andador}";
}

public static class LoteOpcionesCatalogo
{
    private static readonly List<LoteOpcionDto> Opciones = new()
    {
        new LoteOpcionDto
        {
            Clave = "JN-A-042",
            Jardin = "Jardín Norte",
            Andador = "Andador A",
            CapacidadGavetas = 2,
            Precio = 120000.00m,
            Tipo = "Lote Básico",
            SlotId = "42"
        },
        new LoteOpcionDto
        {
            Clave = "JC-C-118",
            Jardin = "Jardín Central",
            Andador = "Andador C",
            CapacidadGavetas = 3,
            Precio = 150000.00m,
            Tipo = "Lote Familiar",
            SlotId = "44"
        },
        new LoteOpcionDto
        {
            Clave = "RO-E-081",
            Jardin = "Robles",
            Andador = "Andador E",
            CapacidadGavetas = 4,
            Precio = 180000.00m,
            Tipo = "Lote Memorial",
            SlotId = "46"
        }
    };

    public static List<LoteOpcionDto> ObtenerOpcionesDisponibles()
    {
        return Opciones.ToList();
    }

    public static LoteOpcionDto? BuscarPorClave(string clave)
    {
        return Opciones.FirstOrDefault(o => o.Clave.Equals(clave, System.StringComparison.OrdinalIgnoreCase));
    }
}
