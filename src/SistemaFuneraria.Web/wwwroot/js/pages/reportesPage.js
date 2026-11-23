/**
 * reportesPage.js - Conector real para /Reportes/Index (SF-090).
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.reportesPage = api;
        if (typeof document !== "undefined") {
            document.addEventListener("DOMContentLoaded", function () {
                api.init();
            });
        }
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    function formatearMonedaCorta(monto) {
        const num = Number(monto);
        const seguro = Number.isFinite(num) ? num : 0;
        return "$" + Math.round(seguro).toLocaleString("es-MX");
    }

    function escapeHtml(text) {
        return String(text ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const reportesApi = deps.reportesApi ?? win?.reportesApi;

        if (!doc) return null;

        const selectPeriodo = doc.getElementById("selectPeriodo");
        const selectSucursal = doc.getElementById("selectSucursal");
        const btnDescargar = doc.getElementById("btnDescargarReporte");
        let ultimoReporte = null;

        function poblar(data) {
            if (!data) return;
            ultimoReporte = data;
            const set = function (id, val) {
                const el = doc.getElementById(id);
                if (el) el.textContent = val;
            };

            set("kpiConversion", `${Number(data.conversionCotizacion ?? 0).toFixed(1)}%`);
            set("kpiTicketPromedio", formatearMonedaCorta(data.ticketPromedio));
            set("kpiIndiceMorosidad", `${Number(data.indiceMorosidad ?? 0).toFixed(1)}%`);

            const tipos = Array.isArray(data.ventasPorTipoLote) ? data.ventasPorTipoLote : [];
            const contenedorTipos = doc.getElementById("contenedorVentasTipoLote");
            if (contenedorTipos && tipos.length > 0) {
                contenedorTipos.innerHTML = tipos.map(function (item) {
                    const pct = Math.min(100, Math.max(0, Number(item.porcentaje ?? 0)));
                    return `<div class="mb-3">` +
                        `<div class="d-flex justify-content-between align-items-center mb-1">` +
                        `<span class="fw-semibold text-dark">${escapeHtml(item.tipoLote)}</span>` +
                        `<div class="d-flex align-items-center gap-2"><span class="small text-muted">${pct.toFixed(1)}%</span><span class="fw-bold sf-money text-dark">${escapeHtml(formatearMonedaCorta(item.montoTotal))}</span></div>` +
                        `</div>` +
                        `<div class="progress rounded-pill" style="height: 12px; background-color: #EFECE6;">` +
                        `<div class="progress-bar rounded-pill" role="progressbar" style="width: ${pct}%; background-color: #2D4224;" aria-valuenow="${pct}" aria-valuemin="0" aria-valuemax="100"></div>` +
                        `</div></div>`;
                }).join("");
            }

            const totalIngresos = tipos.reduce((acc, x) => acc + Number(x.montoTotal || 0), 0);
            set("totalIngresosReportes", formatearMonedaCorta(totalIngresos));

            const meta = data.metaMensual || {};
            const pctMeta = Math.min(100, Math.max(0, Number(meta.porcentajeAvance ?? 0)));
            set("kpiMetaPorcentaje", `${pctMeta.toFixed(0)}%`);
            set("kpiMetaAcumulado", formatearMonedaCorta(meta.acumulado));
            set("kpiMetaObjetivo", formatearMonedaCorta(meta.objetivoMensual));

            const donutMeta = doc.getElementById("donutMetaMensual");
            if (donutMeta) {
                donutMeta.style.background = `conic-gradient(#2D4224 0% ${pctMeta}%, #E5E0D8 ${pctMeta}% 100%)`;
            }
        }

        async function cargar() {
            if (!reportesApi) return;
            if (authApi && apiClient) {
                const storage = deps.storage ?? (typeof sessionStorage !== "undefined" ? sessionStorage : null);
                const token = storage ? storage.getItem(apiClient.TOKEN_STORAGE_KEY) : null;
                const session = authApi.getSession({ client: apiClient, storage });
                if (!token || !session) {
                    win?.location?.assign?.("/Cuenta/Login");
                    return;
                }
            }
            try {
                const data = await reportesApi.obtenerIndicadores({
                    periodo: selectPeriodo?.value || "",
                    sucursal: selectSucursal?.value || ""
                });
                poblar(data);
            } catch {
                // Mantiene fallback SSR en caso de 403/offline
            }
        }

        if (selectPeriodo) selectPeriodo.addEventListener("change", cargar);
        if (selectSucursal) selectSucursal.addEventListener("change", cargar);

        if (btnDescargar) {
            btnDescargar.addEventListener("click", function () {
                if (!ultimoReporte || typeof Blob === "undefined" || !win?.URL?.createObjectURL) return;
                const lineas = [
                    "Indicador,Valor",
                    `Periodo,${ultimoReporte.periodo}`,
                    `Sucursal,${ultimoReporte.sucursal}`,
                    `Conversion,${ultimoReporte.conversionCotizacion}%`,
                    `TicketPromedio,${ultimoReporte.ticketPromedio}`,
                    `IndiceMorosidad,${ultimoReporte.indiceMorosidad}%`
                ];
                const blob = new Blob(["\uFEFF" + lineas.join("\n")], { type: "text/csv;charset=utf-8;" });
                const url = win.URL.createObjectURL(blob);
                const a = doc.createElement("a");
                a.href = url;
                a.download = "reporte-indicadores.csv";
                a.click();
                win.URL.revokeObjectURL(url);
            });
        }

        cargar();
        return { cargar, poblar };
    }

    return { formatearMonedaCorta, init };
});
