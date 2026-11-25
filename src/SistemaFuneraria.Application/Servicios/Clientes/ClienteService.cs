using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;

namespace SistemaFuneraria.Application.Servicios.Clientes;

/// <summary>
/// Implementación del caso de uso de gestión de clientes (SF-018, SF-031).
/// Orquesta la validación de negocio del cliente, búsqueda por filtros, resumen de cartera, actualización y ciclo de vida.
/// </summary>
public class ClienteService(IAppDbContext context, TimeProvider? timeProvider = null) : IClienteService
{
    private static readonly CultureInfo CulturaEsMx = CultureInfo.GetCultureInfo("es-MX");
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
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

        var dtos = await MapearConCarteraAsync([cliente], cancellationToken);
        return dtos[0];
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

        var dtos = await MapearConCarteraAsync([cliente], cancellationToken);
        return dtos[0];
    }

    /// <inheritdoc />
    public async Task<Resultado<IReadOnlyList<ClienteDto>>> ListarClientesAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Apellidos)
            .ThenBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

        var dtos = await MapearConCarteraAsync(clientes, cancellationToken);
        return dtos;
    }

    /// <inheritdoc />
    public async Task<Resultado<IReadOnlyList<ClienteDto>>> BuscarClientesAsync(string? filtro, CancellationToken cancellationToken = default)
    {
        var query = context.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            string f = filtro.Trim().ToLowerInvariant();
            string fTel = f.Replace(" ", string.Empty).Replace("-", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);

            query = query.Where(c =>
                c.Nombre.ToLower().Contains(f) ||
                c.Apellidos.ToLower().Contains(f) ||
                (c.Nombre + " " + c.Apellidos).ToLower().Contains(f) ||
                c.Correo.ToLower().Contains(f) ||
                c.Curp.ToLower().Contains(f) ||
                c.Telefono.Contains(fTel));
        }

        var clientes = await query
            .OrderBy(c => c.Apellidos)
            .ThenBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

        var dtos = await MapearConCarteraAsync(clientes, cancellationToken);
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

        var tieneContratos = await context.Ventas.AnyAsync(v => v.ClienteId == id, cancellationToken);
        if (tieneContratos)
            return Resultado.Fallo("[CONFLICTO] No se puede eliminar un cliente con contratos asociados.");

        context.Clientes.Remove(cliente);
        await context.SaveChangesAsync(cancellationToken);

        return Resultado.Exito();
    }

    private async Task<List<ClienteDto>> MapearConCarteraAsync(
        IReadOnlyList<Cliente> clientes,
        CancellationToken cancellationToken)
    {
        if (clientes.Count == 0)
        {
            return [];
        }

        var clienteIds = clientes.Select(c => c.Id).ToList();

        var ventas = await context.Ventas
            .AsNoTracking()
            .Where(v => clienteIds.Contains(v.ClienteId) && v.Estado != EstadoVenta.Cancelado)
            .Select(v => new { v.Id, v.ClienteId, v.Estado })
            .ToListAsync(cancellationToken);

        var ventaIds = ventas.Select(v => v.Id).ToList();

        var financiamientos = ventaIds.Count == 0
            ? []
            : await context.Financiamientos
                .AsNoTracking()
                .Where(f => ventaIds.Contains(f.VentaId))
                .Select(f => new { f.Id, f.VentaId, f.Estado, SaldoInsoluto = f.SaldoInsoluto.Monto })
                .ToListAsync(cancellationToken);

        var finActivosIds = financiamientos
            .Where(f => f.Estado is EstadoFinanciamiento.Activo or EstadoFinanciamiento.EnMora)
            .Select(f => f.Id)
            .ToList();

        var fechaHoy = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        var financiamientosConVencidas = finActivosIds.Count == 0
            ? new HashSet<Guid>()
            : (await context.Mensualidades
                .AsNoTracking()
                .Where(m => finActivosIds.Contains(m.FinanciamientoId) &&
                    (m.Estado == EstadoMensualidad.Vencida ||
                     (m.Estado == EstadoMensualidad.Pendiente && m.FechaVencimiento < fechaHoy)))
                .Select(m => m.FinanciamientoId)
                .Distinct()
                .ToListAsync(cancellationToken))
                .ToHashSet();

        var pagos = ventaIds.Count == 0
            ? []
            : await context.Pagos
                .AsNoTracking()
                .Where(p => ventaIds.Contains(p.VentaId))
                .Select(p => new { p.VentaId, p.FechaPago })
                .ToListAsync(cancellationToken);

        var ventasPorCliente = ventas.GroupBy(v => v.ClienteId).ToDictionary(g => g.Key, g => g.ToList());
        var finPorVenta = financiamientos.GroupBy(f => f.VentaId).ToDictionary(g => g.Key, g => g.ToList());
        var ultimoPagoPorVenta = pagos
            .GroupBy(p => p.VentaId)
            .ToDictionary(g => g.Key, g => g.Max(x => x.FechaPago));

        var resultado = new List<ClienteDto>(clientes.Count);

        foreach (var c in clientes)
        {
            int contratosActivos = 0;
            decimal saldoActual = 0m;
            DateTime? ultimoPago = null;
            bool tieneVencido = false;
            bool tieneEnRevision = false;

            if (ventasPorCliente.TryGetValue(c.Id, out var ventasCli))
            {
                contratosActivos = ventasCli.Count(v => v.Estado is EstadoVenta.Activo or EstadoVenta.EnRevision);
                tieneEnRevision = ventasCli.Any(v => v.Estado == EstadoVenta.EnRevision);

                DateTimeOffset? maxFechaPago = null;
                foreach (var v in ventasCli)
                {
                    if (ultimoPagoPorVenta.TryGetValue(v.Id, out var fp) && (!maxFechaPago.HasValue || fp > maxFechaPago.Value))
                    {
                        maxFechaPago = fp;
                    }

                    if (finPorVenta.TryGetValue(v.Id, out var fins))
                    {
                        foreach (var f in fins)
                        {
                            if (f.Estado is EstadoFinanciamiento.Activo or EstadoFinanciamiento.EnMora)
                            {
                                saldoActual += f.SaldoInsoluto;
                                if (f.Estado == EstadoFinanciamiento.EnMora || financiamientosConVencidas.Contains(f.Id))
                                {
                                    tieneVencido = true;
                                }
                            }
                        }
                    }
                }

                ultimoPago = maxFechaPago?.UtcDateTime;
            }

            string estadoCartera = tieneVencido
                ? "Pago vencido"
                : tieneEnRevision
                    ? "En revisión"
                    : "Al corriente";

            string fechaFormateada = $"Cliente desde {c.CreatedAt.ToString("MMM. yyyy", CulturaEsMx)}";

            resultado.Add(new ClienteDto(
                c.Id,
                c.Nombre,
                c.Apellidos,
                c.Curp,
                c.Telefono,
                c.Correo,
                c.Direccion,
                c.CreatedAt,
                c.ObtenerIniciales(),
                fechaFormateada,
                contratosActivos,
                saldoActual,
                ultimoPago,
                estadoCartera));
        }

        return resultado;
    }
}
