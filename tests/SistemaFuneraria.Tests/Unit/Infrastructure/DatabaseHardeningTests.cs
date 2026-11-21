using System.Diagnostics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Infrastructure;

/// <summary>
/// Pruebas unitarias de hardening de base de datos, índices compuestos, AsNoTracking y prevención de consultas N+1 (SF-095, RNF-01).
/// </summary>
[Trait("Ticket", "SF-095")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Unit")]
public class DatabaseHardeningTests
{
    private static AppDbContext CrearContextoSoloModelo()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=design_time_only;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }

    private static AppDbContext CrearContextoInMemory(string nombreBd)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nombreBd)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public void ModeloEfCore_DebeRegistrarIndicesCompuestosDeHardening()
    {
        using var db = CrearContextoSoloModelo();
        var model = db.GetService<IDesignTimeModel>().Model;

        var entityLote = model.FindEntityType(typeof(Lote))!;
        var idxLote = entityLote.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "ix_lotes_seccion_estado");
        idxLote.Should().NotBeNull();
        idxLote!.Properties.Select(p => p.Name).Should().ContainInOrder(nameof(Lote.Seccion), nameof(Lote.Estado));

        var entityMensualidad = model.FindEntityType(typeof(Mensualidad))!;
        var idxMensualidad = entityMensualidad.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "ix_mensualidades_estado_vencimiento");
        idxMensualidad.Should().NotBeNull();
        idxMensualidad!.Properties.Select(p => p.Name).Should().ContainInOrder(nameof(Mensualidad.Estado), nameof(Mensualidad.FechaVencimiento));

        var entityVenta = model.FindEntityType(typeof(Venta))!;
        var idxVenta = entityVenta.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "ix_ventas_cliente_fecha");
        idxVenta.Should().NotBeNull();
        idxVenta!.Properties.Select(p => p.Name).Should().ContainInOrder(nameof(Venta.ClienteId), nameof(Venta.FechaVenta));
    }

    [Fact]
    public async Task ConsultaLectura_ConAsNoTracking_DebeDejarCeroEntidadesEnChangeTracker()
    {
        using var db = CrearContextoInMemory(Guid.NewGuid().ToString());

        var lote = Lote.Crear("JC-A-001", "Jardines del Carmen", "Andador 1", 2, new Dinero(120000m));
        db.Lotes.Add(lote);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var lotesLectura = await db.Lotes.AsNoTracking().ToListAsync();

        lotesLectura.Should().NotBeEmpty();
        db.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task ConsultaVentaConFinanciamiento_DebeResolverAtomaSinLazyLoading()
    {
        using var db = CrearContextoInMemory(Guid.NewGuid().ToString());

        var ventaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var loteId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid().ToString();

        var venta = Venta.CrearFinanciada(
            "CTR-2025-0999",
            clienteId,
            loteId,
            vendedorId,
            new Dinero(120000m),
            DateTimeOffset.UtcNow);

        var filas = GeneradorAmortizacion.Generar(96000m, 12);
        var financiamiento = Financiamiento.Crear(
            venta.Id,
            20m,
            new Dinero(24000m),
            new Dinero(96000m),
            12,
            0.16m,
            filas,
            new DateOnly(2025, 1, 1));

        db.Ventas.Add(venta);
        db.Financiamientos.Add(financiamiento);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var proyeccion = await db.Ventas
            .AsNoTracking()
            .Where(v => v.Id == venta.Id)
            .Select(v => new
            {
                v.Id,
                v.FolioContrato,
                Financiamiento = db.Financiamientos
                    .Where(f => f.VentaId == v.Id)
                    .Select(f => new
                    {
                        f.Id,
                        f.SaldoInsoluto,
                        TotalMensualidades = f.Mensualidades.Count
                    })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        proyeccion.Should().NotBeNull();
        proyeccion!.FolioContrato.Should().Be("CTR-2025-0999");
        proyeccion.Financiamiento.Should().NotBeNull();
        proyeccion.Financiamiento!.TotalMensualidades.Should().Be(12);
        db.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task MedicionRendimiento_SimulacionYConsultas_DebeEjecutarEnMenosDe200ms()
    {
        using var db = CrearContextoInMemory(Guid.NewGuid().ToString());

        var sw = Stopwatch.StartNew();

        for (int i = 0; i < 50; i++)
        {
            var simulacion = GeneradorAmortizacion.Generar(96000m, 12);
            simulacion.Should().HaveCount(12);
        }

        var lote = Lote.Crear("JC-B-001", "Jardines del Carmen", "Andador 2", 3, new Dinero(150000m));
        db.Lotes.Add(lote);
        await db.SaveChangesAsync();

        var lotes = await db.Lotes.AsNoTracking().Where(l => l.Seccion == "Jardines del Carmen").ToListAsync();
        lotes.Should().NotBeEmpty();

        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }
}
