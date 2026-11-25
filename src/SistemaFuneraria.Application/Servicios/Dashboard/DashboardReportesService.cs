using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.DTOs.Dashboard;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Enums;

namespace SistemaFuneraria.Application.Servicios.Dashboard;

/// <summary>
/// Implementación agregada con AsNoTracking para métricas de Dashboard Resumen e Indicadores de Reportes (SF-088).
/// </summary>
public class DashboardReportesService(IAppDbContext context, TimeProvider? timeProvider = null) : IDashboardReportesService
{
    private const decimal ObjetivoMensualOficial = 600_000m;
    private static readonly CultureInfo CulturaEsMx = CultureInfo.GetCultureInfo("es-MX");
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <inheritdoc />
    public async Task<Resultado<DashboardResumenDto>> ObtenerResumenDashboardAsync(
        string? rango = null,
        CancellationToken cancellationToken = default)
    {
        var ahora = _timeProvider.GetLocalNow();
        var inicioMes = new DateTimeOffset(ahora.Year, ahora.Month, 1, 0, 0, 0, ahora.Offset);
        var finMes = inicioMes.AddMonths(1);
        var inicioMesAnterior = inicioMes.AddMonths(-1);

        var ventas = await _context.Ventas
            .AsNoTracking()
            .Where(v => v.Estado != EstadoVenta.Cancelado)
            .ToListAsync(cancellationToken);

        var pagos = await _context.Pagos
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lotes = await _context.Lotes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var clientes = await _context.Clientes
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        var financiamientosActivosIds = await _context.Financiamientos
            .AsNoTracking()
            .Where(f => f.Estado == EstadoFinanciamiento.Activo || f.Estado == EstadoFinanciamiento.EnMora)
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        var fechaHoy = DateOnly.FromDateTime(_timeProvider.GetUtcNow().DateTime);
        var cuotasVencidas = financiamientosActivosIds.Count == 0
            ? []
            : await _context.Mensualidades
                .AsNoTracking()
                .Where(m => financiamientosActivosIds.Contains(m.FinanciamientoId) &&
                    (m.Estado == EstadoMensualidad.Vencida ||
                     (m.Estado == EstadoMensualidad.Pendiente && m.FechaVencimiento < fechaHoy)))
                .Select(m => m.MontoTotalCuota.Monto)
                .ToListAsync(cancellationToken);

        var ventasMes = ventas.Where(v => v.FechaVenta >= inicioMes && v.FechaVenta < finMes).ToList();
        var ventasMesAnterior = ventas.Where(v => v.FechaVenta >= inicioMesAnterior && v.FechaVenta < inicioMes).ToList();
        var pagosMes = pagos.Where(p => p.FechaPago >= inicioMes && p.FechaPago < finMes).ToList();
        var pagosMesAnterior = pagos.Where(p => p.FechaPago >= inicioMesAnterior && p.FechaPago < inicioMes).ToList();
        var contratosActivos = ventas.Count(v => v.Estado is EstadoVenta.Activo or EstadoVenta.EnRevision);

        decimal ventasDelMesMonto = ventasMes.Sum(v => v.PrecioPactado.Monto);
        decimal ventasMesAnteriorMonto = ventasMesAnterior.Sum(v => v.PrecioPactado.Monto);
        decimal cobranzaMesMonto = pagosMes.Sum(p => p.MontoTotal.Monto);
        decimal cobranzaMesAnteriorMonto = pagosMesAnterior.Sum(p => p.MontoTotal.Monto);

        var kpis = new DashboardKpisDto(
            VentasDelMes: ventasDelMesMonto,
            CobranzaMensual: cobranzaMesMonto,
            ContratosActivos: contratosActivos,
            ContratosNuevosMes: ventasMes.Count,
            PagosVencidosCount: cuotasVencidas.Count,
            MontoPorRecuperar: cuotasVencidas.Sum(),
            VariacionVentasPct: CalcularVariacionPct(ventasDelMesMonto, ventasMesAnteriorMonto),
            VariacionCobranzaPct: CalcularVariacionPct(cobranzaMesMonto, cobranzaMesAnteriorMonto));

        var series = ConstruirSerieMensual(rango, ahora, inicioMes, ventas, pagos);

        var lotesDisponibles = lotes.Where(l => l.Estado == EstadoLote.Disponible).ToList();
        int totalLotes = lotes.Count;
        int totalDisponibles = lotesDisponibles.Count;
        decimal pctOcupacion = totalLotes > 0
            ? Math.Round((totalLotes - totalDisponibles) * 100m / totalLotes, 1, MidpointRounding.AwayFromZero)
            : 0m;

        var disponibilidad = new DisponibilidadInventarioDto(
            TotalLotes: totalLotes,
            TotalDisponibles: totalDisponibles,
            Gavetas2: lotesDisponibles.Count(l => l.Gavetas == 2),
            Gavetas3: lotesDisponibles.Count(l => l.Gavetas == 3),
            Gavetas4: lotesDisponibles.Count(l => l.Gavetas == 4),
            PorcentajeOcupacion: pctOcupacion);

        var ventasPorId = ventas.ToDictionary(v => v.Id);

        var movimientosVentas = ventas.Select(v =>
        {
            clientes.TryGetValue(v.ClienteId, out var cli);
            var nombre = cli is not null ? $"{cli.Nombre} {cli.Apellidos}".Trim() : "Cliente";
            var email = cli?.Correo ?? "-";
            var ini = TextoHelper.ObtenerIniciales(nombre);
            var estadoTexto = v.Estado switch
            {
                EstadoVenta.Liquidado => "Aplicado",
                EstadoVenta.EnRevision => "En revisión",
                _ => "Firmado"
            };

            return new
            {
                FechaOrden = v.FechaVenta,
                Dto = new ActividadRecienteDto(
                    Cliente: nombre,
                    Email: email,
                    Iniciales: ini,
                    Movimiento: v.Modalidad == ModalidadVenta.Contado ? "Venta de contado" : "Contrato financiado",
                    Referencia: v.FolioContrato,
                    Importe: v.PrecioPactado.Monto,
                    Estado: estadoTexto,
                    FechaRelativa: v.FechaVenta.ToString("dd MMM yyyy", CulturaEsMx),
                    FolioContrato: v.FolioContrato)
            };
        });

        var movimientosPagos = pagos.Select(p =>
        {
            ventasPorId.TryGetValue(p.VentaId, out var ventaPago);
            Core.Entidades.Cliente? cli = null;
            if (ventaPago is not null)
            {
                clientes.TryGetValue(ventaPago.ClienteId, out cli);
            }

            var nombre = cli is not null ? $"{cli.Nombre} {cli.Apellidos}".Trim() : "Cliente";
            var email = cli?.Correo ?? "-";
            var ini = TextoHelper.ObtenerIniciales(nombre);
            var movimiento = p.Tipo switch
            {
                TipoPago.Mensualidad => "Mensualidad",
                TipoPago.AbonoCapital => "Abono a capital",
                TipoPago.Liquidacion => "Liquidación anticipada",
                TipoPago.Enganche => "Enganche",
                TipoPago.Contado => "Pago de contado",
                _ => "Pago registrado"
            };

            return new
            {
                FechaOrden = p.FechaPago,
                Dto = new ActividadRecienteDto(
                    Cliente: nombre,
                    Email: email,
                    Iniciales: ini,
                    Movimiento: movimiento,
                    Referencia: p.Folio,
                    Importe: p.MontoTotal.Monto,
                    Estado: "Aplicado",
                    FechaRelativa: p.FechaPago.ToString("dd MMM yyyy", CulturaEsMx),
                    FolioContrato: ventaPago?.FolioContrato)
            };
        });

        var actividad = movimientosVentas
            .Concat(movimientosPagos)
            .OrderByDescending(x => x.FechaOrden)
            .Take(6)
            .Select(x => x.Dto)
            .ToList();

        return Resultado<DashboardResumenDto>.Exito(new DashboardResumenDto(
            Kpis: kpis,
            VentasYCobranza: series,
            Disponibilidad: disponibilidad,
            ActividadReciente: actividad));
    }

