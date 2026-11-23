using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.DTOs.Cobranza;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Application.Servicios.Cobranza;

/// <summary>
/// Implementación del servicio de Caja y Cobranza para pagos ordinarios, corte del día y recibos (SF-077).
/// </summary>
public class CobranzaService(IAppDbContext context, TimeProvider? timeProvider = null) : ICobranzaService
{
    public const decimal SaldoInicialCaja = 2000.00m;
    private static readonly ConcurrentDictionary<Guid, byte> PagosEnCurso = new();

    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <inheritdoc />
    public async Task<Resultado<CorteDelDiaDto>> ObtenerResumenCajaAsync(CancellationToken cancellationToken = default)
    {
        var hoyOffset = _timeProvider.GetLocalNow();
        var fechaHoy = DateOnly.FromDateTime(hoyOffset.Date);
        var inicioDia = new DateTimeOffset(hoyOffset.Date, hoyOffset.Offset);
        var finDia = inicioDia.AddDays(1);

        var pagosHoy = await _context.Pagos
            .AsNoTracking()
            .Where(p => p.FechaPago >= inicioDia && p.FechaPago < finDia)
            .Select(p => p.MontoTotal.Monto)
            .ToListAsync(cancellationToken);

        var financiamientosActivosIds = await _context.Financiamientos
            .AsNoTracking()
            .Where(f => f.Estado == EstadoFinanciamiento.Activo || f.Estado == EstadoFinanciamiento.EnMora)
            .Select(f => f.Id)
            .ToListAsync(cancellationToken);

        var pagosVencidosCount = financiamientosActivosIds.Count == 0
            ? 0
            : await _context.Mensualidades
                .AsNoTracking()
                .CountAsync(m => financiamientosActivosIds.Contains(m.FinanciamientoId) && m.Estado == EstadoMensualidad.Vencida, cancellationToken);

        decimal ingresosDia = pagosHoy.Sum();
        return Resultado<CorteDelDiaDto>.Exito(new CorteDelDiaDto(
            Fecha: fechaHoy,
            SaldoInicial: SaldoInicialCaja,
            PagosRecibidosHoy: pagosHoy.Count,
            IngresosDelDia: ingresosDia,
            TotalEnCaja: SaldoInicialCaja + ingresosDia,
            PagosVencidosCount: pagosVencidosCount));
    }

