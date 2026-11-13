using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Configuración Fluent API y Seeder inmutable de la entidad <see cref="Lote"/> para PostgreSQL 16 (ERD v1.1 Sección 3.1, SF-022).
/// </summary>
public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("lotes", t =>
        {
            t.HasCheckConstraint("lotes_gavetas_check", "gavetas IN (2, 3, 4)");
            t.HasCheckConstraint("lotes_precio_contado_check", "precio_contado > 0");
        });

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(l => l.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.Gavetas)
            .HasColumnName("gavetas")
            .IsRequired();

        builder.Property(l => l.PrecioContado)
            .HasColumnName("precio_contado")
            .HasColumnType("numeric(14,2)")
            .IsRequired();

        builder.Property(l => l.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // Seeder idempotente de los 3 productos oficiales inmutables (RN-01, SF-022)
        builder.HasData(
            Lote.Crear("LOT-02", "Lote Básico", 2, 120000.00m, true),
            Lote.Crear("LOT-03", "Lote Familiar", 3, 150000.00m, true),
            Lote.Crear("LOT-04", "Lote Ampliado", 4, 180000.00m, true)
        );
    }
}
