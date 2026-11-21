using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="Lote"/> para PostgreSQL 16 y pruebas en memoria (SF-040, SF-041).
/// </summary>
public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("lotes");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
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
            .HasConversion(d => d.Monto, m => new Dinero(m))
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(l => l.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(l => l.Clave)
            .IsUnique();

        builder.HasIndex(l => new { l.Seccion, l.Estado, l.Gavetas });
    }
}
