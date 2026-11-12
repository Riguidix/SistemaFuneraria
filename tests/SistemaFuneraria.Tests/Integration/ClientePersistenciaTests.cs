using System.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Infrastructure.Persistencia;

namespace SistemaFuneraria.Tests.Integration;

/// <summary>
/// Pruebas de integración contra PostgreSQL 16 real para validar la migración Inicial de <see cref="Cliente"/>,
/// restricciones DDL, índices, unicidad (SQLSTATE 23505), CHECK constraint (SQLSTATE 23514) y reversibilidad (Rollback-First).
/// Cada prueba utiliza una base de datos efímera aislada para no colisionar con 'funeraria_dev' ni con ejecuciones paralelas.
/// </summary>
[Trait("Ticket", "SF-016")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Integration")]
public class ClientePersistenciaTests : IAsyncLifetime
{
    private readonly string _adminConnectionString;
    private readonly string _testDatabaseName;
    private readonly string _testConnectionString;

    public ClientePersistenciaTests()
    {
        string baseConnectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=funeraria_dev;Username=postgres;Password=postgres";

        _testDatabaseName = $"sf016_test_{Guid.NewGuid():N}"[..24];

        var adminBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = "postgres",
            Timeout = 5
        };
        _adminConnectionString = adminBuilder.ConnectionString;

        var testBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = _testDatabaseName,
            Timeout = 5
        };
        _testConnectionString = testBuilder.ConnectionString;
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

    private static Cliente CrearClienteValido(
        string curp = "LURG850101HDFRRN09",
        string correo = "gustavo.lucio@example.com") =>
        Cliente.Crear(
            "Gustavo",
            "Lucio Rodriguez",
            curp,
            "5512345678",
            correo,
            "Av. Insurgentes Sur 1234, CDMX");

    [Fact]
    public async Task MigracionInicial_DebeCrearTablaClientesConEsquemaExactoEIndicesEnPostgres()
    {
        await using var db = CrearDbContext();

        await db.Database.MigrateAsync();

        var aplicadas = (await db.Database.GetAppliedMigrationsAsync()).ToList();
        aplicadas.Should().Contain(m => m.EndsWith("_Inicial", StringComparison.Ordinal),
            "la migración inicial '*_Inicial' debe haberse aplicado contra PostgreSQL 16");

        await using var conn = new NpgsqlConnection(_testConnectionString);
        await conn.OpenAsync();

        var columnas = new Dictionary<string, (string DataType, int? MaxLength, string IsNullable)>(StringComparer.Ordinal);
        await using (var colCmd = new NpgsqlCommand("""
            SELECT column_name, data_type, character_maximum_length, is_nullable
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = 'clientes';
            """, conn))
        await using (var reader = await colCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                string colName = reader.GetString(0);
                string dataType = reader.GetString(1);
                int? maxLen = reader.IsDBNull(2) ? null : reader.GetInt32(2);
                string isNullable = reader.GetString(3);
                columnas[colName] = (dataType, maxLen, isNullable);
            }
        }

        columnas.Should().ContainKey("id").WhoseValue.Should().Be(("uuid", null, "NO"));
        columnas.Should().ContainKey("curp").WhoseValue.Should().Be(("character varying", 18, "NO"));
        columnas.Should().ContainKey("correo").WhoseValue.Should().Be(("character varying", 255, "NO"));
        columnas.Should().ContainKey("nombre").WhoseValue.Should().Be(("character varying", 100, "NO"));
        columnas.Should().ContainKey("apellidos").WhoseValue.Should().Be(("character varying", 100, "NO"));
        columnas.Should().ContainKey("telefono").WhoseValue.Should().Be(("character varying", 10, "NO"));
        columnas.Should().ContainKey("direccion").WhoseValue.Should().Be(("text", null, "NO"));
        columnas.Should().ContainKey("created_at").WhoseValue.Should().Be(("timestamp with time zone", null, "NO"));

        var indices = new Dictionary<string, string>(StringComparer.Ordinal);
        await using (var idxCmd = new NpgsqlCommand("""
            SELECT indexname, indexdef
            FROM pg_indexes
            WHERE schemaname = 'public' AND tablename = 'clientes';
            """, conn))
        await using (var reader = await idxCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                indices[reader.GetString(0)] = reader.GetString(1);
            }
        }

        indices.Should().ContainKey("ix_clientes_curp")
            .WhoseValue.Should().Contain("UNIQUE INDEX");
        indices.Should().ContainKey("ix_clientes_correo")
            .WhoseValue.Should().Contain("UNIQUE INDEX");
        indices.Should().ContainKey("ix_clientes_apellidos_nombre")
            .WhoseValue.Should().Contain("text_pattern_ops");
    }

    [Fact]
    public async Task ClientesDbSet_DebePersistirYConsultarClienteValidoCorrectamente()
    {
        await using var db = CrearDbContext();
        await db.Database.MigrateAsync();

        var cliente = CrearClienteValido();
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        await using var verificacionDb = CrearDbContext();
        var persistido = await verificacionDb.Clientes
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == cliente.Id);

        persistido.Should().NotBeNull();
        persistido!.Id.Should().Be(cliente.Id);
        persistido.Curp.Should().Be(cliente.Curp);
        persistido.Correo.Should().Be(cliente.Correo);
        persistido.Nombre.Should().Be(cliente.Nombre);
        persistido.Apellidos.Should().Be(cliente.Apellidos);
        persistido.Telefono.Should().Be(cliente.Telefono);
        persistido.Direccion.Should().Be(cliente.Direccion);
        persistido.CreatedAt.Should().BeCloseTo(cliente.CreatedAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task ClientesDbSet_ConCurpDuplicado_DebeRechazarPorIndiceUnico()
    {
        await using var db = CrearDbContext();
        await db.Database.MigrateAsync();

        var primero = CrearClienteValido(curp: "LURG850101HDFRRN09", correo: "uno@example.com");
        db.Clientes.Add(primero);
        Func<Task> guardarPrimero = () => db.SaveChangesAsync();
        await guardarPrimero.Should().NotThrowAsync("el primer cliente válido debe persistirse en PostgreSQL");

        await using var segundoDb = CrearDbContext();
        var duplicado = CrearClienteValido(curp: "LURG850101HDFRRN09", correo: "otro@example.com");
        segundoDb.Clientes.Add(duplicado);

        Func<Task> guardarDuplicado = () => segundoDb.SaveChangesAsync();
        var ex = (await guardarDuplicado.Should().ThrowAsync<DbUpdateException>()).Which;

        var pgEx = ex.InnerException.Should().BeOfType<PostgresException>().Subject;
        pgEx.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task ClientesDbSet_ConCorreoDuplicado_DebeRechazarPorIndiceUnico()
    {
        await using var db = CrearDbContext();
        await db.Database.MigrateAsync();

        var primero = CrearClienteValido(curp: "LURG850101HDFRRN09", correo: "duplicado@example.com");
        db.Clientes.Add(primero);
        Func<Task> guardarPrimero = () => db.SaveChangesAsync();
        await guardarPrimero.Should().NotThrowAsync("el primer cliente válido debe persistirse en PostgreSQL");

        await using var segundoDb = CrearDbContext();
        var duplicado = CrearClienteValido(curp: "ACEE900202HDFRRN01", correo: "duplicado@example.com");
        segundoDb.Clientes.Add(duplicado);

        Func<Task> guardarDuplicado = () => segundoDb.SaveChangesAsync();
        var ex = (await guardarDuplicado.Should().ThrowAsync<DbUpdateException>()).Which;

        var pgEx = ex.InnerException.Should().BeOfType<PostgresException>().Subject;
        pgEx.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task BaseDatos_DebeRechazarTelefonoInvalidoMedianteCheckConstraint()
    {
        await using var db = CrearDbContext();
        await db.Database.MigrateAsync();

        var aplicadas = await db.Database.GetAppliedMigrationsAsync();
        aplicadas.Should().NotBeEmpty("la tabla clientes debe existir tras la migración antes de validar su CHECK constraint");

        await using var conn = new NpgsqlConnection(_testConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("""
            INSERT INTO clientes (id, curp, correo, nombre, apellidos, telefono, direccion, created_at)
            VALUES (@id, @curp, @correo, @nombre, @apellidos, @telefono, @direccion, NOW());
            """, conn);
        cmd.Parameters.AddWithValue("id", Guid.NewGuid());
        cmd.Parameters.AddWithValue("curp", "LURG850101HDFRRN09");
        cmd.Parameters.AddWithValue("correo", "check@example.com");
        cmd.Parameters.AddWithValue("nombre", "Prueba");
        cmd.Parameters.AddWithValue("apellidos", "Check");
        cmd.Parameters.AddWithValue("telefono", "ABC1234567");
        cmd.Parameters.AddWithValue("direccion", "Domicilio de prueba");

        Func<Task> insertarTelefonoInvalido = () => cmd.ExecuteNonQueryAsync();
        var pgEx = (await insertarTelefonoInvalido.Should().ThrowAsync<PostgresException>()).Which;

        pgEx.SqlState.Should().Be(PostgresErrorCodes.CheckViolation,
            "PostgreSQL debe bloquear teléfonos no numéricos mediante CHECK constraint incluso si se omite la capa de aplicación");
    }

    [Fact]
    public async Task MigracionInicial_DebeSerEstrictamenteReversibleConRollbackClean()
    {
        await using var db = CrearDbContext();
        await db.Database.MigrateAsync();

        await using var conn = new NpgsqlConnection(_testConnectionString);
        await conn.OpenAsync();

        static async Task<bool> ExisteTablaClientesAsync(NpgsqlConnection c)
        {
            if (c.State != ConnectionState.Open)
            {
                await c.OpenAsync();
            }

            await using var checkCmd = new NpgsqlCommand("""
                SELECT EXISTS (
                    SELECT 1 FROM information_schema.tables
                    WHERE table_schema = 'public' AND table_name = 'clientes'
                );
                """, c);
            return (bool)(await checkCmd.ExecuteScalarAsync())!;
        }

        (await ExisteTablaClientesAsync(conn)).Should().BeTrue("la migración Inicial debe crear la tabla clientes");

        var migrator = db.GetService<IMigrator>();
        await migrator.MigrateAsync("0");

        (await ExisteTablaClientesAsync(conn)).Should().BeFalse(
            "al revertir la migración Inicial (Down -> '0'), la tabla clientes debe eliminarse limpiamente (política Rollback-First)");
    }
}
