using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.DTOs.Contratos;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Core.Enums;

namespace SistemaFuneraria.Application.Servicios.Contratos;

/// <summary>
/// Implementación del servicio de cartera de contratos, KPIs y detalle por folio o UUID (SF-068, SF-069).
/// </summary>
public class CarteraContratosService(IAppDbContext context, TimeProvider? timeProvider = null) : ICarteraContratosService
{
    private static readonly CultureInfo CulturaEsMx = CultureInfo.GetCultureInfo("es-MX");
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <inheritdoc />
    public async Task<Resultado<CarteraContratosResponseDto>> ObtenerCarteraAsync(
        string? buscar = null,
        string? tab = "Todos",
        int pagina = 1,
        int tamanoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var paginaSegura = Math.Max(1, pagina);
        var tamanoSeguro = Math.Clamp(tamanoPagina, 1, 100);
        var anioActual = _timeProvider.GetLocalNow().Year;

        var ventas = await _context.Ventas
            .AsNoTracking()
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync(cancellationToken);

        var clientes = await _context.Clientes
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => $"{c.Nombre} {c.Apellidos}", cancellationToken);

        var lotes = await _context.Lotes
            .AsNoTracking()
            .ToDictionaryAsync(l => l.Id, l => l.Clave, cancellationToken);

        var financiamientos = await _context.Financiamientos
            .AsNoTracking()
            .ToDictionaryAsync(f => f.VentaId, cancellationToken);

        var mensualidadesPorFin = (await _context.Mensualidades
            .AsNoTracking()
            .OrderBy(m => m.NumeroCuota)
            .ToListAsync(cancellationToken))
            .GroupBy(m => m.FinanciamientoId)
            .ToDictionary(g => g.Key, g => g.ToList());

        int totalActivos = 0;
        decimal montoEnCartera = 0m;
        int totalAlCorriente = 0;
        int totalConAtraso = 0;
        decimal montoPorRecuperar = 0m;
        int totalLiquidadosAnio = 0;

        var proyecciones = new List<(ContratoCarteraItemDto Item, EstadoVenta EstadoVenta, bool TieneAtraso)>(ventas.Count);

        foreach (var venta in ventas)
        {
            financiamientos.TryGetValue(venta.Id, out var fin);
            var cuotas = fin is not null && mensualidadesPorFin.TryGetValue(fin.Id, out var listaCuotas)
                ? listaCuotas
                : [];

            var cuotasVencidas = cuotas.Where(c => c.Estado == EstadoMensualidad.Vencida).ToList();
            var cuotasPendientes = cuotas.Where(c => c.Estado == EstadoMensualidad.Pendiente).ToList();

            bool esActivo = venta.Estado is EstadoVenta.Activo or EstadoVenta.EnRevision;
            bool tieneAtraso = esActivo && (cuotasVencidas.Count > 0 || fin?.Estado == EstadoFinanciamiento.EnMora);

            decimal saldoInsoluto = venta.Estado is EstadoVenta.Liquidado or EstadoVenta.Cancelado
                ? 0m
                : fin?.SaldoInsoluto.Monto ?? 0m;

            if (esActivo)
            {
                totalActivos++;
                montoEnCartera += saldoInsoluto;
                if (tieneAtraso)
                {
                    totalConAtraso++;
                    montoPorRecuperar += cuotasVencidas.Sum(c => c.MontoTotalCuota.Monto);
                }
                else
                {
                    totalAlCorriente++;
                }
            }
            else if (venta.Estado == EstadoVenta.Liquidado && venta.FechaVenta.Year == anioActual)
            {
                totalLiquidadosAnio++;
            }

            string estadoVisual;
            string accionSugerida;
            DateOnly? proximoFecha = null;
            decimal? proximoMonto = null;
            string proximoTexto;

            if (venta.Estado == EstadoVenta.Liquidado)
            {
                estadoVisual = "Liquidado";
                accionSugerida = "Detalle";
                proximoTexto = "Liquidado";
            }
            else if (venta.Estado == EstadoVenta.Cancelado)
            {
                estadoVisual = "Cancelado";
                accionSugerida = "Detalle";
                proximoTexto = "Cancelado";
            }
            else if (tieneAtraso)
            {
                estadoVisual = "Vencido";
                accionSugerida = "Cobrar";
                var primeraVencida = cuotasVencidas.FirstOrDefault();
                if (primeraVencida is not null)
                {
                    proximoFecha = primeraVencida.FechaVencimiento;
                    proximoMonto = primeraVencida.MontoTotalCuota.Monto;
                    proximoTexto = $"Venció {FormatearFechaCorta(primeraVencida.FechaVencimiento)}";
                }
                else
                {
                    proximoTexto = "En mora";
                }
            }
            else if (venta.Estado == EstadoVenta.EnRevision)
            {
                estadoVisual = "En revisión";
                accionSugerida = "Detalle";
                var proxima = cuotasPendientes.FirstOrDefault();
                if (proxima is not null)
                {
                    proximoFecha = proxima.FechaVencimiento;
                    proximoMonto = proxima.MontoTotalCuota.Monto;
                    proximoTexto = $"{FormatearFechaCorta(proxima.FechaVencimiento)} · {FormatearMoneda(proxima.MontoTotalCuota.Monto)}";
                }
                else
                {
                    proximoTexto = "En revisión";
                }
            }
            else
            {
                estadoVisual = "Al corriente";
                accionSugerida = "Detalle";
                var proxima = cuotasPendientes.FirstOrDefault();
                if (proxima is not null)
                {
                    proximoFecha = proxima.FechaVencimiento;
                    proximoMonto = proxima.MontoTotalCuota.Monto;
                    proximoTexto = $"{FormatearFechaCorta(proxima.FechaVencimiento)} · {FormatearMoneda(proxima.MontoTotalCuota.Monto)}";
                }
                else
                {
                    proximoTexto = "Liquidado";
                }
            }

            var item = new ContratoCarteraItemDto(
                VentaId: venta.Id,
                FolioContrato: venta.FolioContrato,
                ClienteNombre: clientes.TryGetValue(venta.ClienteId, out var nombreCli) ? nombreCli : "N/D",
                LoteClave: lotes.TryGetValue(venta.LoteId, out var claveLote) ? claveLote : "N/D",
                Modalidad: venta.Modalidad.ToString(),
                SaldoInsoluto: saldoInsoluto,
                ProximoPagoFecha: proximoFecha,
                ProximoPagoMonto: proximoMonto,
                ProximoPagoTexto: proximoTexto,
                Estado: estadoVisual,
                AccionSugerida: accionSugerida);

            proyecciones.Add((item, venta.Estado, tieneAtraso));
        }

