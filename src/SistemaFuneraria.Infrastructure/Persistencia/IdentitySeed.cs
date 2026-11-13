using Microsoft.AspNetCore.Identity;

namespace SistemaFuneraria.Infrastructure.Persistencia;

/// <summary>
/// Seeder para inicializar roles y usuarios del sistema RBAC (SF-021, RNF-03).
/// </summary>
public static class IdentitySeed
{
    public const string RolVendedor = "Vendedor";
    public const string RolCajero = "Cajero";

    public static readonly string[] RolesSoportados = [RolVendedor, RolCajero];

    /// <summary>
    /// Siembra los roles oficiales Vendedor y Cajero en el almacén de Identity si no existen previamente.
    /// </summary>
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
