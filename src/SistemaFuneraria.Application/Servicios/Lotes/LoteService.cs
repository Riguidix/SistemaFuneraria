using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.DTOs.Lotes;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Application.Servicios.Lotes;

/// <summary>
/// Implementación del servicio de catálogo y gestión de inventario de lotes (SF-041).
/// </summary>
public class LoteService(IAppDbContext context) : ILoteService
{
    private readonly IAppDbContext _context = context;

    public async Task<Resultado<ResumenCatalogoLotesDto>> ObtenerCatalogoAsync(
        string? buscar = null,
        string? seccion = null,
        string? estado = null,
        int? gavetas = null,
        int pagina = 1,
        int tamanoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);

        var todosLosLotes = await _context.Lotes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        int totalLotes = todosLosLotes.Count;
        var disponibles = todosLosLotes.Where(l => l.Estado == EstadoLote.Disponible).ToList();
        int totalDisponibles = disponibles.Count;
        decimal precioDesde = totalDisponibles > 0 ? disponibles.Min(l => l.PrecioContado.Monto) : 0m;

        var grupoSeccion = disponibles
            .GroupBy(l => l.Seccion)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .FirstOrDefault();

        string seccionMasEspacio = grupoSeccion?.Key ?? (todosLosLotes.FirstOrDefault()?.Seccion ?? "N/A");
        int lotesLibresSeccionMasEspacio = grupoSeccion?.Count() ?? 0;

