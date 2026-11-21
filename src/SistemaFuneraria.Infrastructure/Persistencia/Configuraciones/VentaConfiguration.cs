using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="Venta"/> para PostgreSQL 16 (SF-059).
/// </summary>
public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    /// <summary>
    /// Configura tabla, columnas, claves foráneas restrictivas e índice único de lote activo para <see cref="Venta"/>.
    /// </summary>
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("ventas", t =>
        {
            t.HasCheckConstraint("ventas_modalidad_check", "modalidad IN ('Contado', 'Financiado')");
            t.HasCheckConstraint("ventas_precio_pactado_check", "precio_pactado > 0");
            t.HasCheckConstraint("ventas_estado_check", "estado IN ('Activo', 'Liquidado', 'EnRevision', 'Cancelado')");
        });

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(v => v.FolioContrato).HasColumnName("folio_contrato").HasMaxLength(20).IsRequired();
        builder.Property(v => v.ClienteId).HasColumnName("cliente_id").HasColumnType("uuid").IsRequired();
        builder.Property(v => v.LoteId).HasColumnName("lote_id").HasColumnType("uuid").IsRequired();
        builder.Property(v => v.VendedorId).HasColumnName("vendedor_id").HasMaxLength(450).IsRequired();
        builder.Property(v => v.Modalidad).HasColumnName("modalidad").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(v => v.PrecioPactado).HasColumnName("precio_pactado").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(v => v.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(v => v.MotivoCancelacion).HasColumnName("motivo_cancelacion").HasColumnType("text");
        builder.Property(v => v.FechaVenta).HasColumnName("fecha_venta").HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").IsRequired();

        builder.HasOne<Cliente>().WithMany().HasForeignKey(v => v.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Lote>().WithMany().HasForeignKey(v => v.LoteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<IdentityUser>().WithMany().HasForeignKey(v => v.VendedorId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(v => v.FolioContrato).IsUnique().HasDatabaseName("ix_ventas_folio");
        builder.HasIndex(v => v.LoteId).IsUnique().HasDatabaseName("uq_ventas_lote_activo").HasFilter("estado <> 'Cancelado'");
        builder.HasIndex(v => v.ClienteId).HasDatabaseName("ix_ventas_cliente_id");
        builder.HasIndex(v => v.VendedorId).HasDatabaseName("ix_ventas_vendedor_id");
        builder.HasIndex(v => v.FechaVenta).IsDescending().HasDatabaseName("ix_ventas_fecha");
    }
}
