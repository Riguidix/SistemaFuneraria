using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaFuneraria.Core.Entidades;

namespace SistemaFuneraria.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Configuración EF Core para ConfiguracionSucursal (SF-091).
/// </summary>
public class ConfiguracionSucursalConfiguration : IEntityTypeConfiguration<ConfiguracionSucursal>
{
    public void Configure(EntityTypeBuilder<ConfiguracionSucursal> builder)
    {
        builder.ToTable("configuracion_sucursal");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.NombreComercial).HasColumnName("nombre_comercial").HasMaxLength(150).IsRequired();
        builder.Property(c => c.TelefonoAtencion).HasColumnName("telefono_atencion").HasMaxLength(30).IsRequired();
        builder.Property(c => c.CorreoOperativo).HasColumnName("correo_operativo").HasMaxLength(150).IsRequired();
        builder.Property(c => c.Direccion).HasColumnName("direccion").HasMaxLength(250).IsRequired();
        builder.Property(c => c.Ciudad).HasColumnName("ciudad").HasMaxLength(100);
        builder.Property(c => c.CodigoPostal).HasColumnName("codigo_postal").HasMaxLength(15);
        builder.Property(c => c.RecordatoriosPago).HasColumnName("recordatorios_pago").IsRequired();
        builder.Property(c => c.ResumenDiarioCaja).HasColumnName("resumen_diario_caja").IsRequired();
        builder.Property(c => c.FechaActualizacion).HasColumnName("fecha_actualizacion").IsRequired();
    }
}
