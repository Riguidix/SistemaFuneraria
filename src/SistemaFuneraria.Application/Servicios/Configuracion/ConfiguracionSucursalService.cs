using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.DTOs.Configuracion;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Application.Servicios.Configuracion;

/// <summary>
/// Implementación persistente de configuración de sucursal con inicialización por defecto (SF-091).
/// </summary>
public class ConfiguracionSucursalService(IAppDbContext context) : IConfiguracionSucursalService
{
    private readonly IAppDbContext _context = context;

    /// <inheritdoc />
    public async Task<Resultado<ConfiguracionSucursalDto>> ObtenerSucursalAsync(CancellationToken cancellationToken = default)
    {
        var config = await _context.ConfiguracionesSucursal
            .FirstOrDefaultAsync(cancellationToken);

        if (config is null)
        {
            config = ConfiguracionSucursal.CrearPorDefecto();
            await _context.ConfiguracionesSucursal.AddAsync(config, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Resultado<ConfiguracionSucursalDto>.Exito(MapearDto(config));
    }

    /// <inheritdoc />
    public async Task<Resultado<ConfiguracionSucursalDto>> ActualizarSucursalAsync(
        ActualizarConfiguracionSucursalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Resultado<ConfiguracionSucursalDto>.Fallo("Los datos de configuración son obligatorios.");
        }

        try
        {
            var config = await _context.ConfiguracionesSucursal
                .FirstOrDefaultAsync(cancellationToken);

            if (config is null)
            {
                config = ConfiguracionSucursal.CrearPorDefecto();
                await _context.ConfiguracionesSucursal.AddAsync(config, cancellationToken);
            }

            config.Actualizar(
                nombreComercial: request.NombreComercial,
                telefonoAtencion: request.TelefonoAtencion,
                correoOperativo: request.CorreoOperativo,
                direccion: request.Direccion,
                ciudad: request.Ciudad,
                codigoPostal: request.CodigoPostal,
                recordatoriosPago: request.RecordatoriosPago,
                resumenDiarioCaja: request.ResumenDiarioCaja);

            await _context.SaveChangesAsync(cancellationToken);
            return Resultado<ConfiguracionSucursalDto>.Exito(MapearDto(config));
        }
        catch (ReglaNegocioException ex)
        {
            return Resultado<ConfiguracionSucursalDto>.Fallo(ex.Message);
        }
    }

    private static ConfiguracionSucursalDto MapearDto(ConfiguracionSucursal c) => new(
        Id: c.Id,
        NombreComercial: c.NombreComercial,
        TelefonoAtencion: c.TelefonoAtencion,
        CorreoOperativo: c.CorreoOperativo,
        Direccion: c.Direccion,
        Ciudad: c.Ciudad,
        CodigoPostal: c.CodigoPostal,
        RecordatoriosPago: c.RecordatoriosPago,
        ResumenDiarioCaja: c.ResumenDiarioCaja,
        FechaActualizacion: c.FechaActualizacion);
}
