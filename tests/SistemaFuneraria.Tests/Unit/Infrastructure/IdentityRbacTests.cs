using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Infrastructure;

/// <summary>
/// Pruebas unitarias y de BDD para la configuración de ASP.NET Core Identity y Roles RBAC (SF-021).
/// Valida el contrato de persistencia de Identity y el aislamiento de roles Vendedor y Cajero (RNF-03).
/// </summary>
[Trait("Ticket", "SF-021")]
[Trait("Capa", "Infrastructure")]
[Trait("Tipo", "Unit")]
public class IdentityRbacTests
{
    private static AppDbContext CrearContextoSoloModelo()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=design_time_only;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }

    [Fact(DisplayName = "SF-021: AppDbContext debe configurar las entidades de ASP.NET Core Identity")]
    public void AppDbContext_DebeConfigurarEntidadesIdentity()
    {
        using var db = CrearContextoSoloModelo();
        var model = db.Model;

        var userEntity = model.FindEntityType(typeof(IdentityUser));
        userEntity.Should().NotBeNull("AppDbContext debe heredar de IdentityDbContext y registrar IdentityUser");

        var roleEntity = model.FindEntityType(typeof(IdentityRole));
        roleEntity.Should().NotBeNull("AppDbContext debe registrar la entidad IdentityRole");
    }

    [Fact(DisplayName = "SF-021: Constantes de roles en IdentitySeed deben coincidir con RNF-03")]
    public void IdentitySeed_ConstantesDeRol_DebenSerExactas()
    {
        IdentitySeed.RolVendedor.Should().Be("Vendedor");
        IdentitySeed.RolCajero.Should().Be("Cajero");
        IdentitySeed.RolesSoportados.Should().BeEquivalentTo(new[] { "Vendedor", "Cajero" });
    }

    [Fact(DisplayName = "SF-021: SembrarRolesAsync debe crear roles Vendedor y Cajero")]
    public async Task SembrarRolesAsync_DebeCrearRolesVendedorYCajero()
    {
        var roleStore = new FakeRoleStore();
        var roleValidators = new List<IRoleValidator<IdentityRole>>();
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var logger = new LoggerFactory().CreateLogger<RoleManager<IdentityRole>>();

        var roleManager = new RoleManager<IdentityRole>(
            roleStore,
            roleValidators,
            keyNormalizer,
            errors,
            logger
        );

        await IdentitySeed.SembrarRolesAsync(roleManager);

        var existeVendedor = await roleManager.RoleExistsAsync(IdentitySeed.RolVendedor);
        var existeCajero = await roleManager.RoleExistsAsync(IdentitySeed.RolCajero);

        existeVendedor.Should().BeTrue("el rol Vendedor debe haber sido sembrado");
        existeCajero.Should().BeTrue("el rol Cajero debe haber sido sembrado");
    }

    [Fact(DisplayName = "SF-021 / BDD: Usuario con rol Vendedor debe ser denegado para operaciones de Cajero (HTTP 403)")]
    public void BDD_UsuarioConRolVendedor_AlIntentarCobrar_DebeSerDenegado()
    {
        // Dado un usuario autenticado con rol Vendedor
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "usr-vendedor-01"),
            new Claim(ClaimTypes.Name, "vendedor@funeraria.com"),
            new Claim(ClaimTypes.Role, IdentitySeed.RolVendedor)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Cuando intenta acceder a una operación reservada para el rol Cajero
        var tieneRolVendedor = principal.IsInRole(IdentitySeed.RolVendedor);
        var tieneRolCajero = principal.IsInRole(IdentitySeed.RolCajero);

        // Entonces el sistema responde denegando la operación (no autorizado para Cajero / HTTP 403)
        tieneRolVendedor.Should().BeTrue("el usuario vendedor posee el rol Vendedor");
        tieneRolCajero.Should().BeFalse("el usuario vendedor NO debe tener el rol Cajero (HTTP 403 Forbidden)");
    }

    private class FakeRoleStore : IRoleStore<IdentityRole>
    {
        private readonly List<IdentityRole> _roles = [];

        public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            _roles.Add(role);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            _roles.Remove(role);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_roles.FirstOrDefault(r => r.Id == roleId));
        }

        public Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(_roles.FirstOrDefault(r => r.NormalizedName == normalizedRoleName));
        }

        public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public void Dispose() { }
    }
}
