using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="Lote"/> para PostgreSQL 16 (SF-040).
/// </summary>
public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    /// <summary>
    /// Configura el mapeo de tabla, columnas, conversiones, restricciones e índices de <see cref="Lote"/>.
    /// </summary>
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("lotes", t =>
        {
            t.HasCheckConstraint("lotes_gavetas_check", "gavetas IN (2, 3, 4)");
            t.HasCheckConstraint("lotes_precio_contado_check", "precio_contado > 0");
            t.HasCheckConstraint("lotes_estado_check", "estado IN ('Disponible', 'Reservado', 'Vendido', 'Mantenimiento')");
        });

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(l => l.Clave)
            .HasColumnName("clave")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.Seccion)
            .HasColumnName("seccion")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(l => l.AndadorFila)
            .HasColumnName("andador_fila")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(l => l.Gavetas)
            .HasColumnName("gavetas")
            .IsRequired();

        builder.Property(l => l.PrecioContado)
            .HasColumnName("precio_contado")
            .HasColumnType("numeric(18,2)")
            .HasPrecision(18, 2)
            .HasConversion(d => d.Monto, m => new Dinero(m))
            .IsRequired();

        builder.Property(l => l.Estado)
            .HasColumnName("estado")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property<uint>("xmin")
            .IsRowVersion();

        builder.HasIndex(l => l.Clave)
            .IsUnique()
            .HasDatabaseName("ix_lotes_clave");

        builder.HasIndex(l => new { l.Seccion, l.Estado, l.Gavetas })
            .HasDatabaseName("ix_lotes_filtros");
    }
}
