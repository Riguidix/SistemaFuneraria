using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Application.Servicios.Ventas;

/// <summary>
/// Implementación del servicio de aplicación para registro y liquidación de contratos de venta (SF-060).
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
            var fechaVenta = _timeProvider.GetLocalNow();
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
        catch
        {
            await _context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