        decimal porcentajeAlCorriente = totalActivos > 0
            ? Math.Round(totalAlCorriente * 100m / totalActivos, 1, MidpointRounding.AwayFromZero)
            : 100m;

        var kpis = new CarteraKpisDto(
            TotalActivos: totalActivos,
            MontoEnCartera: montoEnCartera,
            TotalAlCorriente: totalAlCorriente,
            PorcentajeAlCorriente: porcentajeAlCorriente,
            TotalConAtraso: totalConAtraso,
            MontoPorRecuperar: montoPorRecuperar,
            TotalLiquidadosAnio: totalLiquidadosAnio);

        var tabNorm = (tab ?? "Todos").Trim().ToLowerInvariant();
        IEnumerable<(ContratoCarteraItemDto Item, EstadoVenta EstadoVenta, bool TieneAtraso)> filtrados = tabNorm switch
        {
            "activos" => proyecciones.Where(p => p.EstadoVenta is EstadoVenta.Activo or EstadoVenta.EnRevision),
            "vencidos" => proyecciones.Where(p => p.TieneAtraso),
            "liquidados" => proyecciones.Where(p => p.EstadoVenta == EstadoVenta.Liquidado),
            _ => proyecciones
        };

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var q = buscar.Trim();
            filtrados = filtrados.Where(p =>
                p.Item.FolioContrato.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Item.ClienteNombre.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Item.LoteClave.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var listaFiltrada = filtrados.Select(p => p.Item).ToList();
        var paginados = listaFiltrada
            .Skip((paginaSegura - 1) * tamanoSeguro)
            .Take(tamanoSeguro)
            .ToList();

        return Resultado<CarteraContratosResponseDto>.Exito(new CarteraContratosResponseDto(
            Kpis: kpis,
            Items: paginados,
            TotalRegistros: listaFiltrada.Count,
            Pagina: paginaSegura,
            TamanoPagina: tamanoSeguro));
    }

