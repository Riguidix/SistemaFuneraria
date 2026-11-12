using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Core.Entidades;

namespace SistemaFuneraria.Infrastructure.Persistencia;

/// <summary>
/// Contexto principal de base de datos EF Core 8 sobre PostgreSQL 16
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