        var query = todosLosLotes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var termino = buscar.Trim().ToLowerInvariant();
            query = query.Where(l =>
                l.Clave.ToLowerInvariant().Contains(termino) ||
                l.Seccion.ToLowerInvariant().Contains(termino) ||
                l.AndadorFila.ToLowerInvariant().Contains(termino) ||
                $"{l.Gavetas} gavetas".Contains(termino) ||
                $"{l.Gavetas} gaveta".Contains(termino) ||
                l.Gavetas.ToString().Contains(termino));
        }

        if (!string.IsNullOrWhiteSpace(seccion) && !seccion.Equals("todas", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(l => l.Seccion.Equals(seccion.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(estado) && !estado.Equals("todos", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<EstadoLote>(estado.Trim(), true, out var estadoParsed))
            {
                query = query.Where(l => l.Estado == estadoParsed);
            }
        }

        if (gavetas.HasValue && gavetas.Value > 0)
        {
            query = query.Where(l => l.Gavetas == gavetas.Value);
        }

        var listaFiltrada = query.OrderBy(l => l.Clave).ToList();
        int totalFiltrados = listaFiltrada.Count;
        int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)totalFiltrados / tamanoPagina));

        var lotesPagina = listaFiltrada
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToList();

        var loteIds = lotesPagina.Select(l => l.Id).ToList();
        var ventasPorLote = loteIds.Count == 0
            ? new Dictionary<Guid, (Guid Id, string Folio)>()
            : (await _context.Ventas
                .AsNoTracking()
                .Where(v => loteIds.Contains(v.LoteId) && v.Estado != EstadoVenta.Cancelado)
                .Select(v => new { v.LoteId, v.Id, v.FolioContrato })
                .ToListAsync(cancellationToken))
                .GroupBy(v => v.LoteId)
                .ToDictionary(g => g.Key, g => (Id: g.First().Id, Folio: g.First().FolioContrato));

        var itemsPaginados = lotesPagina
            .Select(l =>
            {
                ventasPorLote.TryGetValue(l.Id, out var contrato);
                return new LoteItemDto(
                    l.Id,
                    l.Clave,
                    l.Seccion,
                    l.AndadorFila,
                    l.Gavetas,
                    l.PrecioContado.Monto,
                    l.Estado.ToString(),
                    contrato.Id == Guid.Empty ? null : contrato.Id,
                    contrato.Folio);
            })
            .ToList();

        var resumen = new ResumenCatalogoLotesDto(
            totalDisponibles,
            totalLotes,
            precioDesde,
            seccionMasEspacio,
            lotesLibresSeccionMasEspacio,
            itemsPaginados,
            pagina,
            totalPaginas,
            totalFiltrados);

        return Resultado<ResumenCatalogoLotesDto>.Exito(resumen);
    }

    public async Task<Resultado<LoteItemDto>> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lote = await _context.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (lote == null)
            return Resultado<LoteItemDto>.Fallo("[NO_ENCONTRADO] Lote no encontrado.");

        var contrato = await _context.Ventas
            .AsNoTracking()
            .Where(v => v.LoteId == lote.Id && v.Estado != EstadoVenta.Cancelado)
            .Select(v => new { v.Id, v.FolioContrato })
            .FirstOrDefaultAsync(cancellationToken);

        return Resultado<LoteItemDto>.Exito(new LoteItemDto(
            lote.Id,
            lote.Clave,
            lote.Seccion,
            lote.AndadorFila,
            lote.Gavetas,
            lote.PrecioContado.Monto,
            lote.Estado.ToString(),
            contrato?.Id,
            contrato?.FolioContrato));
    }

    public async Task<Resultado<LoteItemDto>> ObtenerPorClaveAsync(string clave, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return Resultado<LoteItemDto>.Fallo("La clave no puede estar vacía.");

        var claveNorm = clave.Trim().ToUpperInvariant();
        var lote = await _context.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Clave == claveNorm, cancellationToken);

        if (lote == null)
            return Resultado<LoteItemDto>.Fallo("[NO_ENCONTRADO] Lote no encontrado.");

        var contrato = await _context.Ventas
            .AsNoTracking()
            .Where(v => v.LoteId == lote.Id && v.Estado != EstadoVenta.Cancelado)
            .Select(v => new { v.Id, v.FolioContrato })
            .FirstOrDefaultAsync(cancellationToken);

        return Resultado<LoteItemDto>.Exito(new LoteItemDto(
            lote.Id,
            lote.Clave,
            lote.Seccion,
            lote.AndadorFila,
            lote.Gavetas,
            lote.PrecioContado.Monto,
            lote.Estado.ToString(),
            contrato?.Id,
            contrato?.FolioContrato));
    }

    public async Task<Resultado<Guid>> CrearLoteAsync(CrearLoteCommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            return Resultado<Guid>.Fallo("El comando de creación no puede ser nulo.");

        var claveNorm = command.Clave?.Trim().ToUpperInvariant() ?? string.Empty;
        bool existeClave = await _context.Lotes.AnyAsync(l => l.Clave == claveNorm, cancellationToken);
        if (existeClave)
            return Resultado<Guid>.Fallo("[CONFLICTO] Ya existe un lote registrado con la clave especificada.");

        try
        {
            var lote = Lote.Crear(
                command.Clave ?? string.Empty,
                command.Seccion ?? string.Empty,
                command.AndadorFila ?? string.Empty,
                command.Gavetas,
                new Dinero(command.PrecioContado));

            await _context.Lotes.AddAsync(lote, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Resultado<Guid>.Exito(lote.Id);
        }
        catch (ReglaNegocioException ex)
        {
            return Resultado<Guid>.Fallo(ex.Message);
        }
    }

    public async Task<Resultado> ActualizarLoteAsync(Guid id, ActualizarLoteCommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            return Resultado.Fallo("El comando de actualización no puede ser nulo.");

        var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (lote == null)
            return Resultado.Fallo("[NO_ENCONTRADO] Lote no encontrado.");

        if (lote.Estado == EstadoLote.Vendido)
            return Resultado.Fallo("[CONFLICTO] No es posible modificar un lote que ya se encuentra vendido.");

        try
        {
            lote.ActualizarDatosAdmin(
                command.Seccion,
                command.AndadorFila,
                command.Gavetas,
                new Dinero(command.PrecioContado),
                command.Estado);

            await _context.SaveChangesAsync(cancellationToken);
            return Resultado.Exito();
        }
        catch (ReglaNegocioException ex)
        {
            return Resultado.Fallo(ex.Message);
        }
    }

    public async Task<Resultado> CambiarEstadoLoteAsync(Guid id, EstadoLote nuevoEstado, CancellationToken cancellationToken = default)
    {
        var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (lote == null)
            return Resultado.Fallo("[NO_ENCONTRADO] Lote no encontrado.");

        if (!Enum.IsDefined(nuevoEstado))
            return Resultado.Fallo("El estado indicado no es válido.");

        try
        {
            if (nuevoEstado == EstadoLote.Vendido)
            {
                lote.MarcarVendido();
            }
            else if (nuevoEstado == EstadoLote.Reservado)
            {
                lote.Reservar();
            }
            else if (nuevoEstado == EstadoLote.Disponible)
            {
                lote.Liberar();
            }
            else
            {
                lote.ActualizarDatosAdmin(lote.Seccion, lote.AndadorFila, lote.Gavetas, lote.PrecioContado, nuevoEstado);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Resultado.Exito();
        }
        catch (ReglaNegocioException ex)
        {
            return Resultado.Fallo(ex.Message);
        }
    }
}
