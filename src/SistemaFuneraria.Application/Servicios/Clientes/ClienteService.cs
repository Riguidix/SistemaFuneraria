using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Application.Servicios.Clientes;

/// <summary>
/// Implementación del caso de uso de gestión de clientes (SF-018, SF-031).
/// Orquesta la validación de negocio del cliente, búsqueda por filtros, actualización y ciclo de vida.
/// </summary>
public class ClienteService(IAppDbContext context) : IClienteService
{
    /// <inheritdoc />
    public async Task<Resultado<Guid>> RegistrarClienteAsync(RegistrarClienteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var curpNormalizada = request.Curp?.Trim().ToUpperInvariant() ?? string.Empty;
            var correoNormalizado = request.Correo?.Trim().ToLowerInvariant() ?? string.Empty;

            var existeCurp = await context.Clientes.AnyAsync(c => c.Curp == curpNormalizada, cancellationToken);
            if (existeCurp)
                return Resultado<Guid>.Fallo("[CONFLICTO] Ya existe un cliente registrado con esa CURP.");

            var existeCorreo = await context.Clientes.AnyAsync(c => c.Correo == correoNormalizado, cancellationToken);
            if (existeCorreo)
                return Resultado<Guid>.Fallo("[CONFLICTO] Ya existe un cliente registrado con ese correo electrónico.");
            
            var cliente = Cliente.Crear(
                request.Nombre,
                request.Apellidos,
                curpNormalizada,
                request.Telefono,
                correoNormalizado,
                request.Direccion
            );
            
            await context.Clientes.AddAsync(cliente, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Resultado<Guid>.Exito(cliente.Id);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ReglaNegocioException reglaException)
        {
            return Resultado<Guid>.Fallo(reglaException.Message);
        }
        catch (Exception exception)
        {
            return Resultado<Guid>.Fallo(exception.Message);
        }
    }

    /// <inheritdoc />
    public async Task<Resultado<ClienteDto>> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (cliente == null)
            return Resultado<ClienteDto>.Fallo("[NO_ENCONTRADO] Cliente no encontrado.");
        
        return MapearADto(cliente);
    }

    /// <inheritdoc />
    public async Task<Resultado<ClienteDto>> ObtenerPorCurpAsync(string curp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(curp))
            return Resultado<ClienteDto>.Fallo("La CURP no puede estar vacía.");

        var curpNormalizada = curp.Trim().ToUpperInvariant();

        var cliente = await context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Curp == curpNormalizada, cancellationToken);

        if (cliente == null)
            return Resultado<ClienteDto>.Fallo("[NO_ENCONTRADO] Cliente no encontrado.");

        return MapearADto(cliente);
    }

    /// <inheritdoc />
    public async Task<Resultado<IReadOnlyList<ClienteDto>>> ListarClientesAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Apellidos)
            .ThenBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

        var dtos = clientes.Select(MapearADto).ToList();
            
        return dtos;
    }

    /// <inheritdoc />
    public async Task<Resultado<IReadOnlyList<ClienteDto>>> BuscarClientesAsync(string? filtro, CancellationToken cancellationToken = default)
    {
        var query = context.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            string f = filtro.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(f) ||
                c.Apellidos.ToLower().Contains(f) ||
                c.Curp.ToLower().Contains(f) ||
                c.Telefono.Contains(f));
        }

        var clientes = await query
            .OrderBy(c => c.Apellidos)
            .ThenBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

        var dtos = clientes.Select(MapearADto).ToList();
        return dtos;
    }

    /// <inheritdoc />
    public async Task<Resultado> ActualizarClienteAsync(Guid id, ActualizarClienteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (cliente == null)
                return Resultado.Fallo("[NO_ENCONTRADO] Cliente no encontrado.");

            var curpNormalizada = request.Curp?.Trim().ToUpperInvariant() ?? string.Empty;
            var correoNormalizado = request.Correo?.Trim().ToLowerInvariant() ?? string.Empty;

            var existeCurp = await context.Clientes.AnyAsync(c => c.Id != id && c.Curp == curpNormalizada, cancellationToken);
            if (existeCurp)
                return Resultado.Fallo("[CONFLICTO] Ya existe otro cliente registrado con esa CURP.");

            var existeCorreo = await context.Clientes.AnyAsync(c => c.Id != id && c.Correo == correoNormalizado, cancellationToken);
            if (existeCorreo)
                return Resultado.Fallo("[CONFLICTO] Ya existe otro cliente registrado con ese correo electrónico.");

            cliente.Actualizar(
                request.Nombre,
                request.Apellidos,
                curpNormalizada,
                request.Telefono,
                correoNormalizado,
                request.Direccion
            );

            await context.SaveChangesAsync(cancellationToken);
            return Resultado.Exito();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ReglaNegocioException reglaEx)
        {
            return Resultado.Fallo(reglaEx.Message);
        }
        catch (Exception ex)
        {
            return Resultado.Fallo(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<Resultado> EliminarClienteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (cliente == null)
            return Resultado.Fallo("[NO_ENCONTRADO] Cliente no encontrado.");

        context.Clientes.Remove(cliente);
        await context.SaveChangesAsync(cancellationToken);

        return Resultado.Exito();
    }

    private static ClienteDto MapearADto(Cliente c)
    {
        string fechaFormateada = $"Cliente desde {c.CreatedAt:MMM. yyyy}";
        return new ClienteDto(
            c.Id,
            c.Nombre,
            c.Apellidos,
            c.Curp,
            c.Telefono,
            c.Correo,
            c.Direccion,
            c.CreatedAt,
            c.ObtenerIniciales(),
            fechaFormateada
        );
    }
}
