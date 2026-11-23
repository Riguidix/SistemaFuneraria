using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="Financiamiento"/> con concurrencia optimista xmin (SF-059).
/// </summary>
public class FinanciamientoConfiguration : IEntityTypeConfiguration<Financiamiento>
{
    /// <summary>
    /// Configura tabla, precisiones financieras, relación 1:1 con Venta y token xmin para <see cref="Financiamiento"/>.
    /// </summary>
    public void Configure(EntityTypeBuilder<Financiamiento> builder)
    {
        builder.ToTable("financiamientos", t =>
        {
            t.HasCheckConstraint("financiamientos_enganche_pct_check", "porcentaje_enganche IN (10, 20, 30)");
            t.HasCheckConstraint("financiamientos_monto_enganche_check", "monto_enganche > 0");
            t.HasCheckConstraint("financiamientos_capital_check", "capital_financiado > 0");
            t.HasCheckConstraint("financiamientos_plazo_check", "plazo_meses IN (12, 18, 24, 48)");
            t.HasCheckConstraint("financiamientos_tasa_check", "tasa_interes_anual IN (0.1600, 0.1800, 0.2000, 0.2400)");
            t.HasCheckConstraint("financiamientos_cuota_check", "cuota_mensual_actual > 0");
            t.HasCheckConstraint("financiamientos_saldo_check", "saldo_insoluto >= 0");
            t.HasCheckConstraint("financiamientos_estado_check", "estado IN ('Activo', 'EnMora', 'Liquidado', 'Cancelado')");
        });

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(f => f.VentaId).HasColumnName("venta_id").HasColumnType("uuid").IsRequired();
        builder.Property(f => f.PorcentajeEnganche).HasColumnName("porcentaje_enganche").HasColumnType("numeric(5,2)").HasPrecision(5, 2).IsRequired();
        builder.Property(f => f.MontoEnganche).HasColumnName("monto_enganche").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(f => f.CapitalFinanciado).HasColumnName("capital_financiado").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(f => f.PlazoMeses).HasColumnName("plazo_meses").IsRequired();
        builder.Property(f => f.TasaInteresAnual).HasColumnName("tasa_interes_anual").HasColumnType("numeric(5,4)").HasPrecision(5, 4).IsRequired();
        builder.Property(f => f.CuotaMensualActual).HasColumnName("cuota_mensual_actual").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(f => f.SaldoInsoluto).HasColumnName("saldo_insoluto").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(f => f.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(f => f.VersionFila).HasColumnName("xmin").IsRowVersion();

        builder.HasOne<Venta>().WithOne().HasForeignKey<Financiamiento>(f => f.VentaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(f => f.Mensualidades).WithOne().HasForeignKey(m => m.FinanciamientoId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(f => f.Mensualidades).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(f => f.VentaId).IsUnique().HasDatabaseName("uq_financiamientos_venta_id");
    }
}