    /// <inheritdoc />
    public async Task<Resultado<ContratoCajaDto>> ObtenerContratoParaCajaAsync(
        string folioOrId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folioOrId))
        {
            return Resultado<ContratoCajaDto>.Fallo("[NO_ENCONTRADO] Debe proporcionar el folio o identificador del contrato.");
        }

        var texto = folioOrId.Trim();
        var venta = Guid.TryParse(texto, out var ventaId)
            ? await _context.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == ventaId, cancellationToken)
            : await _context.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.FolioContrato == texto.ToUpperInvariant(), cancellationToken);

        if (venta is null)
        {
            return Resultado<ContratoCajaDto>.Fallo($"[NO_ENCONTRADO] No se encontró el contrato '{texto}'.");
        }

        var financiamiento = await _context.Financiamientos
            .AsNoTracking()
            .Include(f => f.Mensualidades)
            .FirstOrDefaultAsync(f => f.VentaId == venta.Id, cancellationToken);

        if (financiamiento is null)
        {
            return Resultado<ContratoCajaDto>.Fallo($"[CONFLICTO] El contrato '{venta.FolioContrato}' corresponde a una venta de contado y no admite cobros en caja.");
        }

        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == venta.ClienteId, cancellationToken);
        var lote = await _context.Lotes.AsNoTracking().FirstOrDefaultAsync(l => l.Id == venta.LoteId, cancellationToken);

        var cuotasOrdenadas = financiamiento.Mensualidades.OrderBy(m => m.NumeroCuota).ToList();
        var vencidas = cuotasOrdenadas.Where(m => m.Estado == EstadoMensualidad.Vencida).ToList();
        var siguienteExigible = cuotasOrdenadas
            .FirstOrDefault(m => m.Estado is EstadoMensualidad.Vencida or EstadoMensualidad.Pendiente);

        string estadoVisual = venta.Estado switch
        {
            EstadoVenta.Liquidado => "Liquidado",
            EstadoVenta.Cancelado => "Cancelado",
            _ when vencidas.Count > 0 => $"{vencidas.Count} vencido{(vencidas.Count == 1 ? "" : "s")}",
            _ => "Al corriente"
        };

        decimal montoLiquidacion = 0m;
        if (financiamiento.Estado != EstadoFinanciamiento.Liquidado && financiamiento.SaldoInsoluto.Monto > 0m)
        {
            montoLiquidacion = CalculadorLiquidacion.Calcular(financiamiento).MontoTotalLiquidacion.Monto;
        }

        var nombreCompleto = cliente is not null ? $"{cliente.Nombre} {cliente.Apellidos}".Trim() : "N/D";
        var iniciales = CalcularIniciales(cliente?.Nombre, cliente?.Apellidos);

        return Resultado<ContratoCajaDto>.Exito(new ContratoCajaDto(
            VentaId: venta.Id,
            FinanciamientoId: financiamiento.Id,
            FolioContrato: venta.FolioContrato,
            ClienteNombre: nombreCompleto,
            InicialesCliente: iniciales,
            LoteClave: lote?.Clave ?? "N/D",
            LoteUbicacion: lote is not null ? $"Lote {lote.Clave} · {lote.Seccion}" : "N/D",
            EstadoVisual: estadoVisual,
            CuotasVencidas: vencidas.Count,
            ProximaCuotaNumero: siguienteExigible?.NumeroCuota,
            ProximaCuotaVencimiento: siguienteExigible?.FechaVencimiento,
            MontoMensualidad: siguienteExigible?.MontoTotalCuota.Monto ?? 0m,
            SaldoInsoluto: financiamiento.SaldoInsoluto.Monto,
            MontoLiquidacionEstimado: montoLiquidacion));
    }

    /// <inheritdoc />
    public async Task<Resultado<ReciboPagoDto>> AplicarPagoMensualidadAsync(
        AplicarPagoMensualidadCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ClaveIdempotencia))
        {
            return Resultado<ReciboPagoDto>.Fallo("La clave de idempotencia (X-Idempotency-Key) es obligatoria.");
        }

        var claveNormalizada = command.ClaveIdempotencia.Trim();

        var pagoExistente = await _context.Pagos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ClaveIdempotencia == claveNormalizada, cancellationToken);
        if (pagoExistente is not null)
        {
            return Resultado<ReciboPagoDto>.Exito(await ConstruirReciboDtoAsync(pagoExistente, null, cancellationToken));
        }

        if (string.IsNullOrWhiteSpace(command.FolioContrato))
        {
            return Resultado<ReciboPagoDto>.Fallo("El folio o identificador del contrato es obligatorio.");
        }

        var texto = command.FolioContrato.Trim();
        var venta = Guid.TryParse(texto, out var ventaId)
            ? await _context.Ventas.FirstOrDefaultAsync(v => v.Id == ventaId, cancellationToken)
            : await _context.Ventas.FirstOrDefaultAsync(v => v.FolioContrato == texto.ToUpperInvariant(), cancellationToken);

        if (venta is null)
        {
            return Resultado<ReciboPagoDto>.Fallo($"[NO_ENCONTRADO] No se encontró el contrato '{texto}'.");
        }

        if (venta.Estado == EstadoVenta.Cancelado)
        {
            return Resultado<ReciboPagoDto>.Fallo("[CONFLICTO] El contrato se encuentra cancelado y no admite pagos.");
        }

        if (venta.Estado == EstadoVenta.Liquidado)
        {
            return Resultado<ReciboPagoDto>.Fallo("[CONFLICTO] El contrato ya se encuentra liquidado.");
        }

        var financiamiento = await _context.Financiamientos
            .Include(f => f.Mensualidades)
            .FirstOrDefaultAsync(f => f.VentaId == venta.Id, cancellationToken);

        if (financiamiento is null)
        {
            return Resultado<ReciboPagoDto>.Fallo($"[NO_ENCONTRADO] El contrato '{venta.FolioContrato}' no tiene financiamiento asociado.");
        }

        if (financiamiento.Estado == EstadoFinanciamiento.Liquidado)
        {
            return Resultado<ReciboPagoDto>.Fallo("[CONFLICTO] El financiamiento ya se encuentra liquidado.");
        }

        if (!PagosEnCurso.TryAdd(financiamiento.Id, 0))
        {
            return Resultado<ReciboPagoDto>.Fallo("[CONFLICTO] El contrato fue modificado simultáneamente por otro cajero.");
        }

        try
        {
            // Re-verificar idempotencia dentro de la sección protegida por si otra petición igual acaba de confirmar
            var pagoReciente = await _context.Pagos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ClaveIdempotencia == claveNormalizada, cancellationToken);
            if (pagoReciente is not null)
            {
                return Resultado<ReciboPagoDto>.Exito(await ConstruirReciboDtoAsync(pagoReciente, null, cancellationToken));
            }

            var siguiente = financiamiento.Mensualidades
                .Where(m => m.Estado is EstadoMensualidad.Pendiente or EstadoMensualidad.Vencida)
                .OrderBy(m => m.NumeroCuota)
                .FirstOrDefault();

            if (siguiente is null)
            {
                return Resultado<ReciboPagoDto>.Fallo("[CONFLICTO] El contrato no tiene mensualidades pendientes por cobrar.");
            }

            if (command.NumeroCuota.HasValue && command.NumeroCuota.Value != siguiente.NumeroCuota)
            {
                return Resultado<ReciboPagoDto>.Fallo(
                    $"[CONFLICTO] La cuota #{command.NumeroCuota.Value} ya fue cubierta o no es la siguiente exigible (corresponde cuota #{siguiente.NumeroCuota}).");
            }

            if (command.ImporteRecibido != siguiente.MontoTotalCuota.Monto)
            {
                return Resultado<ReciboPagoDto>.Fallo(
                    $"El importe recibido ({command.ImporteRecibido:C2}) debe coincidir exactamente con el monto de la cuota #{siguiente.NumeroCuota} ({siguiente.MontoTotalCuota.Monto:C2}).");
            }

            await _context.BeginTransactionAsync(cancellationToken);
            try
            {
                var fechaPago = _timeProvider.GetLocalNow();
                var saldoAnterior = financiamiento.SaldoInsoluto;

                financiamiento.AplicarPagoCuota(siguiente.NumeroCuota, fechaPago);

                if (financiamiento.Estado == EstadoFinanciamiento.Liquidado && venta.Estado != EstadoVenta.Liquidado)
                {
                    venta.MarcarLiquidado();
                }

                var folioPago = await _context.ObtenerSiguienteFolioPagoAsync(fechaPago.Year, cancellationToken);

                var pago = Pago.Crear(
                    folio: folioPago,
                    ventaId: venta.Id,
                    financiamientoId: financiamiento.Id,
                    mensualidadId: siguiente.Id,
                    cajeroId: command.CajeroId,
                    tipo: TipoPago.Mensualidad,
                    metodo: command.MetodoPago,
                    referencia: command.Referencia,
                    montoTotal: siguiente.MontoTotalCuota,
                    montoCapital: siguiente.CapitalProgramado,
                    montoInteres: siguiente.InteresProgramado,
                    saldoAnterior: saldoAnterior,
                    saldoPosterior: financiamiento.SaldoInsoluto,
                    claveIdempotencia: claveNormalizada,
                    fechaPago: fechaPago);

                await _context.Pagos.AddAsync(pago, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await _context.CommitTransactionAsync(cancellationToken);

                var recibo = await ConstruirReciboDtoAsync(pago, siguiente.NumeroCuota, cancellationToken);
                return Resultado<ReciboPagoDto>.Exito(recibo);
            }
            catch (DbUpdateConcurrencyException)
            {
                await _context.RollbackTransactionAsync(cancellationToken);
                return Resultado<ReciboPagoDto>.Fallo("[CONFLICTO] El contrato fue modificado simultáneamente por otro cajero.");
            }
            catch (ReglaNegocioException ex)
            {
                await _context.RollbackTransactionAsync(cancellationToken);
                return Resultado<ReciboPagoDto>.Fallo(ex.Message);
            }
            catch
            {
                await _context.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        finally
        {
            PagosEnCurso.TryRemove(financiamiento.Id, out _);
        }
    }

    /// <inheritdoc />
    public async Task<Resultado<ReciboPagoDto>> ObtenerReciboPorFolioAsync(
        string folioPago,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folioPago))
        {
            return Resultado<ReciboPagoDto>.Fallo("[NO_ENCONTRADO] Debe proporcionar el folio del recibo.");
        }

        var folioNorm = folioPago.Trim().ToUpperInvariant();
        var pago = await _context.Pagos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Folio == folioNorm, cancellationToken);

        if (pago is null)
        {
            return Resultado<ReciboPagoDto>.Fallo($"[NO_ENCONTRADO] No existe el recibo con folio '{folioNorm}'.");
        }

        int? numeroCuota = null;
        if (pago.MensualidadId.HasValue)
        {
            numeroCuota = await _context.Mensualidades
                .AsNoTracking()
                .Where(m => m.Id == pago.MensualidadId.Value)
                .Select(m => (int?)m.NumeroCuota)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Resultado<ReciboPagoDto>.Exito(await ConstruirReciboDtoAsync(pago, numeroCuota, cancellationToken));
    }

    private async Task<ReciboPagoDto> ConstruirReciboDtoAsync(
        Pago pago,
        int? numeroCuota,
        CancellationToken cancellationToken)
    {
        var venta = await _context.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == pago.VentaId, cancellationToken);
        var cliente = venta is null
            ? null
            : await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == venta.ClienteId, cancellationToken);
        var lote = venta is null
            ? null
            : await _context.Lotes.AsNoTracking().FirstOrDefaultAsync(l => l.Id == venta.LoteId, cancellationToken);

        string concepto = pago.Tipo switch
        {
            TipoPago.Mensualidad when numeroCuota.HasValue => $"Mensualidad #{numeroCuota.Value}",
            TipoPago.Mensualidad => "Mensualidad",
            TipoPago.AbonoCapital => "Abono a capital",
            TipoPago.Liquidacion => "Liquidación anticipada",
            _ => "Venta de contado"
        };

        return new ReciboPagoDto(
            PagoId: pago.Id,
            FolioPago: pago.Folio,
            FechaPago: pago.FechaPago,
            FolioContrato: venta?.FolioContrato ?? "N/D",
            ClienteNombre: cliente is not null ? $"{cliente.Nombre} {cliente.Apellidos}".Trim() : "N/D",
            LoteClave: lote?.Clave ?? "N/D",
            Concepto: concepto,
            TipoPago: pago.Tipo.ToString(),
            MetodoPago: pago.Metodo.ToString(),
            Referencia: pago.Referencia,
            SaldoAnterior: pago.SaldoAnterior.Monto,
            MontoCapital: pago.MontoCapital.Monto,
            MontoInteres: pago.MontoInteres.Monto,
            MontoTotal: pago.MontoTotal.Monto,
            SaldoPosterior: pago.SaldoPosterior.Monto,
            CajeroId: pago.CajeroId);
    }

    private static string CalcularIniciales(string? nombre, string? apellidos)
    {
        char primera = string.IsNullOrWhiteSpace(nombre) ? 'S' : char.ToUpperInvariant(nombre.Trim()[0]);
        char segunda = string.IsNullOrWhiteSpace(apellidos) ? 'F' : char.ToUpperInvariant(apellidos.Trim()[0]);
        return $"{primera}{segunda}";
    }
}
