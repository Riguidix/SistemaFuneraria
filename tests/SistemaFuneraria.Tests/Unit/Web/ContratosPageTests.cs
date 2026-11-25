using System.Diagnostics;
using FluentAssertions;
using SistemaFuneraria.Web.Pages.Contratos;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ContratosPageTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string RutaWeb(params string[] segmentos) =>
        Path.Combine(new[] { RepoRoot, "src", "SistemaFuneraria.Web" }.Concat(segmentos).ToArray());

    [Fact]
    public void Contratos_OnGet_DebeInicializarEstadoLimpioYSoportarQueryParams()
    {
        var pageModel = new IndexModel();

        pageModel.OnGet(tab: "vencidos", buscar: "Roberto Méndez");

        pageModel.TotalActivos.Should().Be(0);
        pageModel.TotalAlCorriente.Should().Be(0);
        pageModel.TotalConAtraso.Should().Be(0);
        pageModel.TotalLiquidados.Should().Be(0);
        pageModel.Contratos.Should().BeEmpty();
        pageModel.TabInicial.Should().Be("vencidos");
        pageModel.BusquedaInicial.Should().Be("Roberto Méndez");
    }

    [Fact]
    public void ContratosVistaYJs_DebenEliminarMocksYSoportarPaginacionRbacYBuscar()
    {
        var html = File.ReadAllText(RutaWeb("Pages", "Contratos", "Index.cshtml"));
        var js = File.ReadAllText(RutaWeb("wwwroot", "js", "pages", "contratosIndexPage.js"));

        html.Should().NotContain("+ Nueva venta");
        html.Should().NotContain("CTR-2024-0118");
        html.Should().NotContain("Elena Vargas");
        html.Should().NotContain("93.7%");
        html.Should().NotContain("role=\"tablist\"");

        html.Should().Contain("<span>Nueva venta</span>");
        html.Should().Contain("data-roles=\"Administrador,Vendedor\"");
        html.Should().Contain("role=\"group\"");
        html.Should().Contain("aria-pressed=\"true\"");
        html.Should().Contain("id=\"filaCargandoContratos\"");
        html.Should().Contain("id=\"btnReintentarContratos\"");
        html.Should().Contain("id=\"paginacionContratos\"");
        html.Should().Contain("id=\"conteoPaginacionContratos\"");
        html.Should().Contain("id=\"btnPaginaAnteriorContratos\"");
        html.Should().Contain("id=\"indicadorPaginaContratos\"");
        html.Should().Contain("id=\"btnPaginaSiguienteContratos\"");

        js.Should().Contain("getToken");
        js.Should().Contain("returnUrl");
        js.Should().Contain("applyRolePermissions");
        js.Should().Contain("params.get(\"buscar\")");
        js.Should().Contain("tamanoPagina = 10");
        js.Should().Contain("/Contratos/Detalle?folio=");
        js.Should().Contain("puedeCobrarRol");
    }

    [Fact]
    public void ContratosIndexJs_RenderYFlujo_DebenPasarEnNode()
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"sf_contratos_idx_{Guid.NewGuid():N}.cjs");
        var indexJs = RutaWeb("wwwroot", "js", "pages", "contratosIndexPage.js");

        var script = """
            const assert = require("node:assert/strict");
            const idx = require(process.argv[2]);

            const itemVencido = {
                ventaId: "v-1",
                folioContrato: "CTR-2024-0118",
                clienteNombre: "Elena Vargas",
                loteClave: "RO-B-027",
                saldoInsoluto: 76180.4,
                proximoPagoTexto: "Venció 02 oct 2025",
                estado: "Vencido",
                accionSugerida: "Cobrar"
            };

            const htmlCajero = idx.renderizarFilaContrato(itemVencido, { rol: "Cajero" });
            assert.match(htmlCajero, /href="\/Contratos\/Detalle\?folio=CTR-2024-0118"/);
            assert.match(htmlCajero, /href="\/Caja\?contrato=CTR-2024-0118"/);
            assert.match(htmlCajero, /Detalle/);

            const htmlVendedor = idx.renderizarFilaContrato(itemVencido, { rol: "Vendedor" });
            assert.match(htmlVendedor, /href="\/Contratos\/Detalle\?folio=CTR-2024-0118"/);
            assert.doesNotMatch(htmlVendedor, /\/Caja\?contrato=/);
            assert.match(htmlVendedor, /Detalle/);

            const calls = [];
            let redirectedTo = null;
            function mkEl() {
                return {
                    value: "",
                    textContent: "",
                    innerHTML: "",
                    disabled: false,
                    classList: { add() {}, remove() {}, toggle() {} },
                    addEventListener() {},
                    setAttribute() {},
                    getAttribute() { return ""; }
                };
            }
            const els = {
                inputBuscarContratos: mkEl(),
                tbodyContratos: mkEl(),
                emptyStateContratos: mkEl(),
                alertaErrorContratos: mkEl(),
                paginacionContratos: mkEl(),
                conteoPaginacionContratos: mkEl(),
                indicadorPaginaContratos: mkEl(),
                btnPaginaAnteriorContratos: mkEl(),
                btnPaginaSiguienteContratos: mkEl()
            };
            const mockDoc = {
                getElementById: (id) => els[id] || null,
                querySelectorAll: () => []
            };
            const mockWin = {
                location: {
                    pathname: "/Contratos",
                    search: "?buscar=Mendez&tab=vencidos",
                    assign: (u) => { redirectedTo = u; }
                }
            };
            const mockAuth = {
                applyRolePermissions() {},
                getToken: () => "jwt-local",
                getSession: () => ({ rol: "Administrador" }),
                isTokenExpired: () => false
            };
            const mockContratosApi = {
                obtenerCartera: async (f) => {
                    calls.push(f);
                    return {
                        kpis: { totalActivos: 1, montoEnCartera: 1000, totalAlCorriente: 0, porcentajeAlCorriente: 0, totalConAtraso: 1, montoPorRecuperar: 500, totalLiquidadosAnio: 0 },
                        items: [itemVencido],
                        totalRegistros: 15,
                        pagina: 1,
                        tamanoPagina: 10
                    };
                }
            };

            idx.init({
                document: mockDoc,
                window: mockWin,
                authApi: mockAuth,
                apiClient: { TOKEN_STORAGE_KEY: "sf_token" },
                contratosApi: mockContratosApi
            });

            setTimeout(() => {
                assert.equal(redirectedTo, null);
                assert.equal(calls.length, 1);
                assert.equal(calls[0].buscar, "Mendez");
                assert.equal(calls[0].tab, "Vencidos");
                assert.equal(calls[0].pageSize, 10);
                assert.equal(els.conteoPaginacionContratos.textContent, "Mostrando 1–10 de 15 contratos");
                assert.equal(els.indicadorPaginaContratos.textContent, "Página 1 de 2");
            }, 0);
            """;

        File.WriteAllText(scriptPath, script);
        try
        {
            var psi = new ProcessStartInfo("node")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            psi.ArgumentList.Add(scriptPath);
            psi.ArgumentList.Add(indexJs);

            using var proc = Process.Start(psi)!;
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            proc.ExitCode.Should().Be(0, stderr);
        }
        finally
        {
            if (File.Exists(scriptPath)) File.Delete(scriptPath);
        }
    }
}
