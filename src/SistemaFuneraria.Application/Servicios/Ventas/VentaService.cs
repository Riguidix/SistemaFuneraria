using Microsoft.EntityFrameworkCore;
using SistemaFuneraria.Application.Comun;
using SistemaFuneraria.Application.Comun.Persistencia;
using SistemaFuneraria.Core.Comun;

namespace SistemaFuneraria.Application.Servicios.Ventas;

/// <summary>
/// Implementación del caso de uso de registro de ventas de lotes funerarios (SF-024).
/// Gestiona la venta de contado en 1 exhibición emitiendo folio VTA-2026-XXXXXX sin amortización ni intereses.
/// </summary>
public class VentaService(IAppDbContext context) : IVentaService
{
    /// <inheritdoc />
    public async Task<Resultado<string>> RegistrarVentaContadoAsync(RegistrarVentaContadoRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Validar existencia del cliente comprador
        var existeCliente = await context.Clientes.AnyAsync(c => c.Id == request.ClienteId, cancellationToken);
        if (!existeCliente)
        {
            return Resultado<string>.Fallo("El cliente especificado no existe en el sistema.");
        }

        // 2. Validar que el precio corresponda al catálogo oficial autorizado (RN-01)
        if (!ParametrosNegocio.EsPrecioAutorizado(request.PrecioLote))
        {
            return Resultado<string>.Fallo($"El precio de {request.PrecioLote:C2} no corresponde a ningún lote del catálogo autorizado (RN-01).");
        }

        // 3. Generación del folio canónico oficial (VTA-2026-XXXXXX)
        string sufijoAleatorio = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        string folio = $"VTA-2026-{sufijoAleatorio}";

        // 4. Guardar cambios en el contexto de persistencia si aplica
        await context.SaveChangesAsync(cancellationToken);

        return Resultado<string>.Exito(folio);
    }
}
