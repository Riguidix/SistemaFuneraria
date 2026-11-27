using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Core.Entidades;

namespace SistemaFuneraria.Infrastructure.Persistencia;

/// <summary>
/// Contexto principal de base de datos EF Core 8 sobre PostgreSQL 16
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options), IAppDbContext
{
    private long _inMemoryFolioPagoCounter;
    private long _inMemoryFolioContratoCounter;

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<Financiamiento> Financiamientos => Set<Financiamiento>();
    public DbSet<Mensualidad> Mensualidades => Set<Mensualidad>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<ConfiguracionSucursal> ConfiguracionesSucursal => Set<ConfiguracionSucursal>();

    /// <summary>
    /// Inicia una transacción de base de datos explícita (omitida en proveedor InMemory).
    /// </summary>
    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.IsRelational())
        {
            await Database.BeginTransactionAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Confirma la transacción de base de datos actual.
    /// </summary>
    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction != null)
        {
            await Database.CommitTransactionAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Revierte la transacción de base de datos actual.
    /// </summary>
    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction != null)
        {
            await Database.RollbackTransactionAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Obtiene el siguiente folio único de contrato utilizando la secuencia PostgreSQL seq_contratos_folio.
    /// </summary>
    public virtual async Task<string> ObtenerSiguienteFolioContratoAsync(int anio, CancellationToken cancellationToken = default)
    {
        long consecutivo;
        if (Database.IsNpgsql() || (Database.IsRelational() && !(Database.ProviderName?.Contains("Sqlite") ?? false)))
        {
            await using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT nextval('seq_contratos_folio');";
            if (command.Connection!.State != System.Data.ConnectionState.Open)
            {
                await command.Connection.OpenAsync(cancellationToken);
            }
            if (Database.CurrentTransaction != null)
            {
                command.Transaction = Database.CurrentTransaction.GetDbTransaction();
            }
            var result = await command.ExecuteScalarAsync(cancellationToken);
            consecutivo = Convert.ToInt64(result);
        }
        else
        {
            if (_inMemoryFolioContratoCounter == 0)
            {
                _inMemoryFolioContratoCounter = 189 + await Ventas.CountAsync(cancellationToken);
            }
            consecutivo = Interlocked.Increment(ref _inMemoryFolioContratoCounter);
        }

        return $"CTR-{anio}-{consecutivo:D4}";
    }

    /// <summary>
    /// Obtiene el siguiente folio único de pago utilizando la secuencia PostgreSQL seq_pagos_folio.
    /// </summary>
    public virtual async Task<string> ObtenerSiguienteFolioPagoAsync(int anio, CancellationToken cancellationToken = default)
    {
        long consecutivo;
        if (Database.IsNpgsql() || (Database.IsRelational() && !(Database.ProviderName?.Contains("Sqlite") ?? false)))
        {
            await using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT nextval('seq_pagos_folio');";
            if (command.Connection!.State != System.Data.ConnectionState.Open)
            {
                await command.Connection.OpenAsync(cancellationToken);
            }
            if (Database.CurrentTransaction != null)
            {
                command.Transaction = Database.CurrentTransaction.GetDbTransaction();
            }
            var result = await command.ExecuteScalarAsync(cancellationToken);
            consecutivo = Convert.ToInt64(result);
        }
        else
        {
            if (_inMemoryFolioPagoCounter == 0)
            {
                _inMemoryFolioPagoCounter = await Pagos.CountAsync(cancellationToken);
            }
            consecutivo = Interlocked.Increment(ref _inMemoryFolioPagoCounter);
        }

        return $"PAG-{anio}-{consecutivo:D6}";
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasSequence<long>("seq_contratos_folio")
            .StartsAt(190)
            .IncrementsBy(1);

        modelBuilder.HasSequence<long>("seq_pagos_folio")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
