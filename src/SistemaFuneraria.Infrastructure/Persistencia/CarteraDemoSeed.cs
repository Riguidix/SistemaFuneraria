using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Core.Calculos;
using SistemaFuneraria.Core.Comun;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;

namespace SistemaFuneraria.Infrastructure.Persistencia;

/// <summary>
/// Semilla de demostración para poblar de forma idempotente los contratos de muestra de Figma en desarrollo (SF-072).
/// </summary>
public static class CarteraDemoSeed
{
    private static readonly string[] FoliosOficiales =
    [
        "CTR-2025-0189",
        "CTR-2025-0182",
        "CTR-2024-0118",
        "CTR-2025-0144"
    ];

    /// <summary>
    /// Resuelve <see cref="AppDbContext"/> desde el proveedor de servicios y ejecuta la semilla de contratos.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedAsync(db, cancellationToken);
    }

    /// <summary>
    /// Inserta de forma idempotente los 4 contratos de muestra de Figma con sus clientes, lotes, financiamientos y mensualidades.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        var existentes = (await db.Ventas
            .AsNoTracking()
            .Where(v => FoliosOficiales.Contains(v.FolioContrato))
            .Select(v => v.FolioContrato)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        if (FoliosOficiales.All(existentes.Contains))
        {
            return;
        }

        await LoteSeed.SeedAsync(db, cancellationToken);

        var vendedor = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == "vendedor@sistemafunerario.mx", cancellationToken);
        var vendedorId = vendedor?.Id ?? Guid.NewGuid().ToString();

        var lotes = await db.Lotes
            .Where(l => l.Clave == "JC-C-118" || l.Clave == "JN-A-036" || l.Clave == "RO-B-027" || l.Clave == "JN-D-065")
            .ToDictionaryAsync(l => l.Clave, cancellationToken);

        var clientes = await db.Clientes
            .Where(c => c.Curp == "CASL850315MDFRRN01" || c.Curp == "MENR800520HDFRNT02" || c.Curp == "VARE750810MDFRGL03" || c.Curp == "SALJ901112HDFRRR04")
            .ToDictionaryAsync(c => c.Curp, cancellationToken);

        var cliente1 = ObtenerOCrearCliente(db, clientes, "Laura", "Castañeda", "CASL850315MDFRRN01", "5512345678", "laura.castaneda@ejemplo.com", "Av. Universidad 1200, Col. del Valle, CDMX");
        var cliente2 = ObtenerOCrearCliente(db, clientes, "Roberto", "Méndez", "MENR800520HDFRNT02", "5523456789", "roberto.mendez@ejemplo.com", "Calle Insurgentes Sur 450, CDMX");
        var cliente3 = ObtenerOCrearCliente(db, clientes, "Elena", "Vargas", "VARE750810MDFRGL03", "5534567890", "elena.vargas@ejemplo.com", "Calzada de Tlalpan 890, CDMX");
        var cliente4 = ObtenerOCrearCliente(db, clientes, "Jorge", "Salinas", "SALJ901112HDFRRR04", "5545678901", "jorge.salinas@ejemplo.com", "Paseo de la Reforma 222, CDMX");

        if (!existentes.Contains("CTR-2025-0189") && lotes.TryGetValue("JC-C-118", out var lote1))
        {
            lote1.MarcarVendido();
            var venta1 = Venta.CrearFinanciada("CTR-2025-0189", cliente1.Id, lote1.Id, vendedorId, lote1.PrecioContado, new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
            var tabla1 = ConstruirTablaAmortizacion(127500.00m, 127500.00m, 0, 6248.50m);
            var fin1 = Financiamiento.Crear(venta1.Id, 20m, new Dinero(22500m), new Dinero(127500m), 24, 0.20m, tabla1, new DateOnly(2025, 2, 15));
            await db.Ventas.AddAsync(venta1, cancellationToken);
            await db.Financiamientos.AddAsync(fin1, cancellationToken);
        }

        if (!existentes.Contains("CTR-2025-0182") && lotes.TryGetValue("JN-A-036", out var lote2))
        {
            lote2.MarcarVendido();
            var venta2 = Venta.CrearFinanciada("CTR-2025-0182", cliente2.Id, lote2.Id, vendedorId, lote2.PrecioContado, new DateTimeOffset(2024, 6, 10, 11, 0, 0, TimeSpan.Zero));
            var tabla2 = ConstruirTablaAmortizacion(120000.00m, 58743.20m, 8, 6248.50m);
            var fin2 = Financiamiento.Crear(venta2.Id, 20m, new Dinero(24000m), new Dinero(120000m), 24, 0.20m, tabla2, new DateOnly(2024, 7, 10));
            AplicarCuotasPagadas(fin2, 8, new DateTimeOffset(2024, 7, 10, 12, 0, 0, TimeSpan.Zero));
            await db.Ventas.AddAsync(venta2, cancellationToken);
            await db.Financiamientos.AddAsync(fin2, cancellationToken);
        }

        if (!existentes.Contains("CTR-2024-0118") && lotes.TryGetValue("RO-B-027", out var lote3))
        {
            lote3.MarcarVendido();
            var venta3 = Venta.CrearFinanciada("CTR-2024-0118", cliente3.Id, lote3.Id, vendedorId, lote3.PrecioContado, new DateTimeOffset(2024, 4, 2, 9, 30, 0, TimeSpan.Zero));
            var tabla3 = ConstruirTablaAmortizacion(144000.00m, 76180.40m, 6, 6248.50m);
            var fin3 = Financiamiento.Crear(venta3.Id, 20m, new Dinero(36000m), new Dinero(144000m), 24, 0.20m, tabla3, new DateOnly(2024, 5, 2));
            AplicarCuotasPagadas(fin3, 6, new DateTimeOffset(2024, 5, 2, 10, 0, 0, TimeSpan.Zero));
            var cuotaVencida = fin3.Mensualidades.First(m => m.NumeroCuota == 7);
            cuotaVencida.MarcarVencida();
            fin3.MarcarEnMora();
            await db.Ventas.AddAsync(venta3, cancellationToken);
            await db.Financiamientos.AddAsync(fin3, cancellationToken);
        }

        if (!existentes.Contains("CTR-2025-0144") && lotes.TryGetValue("JN-D-065", out var lote4))
        {
            lote4.MarcarVendido();
            var venta4 = Venta.CrearFinanciada("CTR-2025-0144", cliente4.Id, lote4.Id, vendedorId, lote4.PrecioContado, new DateTimeOffset(2024, 10, 5, 14, 0, 0, TimeSpan.Zero));
            var tabla4 = ConstruirTablaAmortizacion(120000.00m, 94265.80m, 4, 5612.30m);
            var fin4 = Financiamiento.Crear(venta4.Id, 20m, new Dinero(30000m), new Dinero(120000m), 24, 0.20m, tabla4, new DateOnly(2024, 11, 5));
            AplicarCuotasPagadas(fin4, 4, new DateTimeOffset(2024, 11, 5, 15, 0, 0, TimeSpan.Zero));
            venta4.CambiarEstadoAdmin(EstadoVenta.EnRevision, "En revisión de documentación y validación de beneficiarios");
            await db.Ventas.AddAsync(venta4, cancellationToken);
            await db.Financiamientos.AddAsync(fin4, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Cliente ObtenerOCrearCliente(AppDbContext db, Dictionary<string, Cliente> cache, string nombre, string apellidos, string curp, string tel, string correo, string dir)
    {
        if (cache.TryGetValue(curp, out var existente))
        {
            return existente;
        }

        var cliente = Cliente.Crear(nombre, apellidos, curp, tel, correo, dir);
        db.Clientes.Add(cliente);
        cache[curp] = cliente;
        return cliente;
    }

    private static void AplicarCuotasPagadas(Financiamiento fin, int cuotas, DateTimeOffset baseFecha)
    {
        for (int i = 1; i <= cuotas; i++)
        {
            fin.AplicarPagoCuota(i, baseFecha.AddMonths(i - 1));
        }
    }

    private static IReadOnlyList<RenglonAmortizacion> ConstruirTablaAmortizacion(decimal capitalTotal, decimal saldoObjetivo, int cuotasPagadas, decimal cuotaProximo, int plazo = 24)
    {
        var tasaMensual = 0.20m / 12m;
        var renglones = new List<RenglonAmortizacion>(plazo);
        decimal saldoActual = capitalTotal;
        decimal capPagadoTotal = capitalTotal - saldoObjetivo;

        for (int p = 1; p <= cuotasPagadas; p++)
        {
            decimal cap = (p == cuotasPagadas)
                ? capPagadoTotal - renglones.Sum(r => r.CapitalAmortizado)
                : Math.Round(capPagadoTotal / cuotasPagadas, 2, MidpointRounding.ToEven);
            decimal interes = Math.Round(saldoActual * tasaMensual, 2, MidpointRounding.ToEven);
            decimal saldoFinal = saldoActual - cap;
            renglones.Add(new RenglonAmortizacion(p, saldoActual, interes, cap, cap + interes, saldoFinal));
            saldoActual = saldoFinal;
        }

        for (int p = cuotasPagadas + 1; p <= plazo; p++)
        {
            decimal saldoInicial = saldoActual;
            decimal interes = Math.Round(saldoInicial * tasaMensual, 2, MidpointRounding.ToEven);
            decimal cap;
            decimal pago;

            if (p == plazo)
            {
                cap = saldoInicial;
                pago = cap + interes;
            }
            else if (p == cuotasPagadas + 1)
            {
                pago = cuotaProximo;
                cap = pago - interes;
            }
            else
            {
                cap = Math.Round(saldoInicial / (plazo - p + 1), 2, MidpointRounding.ToEven);
                pago = cap + interes;
            }

            decimal saldoFinal = saldoInicial - cap;
            saldoActual = saldoFinal;
            renglones.Add(new RenglonAmortizacion(p, saldoInicial, interes, cap, pago, saldoFinal));
        }

        return renglones.AsReadOnly();
    }
}
