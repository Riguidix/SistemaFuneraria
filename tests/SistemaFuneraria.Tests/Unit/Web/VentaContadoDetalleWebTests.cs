using System.Diagnostics;
using FluentAssertions;
using Xunit;

namespace SistemaFuneraria.Tests.Unit.Web;

[Trait("Ticket", "SF-062")]
[Trait("Ticket", "SF-064")]
[Trait("Capa", "Web")]
[Trait("Tipo", "Unit")]
public class VentaContadoDetalleWebTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact(DisplayName = "SF-062: Paso 4 Confirmación debe cargar ventasApi.js, selector Contado y manejo de 409")]
    public void Paso4Confirmacion_DebeConsumirVentasApiYSoportarConflicto409()
    {
        var nuevaHtml = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Ventas", "Nueva.cshtml"));
        var paso4Html = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Ventas", "Partials", "_Paso4Confirmacion.cshtml"));
        var paso4Js = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "paso4Confirmacion.js"));

        nuevaHtml.Should().Contain("~/js/api/ventasApi.js");
        paso4Html.Should().Contain("selectMetodoPagoContado")
            .And.Contain("alertaErrorContrato")
            .And.Contain("btnElegirOtroLote");

        paso4Js.Should().Contain("ventasApi.registrarVentaContado")
            .And.Contain("module.exports")
            .And.NotContain("CTR-2026-0190");
    }

    [Fact(DisplayName = "SF-064: Contratos/Detalle debe incluir comprobante contado imprimible, estado 404 y gestión Admin")]
    public void ContratosDetalle_DebeIncluirComprobanteContadoPrintY404()
    {
        var detalleHtml = File.ReadAllText(Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "Pages", "Contratos", "Detalle.cshtml"));
        var detalleJsPath = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "pages", "contratoDetallePage.js");

        File.Exists(detalleJsPath).Should().BeTrue();
        detalleHtml.Should().Contain("~/js/api/ventasApi.js")
            .And.Contain("~/js/pages/contratoDetallePage.js")
            .And.Contain("comprobanteContadoCard")
            .And.Contain("contratoNoEncontrado")
            .And.Contain("@media print")
            .And.Contain("data-roles=\"Administrador\"");

        var detalleJs = File.ReadAllText(detalleJsPath);
        detalleJs.Should().Contain("obtenerVentaPorId")
            .And.Contain("cancelarVenta")
            .And.Contain("cambiarEstadoVenta")
            .And.Contain("module.exports");
    }

    [Fact(DisplayName = "SF-062 / SF-064: Helpers JS de paso4Confirmacion y contratoDetallePage validan en Node")]
    public void HelpersJs_Paso4YDetalle_DebenPasarEnNode()
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), $"sf064_helpers_{Guid.NewGuid():N}.cjs");
        var paso4Js = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "ventas", "paso4Confirmacion.js");
        var detalleJs = Path.Combine(RepoRoot, "src", "SistemaFuneraria.Web", "wwwroot", "js", "pages", "contratoDetallePage.js");

        var script = """
            const assert = require("node:assert/strict");
            const p4 = require(process.argv[2]);
            const det = require(process.argv[3]);

            const payload = p4.construirPayloadVentaContado(
                { cliente: { id: "cli-1" }, lote: { id: "lot-1" } },
                "TransferenciaBancaria",
                " SPEI-99 "
            );
            assert.deepEqual(payload, {
                clienteId: "cli-1",
                loteId: "lot-1",
                metodoPago: "TransferenciaBancaria",
                referenciaPago: "SPEI-99"
            });

            assert.equal(det.obtenerBadgeEstadoVenta("Liquidado").texto, "Liquidado");
            assert.equal(det.obtenerBadgeEstadoVenta("Cancelado").texto, "Cancelado");
            const badgeVencido = det.obtenerBadgeEstadoVenta("Vencido");
            assert.equal(badgeVencido.texto, "Vencido");
            assert.match(badgeVencido.clase, /bg-danger-subtle/);
            assert.match(det.validarCambioEstadoAdmin("Cancelado", "corto"), /10 caracteres/);
            assert.equal(det.validarCambioEstadoAdmin("Cancelado", "Error de captura en titular"), null);
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
            psi.ArgumentList.Add(paso4Js);
            psi.ArgumentList.Add(detalleJs);

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
