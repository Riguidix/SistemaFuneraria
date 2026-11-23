using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Application.DTOs.Ventas;
using SistemaFuneraria.Application.Servicios.Ventas;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.Enums;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Application;

[Trait("Ticket", "SF-066")]
[Trait("Capa", "Application")]
[Trait("Tipo", "Unit")]
public class VentaFinanciadaServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDbContext _dbContext;
    private readonly VentaService _ventaService;

    public VentaFinanciadaServiceTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestVentaFinanciada_{Guid.NewGuid():N}"));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        _ventaService = new VentaService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _serviceProvider.Dispose();
    }

    private async Task<(Cliente Cliente, Lote Lote)> CrearClienteYLoteAsync(decimal precio = 120000m, int gavetas = 2, string clave = "JN-A-042")
    {
        var sufijo = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var cliente = Cliente.Crear("Mariana", "Torres Aguirre", $"SF66{sufijo}"[..18], "5512345678", $"mariana.{sufijo[..6].ToLowerInvariant()}@email.mx", "Av. Universidad 100");
        var lote = Lote.Crear(clave, "Jardín Norte", "A", gavetas, new Dinero(precio));
        await _dbContext.Clientes.AddAsync(cliente);
        await _dbContext.Lotes.AddAsync(lote);
        await _dbContext.SaveChangesAsync();
        return (cliente, lote);
    }

    [Fact(DisplayName = "SF-066 (BDD): RegistrarVentaFinanciadaAsync crea Venta Activo, Financiamiento $96,000, 24 mensualidades y lote Vendido en transacción")]
    public async Task RegistrarVentaFinanciadaAsync_Con20PorcientoY24Meses_DebePersistirTodoAtomicamente()
    {
        var (cliente, lote) = await CrearClienteYLoteAsync(120000m, 2, "JN-A-042");

        var resultado = await _ventaService.RegistrarVentaFinanciadaAsync(
            new RegistrarVentaFinanciadaCommand(
                ClienteId: cliente.Id,
                LoteId: lote.Id,
                VendedorId: "vendedor-1",
                PorcentajeEnganche: 20,
                PlazoMeses: 24,
                FechaPrimerPago: new DateOnly(2026, 12, 1)));

        resultado.EsExitoso.Should().BeTrue();
        var resumen = resultado.Valor;
        resumen.Modalidad.Should().Be("Financiado");
        resumen.Estado.Should().Be("Activo");
        resumen.FolioContrato.Should().StartWith("CTR-");
        resumen.MontoEnganche.Should().Be(24000m);
        resumen.CapitalFinanciado.Should().Be(96000m);
        resumen.SaldoInsoluto.Should().Be(96000m);
        resumen.PlazoMeses.Should().Be(24);
        resumen.TotalMensualidades.Should().Be(24);

        var loteDb = await _dbContext.Lotes.FindAsync(lote.Id);
        loteDb!.Estado.Should().Be(EstadoLote.Vendido);

        var cuotasDb = await _dbContext.Mensualidades
            .Where(m => m.FinanciamientoId == resumen.FinanciamientoId)
            .OrderBy(m => m.NumeroCuota)
            .ToListAsync();
        cuotasDb.Should().HaveCount(24);
        cuotasDb.Sum(c => c.CapitalProgramado.Monto).Should().Be(96000m);
        cuotasDb.Should().OnlyContain(c => c.Estado == EstadoMensualidad.Pendiente);
        cuotasDb[0].FechaVencimiento.Should().Be(new DateOnly(2026, 12, 1));
    }

    [Fact(DisplayName = "SF-066: Lote ya Vendido debe retornar conflicto [CONFLICTO] y no duplicar financiamiento")]
    public async Task RegistrarVentaFinanciadaAsync_SobreLoteVendido_DebeRetornarConflicto()
    {
        var (cliente, lote) = await CrearClienteYLoteAsync(120000m, 2, "JN-A-043");
        await _ventaService.RegistrarVentaFinanciadaAsync(
            new RegistrarVentaFinanciadaCommand(cliente.Id, lote.Id, "vendedor-1", 20, 12));

        var segundoIntento = await _ventaService.RegistrarVentaFinanciadaAsync(
            new RegistrarVentaFinanciadaCommand(cliente.Id, lote.Id, "vendedor-1", 20, 12));

        segundoIntento.EsFallo.Should().BeTrue();
        segundoIntento.Error.Should().StartWith("[CONFLICTO]");
    }

    [Theory(DisplayName = "SF-066: Plazos o porcentajes fuera de la matriz oficial retornan error de validación")]
    [InlineData(15, 24)]
    [InlineData(20, 36)]
    public async Task RegistrarVentaFinanciadaAsync_ParametrosInvalidos_DebeRetornarFallo(int enganche, int plazo)
    {
        var (cliente, lote) = await CrearClienteYLoteAsync(150000m, 3, $"JC-B-{plazo:000}");

        var res = await _ventaService.RegistrarVentaFinanciadaAsync(
            new RegistrarVentaFinanciadaCommand(cliente.Id, lote.Id, "vendedor-1", enganche, plazo));

        res.EsFallo.Should().BeTrue();
        var loteDb = await _dbContext.Lotes.FindAsync(lote.Id);
        loteDb!.Estado.Should().Be(EstadoLote.Disponible);
    }
}
