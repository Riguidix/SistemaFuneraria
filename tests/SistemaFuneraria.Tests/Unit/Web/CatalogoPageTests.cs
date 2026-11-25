using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaFuneraria.Application.Servicios.Lotes;
using SistemaFuneraria.Core.Entidades;
using SistemaFuneraria.Core.ValueObjects;
using SistemaFuneraria.Infrastructure.Persistencia;
using SistemaFuneraria.Web.Pages.Catalogo;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class CatalogoPageTests
{
    [Fact]
    public void Catalogo_OnGet_DebeCargarCorrectamente()
    {
        // Arrange
        var pageModel = new IndexModel();

        // Act
        pageModel.OnGet();

        // Assert
        pageModel.Should().NotBeNull();
    }

    [Fact]
    public async Task LoteService_ObtenerCatalogo_DebeRetornarTotalFiltradosYFolioContratoActivo()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"TestCatalogoLotes_{Guid.NewGuid():N}"));

        using var provider = services.BuildServiceProvider();
        using var db = provider.GetRequiredService<AppDbContext>();

        var cliente = Cliente.Crear("Ana", "Ruiz", "RUIA900101MDFRRN01", "5512345678", "ana@ejemplo.mx", "Dir");
        var loteLibre = Lote.Crear("JN-A-001", "Jardín Norte", "Andador A · Fila 1", 2, new Dinero(120000m));
        var loteVendido = Lote.Crear("LM-A-014", "Las Magnolias", "Andador A · Fila 1", 2, new Dinero(120000m));
        loteVendido.MarcarVendido();

        var venta = Venta.CrearContado("CTR-2026-0301", cliente.Id, loteVendido.Id, "vend-1", loteVendido.PrecioContado, DateTimeOffset.UtcNow);

        await db.Clientes.AddAsync(cliente);
        await db.Lotes.AddRangeAsync(loteLibre, loteVendido);
        await db.Ventas.AddAsync(venta);
        await db.SaveChangesAsync();

        var service = new LoteService(db);
        var res = await service.ObtenerCatalogoAsync(estado: "Vendido", pagina: 1, tamanoPagina: 10);

        res.EsExitoso.Should().BeTrue();
        res.Valor.TotalLotes.Should().Be(2);
        res.Valor.TotalDisponibles.Should().Be(1);
        res.Valor.TotalFiltrados.Should().Be(1);
        res.Valor.Items.Should().ContainSingle(l =>
            l.Clave == "LM-A-014" &&
            l.ContratoActivoId == venta.Id &&
            l.FolioContratoActivo == "CTR-2026-0301");
    }
}