    private static decimal? CalcularVariacionPct(decimal actual, decimal anterior)
    {
        if (anterior > 0m)
        {
            return Math.Round((actual - anterior) * 100m / anterior, 1, MidpointRounding.AwayFromZero);
        }

        return actual == 0m ? 0m : null;
    }

    private static List<SerieMensualDto> ConstruirSerieMensual(
        string? rango,
        DateTimeOffset ahora,
        DateTimeOffset inicioMesActual,
        IReadOnlyCollection<Core.Entidades.Venta> ventas,
        IReadOnlyCollection<Core.Entidades.Pago> pagos)
    {
        bool esAnio = string.Equals(rango?.Trim(), "anio", StringComparison.OrdinalIgnoreCase);

        if (esAnio)
        {
            var seriesAnio = new List<SerieMensualDto>(12);
            var inicioAnio = new DateTimeOffset(ahora.Year, 1, 1, 0, 0, 0, ahora.Offset);
            for (int mes = 0; mes < 12; mes++)
            {
                var mesInicio = inicioAnio.AddMonths(mes);
                var mesFin = mesInicio.AddMonths(1);
                var etiqueta = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                    mesInicio.ToString("MMM", CulturaEsMx).TrimEnd('.'));

                decimal sumVentas = ventas.Where(v => v.FechaVenta >= mesInicio && v.FechaVenta < mesFin).Sum(v => v.PrecioPactado.Monto);
                decimal sumCobranza = pagos.Where(p => p.FechaPago >= mesInicio && p.FechaPago < mesFin).Sum(p => p.MontoTotal.Monto);
                seriesAnio.Add(new SerieMensualDto(etiqueta, sumVentas, sumCobranza));
            }
            return seriesAnio;
        }

