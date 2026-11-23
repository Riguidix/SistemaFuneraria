using System.Diagnostics;
using FluentAssertions;
using SistemaFuneraria.Application.DTOs.Simulador;
using SistemaFuneraria.Application.Servicios.Simulador;
using SistemaFuneraria.Web.Pages.Ventas;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

[Trait("Ticket", "SF-056")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class WizardPasosAuditoriaTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact(DisplayName = "SF-056 / QA Gate: Shell y Pasos 1-3 cumplen estructura DOM, stepper y cuadrícula 2x4")]
    public void AuditoriaFigma_ShellYPasos1a3_EstructuraDomYCuadricula_DebeCumplirEspecificacion()
    {
        var pageModel = new NuevaModel();
        pageModel.OnGet();
        pageModel.Kicker.Should().Be("PROCESO COMERCIAL");
        pageModel.Titulo.Should().Be("Nueva venta");

        var nuevaHtml = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Ventas", "Nueva.cshtml"));
        var paso1Html = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Ventas", "Partials", "_Paso1Cliente.cshtml"));
        var paso2Html = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Ventas", "Partials", "_Paso2Lote.cshtml"));
        var paso3Html = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Ventas", "Partials", "_Paso3Financiamiento.cshtml"));

        foreach (var indicator in new[] { "stepIndicator1", "stepIndicator2", "stepIndicator3", "stepIndicator4" })
            nuevaHtml.Should().Contain(indicator);

        foreach (var summaryId in new[] { "resumenLoteTitulo", "resumenLoteClave", "resumenLoteCapacidad", "resumenLotePrecio", "resumenTotalEstimado", "draftBadge" })
            nuevaHtml.Should().Contain(summaryId);

        foreach (var slot in new[] { "A", "B", "42", "43", "C", "44", "45", "46" })
            nuevaHtml.Should().Contain($"data-slot=\"{slot}\"");

        paso1Html.Should().Contain("inputBuscarCliente").And.Contain("btnContinuarPaso1");
        paso2Html.Should().Contain("contenedorTarjetasLote").And.Contain("btnAnteriorPaso2").And.Contain("btnContinuarPaso2");
        paso3Html.Should().Contain("btnModalidadCredito").And.Contain("btnModalidadContado")
            .And.Contain("selectEnganche").And.Contain("selectPlazo")
            .And.Contain("lblBannerMonto").And.Contain("btnAnteriorPaso3");
    }

    [Fact(DisplayName = "SF-056: Motor SimuladorService coincide en Crédito ($4,886.00) y Contado ($120,000.00) sin disparidad")]
    public void AuditoriaFinanciera_SimuladorReal_DebeCoincidirEnCreditoYContadoSinDisparidad()
    {
        var simulador = new SimuladorService();

        var resCredito = simulador.Cotizar(new SimularCotizacionRequest(null, 2, "Credito", 20m, 24, new DateOnly(2026, 12, 1)));
        resCredito.EsExitoso.Should().BeTrue();
        resCredito.Valor.MensualidadEstimada.Should().Be(4_886.00m);
        resCredito.Valor.TablaAmortizacion.Sum(f => f.Capital).Should().Be(96_000.00m);

        var resContado = simulador.Cotizar(new SimularCotizacionRequest(null, 2, "Contado", 0m, 0, new DateOnly(2026, 12, 1)));
        resContado.EsExitoso.Should().BeTrue();
        resContado.Valor.PrecioLista.Should().Be(120_000.00m);
        resContado.Valor.SaldoAFinanciar.Should().Be(0m);
        resContado.Valor.TablaAmortizacion.Should().BeEmpty();
    }

    [Fact(DisplayName = "SF-056 / BDD: Regresión de pasos conserva borrador en wizardState.js y sincroniza simuladorApi.js en Node")]
    public void AuditoriaBDD_RegresionPasos_WizardStateYSimuladorApi_DebeConservarDatosYConsumirContratoEnNode()
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"sf056_audit_{Guid.NewGuid():N}.cjs");
        var wizardStateJs = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "wizardState.js");
        var simuladorApiJs = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "api", "simuladorApi.js");

        var script = """
            const assert = require("node:assert/strict");
            const store = new Map();
            const fakeStorage = {
                getItem: (k) => (store.has(k) ? store.get(k) : null),
                setItem: (k, v) => store.set(k, String(v)),
                removeItem: (k) => store.delete(k)
            };
            const mkEl = () => ({ textContent: "", style: {}, classList: { _s: new Set(), add(c){this._s.add(c);}, remove(c){this._s.delete(c);}, contains(c){return this._s.has(c);} } });
            const dom = {
                draftBadge: mkEl(), resumenLoteTitulo: mkEl(), resumenLoteClave: mkEl(),
                resumenLoteCapacidad: mkEl(), resumenLotePrecio: mkEl(), resumenTotalEstimado: mkEl()
            };
            const events = [];
            global.CustomEvent = class { constructor(type, init) { this.type = type; this.detail = init?.detail; } };
            global.window = { sessionStorage: fakeStorage, dispatchEvent: (e) => events.push(e.type), addEventListener: () => {} };
            global.document = { getElementById: (id) => dom[id] ?? null };

            const wizardState = require(process.argv[2]);
            const simuladorApi = require(process.argv[3]);

            const calls = [];
            const fakeClient = {
                get: async (url) => { calls.push(["GET", url]); return { porcentajesEnganche: [10,20,30], plazos: [{ meses: 24, tasaAnualPorcentaje: 20 }] }; },
                post: async (url, body) => { calls.push(["POST", url, body]); return { modalidad: body.modalidad, precioLista: 120000, mensualidadEstimada: 4886, totalAPagar: 141264 }; }
            };

            (async () => {
                await simuladorApi.obtenerPlanes({ client: fakeClient });
                const cot = await simuladorApi.cotizar({ gavetas: 2, modalidad: "Credito", porcentajeEnganche: 20, plazoMeses: 24 }, { client: fakeClient });
                assert.equal(cot.mensualidadEstimada, 4886);
                assert.equal(calls[0][1], "/api/v1/simulador/planes");
                assert.equal(calls[1][1], "/api/v1/simulador/cotizar");

                wizardState.updateDraft({ cliente: { nombre: "Mariana", apellidos: "Torres" }, paso: 2 });
                wizardState.updateDraft({ lote: { clave: "JN-A-042", tipo: "Lote Básico", gavetas: 2, precio: 120000 }, paso: 3 });
                wizardState.updateDraft({ financiamiento: { modalidad: "Credito", plazoMeses: 24, totalAPagar: cot.totalAPagar }, paso: 3 });

                // Regresión Anterior (Paso 3 -> Paso 2) y retorno (Paso 2 -> Paso 3)
                wizardState.updateDraft({ paso: 2 });
                wizardState.updateDraft({ paso: 3 });

                const draft = wizardState.getDraft();
                assert.equal(draft.cliente.nombre, "Mariana");
                assert.equal(draft.lote.clave, "JN-A-042");
                assert.equal(draft.financiamiento.plazoMeses, 24);
                assert.equal(dom.resumenLoteClave.textContent, "JN-A-042");
                assert.ok(events.includes("sf:draft-updated"));
                assert.equal(dom.draftBadge.classList.contains("d-none"), false);

                wizardState.clearDraft();
                assert.equal(Object.keys(wizardState.getDraft()).length, 0);
                assert.equal(dom.draftBadge.classList.contains("d-none"), true);
            })().catch((err) => { console.error(err); process.exit(1); });
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
            psi.ArgumentList.Add(wizardStateJs);
            psi.ArgumentList.Add(simuladorApiJs);

            using var proc = Process.Start(psi)!;
            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            proc.ExitCode.Should().Be(0, $"Node audit falló: {stderr}\n{stdout}");
        }
        finally
        {
            if (File.Exists(scriptPath)) File.Delete(scriptPath);
        }
    }
}
