using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia;

public static class LoteSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.Lotes.AnyAsync())
        {
            return;
        }

        var lotes = new List<Lote>
        {
            Lote.Crear("JN-A-042", "Jardín Norte", "Andador A · Fila 4", 2, new Dinero(120000m)),
            Lote.Crear("JN-A-043", "Jardín Norte", "Andador A · Fila 4", 2, new Dinero(120000m)),
            Lote.Crear("JC-C-118", "Jardín Central", "Andador C · Fila 11", 3, new Dinero(150000m)),
            Lote.Crear("JN-D-065", "Jardín Norte", "Andador D · Fila 6", 3, new Dinero(150000m)),
        };

        var loteReservado = Lote.Crear("RO-B-027", "Robles", "Andador B · Fila 2", 4, new Dinero(180000m));
        loteReservado.Reservar();
        lotes.Add(loteReservado);

        var loteVendido = Lote.Crear("LM-A-014", "Las Magnolias", "Andador A · Fila 1", 2, new Dinero(120000m));
        loteVendido.MarcarVendido();
        lotes.Add(loteVendido);

        await db.Lotes.AddRangeAsync(lotes);
        await db.SaveChangesAsync();
    }
}
