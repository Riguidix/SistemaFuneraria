using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.Excepciones;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Application.Servicios.Ventas;

/// <summary>
/// Implementación del servicio de aplicación para registro y liquidación de contratos de venta (SF-060, SF-061).
/// </summary>
public class VentaService(IAppDbContext context, TimeProvider? timeProvider = null) : IVentaService
{
    private readonly IAppDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <inheritdoc />
    public async Task<Resultado<ContratoResumenDto>> RegistrarVentaContadoAsync(
        RegistrarVentaContadoCommand command,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == command.ClienteId, cancellationToken);
        if (cliente is null)
        {
            return Resultado<ContratoResumenDto>.Fallo("[NO_ENCONTRADO] El cliente especificado no existe.");
        }

        var lote = await _context.Lotes
            .FirstOrDefaultAsync(l => l.Id == command.LoteId, cancellationToken);
        if (lote is null)
        {
            return Resultado<ContratoResumenDto>.Fallo("[NO_ENCONTRADO] El lote especificado no existe.");
        }

        if (lote.Estado == EstadoLote.Vendido)
        {
            return Resultado<ContratoResumenDto>.Fallo("[CONFLICTO] El lote seleccionado ya fue vendido.");
        }

        if (lote.Estado == EstadoLote.Mantenimiento)
        {
            return Resultado<ContratoResumenDto>.Fallo("[CONFLICTO] El lote seleccionado se encuentra en mantenimiento y no está disponible para venta.");
        }

        await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            var fechaVenta = _timeProvider.GetUtcNow();
            var folioContrato = await _context.ObtenerSiguienteFolioContratoAsync(fechaVenta.Year, cancellationToken);

            var venta = Venta.CrearContado(
                folioContrato,
                cliente.Id,
                lote.Id,
                command.VendedorId,
                lote.PrecioContado,
                fechaVenta);

            lote.MarcarVendido();

            var folioPago = await _context.ObtenerSiguienteFolioPagoAsync(fechaVenta.Year, cancellationToken);

            var claveIdempotencia = $"IDEMP-CONTADO-{venta.Id}";

            var pago = Pago.Crear(
                folio: folioPago,
                ventaId: venta.Id,
                financiamientoId: null,
                mensualidadId: null,
                cajeroId: command.VendedorId,
                tipo: TipoPago.Contado,
                metodo: command.MetodoPago,
                referencia: command.ReferenciaPago,
                montoTotal: lote.PrecioContado,
                montoCapital: lote.PrecioContado,
                montoInteres: Dinero.Zero,
                saldoAnterior: lote.PrecioContado,
                saldoPosterior: Dinero.Zero,
                claveIdempotencia: claveIdempotencia,
                fechaPago: fechaVenta);

            await _context.Ventas.AddAsync(venta, cancellationToken);
            await _context.Pagos.AddAsync(pago, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await _context.CommitTransactionAsync(cancellationToken);

            var resumen = new ContratoResumenDto(
                VentaId: venta.Id,
                FolioContrato: venta.FolioContrato,
                ClienteId: cliente.Id,
                ClienteNombre: $"{cliente.Nombre} {cliente.Apellidos}",
                LoteId: lote.Id,
                LoteClave: lote.Clave,
                Modalidad: venta.Modalidad.ToString(),
                PrecioPactado: venta.PrecioPactado.Monto,
                Estado: venta.Estado.ToString(),
                FechaVenta: venta.FechaVenta,
                FolioPago: pago.Folio);

            return Resultado<ContratoResumenDto>.Exito(resumen);
        }
        catch (ReglaNegocioException ex)
        {
            await _context.RollbackTransactionAsync(cancellationToken);
            return Resultado<ContratoResumenDto>.Fallo(ex.Message);
        }
        catch
        {
            await _context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Resultado<ContratoResumenDto>> ObtenerPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var venta = await _context.Ventas
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (venta is null)
        {
            return Resultado<ContratoResumenDto>.Fallo("[NO_ENCONTRADO] La venta especificada no existe.");
        }

        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == venta.ClienteId, cancellationToken);

