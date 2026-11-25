using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Infrastructure.Persistencia;

/// <summary>
/// Seeder idempotente de los 3 roles RBAC oficiales y las cuentas base del Sistema Funerario (SF-022).
/// </summary>
public static class IdentitySeed
{
    private const string PasswordDemo = "Sistema Funerario2026!";

    private sealed record SeedUsuarioSpec(
        string Correo,
        string NombreCompleto,
        string Iniciales,
        string Rol);

    private static readonly string[] RolesSistema =
    [
        ParametrosNegocio.RolAdministrador,
        ParametrosNegocio.RolVendedor,
        ParametrosNegocio.RolCajero
    ];

    private static readonly SeedUsuarioSpec[] UsuariosOficiales =
    [
        new("andrea.morales@sistemafunerario.mx", "Andrea Morales", "AM", ParametrosNegocio.RolAdministrador),
        new("vendedor@sistemafunerario.mx", "Carlos Vega", "CV", ParametrosNegocio.RolVendedor),
        new("cajero@sistemafunerario.mx", "Luis Torres", "LT", ParametrosNegocio.RolCajero)
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await SeedRolesAsync(roleManager);

        foreach (var spec in UsuariosOficiales)
        {
            await SeedUsuarioAsync(userManager, spec);
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var rol in RolesSistema)
        {
            if (await roleManager.RoleExistsAsync(rol))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(rol));
            AsegurarExito(result, $"crear el rol '{rol}'");
        }
    }

    private static async Task SeedUsuarioAsync(UserManager<IdentityUser> userManager, SeedUsuarioSpec spec)
    {
        var user = await userManager.FindByEmailAsync(spec.Correo);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = spec.Correo,
                Email = spec.Correo,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, PasswordDemo);
            AsegurarExito(createResult, $"crear el usuario '{spec.Correo}'");
        }

        if (!await userManager.IsInRoleAsync(user, spec.Rol))
        {
            var roleResult = await userManager.AddToRoleAsync(user, spec.Rol);
            AsegurarExito(roleResult, $"asignar el rol '{spec.Rol}' a '{spec.Correo}'");
        }

        await AsegurarClaimsAsync(userManager, user, spec);
    }

    private static async Task AsegurarClaimsAsync(
        UserManager<IdentityUser> userManager,
        IdentityUser user,
        SeedUsuarioSpec spec)
    {
        var existentes = await userManager.GetClaimsAsync(user);
        var requeridos = new[]
        {
            new Claim(ParametrosNegocio.ClaimNombreCompleto, spec.NombreCompleto),
            new Claim(ParametrosNegocio.ClaimIniciales, spec.Iniciales),
            new Claim(ParametrosNegocio.ClaimSucursal, ParametrosNegocio.SucursalPorDefecto)
        };

        var faltantes = requeridos
            .Where(req => !existentes.Any(c => c.Type == req.Type && c.Value == req.Value))
            .ToArray();

        if (faltantes.Length > 0)
        {
            var claimResult = await userManager.AddClaimsAsync(user, faltantes);
            AsegurarExito(claimResult, $"agregar claims a '{spec.Correo}'");
        }
    }

    private static void AsegurarExito(IdentityResult result, string contexto)
    {
        if (result.Succeeded)
        {
            return;
        }

        var detalle = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
        throw new InvalidOperationException($"No se pudo {contexto}: {detalle}");
    }
}
