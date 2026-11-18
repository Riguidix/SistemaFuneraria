using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("lotes");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(l => l.Clave)
            .HasColumnName("clave")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(l => l.Clave)
            .HasDatabaseName("ix_lotes_clave")
            .IsUnique();

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
            .HasPrecision(18, 2)
            .HasConversion(
                d => d.Monto,
                m => new Dinero(m))
            .IsRequired();

        builder.Property(l => l.Estado)
            .HasColumnName("estado")
            .HasMaxLength(20)
            .HasConversion(
                e => e.ToString(),
                s => Enum.Parse<EstadoLote>(s))
            .IsRequired();

        builder.HasIndex(l => new { l.Seccion, l.Estado, l.Gavetas })
            .HasDatabaseName("ix_lotes_seccion_estado_gavetas");
    }
}
