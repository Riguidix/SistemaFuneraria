using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Infrastructure.Persistencia;

namespace SistemaFuneraria.Tests.Integration;

/// <summary>
/// Pruebas de integración contra PostgreSQL 16 real para validar el seeder idempotente
/// <see cref="IdentitySeed"/> con los 3 roles RBAC y las 3 cuentas oficiales de demostración (SF-022).
/// </summary>
[Trait("Ticket", "SF-022")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Integration")]
public class IdentitySeedTests : IAsyncLifetime
{
    private readonly string _adminConnectionString;
    private readonly string _testDatabaseName;
    private readonly string _testConnectionString;

    public IdentitySeedTests()
    {
        string baseConnectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=funeraria_dev;Username=postgres;Password=postgres";

        _testDatabaseName = $"sf022_test_{Guid.NewGuid():N}"[..24];

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

    private ServiceProvider ConstruirServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(_testConnectionString));
        services.AddIdentityCore<IdentityUser>(opt =>
            {
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services.BuildServiceProvider();
    }

    [Fact(DisplayName = "SF-022 (BDD): IdentitySeed siembra de forma idempotente los 3 roles y las 3 cuentas oficiales")]
    public async Task SeedAsync_DosVecesConsecutivas_DebeSembrarRolesYUsuariosIdempotentemente()
    {
        ParametrosNegocio.RolAdministrador.Should().Be("Administrador");
        ParametrosNegocio.RolVendedor.Should().Be("Vendedor");
        ParametrosNegocio.RolCajero.Should().Be("Cajero");

        await using var provider = ConstruirServiceProvider();

        using (var initScope = provider.CreateScope())
        {
            var db = initScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        // Ejecución consecutiva (idempotencia)
        await IdentitySeed.SeedAsync(provider);
        await IdentitySeed.SeedAsync(provider);

        using var verifyScope = provider.CreateScope();
        var roleManager = verifyScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = verifyScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
        roles.Should().BeEquivalentTo(new[] { "Administrador", "Vendedor", "Cajero" });

        var usuariosEsperados = new[]
        {
            ("andrea.morales@sistemafunerario.mx", "Andrea Morales", "AM", "Administrador"),
            ("vendedor@sistemafunerario.mx", "Carlos Vega", "CV", "Vendedor"),
            ("cajero@sistemafunerario.mx", "Luis Torres", "LT", "Cajero")
        };

        var totalUsuarios = await userManager.Users.CountAsync();
        totalUsuarios.Should().Be(3, "la ejecución idempotente no debe duplicar usuarios");

        foreach (var (correo, nombreCompleto, iniciales, rol) in usuariosEsperados)
        {
            var user = await userManager.FindByEmailAsync(correo);
            user.Should().NotBeNull($"debe existir el usuario '{correo}'");

            var passwordValido = await userManager.CheckPasswordAsync(user!, "Sistema Funerario2026!");
            passwordValido.Should().BeTrue($"la contraseña oficial de '{correo}' debe ser válida");

            var rolesUsuario = await userManager.GetRolesAsync(user!);
            rolesUsuario.Should().ContainSingle().Which.Should().Be(rol);

            var claims = await userManager.GetClaimsAsync(user!);
            claims.Should().Contain(c => c.Type == "NombreCompleto" && c.Value == nombreCompleto);
            claims.Should().Contain(c => c.Type == "Iniciales" && c.Value == iniciales);
            claims.Should().Contain(c => c.Type == "Sucursal" && c.Value == "Jardines del Valle");
        }
    }
}
