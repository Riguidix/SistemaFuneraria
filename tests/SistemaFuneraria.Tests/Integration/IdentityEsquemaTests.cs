using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Infrastructure.Persistencia;

namespace SistemaFuneraria.Tests.Integration;

/// <summary>
/// Valida que AppDbContext herede de IdentityDbContext&lt;IdentityUser&gt; sin requerir conexión a PostgreSQL.
/// </summary>
[Trait("Ticket", "SF-021")]
[Trait("Capa", "Infrastructure")]
public class AppDbContextIdentityContratoTests
{
    [Fact(DisplayName = "SF-021: AppDbContext debe heredar de IdentityDbContext<IdentityUser>")]
    public void AppDbContext_DebeHeredarDe_IdentityDbContext()
    {
        var baseType = typeof(AppDbContext).BaseType;

        baseType.Should().NotBeNull();
        baseType!.Name.Should().StartWith("IdentityDbContext",
            "AppDbContext debe heredar de IdentityDbContext<IdentityUser> para exponer las tablas AspNet*.");
        baseType.GenericTypeArguments.Should().ContainSingle(t => t.Name == "IdentityUser",
            "el tipo de usuario base debe ser IdentityUser.");
    }
}

/// <summary>
/// Pruebas de integración contra PostgreSQL 16 real para validar la migración EsquemaIdentity
/// (tablas AspNetUsers, AspNetRoles, AspNetUserRoles) y su reversión limpia sin afectar clientes.
/// </summary>
[Trait("Ticket", "SF-021")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Integration")]
public class IdentityEsquemaTests : IAsyncLifetime
{
    private readonly string _adminConnectionString;
    private readonly string _testDatabaseName;
    private readonly string _testConnectionString;

    public IdentityEsquemaTests()
    {
        string baseConnectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=funeraria_dev;Username=postgres;Password=postgres";

        _testDatabaseName = $"sf021_test_{Guid.NewGuid():N}"[..24];

        _adminConnectionString = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = "postgres",
            Timeout = 5
        }.ConnectionString;

        _testConnectionString = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = _testDatabaseName,
            Timeout = 5
        }.ConnectionString;
    }

    public async Task InitializeAsync()
    {
        await using var conn = new NpgsqlConnection(_adminConnectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""CREATE DATABASE "{_testDatabaseName}";""";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();

        await using var conn = new NpgsqlConnection(_adminConnectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""DROP DATABASE IF EXISTS "{_testDatabaseName}" WITH (FORCE);""";
        await cmd.ExecuteNonQueryAsync();
    }

    private AppDbContext CrearDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_testConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    private async Task<bool> TablaExisteAsync(NpgsqlConnection conn, string tableName)
    {
        await using var cmd = new NpgsqlCommand("""
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = @tableName
            );
            """, conn);
        cmd.Parameters.AddWithValue("tableName", tableName);
        var result = await cmd.ExecuteScalarAsync();
        return result is true;
    }

    [Fact(DisplayName = "SF-021 (BDD): EsquemaIdentity crea AspNetUsers, AspNetRoles y AspNetUserRoles y revierte sin perder clientes")]
    public async Task EsquemaIdentity_DebeAplicarseYRevertirseSinPerderClientes()
    {
        await using var db = CrearDbContext();
        var migrator = db.GetService<IMigrator>();

        // 1. Aplicar migración Inicial e insertar un cliente para verificar preservación de datos
        await migrator.MigrateAsync("20261112005406_Inicial");

        var cliente = Cliente.Crear(
            "Andrea",
            "Morales",
            "MORA900101HDFRRN01",
            "5512345678",
            "andrea.morales@sistemafunerario.mx",
            "Av. Jardines del Valle 100");
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        // 2. Aplicar todas las migraciones (incluyendo EsquemaIdentity)
        await db.Database.MigrateAsync();

        var aplicadas = (await db.Database.GetAppliedMigrationsAsync()).ToList();
        aplicadas.Should().Contain(m => m.EndsWith("_EsquemaIdentity", StringComparison.Ordinal),
            "debe existir y aplicarse la migración '*_EsquemaIdentity'.");

        await using var conn = new NpgsqlConnection(_testConnectionString);
        await conn.OpenAsync();

        (await TablaExisteAsync(conn, "AspNetUsers")).Should().BeTrue();
        (await TablaExisteAsync(conn, "AspNetRoles")).Should().BeTrue();
        (await TablaExisteAsync(conn, "AspNetUserRoles")).Should().BeTrue();
        (await TablaExisteAsync(conn, "clientes")).Should().BeTrue();

        // 3. Revertir Down() hacia Inicial y comprobar limpieza sin pérdida de clientes
        await migrator.MigrateAsync("20261112005406_Inicial");

        (await TablaExisteAsync(conn, "AspNetUsers")).Should().BeFalse();
        (await TablaExisteAsync(conn, "AspNetRoles")).Should().BeFalse();
        (await TablaExisteAsync(conn, "AspNetUserRoles")).Should().BeFalse();
        (await TablaExisteAsync(conn, "clientes")).Should().BeTrue();

        var totalClientes = await db.Clientes.CountAsync();
        totalClientes.Should().Be(1, "la reversión de EsquemaIdentity no debe eliminar registros de clientes.");
    }
}
