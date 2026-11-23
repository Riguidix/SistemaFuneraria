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
    DbSet<Venta> Ventas { get; }
    DbSet<Financiamiento> Financiamientos { get; }
    DbSet<Mensualidad> Mensualidades { get; }
    DbSet<Pago> Pagos { get; }
    Task<string> ObtenerSiguienteFolioPagoAsync(int anio, CancellationToken cancellationToken = default);
    Task<string> ObtenerSiguienteFolioContratoAsync(int anio, CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
