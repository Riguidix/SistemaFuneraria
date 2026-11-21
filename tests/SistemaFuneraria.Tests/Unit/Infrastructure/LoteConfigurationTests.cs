using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

namespace SistemaFuneraria.Tests.Unit.Infrastructure;

/// <summary>
/// Pruebas unitarias de metadatos EF Core para <see cref="LoteConfiguration"/> y <see cref="AppDbContext"/> (SF-040).
/// </summary>
[Trait("Ticket", "SF-040")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Unit")]
public class LoteConfigurationTests
{
    private static AppDbContext CrearContextoSoloModelo()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=design_time_only;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }

    private static IEntityType ObtenerEntidadLote(AppDbContext db)
    {
        var designModel = db.GetService<IDesignTimeModel>().Model;
        var entityType = designModel.FindEntityType(typeof(Lote));
        entityType.Should().NotBeNull("la entidad Lote debe estar registrada como DbSet en AppDbContext");
        return entityType!;
    }

    [Fact]
    public void ModeloEfCore_DebeMapearTablaLotesConClavePrimariaUuid()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadLote(db);

        entityType.GetTableName().Should().Be("lotes");

        var pk = entityType.FindPrimaryKey();
        pk.Should().NotBeNull();
        pk!.Properties.Should().ContainSingle(p => p.Name == nameof(Lote.Id));

        var storeObject = StoreObjectIdentifier.Table("lotes", null);
        var idProp = entityType.FindProperty(nameof(Lote.Id))!;
        idProp.GetColumnName(storeObject).Should().Be("id");
        idProp.GetColumnType(storeObject).Should().Be("uuid");
        idProp.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void ModeloEfCore_DebeMapearColumnasYConversionesSegunErd()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadLote(db);
        var storeObject = StoreObjectIdentifier.Table("lotes", null);

        var clave = entityType.FindProperty(nameof(Lote.Clave))!;
        clave.GetColumnName(storeObject).Should().Be("clave");
        clave.IsNullable.Should().BeFalse();
        clave.GetColumnType(storeObject).Should().BeOneOf("character varying(20)", "varchar(20)");

        var seccion = entityType.FindProperty(nameof(Lote.Seccion))!;
        seccion.GetColumnName(storeObject).Should().Be("seccion");
        seccion.IsNullable.Should().BeFalse();
        seccion.GetColumnType(storeObject).Should().BeOneOf("character varying(60)", "varchar(60)");

        var andador = entityType.FindProperty(nameof(Lote.AndadorFila))!;
        andador.GetColumnName(storeObject).Should().Be("andador_fila");
        andador.IsNullable.Should().BeFalse();
        andador.GetColumnType(storeObject).Should().BeOneOf("character varying(60)", "varchar(60)");

        var gavetas = entityType.FindProperty(nameof(Lote.Gavetas))!;
        gavetas.GetColumnName(storeObject).Should().Be("gavetas");
        gavetas.IsNullable.Should().BeFalse();

        var precio = entityType.FindProperty(nameof(Lote.PrecioContado))!;
        precio.GetColumnName(storeObject).Should().Be("precio_contado");
        precio.IsNullable.Should().BeFalse();
        precio.GetColumnType(storeObject).Should().Be("numeric(18,2)");
        var converter = precio.GetValueConverter();
        converter.Should().NotBeNull();
        converter!.ConvertToProvider(new Dinero(120000m)).Should().Be(120000m);
        converter.ConvertFromProvider(150000m).Should().Be(new Dinero(150000m));

        var estado = entityType.FindProperty(nameof(Lote.Estado))!;
        estado.GetColumnName(storeObject).Should().Be("estado");
        estado.IsNullable.Should().BeFalse();
        estado.GetColumnType(storeObject).Should().BeOneOf("character varying(20)", "varchar(20)");
        var estadoConverter = estado.GetValueConverter() ?? estado.GetTypeMapping().Converter;
        estadoConverter.Should().NotBeNull();
        estadoConverter!.ConvertToProvider(EstadoLote.Disponible).Should().Be("Disponible");
    }

    [Fact]
    public void ModeloEfCore_DebeConfigurarXminCheckConstraintsEIndices()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadLote(db);

        var xmin = entityType.FindProperty("xmin");
        xmin.Should().NotBeNull();
        xmin!.IsConcurrencyToken.Should().BeTrue();

        var checks = entityType.GetCheckConstraints().ToList();
        checks.Should().Contain(c => c.Sql != null && c.Sql.Contains("gavetas"));
        checks.Should().Contain(c => c.Sql != null && c.Sql.Contains("precio_contado"));
        checks.Should().Contain(c => c.Sql != null && c.Sql.Contains("estado"));

        var idxClave = entityType.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "ix_lotes_clave");
        idxClave.Should().NotBeNull();
        idxClave!.IsUnique.Should().BeTrue();
        idxClave.Properties.Should().ContainSingle(p => p.Name == nameof(Lote.Clave));

        var idxFiltros = entityType.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == "ix_lotes_filtros");
        idxFiltros.Should().NotBeNull();
        idxFiltros!.Properties.Select(p => p.Name).Should().ContainInOrder(
            nameof(Lote.Seccion),
            nameof(Lote.Estado),
            nameof(Lote.Gavetas));
    }
}
