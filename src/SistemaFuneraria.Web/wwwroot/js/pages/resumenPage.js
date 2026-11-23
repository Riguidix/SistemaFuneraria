/**
 * resumenPage.js - Conector real para /Resumen/Index (SF-089).
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.resumenPage = api;
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

    function formatearMonedaExacta(monto) {
        const num = Number(monto);
        const seguro = Number.isFinite(num) ? num : 0;
        return "$" + seguro.toLocaleString("es-MX", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
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

        function poblar(data) {
            if (!data) return;
            const kpis = data.kpis || {};
            const disp = data.disponibilidad || {};
            const series = Array.isArray(data.ventasYCobranza) ? data.ventasYCobranza : [];
            const actividad = Array.isArray(data.actividadReciente) ? data.actividadReciente : [];

            const set = function (id, val) {
                const el = doc.getElementById(id);
                if (el) el.textContent = val;
            };

            set("kpiVentasMes", formatearMonedaCorta(kpis.ventasDelMes));
            set("kpiCobranzaMensual", formatearMonedaCorta(kpis.cobranzaMensual));
            set("kpiContratosActivos", String(kpis.contratosActivos ?? 0));
            set("kpiContratosNuevos", `${kpis.contratosNuevosMes ?? 0} nuevos este mes`);
            set("kpiPagosVencidos", String(kpis.pagosVencidosCount ?? 0));
            set("kpiMontoPorRecuperar", `${formatearMonedaCorta(kpis.montoPorRecuperar)} por recuperar`);

            set("kpiLotesDisponibles", String(disp.totalDisponibles ?? 0));
            set("kpiLotes2", String(disp.gavetas2 ?? 0));
            set("kpiLotes3", String(disp.gavetas3 ?? 0));
            set("kpiLotes4", String(disp.gavetas4 ?? 0));
            set("pctOcupacion", `${Number(disp.porcentajeOcupacion ?? 0).toFixed(1)}%`);

            const barOcupacion = doc.getElementById("barraOcupacion");
            if (barOcupacion) {
                const pct = Math.min(100, Math.max(0, Number(disp.porcentajeOcupacion ?? 0)));
                barOcupacion.style.width = `${pct}%`;
                barOcupacion.setAttribute("aria-valuenow", String(pct));
            }

            const donut = doc.getElementById("donutDisponibilidad");
            const totalDisp = Number(disp.totalDisponibles ?? 0);
            if (donut && totalDisp > 0) {
                const p2 = (Number(disp.gavetas2 ?? 0) * 100) / totalDisp;
                const p3 = p2 + (Number(disp.gavetas3 ?? 0) * 100) / totalDisp;
                donut.style.background = `conic-gradient(#2D4224 0% ${p2.toFixed(2)}%, #C59D52 ${p2.toFixed(2)}% ${p3.toFixed(2)}%, #7D9B76 ${p3.toFixed(2)}% 100%)`;
            }

            const barsContainer = doc.getElementById("barsVentasCobranza");
            if (barsContainer && series.length > 0) {
                const maxVal = Math.max(100000, ...series.map(s => Math.max(Number(s.ventas || 0), Number(s.cobranza || 0))));
                barsContainer.innerHTML = series.map(function (punto) {
                    const hv = Math.min(100, Math.round((Number(punto.ventas || 0) * 100) / maxVal));
                    const hc = Math.min(100, Math.round((Number(punto.cobranza || 0) * 100) / maxVal));
                    return `<div class="d-flex flex-column align-items-center h-100 justify-content-end" style="width: 14%;">` +
                        `<div class="d-flex align-items-end justify-content-center gap-1 w-100" style="height: 200px;">` +
                        `<div class="sf-bar sf-bar-ventas rounded-top-2" style="height: ${hv}%; width: 14px; background-color: #2D4224;" title="${escapeHtml(punto.mes)} · Ventas: ${escapeHtml(formatearMonedaCorta(punto.ventas))}"></div>` +
                        `<div class="sf-bar sf-bar-cobranza rounded-top-2" style="height: ${hc}%; width: 14px; background-color: #C59D52;" title="${escapeHtml(punto.mes)} · Cobranza: ${escapeHtml(formatearMonedaCorta(punto.cobranza))}"></div>` +
                        `</div>` +
                        `<span class="small text-muted fw-semibold mt-2" style="font-size: 0.75rem;">${escapeHtml(punto.mes)}</span>` +
                        `</div>`;
                }).join("");
            }

            const tbody = doc.getElementById("tbodyActividadReciente");
            if (tbody && actividad.length > 0) {
                tbody.innerHTML = actividad.map(function (mov) {
                    const badge = String(mov.estado || "").toLowerCase() === "aplicado"
                        ? '<span class="badge bg-success-subtle text-success px-3 py-1 rounded-pill fw-medium"><i class="fa-solid fa-circle-check me-1 small" aria-hidden="true"></i> Aplicado</span>'
                        : `<span class="badge bg-warning-subtle text-warning-emphasis px-3 py-1 rounded-pill fw-medium"><i class="fa-solid fa-clock me-1 small" aria-hidden="true"></i> ${escapeHtml(mov.estado || "Firmado")}</span>`;
                    return `<tr>` +
                        `<td class="py-3"><div class="d-flex align-items-center gap-2">` +
                        `<div class="rounded-circle d-flex align-items-center justify-content-center text-white fw-bold shadow-sm" style="width: 36px; height: 36px; min-width: 36px; background-color: #485635; font-size: 0.8rem;">${escapeHtml(mov.iniciales || "SF")}</div>` +
                        `<div><div class="fw-semibold text-dark">${escapeHtml(mov.cliente)}</div><div class="text-muted small" style="font-size: 0.75rem;">${escapeHtml(mov.email)}</div></div>` +
                        `</div></td>` +
                        `<td class="py-3 text-secondary">${escapeHtml(mov.movimiento)}</td>` +
                        `<td class="py-3"><span class="badge bg-light text-dark border px-2 py-1 fw-medium" style="font-family: monospace;">${escapeHtml(mov.referencia)}</span></td>` +
                        `<td class="py-3 sf-money fw-bold text-dark">${escapeHtml(formatearMonedaExacta(mov.importe))}</td>` +
                        `<td class="py-3">${badge}</td>` +
                        `<td class="py-3 text-muted text-end small">${escapeHtml(mov.fechaRelativa)}</td>` +
                        `</tr>`;
                }).join("");
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
                const data = await reportesApi.obtenerResumenDashboard();
                poblar(data);
            } catch {
                // Mantiene fallback SSR si falla consulta temporalmente
            }
        }

        cargar();
        return { cargar, poblar };
    }

    return { formatearMonedaCorta, init };
});