        var series6m = new List<SerieMensualDto>(6);
        for (int offset = 5; offset >= 0; offset--)
        {
            var mesInicio = inicioMesActual.AddMonths(-offset);
            var mesFin = mesInicio.AddMonths(1);
            var etiqueta = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                mesInicio.ToString("MMM", CulturaEsMx).TrimEnd('.'));

            decimal sumVentas = ventas.Where(v => v.FechaVenta >= mesInicio && v.FechaVenta < mesFin).Sum(v => v.PrecioPactado.Monto);
            decimal sumCobranza = pagos.Where(p => p.FechaPago >= mesInicio && p.FechaPago < mesFin).Sum(p => p.MontoTotal.Monto);
            series6m.Add(new SerieMensualDto(etiqueta, sumVentas, sumCobranza));
        }
        return series6m;
    }

    /// <inheritdoc />
    public async Task<Resultado<ReportesIndicadoresDto>> ObtenerIndicadoresReportesAsync(
        string? periodo = null,
        string? sucursal = null,
        CancellationToken cancellationToken = default)
    {
        var ahora = _timeProvider.GetLocalNow();
        var periodoEfectivo = string.IsNullOrWhiteSpace(periodo)
            ? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(ahora.ToString("MMMM yyyy", CulturaEsMx))
            : periodo.Trim();
        var sucursalEfectiva = string.IsNullOrWhiteSpace(sucursal) ? "Jardines del Valle" : sucursal.Trim();

        var ventas = await _context.Ventas
            .AsNoTracking()
            .Where(v => v.Estado != EstadoVenta.Cancelado)
            .ToListAsync(cancellationToken);

        var lotes = await _context.Lotes
            .AsNoTracking()
            .ToDictionaryAsync(l => l.Id, cancellationToken);

        var financiamientos = await _context.Financiamientos
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        decimal totalVentas = ventas.Sum(v => v.PrecioPactado.Monto);
        decimal ticketPromedio = ventas.Count > 0
            ? Math.Round(totalVentas / ventas.Count, 2, MidpointRounding.AwayFromZero)
            : 0m;

        int activos = financiamientos.Count(f => f.Estado is EstadoFinanciamiento.Activo or EstadoFinanciamiento.EnMora);
        int enMora = financiamientos.Count(f => f.Estado == EstadoFinanciamiento.EnMora);
        decimal indiceMorosidad = activos > 0
            ? Math.Round(enMora * 100m / activos, 1, MidpointRounding.AwayFromZero)
            : 0m;

        int totalClientes = await _context.Clientes.AsNoTracking().CountAsync(cancellationToken);
        decimal conversion = totalClientes > 0
            ? Math.Min(100m, Math.Round(ventas.Count * 100m / totalClientes, 1, MidpointRounding.AwayFromZero))
            : 0m;

        var desgloseGavetas = new[] { 2, 3, 4 }
            .Select(g =>
            {
                var delTipo = ventas.Where(v => lotes.TryGetValue(v.LoteId, out var l) && l.Gavetas == g).ToList();
                var monto = delTipo.Sum(v => v.PrecioPactado.Monto);
                var pct = totalVentas > 0m
                    ? Math.Round(monto * 100m / totalVentas, 1, MidpointRounding.AwayFromZero)
                    : 0m;
                return new VentaPorTipoLoteDto($"Lote {g} gavetas", g, delTipo.Count, monto, pct);
            })
            .ToList();

        decimal porcentajeAvance = ObjetivoMensualOficial > 0m
            ? Math.Round(totalVentas * 100m / ObjetivoMensualOficial, 1, MidpointRounding.AwayFromZero)
            : 0m;

        return Resultado<ReportesIndicadoresDto>.Exito(new ReportesIndicadoresDto(
            Periodo: periodoEfectivo,
            Sucursal: sucursalEfectiva,
            ConversionCotizacion: conversion,
            TicketPromedio: ticketPromedio,
            IndiceMorosidad: indiceMorosidad,
            VentasPorTipoLote: desgloseGavetas,
            MetaMensual: new MetaMensualDto(
                ObjetivoMensual: ObjetivoMensualOficial,
                Acumulado: totalVentas,
                PorcentajeAvance: porcentajeAvance)));
    }
}
