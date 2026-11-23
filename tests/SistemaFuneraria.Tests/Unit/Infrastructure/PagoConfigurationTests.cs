using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Infrastructure.Persistencia;

namespace SistemaFuneraria.Tests.Unit.Infrastructure;

/// <summary>
/// Pruebas unitarias de metadatos EF Core para la configuración de Pago, índices y secuencia seq_pagos_folio (SF-076).
/// </summary>
[Trait("Ticket", "SF-076")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Unit")]
public class PagoConfigurationTests
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

    /// <summary>
    /// Valida que el modelo EF Core registre el DbSet Pagos y la secuencia oficial seq_pagos_folio.
    /// </summary>
    [Fact]
    public void ModeloEfCore_DebeRegistrarDbSetPagosYSecuenciaPagosFolio()
    {
        using var db = CrearContextoSoloModelo();
        var model = db.GetService<IDesignTimeModel>().Model;

        db.Pagos.Should().NotBeNull();

        var secuencia = model.FindSequence("seq_pagos_folio");
        secuencia.Should().NotBeNull();
        secuencia!.StartValue.Should().Be(1);
        secuencia.IncrementBy.Should().Be(1);
    }

    /// <summary>
    /// Valida que PagoConfiguration mapee la tabla pagos, tipos numéricos, conversiones, restricciones y FKs restrictivas.
    /// </summary>
    [Fact]
    public void ModeloEfCore_PagoConfiguration_DebeMapearTablaColumnasCheckConstraintsYFks()
    {
        using var db = CrearContextoSoloModelo();
        var model = db.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(Pago))!;
        entityType.Should().NotBeNull();
        entityType.GetTableName().Should().Be("pagos");

        var store = StoreObjectIdentifier.Table("pagos", null);
        entityType.FindProperty(nameof(Pago.Folio))!.GetColumnName(store).Should().Be("folio");
        entityType.FindProperty(nameof(Pago.CajeroId))!.GetColumnName(store).Should().Be("cajero_id");
        entityType.FindProperty(nameof(Pago.Tipo))!.GetColumnType(store).Should().BeOneOf("character varying(25)", "varchar(25)");
        entityType.FindProperty(nameof(Pago.Metodo))!.GetColumnType(store).Should().BeOneOf("character varying(25)", "varchar(25)");
        entityType.FindProperty(nameof(Pago.Referencia))!.GetColumnName(store).Should().Be("referencia");
        entityType.FindProperty(nameof(Pago.Referencia))!.IsNullable.Should().BeTrue();
        entityType.FindProperty(nameof(Pago.MontoTotal))!.GetColumnType(store).Should().Be("numeric(18,2)");
        entityType.FindProperty(nameof(Pago.MontoCapital))!.GetColumnType(store).Should().Be("numeric(18,2)");
        entityType.FindProperty(nameof(Pago.MontoInteres))!.GetColumnType(store).Should().Be("numeric(18,2)");
        entityType.FindProperty(nameof(Pago.SaldoAnterior))!.GetColumnType(store).Should().Be("numeric(18,2)");
        entityType.FindProperty(nameof(Pago.SaldoPosterior))!.GetColumnType(store).Should().Be("numeric(18,2)");
        entityType.FindProperty(nameof(Pago.ClaveIdempotencia))!.GetColumnName(store).Should().Be("clave_idempotencia");

        var fks = entityType.GetForeignKeys().ToList();
        fks.Should().Contain(fk => fk.PrincipalEntityType.ClrType == typeof(Venta) && fk.DeleteBehavior == DeleteBehavior.Restrict);
        fks.Should().Contain(fk => fk.PrincipalEntityType.ClrType == typeof(Financiamiento) && fk.DeleteBehavior == DeleteBehavior.Restrict);
        fks.Should().Contain(fk => fk.PrincipalEntityType.ClrType == typeof(Mensualidad) && fk.DeleteBehavior == DeleteBehavior.Restrict);
        fks.Should().Contain(fk => fk.PrincipalEntityType.ClrType == typeof(IdentityUser) && fk.DeleteBehavior == DeleteBehavior.Restrict);

        var idxFolio = entityType.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "ix_pagos_folio");
        idxFolio.Should().NotBeNull();
        idxFolio!.IsUnique.Should().BeTrue();

        var idxIdempotencia = entityType.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "ix_pagos_clave_idempotencia");
        idxIdempotencia.Should().NotBeNull();
        idxIdempotencia!.IsUnique.Should().BeTrue();

        var checks = entityType.GetCheckConstraints().Select(c => c.Name).ToList();
        checks.Should().Contain("pagos_tipo_pago_check");
        checks.Should().Contain("pagos_metodo_pago_check");
        checks.Should().Contain("pagos_monto_total_check");
        checks.Should().Contain("pagos_monto_capital_check");
        checks.Should().Contain("pagos_monto_interes_check");
        checks.Should().Contain("pagos_saldo_anterior_check");
        checks.Should().Contain("pagos_saldo_posterior_check");
    }

    /// <summary>
    /// Valida que el helper ObtenerSiguienteFolioPagoAsync genere folios secuenciales con formato PAG-YYYY-XXXXXX.
    /// </summary>
    [Fact]
    public async Task ObtenerSiguienteFolioPagoAsync_DebeGenerarFoliosSecuencialesConFormatoOficial()
    {
        using var db = CrearContextoInMemory($"pagos_folio_test_{Guid.NewGuid():N}");

        var folio1 = await db.ObtenerSiguienteFolioPagoAsync(2026);
        var folio2 = await db.ObtenerSiguienteFolioPagoAsync(2026);

        folio1.Should().Be("PAG-2026-000001");
        folio2.Should().Be("PAG-2026-000002");
    }
}
