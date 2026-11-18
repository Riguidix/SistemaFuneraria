using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.DTOs.Lotes;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Application.Servicios.Lotes;

public class LoteService(IAppDbContext context) : ILoteService
{
    private readonly IAppDbContext _context = context;

    public async Task<Resultado<ResumenCatalogoLotesDto>> ObtenerCatalogoAsync(
        string? buscar = null,
        string? seccion = null,
        string? estado = null,
        int? gavetas = null,
        int pagina = 1,
        int tamanoPagina = 20,
        CancellationToken ct = default)
    {
        var todosLotes = await _context.Lotes
            .AsNoTracking()
            .ToListAsync(ct);

        var disponibles = todosLotes.Where(l => l.Estado == EstadoLote.Disponible).ToList();
        int totalDisponibles = disponibles.Count;
        int totalLotes = todosLotes.Count;
        decimal precioDesde = disponibles.Count > 0 ? disponibles.Min(l => l.PrecioContado.Monto) : 0m;

        var grupoSeccion = disponibles
            .GroupBy(l => l.Seccion)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        string seccionMasEspacio = grupoSeccion?.Key ?? "Jardín Norte";
        int libresSeccion = grupoSeccion?.Count() ?? 0;

        var query = todosLotes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var b = buscar.Trim();
            query = query.Where(l =>
                l.Clave.Contains(b, StringComparison.OrdinalIgnoreCase) ||
                l.Seccion.Contains(b, StringComparison.OrdinalIgnoreCase) ||
                l.AndadorFila.Contains(b, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(seccion))
        {
            query = query.Where(l => l.Seccion.Equals(seccion.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (Enum.TryParse<EstadoLote>(estado.Trim(), true, out var est))
            {
                query = query.Where(l => l.Estado == est);
            }
        }

        if (gavetas.HasValue)
        {
            query = query.Where(l => l.Gavetas == gavetas.Value);
        }

        var filtrados = query
            .OrderBy(l => l.Seccion)
            .ThenBy(l => l.Clave)
            .ToList();

        var items = filtrados
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(l => new LoteItemDto(
                l.Id,
                l.Clave,
                l.Seccion,
                l.AndadorFila,
                l.Gavetas,
                l.PrecioContado.Monto,
                l.Estado.ToString()
            ))
            .ToList();

        int totalPaginas = (int)Math.Ceiling(filtrados.Count / (double)tamanoPagina);
        if (totalPaginas < 1) totalPaginas = 1;

        var resumen = new ResumenCatalogoLotesDto(
            totalDisponibles,
            totalLotes,
            precioDesde,
            seccionMasEspacio,
            libresSeccion,
            items,
            pagina,
            totalPaginas
        );

        return Resultado<ResumenCatalogoLotesDto>.Exito(resumen);
    }

    public async Task<Resultado<LoteItemDto>> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var lote = await _context.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (lote is null)
        {
            return Resultado<LoteItemDto>.Fallo("Lote no encontrado.");
        }

        var dto = new LoteItemDto(
            lote.Id,
            lote.Clave,
            lote.Seccion,
            lote.AndadorFila,
            lote.Gavetas,
            lote.PrecioContado.Monto,
            lote.Estado.ToString()
        );

        return Resultado<LoteItemDto>.Exito(dto);
    }

    public async Task<Resultado<Guid>> CrearLoteAsync(CrearLoteRequest request, CancellationToken ct = default)
    {
        try
        {
            var existeClave = await _context.Lotes
                .AnyAsync(l => l.Clave == request.Clave.Trim().ToUpperInvariant(), ct);

            if (existeClave)
            {
                return Resultado<Guid>.Fallo("Ya existe un lote registrado con esa clave.");
            }

            var lote = Lote.Crear(
                request.Clave,
                request.Seccion,
                request.AndadorFila,
                request.Gavetas,
                new Dinero(request.PrecioContado)
            );

            await _context.Lotes.AddAsync(lote, ct);
            await _context.SaveChangesAsync(ct);

            return Resultado<Guid>.Exito(lote.Id);
        }
        catch (ReglaNegocioException ex)
        {
            return Resultado<Guid>.Fallo(ex.Message);
        }
        catch (Exception ex)
        {
            return Resultado<Guid>.Fallo(ex.Message);
        }
    }
}