    /// <inheritdoc />
    public async Task<Resultado<ContratoDetalleDto>> ObtenerDetalleAsync(
        string folioOrId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folioOrId))
        {
            return Resultado<ContratoDetalleDto>.Fallo("[NO_ENCONTRADO] Debe proporcionar un identificador o folio de contrato.");
        }

        var texto = folioOrId.Trim();
        var venta = Guid.TryParse(texto, out var id)
            ? await _context.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            : await _context.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.FolioContrato == texto.ToUpperInvariant(), cancellationToken);

        if (venta is null)
        {
            return Resultado<ContratoDetalleDto>.Fallo($"[NO_ENCONTRADO] No se encontró el contrato '{texto}'.");
        }

        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == venta.ClienteId, cancellationToken);
        var lote = await _context.Lotes.AsNoTracking().FirstOrDefaultAsync(l => l.Id == venta.LoteId, cancellationToken);
        var pagos = await _context.Pagos
            .AsNoTracking()
            .Where(p => p.VentaId == venta.Id)
            .OrderByDescending(p => p.FechaPago)
            .Select(p => new ContratoPagoHistorialDto(
                p.Folio,
                p.FechaPago,
                p.Tipo.ToString(),
                p.Metodo.ToString(),
                p.Referencia,
                p.MontoTotal.Monto,
                p.MontoCapital.Monto,
                p.MontoInteres.Monto,
                p.SaldoAnterior.Monto,
                p.SaldoPosterior.Monto))
            .ToListAsync(cancellationToken);
        var financiamiento = await _context.Financiamientos.AsNoTracking().FirstOrDefaultAsync(f => f.VentaId == venta.Id, cancellationToken);

        var cuotas = financiamiento is null
            ? new List<CuotaAmortizacionContratoDto>()
            : await _context.Mensualidades
                .AsNoTracking()
                .Where(m => m.FinanciamientoId == financiamiento.Id)
                .OrderBy(m => m.NumeroCuota)
                .Select(m => new CuotaAmortizacionContratoDto(
                    m.NumeroCuota,
                    m.FechaVencimiento,
                    m.SaldoInicialPeriodo.Monto,
                    m.CapitalProgramado.Monto,
                    m.InteresProgramado.Monto,
                    m.MontoTotalCuota.Monto,
                    m.SaldoFinalPeriodo.Monto,
                    m.Estado.ToString(),
                    m.FechaPago))
                .ToListAsync(cancellationToken);

        bool tieneVencidas = cuotas.Any(c => c.Estado == EstadoMensualidad.Vencida.ToString());
        string estadoVisual = venta.Estado switch
        {
            EstadoVenta.Liquidado => "Liquidado",
            EstadoVenta.Cancelado => "Cancelado",
            EstadoVenta.EnRevision => "En revisión",
            _ => tieneVencidas || financiamiento?.Estado == EstadoFinanciamiento.EnMora ? "Vencido" : "Al corriente"
        };

        var finDto = financiamiento is null
            ? null
            : new ContratoFinanciamientoDetalleDto(
                FinanciamientoId: financiamiento.Id,
                PorcentajeEnganche: financiamiento.PorcentajeEnganche,
                MontoEnganche: financiamiento.MontoEnganche.Monto,
                CapitalFinanciado: financiamiento.CapitalFinanciado.Monto,
                PlazoMeses: financiamiento.PlazoMeses,
                TasaInteresAnual: financiamiento.TasaInteresAnual,
                CuotaMensualActual: financiamiento.CuotaMensualActual.Monto,
                SaldoInsoluto: financiamiento.SaldoInsoluto.Monto,
                Estado: financiamiento.Estado.ToString());

        return Resultado<ContratoDetalleDto>.Exito(new ContratoDetalleDto(
            VentaId: venta.Id,
            FolioContrato: venta.FolioContrato,
            ClienteId: venta.ClienteId,
            ClienteNombre: cliente is not null ? $"{cliente.Nombre} {cliente.Apellidos}" : "N/D",
            LoteId: venta.LoteId,
            LoteClave: lote?.Clave ?? "N/D",
            LoteSeccion: lote?.Seccion ?? "N/D",
            LoteFila: lote?.AndadorFila ?? "N/D",
            LoteGavetas: lote?.Gavetas ?? 0,
            Modalidad: venta.Modalidad.ToString(),
            PrecioPactado: venta.PrecioPactado.Monto,
            Estado: venta.Estado.ToString(),
            EstadoVisual: estadoVisual,
            FechaVenta: venta.FechaVenta,
            FolioPago: pagos.FirstOrDefault()?.Folio,
            Financiamiento: finDto,
            Amortizacion: cuotas,
            HistorialPagos: pagos));
    }

    private static string FormatearFechaCorta(DateOnly fecha) =>
        fecha.ToString("dd MMM yyyy", CulturaEsMx);

    private static string FormatearMoneda(decimal monto) =>
        monto.ToString("C2", CulturaEsMx);
}
