using System.Diagnostics;
using FluentAssertions;
using SistemaFuneraria.Web.Pages.Contratos;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ContratosDetallePageTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string RutaWeb(params string[] segmentos) =>
        Path.Combine(new[] { RepoRoot, "src", "SistemaFuneraria.Web" }.Concat(segmentos).ToArray());

    [Fact]
    public void Detalle_OnGet_SinParametros_DebeInicializarEstadoLimpioSinAmortizacionMock()
    {
        var page = new DetalleModel();

        page.OnGet();

        page.Folio.Should().Be("Cargando...");
        page.Titular.Should().Be("Cargando expediente...");
        page.Lote.Should().Be("—");
        page.PrecioPactado.Should().Be(0m);
        page.SaldoInsoluto.Should().Be(0m);
        page.PlazoMeses.Should().Be(0);
        page.Amortizacion.Should().BeEmpty();
    }

    [Fact]
    public void Detalle_OnGet_ConFolioOLoteClave_DebePreservarParametrosSinDatosFicticios()
    {
        var pageFolio = new DetalleModel();
        pageFolio.OnGet(folio: "CTR-2024-0118");
        pageFolio.FolioParam.Should().Be("CTR-2024-0118");
        pageFolio.Folio.Should().Be("CTR-2024-0118");
        pageFolio.Amortizacion.Should().BeEmpty();

        var pageLote = new DetalleModel();
        pageLote.OnGet(loteClave: "LM-A-014");
        pageLote.LoteClaveParam.Should().Be("LM-A-014");
        pageLote.Lote.Should().Be("LM-A-014");
        pageLote.Amortizacion.Should().BeEmpty();
    }

    [Fact]
    public void DetalleVistaYJs_DebenCumplirRbacRecargaAdminYEstadosEnNode()
    {
        var html = File.ReadAllText(RutaWeb("Pages", "Contratos", "Detalle.cshtml"));
        var jsPath = RutaWeb("wwwroot", "js", "pages", "contratoDetallePage.js");
        var js = File.ReadAllText(jsPath);

        html.Should().NotContain("Laura Castañeda");
        html.Should().Contain("<h1 class=\"h2 fw-bold text-dark mb-1\" id=\"detalleFolioContrato\">");
        html.Should().Contain("id=\"btnIrACajaContrato\" data-roles=\"Administrador,Cajero\"");
        html.Should().Contain("id=\"alertaErrorDetalleContrato\"");
        html.Should().Contain("id=\"btnReintentarDetalleContrato\"");
        html.Should().Contain("id=\"filaCargandoAmortizacion\"");

        js.Should().Contain("getToken");
        js.Should().Contain("returnUrl");
        js.Should().Contain("loteClaveParam");
        js.Should().Contain("await cargarPorId()");

        var scriptPath = Path.Combine(Path.GetTempPath(), $"sf_detalle_page_{Guid.NewGuid():N}.cjs");
        var script = """
            const assert = require("node:assert/strict");
            const det = require(process.argv[2]);

            const badgeVencido = det.obtenerBadgeEstadoVenta("Vencido");
            assert.equal(badgeVencido.texto, "Vencido");
            assert.match(badgeVencido.clase, /bg-danger-subtle/);

            function mkEl() {
                const classes = new Set();
                return {
                    textContent: "",
                    innerHTML: "",
                    value: "",
                    disabled: false,
                    title: "",
                    attrs: {},
                    classList: {
                        add: (...c) => c.forEach((x) => classes.add(x)),
                        remove: (...c) => c.forEach((x) => classes.delete(x)),
                        toggle: (c, force) => (force ? classes.add(c) : classes.delete(c)),
                        contains: (c) => classes.has(c)
                    },
                    addEventListener() {},
                    setAttribute(k, v) { this.attrs[k] = v; }
                };
            }

            // 1. Sin folio/id/loteClave muestra estado 404
            const els404 = {
                contratoContenidoPrincipal: mkEl(),
                contratoNoEncontrado: mkEl(),
                alertaErrorDetalleContrato: mkEl()
            };
            els404.contratoNoEncontrado.classList.add("d-none");
            det.init({
                document: { getElementById: (id) => els404[id] || null },
                window: { location: { pathname: "/Contratos/Detalle", search: "" } },
                authApi: { applyRolePermissions() {}, getToken: () => "t", getSession: () => ({ rol: "Administrador" }), isTokenExpired: () => false },
                apiClient: { TOKEN_STORAGE_KEY: "sf_token" },
                contratosApi: { obtenerDetalleContrato: async () => { throw new Error("No debe llamarse"); } }
            });
            assert.equal(els404.contratoContenidoPrincipal.classList.contains("d-none"), true);
            assert.equal(els404.contratoNoEncontrado.classList.contains("d-none"), false);

            // 2. Contrato Liquidado deshabilita gestión Admin y oculta Caja; lote muestra sección y gavetas
            const elsDto = {
                contratoContenidoPrincipal: mkEl(),
                contratoNoEncontrado: mkEl(),
                alertaErrorDetalleContrato: mkEl(),
                detalleFolioContrato: mkEl(),
                detalleTitularContrato: mkEl(),
                detalleLoteContrato: mkEl(),
                badgeEstadoContrato: mkEl(),
                detallePrecioPactado: mkEl(),
                detalleSubtituloPrecio: mkEl(),
                detalleSaldoInsoluto: mkEl(),
                detalleMensualidad: mkEl(),
                detalleSubtituloMensualidad: mkEl(),
                detallePlazoTasa: mkEl(),
                detalleSubtituloTasa: mkEl(),
                btnIrACajaContrato: mkEl(),
                btnAbrirModalAdminEstado: mkEl(),
                seccionTablaAmortizacion: mkEl(),
                badgeConteoAmortizacion: mkEl(),
                tbodyAmortizacion: mkEl(),
                comprobanteContadoCard: mkEl()
            };
            const instance = det.init({
                document: { getElementById: (id) => elsDto[id] || null },
                window: { location: { pathname: "/Contratos/Detalle", search: "" } }
            });
            instance.poblarDesdeApi({
                ventaId: "v-99",
                folioContrato: "CTR-2025-0999",
                clienteNombre: "Laura Castañeda",
                loteClave: "JC-C-118",
                loteSeccion: "Jardín Central",
                loteGavetas: 3,
                modalidad: "Credito",
                precioPactado: 150000,
                estado: "Liquidado",
                estadoVisual: "Liquidado",
                financiamiento: { porcentajeEnganche: 20, montoEnganche: 30000, plazoMeses: 24, tasaInteresAnual: 0.2, cuotaMensualActual: 6187.5, saldoInsoluto: 0 },
                amortizacion: []
            });
            assert.equal(elsDto.detalleLoteContrato.textContent, "JC-C-118 (Jardín Central, 3 gavetas)");
            assert.equal(elsDto.btnAbrirModalAdminEstado.disabled, true);
            assert.equal(elsDto.btnIrACajaContrato.classList.contains("d-none"), true);
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
            psi.ArgumentList.Add(jsPath);

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
