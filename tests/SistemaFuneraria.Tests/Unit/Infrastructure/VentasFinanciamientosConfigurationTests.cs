using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;

namespace SistemaFuneraria.Tests.Unit.Infrastructure;

/// <summary>
/// Pruebas unitarias de metadatos EF Core para Venta, Financiamiento (xmin), Mensualidad y seq_contratos_folio (SF-059).
/// </summary>
[Trait("Ticket", "SF-059")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Unit")]
public class VentasFinanciamientosConfigurationTests
{
    private static AppDbContext CrearContextoSoloModelo()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=design_time_only;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public void ModeloEfCore_DebeRegistrarDbSetsYSecuenciaContratosFolio()
    {
        using var db = CrearContextoSoloModelo();
        var model = db.GetService<IDesignTimeModel>().Model;

        db.Ventas.Should().NotBeNull();
        db.Financiamientos.Should().NotBeNull();
        db.Mensualidades.Should().NotBeNull();

        var secuencia = model.FindSequence("seq_contratos_folio");
        secuencia.Should().NotBeNull();
        secuencia!.StartValue.Should().Be(190);
        secuencia.IncrementBy.Should().Be(1);
    }

    [Fact]
    public void ModeloEfCore_VentaConfiguration_DebeMapearTablaFksRestriccionesEIndices()
    {
        using var db = CrearContextoSoloModelo();
        var model = db.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(Venta))!;
        entityType.Should().NotBeNull();
        entityType.GetTableName().Should().Be("ventas");

        var store = StoreObjectIdentifier.Table("ventas", null);
        entityType.FindProperty(nameof(Venta.FolioContrato))!.GetColumnName(store).Should().Be("folio_contrato");
        entityType.FindProperty(nameof(Venta.PrecioPactado))!.GetColumnType(store).Should().Be("numeric(18,2)");
        entityType.FindProperty(nameof(Venta.Modalidad))!.GetColumnType(store).Should().BeOneOf("character varying(20)", "varchar(20)");
        entityType.FindProperty(nameof(Venta.Estado))!.GetColumnType(store).Should().BeOneOf("character varying(20)", "varchar(20)");
        entityType.FindProperty(nameof(Venta.MotivoCancelacion))!.IsNullable.Should().BeTrue();
        entityType.FindProperty(nameof(Venta.FirmaClienteBase64))!.GetColumnName(store).Should().Be("firma_cliente_base64");
        entityType.FindProperty(nameof(Venta.FirmaClienteBase64))!.IsNullable.Should().BeTrue();
        entityType.FindProperty(nameof(Venta.VendedorNombre))!.GetColumnName(store).Should().Be("vendedor_nombre");
        entityType.FindProperty(nameof(Venta.VendedorNombre))!.IsNullable.Should().BeTrue();
        entityType.FindProperty(nameof(Venta.FechaFirma))!.GetColumnName(store).Should().Be("fecha_firma");
        entityType.FindProperty(nameof(Venta.FechaFirma))!.IsNullable.Should().BeTrue();

        var fks = entityType.GetForeignKeys().ToList();
        fks.Should().Contain(fk => fk.PrincipalEntityType.ClrType == typeof(Cliente) && fk.DeleteBehavior == DeleteBehavior.Restrict);
        fks.Should().Contain(fk => fk.PrincipalEntityType.ClrType == typeof(Lote) && fk.DeleteBehavior == DeleteBehavior.Restrict);

        var idxFolio = entityType.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "ix_ventas_folio");
        idxFolio.Should().NotBeNull();
        idxFolio!.IsUnique.Should().BeTrue();

        var idxLoteActivo = entityType.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "uq_ventas_lote_activo");
        idxLoteActivo.Should().NotBeNull();
        idxLoteActivo!.IsUnique.Should().BeTrue();
        idxLoteActivo.GetFilter().Should().Be("estado <> 'Cancelado'");
    }

    [Fact]
    public void ModeloEfCore_FinanciamientoConfiguration_DebeMapearXminPrecisionesYFkRestrict()
    {
        using var db = CrearContextoSoloModelo();
        var model = db.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(Financiamiento))!;
        entityType.Should().NotBeNull();
        entityType.GetTableName().Should().Be("financiamientos");

        var store = StoreObjectIdentifier.Table("financiamientos", null);
        entityType.FindProperty(nameof(Financiamiento.PorcentajeEnganche))!.GetColumnType(store).Should().Be("numeric(5,2)");
        entityType.FindProperty(nameof(Financiamiento.TasaInteresAnual))!.GetColumnType(store).Should().Be("numeric(5,4)");
        entityType.FindProperty(nameof(Financiamiento.SaldoInsoluto))!.GetColumnType(store).Should().Be("numeric(18,2)");
        entityType.FindProperty(nameof(Financiamiento.CapitalFinanciado))!.GetColumnType(store).Should().Be("numeric(18,2)");

        var versionProp = entityType.FindProperty(nameof(Financiamiento.VersionFila))!;
        versionProp.Should().NotBeNull();
        versionProp.IsConcurrencyToken.Should().BeTrue();

        var fkVenta = entityType.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Venta));
        fkVenta.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
        fkVenta.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void ModeloEfCore_MensualidadConfiguration_DebeMapearColumnasFkRestrictEIndices()
    {
        using var db = CrearContextoSoloModelo();
        var model = db.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(Mensualidad))!;
        entityType.Should().NotBeNull();
        entityType.GetTableName().Should().Be("mensualidades");

        var store = StoreObjectIdentifier.Table("mensualidades", null);
        entityType.FindProperty(nameof(Mensualidad.FechaVencimiento))!.GetColumnType(store).Should().Be("date");
        entityType.FindProperty(nameof(Mensualidad.CapitalProgramado))!.GetColumnType(store).Should().Be("numeric(18,2)");
        entityType.FindProperty(nameof(Mensualidad.FechaPago))!.IsNullable.Should().BeTrue();

        var fkFin = entityType.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Financiamiento));
        fkFin.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);

        var idxCuota = entityType.GetIndexes().FirstOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Mensualidad.FinanciamientoId), nameof(Mensualidad.NumeroCuota) }));
        idxCuota.Should().NotBeNull();
        idxCuota!.IsUnique.Should().BeTrue();

        var idxVenc = entityType.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "ix_mensualidades_vencimiento");
        idxVenc.Should().NotBeNull();
    }
}
