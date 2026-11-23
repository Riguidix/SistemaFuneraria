using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="Pago"/> para PostgreSQL 16 (SF-076).
/// </summary>
public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    /// <summary>
    /// Configura tabla, columnas monetarias, relaciones foráneas e índices únicos de auditoría de caja.
    /// </summary>
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("pagos", t =>
        {
            t.HasCheckConstraint("pagos_tipo_pago_check", "tipo_pago IN ('Enganche', 'Contado', 'Mensualidad', 'AbonoCapital', 'Liquidacion')");
            t.HasCheckConstraint("pagos_metodo_pago_check", "metodo_pago IN ('Efectivo', 'TransferenciaBancaria', 'Tarjeta')");
            t.HasCheckConstraint("pagos_monto_total_check", "monto_total > 0");
            t.HasCheckConstraint("pagos_monto_capital_check", "monto_capital >= 0");
            t.HasCheckConstraint("pagos_monto_interes_check", "monto_interes >= 0");
            t.HasCheckConstraint("pagos_saldo_anterior_check", "saldo_anterior >= 0");
            t.HasCheckConstraint("pagos_saldo_posterior_check", "saldo_posterior >= 0");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(p => p.Folio).HasColumnName("folio").HasMaxLength(20).IsRequired();
        builder.Property(p => p.VentaId).HasColumnName("venta_id").HasColumnType("uuid").IsRequired();
        builder.Property(p => p.FinanciamientoId).HasColumnName("financiamiento_id").HasColumnType("uuid");
        builder.Property(p => p.MensualidadId).HasColumnName("mensualidad_id").HasColumnType("uuid");
        builder.Property(p => p.CajeroId).HasColumnName("cajero_id").HasMaxLength(450).IsRequired();
        builder.Property(p => p.Tipo).HasColumnName("tipo_pago").HasMaxLength(25).HasConversion<string>().IsRequired();
        builder.Property(p => p.Metodo).HasColumnName("metodo_pago").HasMaxLength(25).HasConversion<string>().IsRequired();
        builder.Property(p => p.Referencia).HasColumnName("referencia").HasMaxLength(60);
        builder.Property(p => p.MontoTotal).HasColumnName("monto_total").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(p => p.MontoCapital).HasColumnName("monto_capital").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(p => p.MontoInteres).HasColumnName("monto_interes").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(p => p.SaldoAnterior).HasColumnName("saldo_anterior").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(p => p.SaldoPosterior).HasColumnName("saldo_posterior").HasColumnType("numeric(18,2)").HasPrecision(18, 2).HasConversion(d => d.Monto, m => new Dinero(m)).IsRequired();
        builder.Property(p => p.ClaveIdempotencia).HasColumnName("clave_idempotencia").HasMaxLength(64).IsRequired();
        builder.Property(p => p.FechaPago).HasColumnName("fecha_pago").HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").IsRequired();

        builder.HasOne<Venta>().WithMany().HasForeignKey(p => p.VentaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Financiamiento>().WithMany().HasForeignKey(p => p.FinanciamientoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Mensualidad>().WithMany().HasForeignKey(p => p.MensualidadId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<IdentityUser>().WithMany().HasForeignKey(p => p.CajeroId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Folio).IsUnique().HasDatabaseName("ix_pagos_folio");
        builder.HasIndex(p => p.ClaveIdempotencia).IsUnique().HasDatabaseName("ix_pagos_clave_idempotencia");
        builder.HasIndex(p => p.VentaId).HasDatabaseName("ix_pagos_venta_id");
        builder.HasIndex(p => p.FinanciamientoId).HasDatabaseName("ix_pagos_financiamiento_id");
        builder.HasIndex(p => p.MensualidadId).HasDatabaseName("ix_pagos_mensualidad_id");
        builder.HasIndex(p => p.CajeroId).HasDatabaseName("ix_pagos_cajero_id");
        builder.HasIndex(p => p.FechaPago).IsDescending().HasDatabaseName("ix_pagos_fecha");
    }
}
