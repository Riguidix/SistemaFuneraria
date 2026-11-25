using System.Diagnostics;
using FluentAssertions;
using SistemaFuneraria.Web.Pages.Reportes;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

public class ReportesPageTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static string RutaWeb(params string[] segmentos) =>
        Path.Combine(new[] { RepoRoot, "src", "SistemaFuneraria.Web" }.Concat(segmentos).ToArray());

    [Fact]
    public void Reportes_OnGet_DebeInicializarEstadoLimpioSinDatosMock()
    {
        var pageModel = new IndexModel();

        pageModel.OnGet();

        pageModel.PeriodoSeleccionado.Should().Be("mes-actual");
        pageModel.SucursalSeleccionada.Should().Be("Jardines del Valle");
        pageModel.ConversionCotizacion.Should().Be(0m);
        pageModel.TicketPromedio.Should().Be(0m);
        pageModel.IndiceMorosidad.Should().Be(0m);
        pageModel.PorcentajeCumplimientoMeta.Should().Be(0);
        pageModel.AcumuladoMes.Should().Be(0m);
        pageModel.ObjetivoMensual.Should().Be(0m);
        pageModel.VentasPorTipoLote.Should().BeEmpty();
    }

    [Fact]
    public void ReportesVistaYJs_DebenEliminarMocksYSoportarRbac403YPeriodosReales()
    {
        var html = File.ReadAllText(RutaWeb("Pages", "Reportes", "Index.cshtml"));
        var js = File.ReadAllText(RutaWeb("wwwroot", "js", "pages", "reportesPage.js"));

        html.Should().NotContain("Noviembre 2025");
        html.Should().NotContain("147,420");
        html.Should().Contain("id=\"reportesAccesoRestringido\"");
        html.Should().Contain("id=\"alertaErrorReportes\"");
        html.Should().Contain("id=\"btnReintentarReportes\"");
        html.Should().Contain("value=\"mes-actual\"");
        html.Should().Contain("value=\"trimestre-actual\"");
        html.Should().Contain("value=\"anio-actual\"");
        html.Should().Contain("value=\"historico\"");

        js.Should().Contain("getToken");
        js.Should().Contain("returnUrl");
        js.Should().Contain("construirCsvReporte");
        js.Should().Contain("Sin ventas registradas en este periodo.");
    }

    [Fact]
    public async Task ReportesPageJs_Flujo403YDataCsv_DebenPasarEnNode()
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"sf_reportes_page_{Guid.NewGuid():N}.cjs");
        var jsPath = RutaWeb("wwwroot", "js", "pages", "reportesPage.js");

        var script = """
            const assert = require("node:assert/strict");
            const rep = require(process.argv[2]);

            function mkEl(initial = {}) {
                const classes = new Set(initial.classes || []);
                return {
                    value: initial.value ?? "",
                    textContent: "",
                    innerHTML: "",
                    className: "",
                    disabled: Boolean(initial.disabled),
                    style: {},
                    attrs: {},
                    classList: {
                        add: (...c) => c.forEach((x) => classes.add(x)),
                        remove: (...c) => c.forEach((x) => classes.delete(x)),
                        contains: (c) => classes.has(c)
                    },
                    addEventListener() {},
                    setAttribute(k, v) { this.attrs[k] = v; }
                };
            }

            (async () => {
                // 1. Vendedor recibe 403 en Reportes
                const els403 = {
                    reportesContenidoPrincipal: mkEl(),
                    reportesAccesoRestringido: mkEl({ classes: ["d-none"] }),
                    alertaErrorReportes: mkEl({ classes: ["d-none"] }),
                    btnVolver403Reportes: mkEl()
                };
                rep.init({
                    document: { getElementById: (id) => els403[id] || null },
                    window: { location: { pathname: "/Reportes", search: "" } },
                    authApi: {
                        getToken: () => "jwt",
                        getSession: () => ({ rol: "Vendedor" }),
                        isTokenExpired: () => false,
                        getDefaultRouteForRole: () => "/Resumen"
                    },
                    apiClient: { TOKEN_STORAGE_KEY: "sf_token" }
                });
                assert.equal(els403.reportesContenidoPrincipal.classList.contains("d-none"), true);
                assert.equal(els403.reportesAccesoRestringido.classList.contains("d-none"), false);
                assert.equal(els403.btnVolver403Reportes.attrs.href, "/Resumen");

                // 2. Administrador carga indicadores y genera CSV completo
                const payload = {
                    periodo: "Noviembre 2026",
                    sucursal: "Jardines del Valle",
                    conversionCotizacion: 45.5,
                    ticketPromedio: 150000,
                    indiceMorosidad: 4.0,
                    ventasPorTipoLote: [
                        { tipoLote: "Lote 2 gavetas", gavetas: 2, cantidadVentas: 1, montoTotal: 120000, porcentaje: 44.4 },
                        { tipoLote: "Lote 3 gavetas", gavetas: 3, cantidadVentas: 1, montoTotal: 150000, porcentaje: 55.6 }
                    ],
                    metaMensual: { acumulado: 270000, objetivoMensual: 600000, porcentajeAvance: 45 }
                };

                const elsAdmin = {
                    reportesContenidoPrincipal: mkEl(),
                    reportesAccesoRestringido: mkEl({ classes: ["d-none"] }),
                    alertaErrorReportes: mkEl({ classes: ["d-none"] }),
                    selectPeriodo: mkEl({ value: "mes-actual" }),
                    selectSucursal: mkEl({ value: "Jardines del Valle" }),
                    btnDescargarReporte: mkEl({ disabled: true }),
                    kpiConversion: mkEl(),
                    kpiTicketPromedio: mkEl(),
                    kpiIndiceMorosidad: mkEl(),
                    subtituloMorosidad: mkEl(),
                    subtituloMetaMensual: mkEl(),
                    contenedorVentasTipoLote: mkEl(),
                    totalIngresosReportes: mkEl(),
                    kpiMetaPorcentaje: mkEl(),
                    kpiMetaAcumulado: mkEl(),
                    kpiMetaObjetivo: mkEl(),
                    donutMetaMensual: mkEl()
                };

                rep.init({
                    document: { getElementById: (id) => elsAdmin[id] || null },
                    window: { location: { pathname: "/Reportes", search: "" } },
                    authApi: { getToken: () => "jwt", getSession: () => ({ rol: "Administrador" }), isTokenExpired: () => false },
                    apiClient: { TOKEN_STORAGE_KEY: "sf_token" },
                    reportesApi: { obtenerIndicadores: async () => payload }
                });

                await new Promise((r) => setTimeout(r, 10));
                assert.equal(elsAdmin.kpiConversion.textContent, "45.5%");
                assert.equal(elsAdmin.btnDescargarReporte.disabled, false);

                const csv = rep.construirCsvReporte(payload);
                assert.match(csv, /Lote 2 gavetas/);
                assert.match(csv, /Meta mensual,Acumulado/);
            })().catch((err) => {
                console.error(err);
                process.exit(1);
            });
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
            psi.ArgumentList.Add(jsPath);

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