        var lote = await _context.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == venta.LoteId, cancellationToken);

        var pago = await _context.Pagos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.VentaId == venta.Id, cancellationToken);

        var resumen = new ContratoResumenDto(
            VentaId: venta.Id,
            FolioContrato: venta.FolioContrato,
            ClienteId: venta.ClienteId,
            ClienteNombre: cliente is not null ? $"{cliente.Nombre} {cliente.Apellidos}" : "N/D",
            LoteId: venta.LoteId,
            LoteClave: lote is not null ? lote.Clave : "N/D",
            Modalidad: venta.Modalidad.ToString(),
            PrecioPactado: venta.PrecioPactado.Monto,
            Estado: venta.Estado.ToString(),
            FechaVenta: venta.FechaVenta,
            FolioPago: pago?.Folio ?? "N/D");

        return Resultado<ContratoResumenDto>.Exito(resumen);
    }

    /// <inheritdoc />
    public async Task<Resultado> CambiarEstadoContratoAdminAsync(
        Guid ventaId,
        EstadoVenta nuevoEstado,
        string motivo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 10)
        {
            return Resultado.Fallo("El motivo administrativo es obligatorio y debe contener al menos 10 caracteres.");
        }

        if (!Enum.IsDefined(nuevoEstado))
        {
            return Resultado.Fallo("El estado de venta indicado no es válido.");
        }

        var venta = await _context.Ventas
            .FirstOrDefaultAsync(v => v.Id == ventaId, cancellationToken);
        if (venta is null)
        {
            return Resultado.Fallo("[NO_ENCONTRADO] La venta especificada no existe.");
        }

        await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            venta.CambiarEstadoAdmin(nuevoEstado, motivo);

            if (nuevoEstado == EstadoVenta.Cancelado)
            {
                var lote = await _context.Lotes
                    .FirstOrDefaultAsync(l => l.Id == venta.LoteId, cancellationToken);
                if (lote is not null)
                {
                    lote.LiberarPorCancelacionVenta();
                }

                var financiamiento = await _context.Financiamientos
                    .FirstOrDefaultAsync(f => f.VentaId == venta.Id, cancellationToken);
                if (financiamiento is not null)
                {
                    financiamiento.Cancelar();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await _context.CommitTransactionAsync(cancellationToken);
            return Resultado.Exito();
        }
        catch (ReglaNegocioException ex)
        {
            await _context.RollbackTransactionAsync(cancellationToken);
            return Resultado.Fallo(ex.Message);
        }
        catch
        {
            await _context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Resultado<ContratoFinanciadoResumenDto>> RegistrarVentaFinanciadaAsync(
        RegistrarVentaFinanciadaCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!ParametrosNegocio.EsEngancheAutorizado(command.PorcentajeEnganche))
        {
            return Resultado<ContratoFinanciadoResumenDto>.Fallo("El porcentaje de enganche autorizado debe ser 10%, 20% o 30%.");
        }

        if (!ParametrosNegocio.EsPlazoAutorizado(command.PlazoMeses))
        {
            return Resultado<ContratoFinanciadoResumenDto>.Fallo("El plazo autorizado debe ser 12, 18, 24 o 48 meses.");
        }

        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == command.ClienteId, cancellationToken);
        if (cliente is null)
        {
            return Resultado<ContratoFinanciadoResumenDto>.Fallo("[NO_ENCONTRADO] El cliente especificado no existe.");
        }

        var lote = await _context.Lotes
            .FirstOrDefaultAsync(l => l.Id == command.LoteId, cancellationToken);
        if (lote is null)
        {
            return Resultado<ContratoFinanciadoResumenDto>.Fallo("[NO_ENCONTRADO] El lote especificado no existe.");
        }

        if (lote.Estado == EstadoLote.Vendido)
        {
            return Resultado<ContratoFinanciadoResumenDto>.Fallo("[CONFLICTO] El lote seleccionado ya fue vendido.");
        }

        if (lote.Estado == EstadoLote.Mantenimiento)
        {
            return Resultado<ContratoFinanciadoResumenDto>.Fallo("[CONFLICTO] El lote seleccionado se encuentra en mantenimiento y no está disponible para venta.");
        }

        await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            var fechaVenta = _timeProvider.GetUtcNow();
            var folioContrato = await _context.ObtenerSiguienteFolioContratoAsync(fechaVenta.Year, cancellationToken);

            var (montoEnganche, capitalFinanciado) = CalculadoraFinanciamiento.CalcularEnganche(
                lote.PrecioContado.Monto,
                command.PorcentajeEnganche);
            var (tasaAnual, tasaMensual) = CalculadoraFinanciamiento.ObtenerTasas(command.PlazoMeses);
            var renglones = GeneradorAmortizacion.Generar(capitalFinanciado, tasaMensual, command.PlazoMeses);

            var venta = Venta.CrearFinanciada(
                folioContrato,
                cliente.Id,
                lote.Id,
                command.VendedorId,
                lote.PrecioContado,
                fechaVenta);

            var fechaPrimerPago = command.FechaPrimerPago
                ?? DateOnly.FromDateTime(fechaVenta.DateTime).AddMonths(1);

            var financiamiento = Financiamiento.Crear(
                ventaId: venta.Id,
                porcentajeEnganche: command.PorcentajeEnganche,
                montoEnganche: new Dinero(montoEnganche),
                capitalFinanciado: new Dinero(capitalFinanciado),
                plazoMeses: command.PlazoMeses,
                tasaAnual: tasaAnual,
                filasAmortizacion: renglones,
                fechaPrimerPago: fechaPrimerPago);

            lote.MarcarVendido();

            await _context.Ventas.AddAsync(venta, cancellationToken);
            await _context.Financiamientos.AddAsync(financiamiento, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await _context.CommitTransactionAsync(cancellationToken);

            return Resultado<ContratoFinanciadoResumenDto>.Exito(new ContratoFinanciadoResumenDto(
                VentaId: venta.Id,
                FinanciamientoId: financiamiento.Id,
                FolioContrato: venta.FolioContrato,
                ClienteId: cliente.Id,
                ClienteNombre: $"{cliente.Nombre} {cliente.Apellidos}",
                LoteId: lote.Id,
                LoteClave: lote.Clave,
                Modalidad: venta.Modalidad.ToString(),
                PrecioPactado: venta.PrecioPactado.Monto,
                PorcentajeEnganche: financiamiento.PorcentajeEnganche,
                MontoEnganche: financiamiento.MontoEnganche.Monto,
                CapitalFinanciado: financiamiento.CapitalFinanciado.Monto,
                PlazoMeses: financiamiento.PlazoMeses,
                TasaInteresAnual: financiamiento.TasaInteresAnual,
                CuotaMensualActual: financiamiento.CuotaMensualActual.Monto,
                SaldoInsoluto: financiamiento.SaldoInsoluto.Monto,
                Estado: venta.Estado.ToString(),
                FechaVenta: venta.FechaVenta,
                TotalMensualidades: financiamiento.Mensualidades.Count));
        }
        catch (ReglaNegocioException ex)
        {
            await _context.RollbackTransactionAsync(cancellationToken);
            return Resultado<ContratoFinanciadoResumenDto>.Fallo(ex.Message);
        }
        catch
        {
            await _context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Resultado<IReadOnlyList<CuotaAmortizacionContratoDto>>> ObtenerAmortizacionPorVentaIdAsync(
        Guid ventaId,
        CancellationToken cancellationToken = default)
    {
        var financiamiento = await _context.Financiamientos
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.VentaId == ventaId, cancellationToken);

        if (financiamiento is null)
        {
            return Resultado<IReadOnlyList<CuotaAmortizacionContratoDto>>.Fallo("[NO_ENCONTRADO] No se encontró tabla de amortización para la venta especificada.");
        }

        var cuotas = await _context.Mensualidades
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

        return Resultado<IReadOnlyList<CuotaAmortizacionContratoDto>>.Exito(cuotas);
    }
}
