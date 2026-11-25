using System.Diagnostics;
using FluentAssertions;
using CajaIndexModel = SistemaFuneraria.Web.Pages.Caja.IndexModel;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

[Trait("Ticket", "SF-082")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class CajaYReciboWebTests
{
    private static string RutaWeb(string rutaRelativa)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "SistemaFuneraria.sln")))
        {
            dir = dir.Parent;
        }
        dir.Should().NotBeNull("debe existir SistemaFuneraria.sln en el repositorio");
        return Path.Combine(dir!.FullName, "src", "SistemaFuneraria.Web", rutaRelativa);
    }

    [Fact(DisplayName = "SF-079 / SF-080 / SF-081: Caja/Index.cshtml y cajaPage.js conectan cobranzaApi con X-Idempotency-Key")]
    public void CajaIndex_DebeConectarCobranzaApiYCajaPage()
    {
        var cajaHtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Caja", "Index.cshtml")));
        var cajaJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "pages", "cajaPage.js")));
        var apiJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "api", "cobranzaApi.js")));

        cajaHtml.Should().Contain("/Contratos?tab=Vencidos");
        cajaHtml.Should().Contain("~/js/api/cobranzaApi.js");
        cajaHtml.Should().Contain("~/js/pages/cajaPage.js");
        cajaHtml.Should().Contain("lnkVerReciboPago");

        apiJs.Should().Contain("crypto.randomUUID");
        apiJs.Should().Contain("X-Idempotency-Key");
        apiJs.Should().Contain("/api/v1/cobranza");

        cajaJs.Should().Contain("cobranzaApi.aplicarPagoMensualidad");
        cajaJs.Should().Contain("cobranzaApi.obtenerResumenCaja");
        cajaJs.Should().Contain("/Caja/Recibo?folio=");
    }

    [Fact(DisplayName = "SF-082: Recibo.cshtml y reciboPagoPage.js muestran desglose financiero e impresión @media print")]
    public void CajaRecibo_DebeRenderizarDesgloseEImpresion()
    {
        var reciboHtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Caja", "Recibo.cshtml")));
        var reciboJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "pages", "reciboPagoPage.js")));

        reciboHtml.Should().Contain("@media print");
        reciboHtml.Should().Contain("reciboSaldoAnterior");
        reciboHtml.Should().Contain("reciboMontoCapital");
        reciboHtml.Should().Contain("reciboMontoInteres");
        reciboHtml.Should().Contain("reciboMontoTotal");
        reciboHtml.Should().Contain("reciboSaldoPosterior");
        reciboHtml.Should().Contain("~/js/pages/reciboPagoPage.js");

        reciboJs.Should().Contain("cobranzaApi.obtenerRecibo");
    }

    [Fact]
    public void CajaIndexModel_OnGet_DebeIniciarLimpioYPreservarContrato()
    {
        var modelSinContrato = new CajaIndexModel();
        modelSinContrato.OnGet();

        modelSinContrato.FolioContrato.Should().BeEmpty();
        modelSinContrato.ClienteNombre.Should().BeEmpty();
        modelSinContrato.MontoMensualidad.Should().Be(0m);
        modelSinContrato.MontoLiquidacion.Should().Be(0m);

        var modelConContrato = new CajaIndexModel();
        modelConContrato.OnGet("  CTR-2025-0189 ");
        modelConContrato.FolioContrato.Should().Be("CTR-2025-0189");
    }

    [Fact]
    public void CajaIndex_DebeCumplirWcagRbacYSinDatosMock()
    {
        var cajaHtml = File.ReadAllText(RutaWeb(Path.Combine("Pages", "Caja", "Index.cshtml")));
        var cajaJs = File.ReadAllText(RutaWeb(Path.Combine("wwwroot", "js", "pages", "cajaPage.js")));

        cajaHtml.Should().NotContain("Elena Vargas");
        cajaHtml.Should().NotContain("Desde las 08:02");
        cajaHtml.Should().Contain("id=\"cajaAccesoRestringido\"");
        cajaHtml.Should().Contain("role=\"button\"");
        cajaHtml.Should().Contain("aria-pressed=\"true\"");
        cajaHtml.Should().Contain("for=\"inputImporteRecibir\"");
        cajaHtml.Should().Contain("id=\"iconoBtnAplicarPago\"");
        cajaHtml.Should().Contain("id=\"estadoSinContratoCaja\"");

        cajaJs.Should().Contain("getToken");
        cajaJs.Should().Contain("returnUrl");
        cajaJs.Should().Contain("sidebarCajaBadge");
        cajaJs.Should().Contain("esContratoCerrado");
    }

    [Fact]
    public async Task CajaPageJs_FlujoCompletoYRegresionBtnAplicar_DebenPasarEnNode()
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"sf_caja_page_{Guid.NewGuid():N}.cjs");
        var cajaJs = RutaWeb(Path.Combine("wwwroot", "js", "pages", "cajaPage.js"));

        var script = """
            const assert = require("node:assert/strict");
            const caja = require(process.argv[2]);

            function mkEl(initial = {}) {
                const classes = new Set(initial.classes || []);
                const listeners = {};
                return {
                    value: initial.value ?? "",
                    textContent: initial.textContent ?? "",
                    innerHTML: initial.innerHTML ?? "",
                    placeholder: "",
                    disabled: Boolean(initial.disabled),
                    readOnly: false,
                    style: {},
                    attrs: {},
                    classList: {
                        add: (...c) => c.forEach((x) => classes.add(x)),
                        remove: (...c) => c.forEach((x) => classes.delete(x)),
                        toggle: (c, force) => (force === undefined ? (classes.has(c) ? classes.delete(c) : classes.add(c)) : (force ? classes.add(c) : classes.delete(c))),
                        contains: (c) => classes.has(c)
                    },
                    addEventListener(ev, fn) {
                        listeners[ev] = listeners[ev] || [];
                        listeners[ev].push(fn);
                    },
                    async trigger(ev, arg = {}) {
                        for (const fn of (listeners[ev] || [])) {
                            await fn(arg);
                        }
                    },
                    setAttribute(k, v) { this.attrs[k] = v; },
                    focus() {}
                };
            }

            (async () => {
                // 1. Vendedor recibe 403 en Caja
                const els403 = {
                    cajaContenidoPrincipal: mkEl(),
                    cajaAccesoRestringido: mkEl({ classes: ["d-none"] }),
                    btnVolver403Caja: mkEl()
                };
                caja.init({
                    document: { getElementById: (id) => els403[id] || null },
                    window: { location: { pathname: "/Caja", search: "" } },
                    authApi: {
                        getToken: () => "jwt",
                        getSession: () => ({ rol: "Vendedor" }),
                        isTokenExpired: () => false,
                        getDefaultRouteForRole: () => "/Resumen"
                    },
                    apiClient: { TOKEN_STORAGE_KEY: "sf_token" }
                });
                assert.equal(els403.cajaContenidoPrincipal.classList.contains("d-none"), true);
                assert.equal(els403.cajaAccesoRestringido.classList.contains("d-none"), false);
                assert.equal(els403.btnVolver403Caja.attrs.href, "/Resumen");

                // 2. Cajero: pago exitoso no deja huérfano a textoBtnAplicarPago, limpiar resetea y Liquidado bloquea
                const els = {
                    cajaContenidoPrincipal: mkEl(),
                    cajaAccesoRestringido: mkEl({ classes: ["d-none"] }),
                    inputBuscadorContrato: mkEl({ value: "CTR-2025-0189" }),
                    btnLimpiarBuscador: mkEl(),
                    btnBuscarContratoCaja: mkEl(),
                    estadoSinContratoCaja: mkEl(),
                    cardClienteLocalizado: mkEl({ classes: ["d-none"] }),
                    avatarCliente: mkEl(),
                    nombreCliente: mkEl(),
                    detalleContrato: mkEl(),
                    badgeEstadoCliente: mkEl(),
                    cardOpcionMensualidad: mkEl({ classes: ["active"] }),
                    cardOpcionAbono: mkEl(),
                    cardOpcionLiquidacion: mkEl(),
                    formAplicarPago: mkEl(),
                    inputImporteRecibir: mkEl({ value: "$0.00" }),
                    selectMetodoPago: mkEl({ value: "Efectivo" }),
                    inputReferencia: mkEl(),
                    asteriscoReferenciaCaja: mkEl(),
                    ayudaReferenciaCaja: mkEl(),
                    btnAplicarPago: mkEl({ disabled: true }),
                    iconoBtnAplicarPago: mkEl(),
                    textoBtnAplicarPago: mkEl(),
                    alertaPagoExitoso: mkEl({ classes: ["d-none"] }),
                    textoPagoExitoso: mkEl(),
                    subtextoPagoExitoso: mkEl(),
                    lnkVerReciboPago: mkEl({ classes: ["d-none"] }),
                    alertaErrorCaja: mkEl({ classes: ["d-none"] }),
                    fechaCorteDia: mkEl(),
                    montoSaldoInicial: mkEl(),
                    contadorPagosRecibidos: mkEl(),
                    montoIngresosDia: mkEl(),
                    montoTotalCaja: mkEl(),
                    tituloPagosAtencion: mkEl(),
                    sidebarCajaBadge: mkEl({ classes: ["d-none"] })
                };

                const contratoAbierto = {
                    folioContrato: "CTR-2025-0189",
                    clienteNombre: "Laura Castañeda",
                    inicialesCliente: "LC",
                    loteClave: "JC-C-118",
                    loteUbicacion: "Lote JC-C-118 · Jardín Central",
                    estadoVisual: "Al corriente",
                    cuotasVencidas: 0,
                    proximaCuotaNumero: 1,
                    montoMensualidad: 6187.50,
                    saldoInsoluto: 127500,
                    montoLiquidacionEstimado: 120000
                };

                const instance = caja.init({
                    document: { getElementById: (id) => els[id] || null },
                    window: { location: { pathname: "/Caja", search: "?contrato=CTR-2025-0189" } },
                    authApi: { getToken: () => "jwt", getSession: () => ({ rol: "Cajero" }), isTokenExpired: () => false },
                    apiClient: { TOKEN_STORAGE_KEY: "sf_token" },
                    cobranzaApi: {
                        generarClaveIdempotencia: () => "uuid-1",
                        obtenerResumenCaja: async () => ({ fecha: "2026-11-24", saldoInicial: 2000, pagosRecibidosHoy: 2, ingresosDelDia: 12000, totalEnCaja: 14000, pagosVencidosCount: 3 }),
                        obtenerContratoParaCaja: async () => contratoAbierto,
                        aplicarPagoMensualidad: async () => ({ folioPago: "PAG-2026-000001", concepto: "Cuota 1", montoTotal: 6187.50 })
                    }
                });

                await new Promise((r) => setTimeout(r, 10));
                assert.equal(els.sidebarCajaBadge.textContent, "3");
                assert.equal(els.btnAplicarPago.disabled, false);

                // Enviar pago mensualidad
                await els.formAplicarPago.trigger("submit", { preventDefault() {} });
                // Cambiar tarjeta a Liquidación después del pago: textoBtnAplicarPago debe actualizarse
                await els.cardOpcionLiquidacion.trigger("click");
                assert.match(els.textoBtnAplicarPago.textContent, /Aplicar liquidación por/);

                // Limpiar buscador resetea contrato
                await els.btnLimpiarBuscador.trigger("click");
                assert.equal(els.cardClienteLocalizado.classList.contains("d-none"), true);
                assert.equal(els.btnAplicarPago.disabled, true);
                assert.equal(els.textoBtnAplicarPago.textContent, "Selecciona un contrato para cobrar");

                // Contrato Liquidado bloquea botón
                instance.pintarContrato({ ...contratoAbierto, estadoVisual: "Liquidado", saldoInsoluto: 0, montoLiquidacionEstimado: 0 });
                assert.equal(els.btnAplicarPago.disabled, true);
                assert.equal(els.textoBtnAplicarPago.textContent, "Contrato liquidado / sin saldo pendiente");
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
            psi.ArgumentList.Add(cajaJs);

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

    [Fact]
    public async Task ReciboPagoPageJs_FlujoCompleto_DebenPasarEnNode()
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"sf_recibo_page_{Guid.NewGuid():N}.cjs");
        var reciboJs = RutaWeb(Path.Combine("wwwroot", "js", "pages", "reciboPagoPage.js"));

        var script = """
            const assert = require("node:assert/strict");
            const rec = require(process.argv[2]);

            assert.equal(rec.formatearMetodoPago("TransferenciaBancaria"), "Transferencia bancaria");
            assert.equal(rec.formatearMetodoPago("Tarjeta"), "Tarjeta de débito / crédito");
            assert.equal(rec.formatearMetodoPago("Efectivo"), "Efectivo");

            function mkEl(initial = {}) {
                const classes = new Set(initial.classes || []);
                return {
                    textContent: "",
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

            // 1. Sin ?folio= muestra 404
            const els404 = {
                reciboContenidoPrincipal: mkEl(),
                reciboNoEncontrado: mkEl({ classes: ["d-none"] }),
                reciboAccesoRestringido: mkEl({ classes: ["d-none"] }),
                alertaErrorRecibo: mkEl({ classes: ["d-none"] })
            };
            rec.init({
                document: { getElementById: (id) => els404[id] || null },
                window: { location: { pathname: "/Caja/Recibo", search: "" } },
                authApi: { getToken: () => "jwt", getSession: () => ({ rol: "Cajero" }), isTokenExpired: () => false },
                apiClient: { TOKEN_STORAGE_KEY: "sf_token" },
                cobranzaApi: { obtenerRecibo: async () => { throw new Error("No debe llamarse"); } }
            });
            assert.equal(els404.reciboContenidoPrincipal.classList.contains("d-none"), true);
            assert.equal(els404.reciboNoEncontrado.classList.contains("d-none"), false);

            // 2. Vendedor muestra 403
            const els403 = {
                reciboContenidoPrincipal: mkEl(),
                reciboNoEncontrado: mkEl({ classes: ["d-none"] }),
                reciboAccesoRestringido: mkEl({ classes: ["d-none"] }),
                btnVolver403Recibo: mkEl()
            };
            rec.init({
                document: { getElementById: (id) => els403[id] || null },
                window: { location: { pathname: "/Caja/Recibo", search: "?folio=PAG-2026-000001" } },
                authApi: {
                    getToken: () => "jwt",
                    getSession: () => ({ rol: "Vendedor" }),
                    isTokenExpired: () => false,
                    getDefaultRouteForRole: () => "/Resumen"
                },
                apiClient: { TOKEN_STORAGE_KEY: "sf_token" }
            });
            assert.equal(els403.reciboContenidoPrincipal.classList.contains("d-none"), true);
            assert.equal(els403.reciboAccesoRestringido.classList.contains("d-none"), false);
            assert.equal(els403.btnVolver403Recibo.attrs.href, "/Resumen");
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
            psi.ArgumentList.Add(reciboJs);

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
