using System.Diagnostics;
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
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string RutaWeb(params string[] segmentos) =>
        Path.Combine(new[] { RepoRoot, "src", "SistemaFuneraria.Web" }.Concat(segmentos).ToArray());
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

    [Fact]
    public async Task MapaLotesJs_IntegracionYCoordenadas_DebenPasarEnNode()
    {
        var catalogoHtml = File.ReadAllText(RutaWeb("Pages", "Catalogo", "Index.cshtml"));
        var paso2Html = File.ReadAllText(RutaWeb("Pages", "Ventas", "Partials", "_Paso2Lote.cshtml"));
        var nuevaHtml = File.ReadAllText(RutaWeb("Pages", "Ventas", "Nueva.cshtml"));
        var mapaJsPath = RutaWeb("wwwroot", "js", "components", "mapaLotes.js");

        File.Exists(mapaJsPath).Should().BeTrue();
        catalogoHtml.Should().Contain("id=\"btnVistaMapaCatalogo\"")
            .And.Contain("id=\"contenedorMapaCatalogo\"")
            .And.Contain("~/js/components/mapaLotes.js");
        paso2Html.Should().Contain("id=\"btnVistaMapaPaso2\"")
            .And.Contain("id=\"contenedorMapaPaso2\"");
        nuevaHtml.Should().Contain("~/js/components/mapaLotes.js");

        var scriptPath = Path.Combine(Path.GetTempPath(), $"sf_mapa_lotes_{Guid.NewGuid():N}.cjs");
        var script = """
            const assert = require("node:assert/strict");
            const mapa = require(process.argv[2]);

            const c1 = mapa.extraerCoordenadas({ clave: "JN-A-042", seccion: "Jardín Norte", andadorFila: "Andador A · Fila 4" });
            assert.equal(c1.andador, "A");
            assert.equal(c1.fila, 4);
            assert.equal(c1.columna, 2);
            assert.equal(c1.numeroCorto, "042");

            const c2 = mapa.extraerCoordenadas({ clave: "JC-C-118", seccion: "Jardín Central", andadorFila: "Andador C · Fila 11" });
            assert.equal(c2.andador, "C");
            assert.equal(c2.fila, 11);
            assert.equal(c2.columna, 8);

            const container = {
                innerHTML: "",
                addEventListener() {}
            };
            const inst = mapa.crearMapa(container, { modo: "seleccion" });
            inst.setLotes([
                { id: "1", clave: "JN-A-042", seccion: "Jardín Norte", andadorFila: "Andador A · Fila 4", gavetas: 2, precioContado: 120000, estado: "Disponible" },
                { id: "2", clave: "JN-A-043", seccion: "Jardín Norte", andadorFila: "Andador A · Fila 4", gavetas: 3, precioContado: 150000, estado: "Vendido" }
            ], { claveSeleccionada: "JN-A-042" });

            assert.match(container.innerHTML, /data-map-clave="JN-A-042"/);
            assert.match(container.innerHTML, /sf-plot-selected/);
            assert.match(container.innerHTML, /data-map-clave="JN-A-043"[^>]*disabled/);
            assert.match(container.innerHTML, /sf-plot-empty/);
            """;

        await File.WriteAllTextAsync(scriptPath, script);
        try
        {
            var psi = new ProcessStartInfo("node")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            psi.ArgumentList.Add(scriptPath);
            psi.ArgumentList.Add(mapaJsPath);

            using var proc = Process.Start(psi)!;
            var stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();
            proc.ExitCode.Should().Be(0, stderr);
        }
        finally
        {
            if (File.Exists(scriptPath)) File.Delete(scriptPath);
        }
    }
}
