using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia;

/// <summary>
/// Seeder idempotente del catálogo inicial de lotes para Jardín Norte, Jardín Central, Robles y Las Magnolias (SF-040).
/// </summary>
public static class LoteSeed
{
    private sealed record SeedLoteSpec(
        string Clave,
        string Seccion,
        string AndadorFila,
        int Gavetas,
        decimal PrecioContado,
        EstadoLote Estado);

    private static readonly SeedLoteSpec[] LotesOficiales =
    [
        new("JN-A-042", "Jardín Norte", "Andador A · Fila 4", 2, 120000m, EstadoLote.Disponible),
        new("JC-C-118", "Jardín Central", "Andador C · Fila 11", 3, 150000m, EstadoLote.Disponible),
        new("RO-B-027", "Robles", "Andador B · Fila 2", 4, 180000m, EstadoLote.Reservado),
        new("JN-D-065", "Jardín Norte", "Andador D · Fila 6", 3, 150000m, EstadoLote.Disponible),
        new("LM-A-014", "Las Magnolias", "Andador A · Fila 1", 2, 120000m, EstadoLote.Vendido),
        new("JN-A-043", "Jardín Norte", "Andador A · Fila 4", 2, 120000m, EstadoLote.Disponible),
        new("JN-A-044", "Jardín Norte", "Andador A · Fila 4", 2, 120000m, EstadoLote.Disponible),
        new("JN-A-045", "Jardín Norte", "Andador A · Fila 4", 3, 150000m, EstadoLote.Disponible),
        new("JN-A-046", "Jardín Norte", "Andador A · Fila 4", 3, 150000m, EstadoLote.Disponible),
        new("RO-E-081", "Robles", "Andador E · Fila 8", 4, 180000m, EstadoLote.Disponible),
        new("JN-A-036", "Jardín Norte", "Andador A · Fila 3", 2, 120000m, EstadoLote.Disponible)
    ];

    /// <summary>
    /// Resuelve <see cref="AppDbContext"/> desde el contenedor de dependencias y ejecuta la carga inicial de lotes.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedAsync(db, cancellationToken);
    }

    /// <summary>
    /// Inserta de forma idempotente los lotes oficiales del catálogo cuando su clave no existe en base de datos.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        var clavesExistentes = (await db.Lotes
            .AsNoTracking()
            .Select(l => l.Clave)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var nuevos = new List<Lote>();

        foreach (var spec in LotesOficiales)
        {
            if (clavesExistentes.Contains(spec.Clave))
            {
                continue;
            }

            var lote = Lote.Crear(spec.Clave, spec.Seccion, spec.AndadorFila, spec.Gavetas, new Dinero(spec.PrecioContado));
            AplicarEstadoInicial(lote, spec.Estado);
            nuevos.Add(lote);
        }

        if (nuevos.Count > 0)
        {
            await db.Lotes.AddRangeAsync(nuevos, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static void AplicarEstadoInicial(Lote lote, EstadoLote estado)
    {
        switch (estado)
        {
            case EstadoLote.Reservado:
                lote.Reservar();
                break;
            case EstadoLote.Vendido:
                lote.MarcarVendido();
                break;
            case EstadoLote.Mantenimiento:
                lote.ActualizarDatosAdmin(lote.Seccion, lote.AndadorFila, lote.Gavetas, lote.PrecioContado, EstadoLote.Mantenimiento);
                break;
        }
    }
}
