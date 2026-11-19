using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Core.Entidades;

namespace SistemaFuneraria.Application.Comun.Persistencia;

/// <summary>
/// Abstracción del contexto de base de datos para desacoplar la capa Application de la infraestructura (Clean Architecture).
/// </summary>
public interface IAppDbContext
{
    DbSet<Cliente> Clientes { get; }
    DbSet<Lote> Lotes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
