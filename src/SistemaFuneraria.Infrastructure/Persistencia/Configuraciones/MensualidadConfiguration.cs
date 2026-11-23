using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="Mensualidad"/> para PostgreSQL 16 (SF-059).
/// </summary>
public class MensualidadConfiguration : IEntityTypeConfiguration<Mensualidad>
{
    /// <summary>
    /// Configura tabla, columnas monetarias, fechas, restricciones e índices de <see cref="Mensualidad"/>.
    /// </summary>
    public void Configure(EntityTypeBuilder<Mensualidad> builder)
    {
        builder.ToTable("mensualidades", t =>
        {
            t.HasCheckConstraint("mensualidades_numero_check", "numero_cuota >= 1");
            t.HasCheckConstraint("mensualidades_saldo_ini_check", "saldo_inicial_periodo >= 0");
            t.HasCheckConstraint("mensualidades_capital_check", "capital_programado >= 0");
            t.HasCheckConstraint("mensualidades_interes_check", "interes_programado >= 0");
            t.HasCheckConstraint("mensualidades_cuota_check", "monto_total_cuota > 0");
            t.HasCheckConstraint("mensualidades_saldo_fin_check", "saldo_final_periodo >= 0");
            t.HasCheckConstraint("mensualidades_estado_check", "estado IN ('Pendiente', 'Pagada', 'Vencida')");
        });

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(m => m.FinanciamientoId).HasColumnName("financiamiento_id").HasColumnType("uuid").IsRequired();
        builder.Property(m => m.NumeroCuota).HasColumnName("numero_cuota").IsRequired();
        builder.Property(m => m.FechaVencimiento).HasColumnName("fecha_vencimiento").HasColumnType("date").IsRequired();
        builder.Property(m => m.SaldoInicialPeriodo).HasColumnName("saldo_inicial_periodo").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, v => new Dinero(v)).IsRequired();
        builder.Property(m => m.CapitalProgramado).HasColumnName("capital_programado").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, v => new Dinero(v)).IsRequired();
        builder.Property(m => m.InteresProgramado).HasColumnName("interes_programado").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, v => new Dinero(v)).IsRequired();
        builder.Property(m => m.MontoTotalCuota).HasColumnName("monto_total_cuota").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, v => new Dinero(v)).IsRequired();
        builder.Property(m => m.SaldoFinalPeriodo).HasColumnName("saldo_final_periodo").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, v => new Dinero(v)).IsRequired();
        builder.Property(m => m.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(m => m.FechaPago).HasColumnName("fecha_pago").HasColumnType("timestamp with time zone");

        builder.HasIndex(m => new { m.FinanciamientoId, m.NumeroCuota }).IsUnique().HasDatabaseName("uq_mensualidades_cuota");
        builder.HasIndex(m => m.FechaVencimiento).HasDatabaseName("ix_mensualidades_vencimiento");
        builder.HasIndex(m => new { m.Estado, m.FechaVencimiento }).HasDatabaseName("ix_mensualidades_estado_vencimiento");
    }
}
