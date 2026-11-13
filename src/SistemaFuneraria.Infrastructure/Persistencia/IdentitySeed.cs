using Microsoft.AspNetCore.Identity;

namespace SistemaFuneraria.Infrastructure.Persistencia;

public static class IdentitySeed
{
    public const string RolVendedor = "Vendedor";
    public const string RolCajero = "Cajero";

    public static readonly string[] RolesSoportados = [RolVendedor, RolCajero];

    public static async Task SembrarRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        ArgumentNullException.ThrowIfNull(roleManager);

        foreach (var rol in RolesSoportados)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }
    }
}
