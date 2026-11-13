using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Infrastructure.Persistencia;
using SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

namespace SistemaFuneraria.Tests.Unit.Infrastructure;

/// <summary>
/// Pruebas unitarias de metadatos EF Core para <see cref="LoteConfiguration"/> y <see cref="AppDbContext"/> (SF-022).
/// Verifican el mapeo exacto, check constraints y seeder inmutable contra ERD v1.1 Sección 3.1.
/// </summary>
[Trait("Ticket", "SF-022")]
[Trait("CP", "CP-01")]
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
    public void ModeloEfCore_DebeMapearTablaLotesConClavePrimariaId()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadLote(db);

        entityType.GetTableName().Should().Be("lotes", "el ERD v1.1 Sección 3.1 define el nombre de tabla en minúsculas 'lotes'");

        var pk = entityType.FindPrimaryKey();
        pk.Should().NotBeNull();
        pk!.Properties.Should().ContainSingle(p => p.Name == nameof(Lote.Id));

        var storeObject = StoreObjectIdentifier.Table("lotes", null);
        var idProp = entityType.FindProperty(nameof(Lote.Id))!;
        idProp.GetColumnName(storeObject).Should().Be("id");
        idProp.GetMaxLength().Should().Be(10);
        idProp.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void ModeloEfCore_DebeMapearColumnasConTiposNpgsqlYLongitudesExactasDelErd()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadLote(db);
        var storeObject = StoreObjectIdentifier.Table("lotes", null);

        var nombre = entityType.FindProperty(nameof(Lote.Nombre))!;
        nombre.GetColumnName(storeObject).Should().Be("nombre");
        nombre.IsNullable.Should().BeFalse();
        nombre.GetMaxLength().Should().Be(50);
        nombre.GetColumnType(storeObject).Should().BeOneOf("character varying(50)", "varchar(50)");

        var gavetas = entityType.FindProperty(nameof(Lote.Gavetas))!;
        gavetas.GetColumnName(storeObject).Should().Be("gavetas");
        gavetas.IsNullable.Should().BeFalse();
        gavetas.GetColumnType(storeObject).Should().BeOneOf("integer", "int");

        var precioContado = entityType.FindProperty(nameof(Lote.PrecioContado))!;
        precioContado.GetColumnName(storeObject).Should().Be("precio_contado");
        precioContado.IsNullable.Should().BeFalse();
        precioContado.GetColumnType(storeObject).Should().Be("numeric(14,2)", "el tipo financiero en PostgreSQL debe ser numeric(14,2)");

        var isActive = entityType.FindProperty(nameof(Lote.IsActive))!;
        isActive.GetColumnName(storeObject).Should().Be("is_active");
        isActive.IsNullable.Should().BeFalse();
        isActive.GetColumnType(storeObject).Should().BeOneOf("boolean", "bool");
        isActive.GetDefaultValue().Should().Be(true);
    }

    [Fact]
    public void ModeloEfCore_DebeIncluirCheckConstraintsDeGavetasYPrecioContado()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadLote(db);
        var checks = entityType.GetCheckConstraints().ToList();

        var checkGavetas = checks.SingleOrDefault(c => c.ModelName == "lotes_gavetas_check" || c.Name == "lotes_gavetas_check");
        checkGavetas.Should().NotBeNull("debe existir check constraint para gavetas autorizadas (2, 3, 4)");
        checkGavetas!.Sql.Should().Contain("gavetas").And.Contain("2, 3, 4");

        var checkPrecio = checks.SingleOrDefault(c => c.ModelName == "lotes_precio_contado_check" || c.Name == "lotes_precio_contado_check");
        checkPrecio.Should().NotBeNull("debe existir check constraint que impida precio contado no positivo");
        checkPrecio!.Sql.Should().Contain("precio_contado").And.Contain("> 0");
    }

    [Fact]
    public void ModeloEfCore_DebeContenerSeederInmutableConExactamente3LotesOficiales()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadLote(db);

        var seedData = entityType.GetSeedData().ToList();
        seedData.Should().HaveCount(3, "el seeder debe contener exactamente los 3 lotes oficiales (RN-01)");

        var lote2 = seedData.Cast<IDictionary<string, object?>>().SingleOrDefault(d => d[nameof(Lote.Id)]?.ToString() == "LOT-02");
        lote2.Should().NotBeNull();
        lote2![nameof(Lote.Nombre)].Should().Be("Lote Básico");
        lote2[nameof(Lote.Gavetas)].Should().Be(2);
        lote2[nameof(Lote.PrecioContado)].Should().Be(120000.00m);
        lote2[nameof(Lote.IsActive)].Should().Be(true);

        var lote3 = seedData.Cast<IDictionary<string, object?>>().SingleOrDefault(d => d[nameof(Lote.Id)]?.ToString() == "LOT-03");
        lote3.Should().NotBeNull();
        lote3![nameof(Lote.Nombre)].Should().Be("Lote Familiar");
        lote3[nameof(Lote.Gavetas)].Should().Be(3);
        lote3[nameof(Lote.PrecioContado)].Should().Be(150000.00m);
        lote3[nameof(Lote.IsActive)].Should().Be(true);

        var lote4 = seedData.Cast<IDictionary<string, object?>>().SingleOrDefault(d => d[nameof(Lote.Id)]?.ToString() == "LOT-04");
        lote4.Should().NotBeNull();
        lote4![nameof(Lote.Nombre)].Should().Be("Lote Ampliado");
        lote4[nameof(Lote.Gavetas)].Should().Be(4);
        lote4[nameof(Lote.PrecioContado)].Should().Be(180000.00m);
        lote4[nameof(Lote.IsActive)].Should().Be(true);
    }

    [Theory]
    [InlineData("LOT-02", "Lote Básico", 2, 120000.00)]
    [InlineData("LOT-03", "Lote Familiar", 3, 150000.00)]
    [InlineData("LOT-04", "Lote Ampliado", 4, 180000.00)]
    public void LoteEntidad_Crear_ConDatosValidos_DebeInstanciarLoteCorrectamente(string id, string nombre, int gavetas, decimal precio)
    {
        var lote = Lote.Crear(id, nombre, gavetas, precio);

        lote.Should().NotBeNull();
        lote.Id.Should().Be(id);
        lote.Nombre.Should().Be(nombre);
        lote.Gavetas.Should().Be(gavetas);
        lote.PrecioContado.Should().Be(precio);
        lote.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Lote Básico", 2, 120000.00, "identificador")]
    [InlineData("LOT-99", "Lote Desconocido", 2, 120000.00, "identificador")]
    [InlineData("LOT-02", "", 2, 120000.00, "nombre")]
    [InlineData("LOT-02", "Lote Básico", 5, 120000.00, "gavetas")]
    [InlineData("LOT-02", "Lote Básico", 2, -100.00, "precio")]
    [InlineData("LOT-02", "Lote Básico", 2, 130000.00, "precio")]
    public void LoteEntidad_Crear_ConDatosInvalidos_DebeLanzarReglaNegocioException(
        string id, string nombre, int gavetas, decimal precio, string motivo)
    {
        Action actuar = () => Lote.Crear(id, nombre, gavetas, precio);

        actuar.Should().Throw<ReglaNegocioException>()
            .WithMessage($"*{motivo}*");
    }
}
