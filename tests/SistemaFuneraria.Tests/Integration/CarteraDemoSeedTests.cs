using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Infrastructure.Persistencia;

namespace SistemaFuneraria.Tests.Integration;

/// <summary>
/// Pruebas de integración para validar la carga idempotente de contratos semilla de Figma (SF-072).
/// </summary>
[Trait("Ticket", "SF-072")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Integration")]
public class CarteraDemoSeedTests
{
    private static AppDbContext CrearContexto(string nombreBd)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nombreBd)
            .Options;

        return new AppDbContext(options);
    }

    /// <summary>
    /// Valida que la semilla inserte los 4 contratos oficiales de Figma y que una segunda ejecución no duplique registros.
    /// </summary>
    [Fact]
    public async Task SeedAsync_EjecutadoDosVeces_DebeSerIdempotenteYCrearExactamente4ContratosFigma()
    {
        using var db = CrearContexto($"cartera_demo_seed_{Guid.NewGuid():N}");

        await CarteraDemoSeed.SeedAsync(db);
        await CarteraDemoSeed.SeedAsync(db);

        var ventas = await db.Ventas.ToListAsync();

        ventas.Should().HaveCount(4);

        var c1 = await db.Ventas.SingleOrDefaultAsync(v => v.FolioContrato == "CTR-2025-0189");
        c1.Should().NotBeNull();
        c1!.Estado.Should().Be(EstadoVenta.Activo);

        var fin1 = await db.Financiamientos.Include(f => f.Mensualidades).SingleAsync(f => f.VentaId == c1.Id);
        fin1.SaldoInsoluto.Monto.Should().Be(127500.00m);
        fin1.CuotaMensualActual.Monto.Should().Be(6248.50m);
        fin1.Estado.Should().Be(EstadoFinanciamiento.Activo);

        var lote1 = await db.Lotes.SingleAsync(l => l.Id == c1.LoteId);
        lote1.Clave.Should().Be("JC-C-118");
        lote1.Estado.Should().Be(EstadoLote.Vendido);

        var cliente1 = await db.Clientes.SingleAsync(c => c.Id == c1.ClienteId);
        cliente1.Nombre.Should().Be("Laura");
        cliente1.Apellidos.Should().Be("Castañeda");

        var c2 = await db.Ventas.SingleOrDefaultAsync(v => v.FolioContrato == "CTR-2025-0182");
        c2.Should().NotBeNull();
        c2!.Estado.Should().Be(EstadoVenta.Activo);

        var fin2 = await db.Financiamientos.Include(f => f.Mensualidades).SingleAsync(f => f.VentaId == c2.Id);
        fin2.SaldoInsoluto.Monto.Should().Be(58743.20m);
        fin2.Estado.Should().Be(EstadoFinanciamiento.Activo);

        var lote2 = await db.Lotes.SingleAsync(l => l.Id == c2.LoteId);
        lote2.Clave.Should().Be("JN-A-036");
        lote2.Estado.Should().Be(EstadoLote.Vendido);

        var cliente2 = await db.Clientes.SingleAsync(c => c.Id == c2.ClienteId);
        cliente2.Nombre.Should().Be("Roberto");
        cliente2.Apellidos.Should().Be("Méndez");

        var c3 = await db.Ventas.SingleOrDefaultAsync(v => v.FolioContrato == "CTR-2024-0118");
        c3.Should().NotBeNull();
        c3!.Estado.Should().Be(EstadoVenta.Activo);

        var fin3 = await db.Financiamientos.Include(f => f.Mensualidades).SingleAsync(f => f.VentaId == c3.Id);
        fin3.SaldoInsoluto.Monto.Should().Be(76180.40m);
        fin3.Estado.Should().Be(EstadoFinanciamiento.EnMora);
        fin3.Mensualidades.Should().Contain(m => m.Estado == EstadoMensualidad.Vencida && m.MontoTotalCuota.Monto == 6248.50m);

        var lote3 = await db.Lotes.SingleAsync(l => l.Id == c3.LoteId);
        lote3.Clave.Should().Be("RO-B-027");
        lote3.Estado.Should().Be(EstadoLote.Vendido);

        var cliente3 = await db.Clientes.SingleAsync(c => c.Id == c3.ClienteId);
        cliente3.Nombre.Should().Be("Elena");
        cliente3.Apellidos.Should().Be("Vargas");

        var c4 = await db.Ventas.SingleOrDefaultAsync(v => v.FolioContrato == "CTR-2025-0144");
        c4.Should().NotBeNull();
        c4!.Estado.Should().Be(EstadoVenta.EnRevision);

        var fin4 = await db.Financiamientos.Include(f => f.Mensualidades).SingleAsync(f => f.VentaId == c4.Id);
        fin4.SaldoInsoluto.Monto.Should().Be(94265.80m);
        fin4.Mensualidades.Should().Contain(m => m.Estado == EstadoMensualidad.Pendiente && m.MontoTotalCuota.Monto == 5612.30m);

        var lote4 = await db.Lotes.SingleAsync(l => l.Id == c4.LoteId);
        lote4.Clave.Should().Be("JN-D-065");
        lote4.Estado.Should().Be(EstadoLote.Vendido);

        var cliente4 = await db.Clientes.SingleAsync(c => c.Id == c4.ClienteId);
        cliente4.Nombre.Should().Be("Jorge");
        cliente4.Apellidos.Should().Be("Salinas");
    }

    /// <summary>
    /// Valida que la sobrecarga con <see cref="IServiceProvider"/> resuelva el contexto y ejecute la carga.
    /// </summary>
    [Fact]
    public async Task SeedAsync_ConServiceProvider_DebePoblarCorrectamente()
    {
        var services = new ServiceCollection();
        var nombreBd = $"cartera_demo_sp_{Guid.NewGuid():N}";
        services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(nombreBd));

        using var sp = services.BuildServiceProvider();

        await CarteraDemoSeed.SeedAsync(sp);

        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var totalVentas = await db.Ventas.CountAsync();
        totalVentas.Should().Be(4);
    }
}
