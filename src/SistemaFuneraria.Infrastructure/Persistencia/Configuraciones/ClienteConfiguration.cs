using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Configuración Fluent API de la entidad <see cref="Cliente"/> para PostgreSQL 16 (ERD v1.1 Sección 3.2, SF-016).
/// </summary>
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes", t =>
            t.HasCheckConstraint("clientes_telefono_check", "telefono ~ '^[0-9]{10}$'"));

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();
        
        builder.Property(c => c.Curp)
            .HasColumnName("curp")
            .HasMaxLength(18)
            .IsRequired();
        
        builder.Property(c => c.Correo)
            .HasColumnName("correo")
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(c => c.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(c => c.Apellidos)
            .HasColumnName("apellidos")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(c => c.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(10)
            .IsRequired();
        
        builder.Property(c => c.Direccion)
            .HasColumnName("direccion")
            .HasColumnType("text")
            .IsRequired();
        
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(c => c.Curp)
            .IsUnique()
            .HasDatabaseName("ix_clientes_curp");
        
        builder.HasIndex(c => c.Correo)
            .IsUnique()
            .HasDatabaseName("ix_clientes_correo");
            
        builder.HasIndex(c => new { c.Apellidos, c.Nombre })
            .HasDatabaseName("ix_clientes_apellidos_nombre")
            .HasOperators("text_pattern_ops", "text_pattern_ops");
    }
}
