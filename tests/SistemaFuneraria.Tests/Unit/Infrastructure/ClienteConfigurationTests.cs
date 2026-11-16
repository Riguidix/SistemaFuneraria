using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Infrastructure.Persistencia;
using SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

namespace SistemaFuneraria.Tests.Unit.Infrastructure;

/// <summary>
/// Pruebas unitarias de metadatos EF Core para <see cref="ClienteConfiguration"/> y <see cref="AppDbContext"/> (SF-016).
/// Verifican sin requerir base de datos activa el mapeo exacto contra ERD v1.1 Sección 3.2 y Sección 4.
/// </summary>
[Trait("Ticket", "SF-016")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Unit")]
public class ClienteConfigurationTests
{
    private static AppDbContext CrearContextoSoloModelo()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=design_time_only;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }

    private static IEntityType ObtenerEntidadCliente(AppDbContext db)
    {
        var designModel = db.GetService<IDesignTimeModel>().Model;
        var entityType = designModel.FindEntityType(typeof(Cliente));
        entityType.Should().NotBeNull("la entidad Cliente debe estar registrada como DbSet en AppDbContext");
        return entityType!;
    }

    [Fact]
    public void ModeloEfCore_DebeMapearTablaClientesConClavePrimariaUuid()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadCliente(db);

        entityType.GetTableName().Should().Be("clientes", "el ERD v1.1 Sección 3.2 define el nombre de tabla en minúsculas 'clientes'");

        var pk = entityType.FindPrimaryKey();
        pk.Should().NotBeNull();
        pk!.Properties.Should().ContainSingle(p => p.Name == nameof(Cliente.Id));

        var storeObject = StoreObjectIdentifier.Table("clientes", null);
        var idProp = entityType.FindProperty(nameof(Cliente.Id))!;
        idProp.GetColumnName(storeObject).Should().Be("id");
        idProp.GetColumnType(storeObject).Should().Be("uuid");
        idProp.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void ModeloEfCore_DebeMapearColumnasConTiposNpgsqlYLongitudesExactasDelErd()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadCliente(db);
        var storeObject = StoreObjectIdentifier.Table("clientes", null);

        var curp = entityType.FindProperty(nameof(Cliente.Curp))!;
        curp.GetColumnName(storeObject).Should().Be("curp");
        curp.IsNullable.Should().BeFalse();
        curp.GetColumnType(storeObject).Should().BeOneOf("character varying(18)", "varchar(18)");

        var correo = entityType.FindProperty(nameof(Cliente.Correo))!;
        correo.GetColumnName(storeObject).Should().Be("correo");
        correo.IsNullable.Should().BeFalse();
        correo.GetColumnType(storeObject).Should().BeOneOf("character varying(255)", "varchar(255)");

        var nombre = entityType.FindProperty(nameof(Cliente.Nombre))!;
        nombre.GetColumnName(storeObject).Should().Be("nombre");
        nombre.IsNullable.Should().BeFalse();
        nombre.GetColumnType(storeObject).Should().BeOneOf("character varying(100)", "varchar(100)");

        var apellidos = entityType.FindProperty(nameof(Cliente.Apellidos))!;
        apellidos.GetColumnName(storeObject).Should().Be("apellidos");
        apellidos.IsNullable.Should().BeFalse();
        apellidos.GetColumnType(storeObject).Should().BeOneOf("character varying(100)", "varchar(100)");

        var telefono = entityType.FindProperty(nameof(Cliente.Telefono))!;
        telefono.GetColumnName(storeObject).Should().Be("telefono");
        telefono.IsNullable.Should().BeFalse();
        telefono.GetColumnType(storeObject).Should().BeOneOf("character varying(10)", "varchar(10)");

        var direccion = entityType.FindProperty(nameof(Cliente.Direccion))!;
        direccion.GetColumnName(storeObject).Should().Be("direccion");
        direccion.IsNullable.Should().BeFalse();
        direccion.GetColumnType(storeObject).Should().Be("text");

        var createdAt = entityType.FindProperty(nameof(Cliente.CreatedAt))!;
        createdAt.GetColumnName(storeObject).Should().Be("created_at");
        createdAt.IsNullable.Should().BeFalse();
        createdAt.GetColumnType(storeObject).Should().BeOneOf("timestamp with time zone", "timestamptz");
    }

    [Fact]
    public void ModeloEfCore_DebeConfigurarIndicesUnicosYBusquedaPorApellidosNombre()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadCliente(db);
        var indices = entityType.GetIndexes().ToList();

        var ixCurp = indices.SingleOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == nameof(Cliente.Curp));
        ixCurp.Should().NotBeNull("debe existir índice único sobre curp");
        ixCurp!.IsUnique.Should().BeTrue();
        ixCurp.GetDatabaseName().Should().Be("ix_clientes_curp");

        var ixCorreo = indices.SingleOrDefault(i =>
            i.Properties.Count == 1 && i.Properties[0].Name == nameof(Cliente.Correo));
        ixCorreo.Should().NotBeNull("debe existir índice único sobre correo");
        ixCorreo!.IsUnique.Should().BeTrue();
        ixCorreo.GetDatabaseName().Should().Be("ix_clientes_correo");

        var ixApellidosNombre = indices.SingleOrDefault(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Cliente.Apellidos), nameof(Cliente.Nombre) }));
        ixApellidosNombre.Should().NotBeNull("debe existir índice compuesto sobre (apellidos, nombre) para búsqueda rápida");
        ixApellidosNombre!.IsUnique.Should().BeFalse();
        ixApellidosNombre.GetDatabaseName().Should().Be("ix_clientes_apellidos_nombre");
        ixApellidosNombre.GetOperators().Should().Equal(new[] { "text_pattern_ops", "text_pattern_ops" },
            "el ERD v1.1 Sección 4 exige operadores text_pattern_ops para búsquedas por prefijo");
    }

    [Fact]
    public void ModeloEfCore_DebeIncluirCheckConstraintDeTelefono10Digitos()
    {
        using var db = CrearContextoSoloModelo();
        var entityType = ObtenerEntidadCliente(db);

        var checkTelefono = entityType.GetCheckConstraints()
            .SingleOrDefault(c => c.ModelName == "clientes_telefono_check" || c.Name == "clientes_telefono_check");

        checkTelefono.Should().NotBeNull("la tabla clientes debe proteger telefono a 10 dígitos mediante CHECK constraint");
        checkTelefono!.Sql.Should().Contain("telefono").And.Contain("^[0-9]{10}$");
    }

    [Fact]
    public void ClienteConfiguration_AlAplicarseDirectamente_DebeEjecutarseSinExcepcion()
    {
        var modelBuilder = new ModelBuilder();
        var configuracion = new ClienteConfiguration();

        Action actuar = () => modelBuilder.ApplyConfiguration(configuracion);

        actuar.Should().NotThrow("ClienteConfiguration debe implementar el mapeo Fluent API completo de Cliente");
    }

    [Fact]
    public void AppDbContext_DebeContenerMigracionInicialCompilada()
    {
        using var db = CrearContextoSoloModelo();
        var migraciones = db.Database.GetMigrations().ToList();

        migraciones.Should().Contain(m => m.EndsWith("_Inicial", StringComparison.Ordinal),
            "SF-016 requiere generar la migración inicial '*_Inicial.cs' en Infrastructure/Persistencia/Migrations");
    }
}
