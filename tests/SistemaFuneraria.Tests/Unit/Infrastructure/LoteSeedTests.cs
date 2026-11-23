using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;

namespace SistemaFuneraria.Tests.Unit.Infrastructure;

/// <summary>
/// Pruebas unitarias de idempotencia y datos oficiales para <see cref="LoteSeed"/> (SF-040).
/// </summary>
[Trait("Ticket", "SF-040")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Unit")]
public class LoteSeedTests
{
    private static AppDbContext CrearContextoInMemory(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_EjecutadoDosVeces_DebeSerIdempotenteYPoblarCatalogoOficial()
    {
        using var db = CrearContextoInMemory($"lote_seed_{Guid.NewGuid():N}");

        await LoteSeed.SeedAsync(db);
        await LoteSeed.SeedAsync(db);

        var lotes = await db.Lotes.AsNoTracking().ToListAsync();
        lotes.Should().HaveCount(11);

        var jn042 = lotes.Single(l => l.Clave == "JN-A-042");
        jn042.Seccion.Should().Be("Jardín Norte");
        jn042.AndadorFila.Should().Be("Andador A · Fila 4");
        jn042.Gavetas.Should().Be(2);
        jn042.PrecioContado.Should().Be(new Dinero(120000m));
        jn042.Estado.Should().Be(EstadoLote.Disponible);

        var jc118 = lotes.Single(l => l.Clave == "JC-C-118");
        jc118.Seccion.Should().Be("Jardín Central");
        jc118.Gavetas.Should().Be(3);
        jc118.PrecioContado.Should().Be(new Dinero(150000m));
        jc118.Estado.Should().Be(EstadoLote.Disponible);

        var ro027 = lotes.Single(l => l.Clave == "RO-B-027");
        ro027.Seccion.Should().Be("Robles");
        ro027.Gavetas.Should().Be(4);
        ro027.PrecioContado.Should().Be(new Dinero(180000m));
        ro027.Estado.Should().Be(EstadoLote.Reservado);

        var jn065 = lotes.Single(l => l.Clave == "JN-D-065");
        jn065.Seccion.Should().Be("Jardín Norte");
        jn065.Gavetas.Should().Be(3);
        jn065.PrecioContado.Should().Be(new Dinero(150000m));
        jn065.Estado.Should().Be(EstadoLote.Disponible);

        var lm014 = lotes.Single(l => l.Clave == "LM-A-014");
        lm014.Seccion.Should().Be("Las Magnolias");
        lm014.Gavetas.Should().Be(2);
        lm014.PrecioContado.Should().Be(new Dinero(120000m));
        lm014.Estado.Should().Be(EstadoLote.Vendido);

        lotes.Select(l => l.Clave).Should().Contain(new[]
        {
            "JN-A-043", "JN-A-044", "JN-A-045", "JN-A-046", "RO-E-081", "JN-A-036"
        });
    }
}
