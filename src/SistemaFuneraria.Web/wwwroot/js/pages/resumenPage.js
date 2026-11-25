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

    function obtenerSaludoPorHora(fecha = new Date()) {
        const hora = fecha.getHours();
        if (hora < 12) return "Buenos días";
        if (hora < 19) return "Buenas tardes";
        return "Buenas noches";
    }

    function obtenerPrimerNombre(nombreCompleto) {
        const texto = String(nombreCompleto || "").trim();
        if (!texto) return "Usuario";
        return texto.split(/\s+/)[0] || "Usuario";
    }

    function formatearFechaKicker(fecha = new Date()) {
        try {
            return new Intl.DateTimeFormat("es-MX", {
                weekday: "long",
                day: "numeric",
                month: "long"
            }).format(fecha).toUpperCase();
        } catch {
            return "";
        }
    }

    function calcularEscalaGrafica(series) {
        const maxDato = series.reduce(function (acc, s) {
            return Math.max(acc, Number(s.ventas || 0), Number(s.cobranza || 0));
        }, 0);
        return Math.max(100000, Math.ceil(maxDato / 50000) * 50000);
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const reportesApi = deps.reportesApi ?? win?.reportesApi;

        if (!doc) return null;

        function actualizarEncabezado(session) {
            const ahora = new Date();
            const kickerEl = doc.getElementById("dashboardFechaKicker");
            const saludoEl = doc.getElementById("dashboardSaludo");
            const subtituloEl = doc.getElementById("dashboardSubtitulo");

            const fechaTexto = formatearFechaKicker(ahora);
            if (kickerEl && fechaTexto) {
                kickerEl.textContent = fechaTexto;
            }
            if (saludoEl) {
                saludoEl.textContent = `${obtenerSaludoPorHora(ahora)}, ${obtenerPrimerNombre(session?.nombreCompleto)}`;
            }
            if (subtituloEl) {
                const sucursal = session?.sucursal || "Jardines del Valle";
                subtituloEl.textContent = `Este es el estado de la operación en ${sucursal}.`;
            }
        }

        function renderComparativa(el, pct) {
            if (!el) return;
            if (pct === null || pct === undefined || Number.isNaN(Number(pct))) {
                el.className = "text-muted small fw-medium d-flex align-items-center";
                el.innerHTML = '<i class="fa-solid fa-minus me-1" aria-hidden="true"></i> Sin comparativa previa';
                return;
            }

            const num = Number(pct);
            const esPositiva = num >= 0;
            const signo = num > 0 ? "+" : "";
            const icono = esPositiva ? "fa-arrow-trend-up" : "fa-arrow-trend-down";
            el.className = `${esPositiva ? "text-success" : "text-danger"} small fw-semibold d-flex align-items-center`;
            el.innerHTML = `<i class="fa-solid ${icono} me-1" aria-hidden="true"></i> ${signo}${num.toFixed(1)}% vs. mes anterior`;
        }

        function ocultarError() {
            const banner = doc.getElementById("dashboardErrorBanner");
            if (banner) {
                banner.classList.add("d-none");
                banner.classList.remove("d-flex");
            }
        }

        function mostrarError(mensaje) {
            const banner = doc.getElementById("dashboardErrorBanner");
            const textEl = doc.getElementById("dashboardErrorText");
            if (textEl) {
                textEl.textContent = mensaje || "No se pudo cargar la información del dashboard.";
            }
            if (banner) {
                banner.classList.remove("d-none");
                banner.classList.add("d-flex");
            }
        }

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

            renderComparativa(doc.getElementById("kpiVentasComparativa"), kpis.variacionVentasPct);
            renderComparativa(doc.getElementById("kpiCobranzaComparativa"), kpis.variacionCobranzaPct);

            const badgeCaja = doc.getElementById("sidebarCajaBadge");
            if (badgeCaja) {
                const vencidos = Number(kpis.pagosVencidosCount ?? 0);
                badgeCaja.textContent = String(vencidos);
                if (vencidos > 0) {
                    badgeCaja.classList.remove("d-none");
                } else {
                    badgeCaja.classList.add("d-none");
                }
            }

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
            if (donut) {
                if (totalDisp > 0) {
                    const p2 = (Number(disp.gavetas2 ?? 0) * 100) / totalDisp;
                    const p3 = p2 + (Number(disp.gavetas3 ?? 0) * 100) / totalDisp;
                    donut.style.background = `conic-gradient(#2D4224 0% ${p2.toFixed(2)}%, #C59D52 ${p2.toFixed(2)}% ${p3.toFixed(2)}%, #7D9B76 ${p3.toFixed(2)}% 100%)`;
                } else {
                    donut.style.background = "#E5E7EB";
                }
            }

            const maxVal = calcularEscalaGrafica(series);
            const gridContainer = doc.getElementById("yAxisGridLines");
            if (gridContainer) {
                const fracciones = [1, 0.8, 0.6, 0.4, 0.2, 0];
                gridContainer.innerHTML = fracciones.map(function (frac) {
                    const etiqueta = `$${Math.round((maxVal * frac) / 1000)}k`;
                    const borderClass = frac === 0 ? "border-secondary-subtle" : "border-light-subtle";
                    return `<div class="d-flex align-items-center text-muted" style="font-size: 0.7rem;">` +
                        `<span style="width: 48px;">${escapeHtml(etiqueta)}</span>` +
                        `<div class="flex-grow-1 border-bottom ${borderClass}"></div>` +
                        `</div>`;
                }).join("");
            }

            const barsContainer = doc.getElementById("barsVentasCobranza");
            if (barsContainer) {
                if (series.length === 0) {
                    barsContainer.innerHTML = '<div class="text-muted small align-self-center">Sin datos históricos disponibles para este rango.</div>';
                } else {
                    const colWidth = series.length > 6 ? "8%" : "14%";
                    const barWidth = series.length > 6 ? "10px" : "14px";
                    barsContainer.innerHTML = series.map(function (punto) {
                        const hv = Math.min(100, Math.round((Number(punto.ventas || 0) * 100) / maxVal));
                        const hc = Math.min(100, Math.round((Number(punto.cobranza || 0) * 100) / maxVal));
                        return `<div class="d-flex flex-column align-items-center h-100 justify-content-end" style="width: ${colWidth};">` +
                            `<div class="d-flex align-items-end justify-content-center gap-1 w-100" style="height: 200px;">` +
                            `<div class="sf-bar sf-bar-ventas rounded-top-2" style="height: ${hv}%; width: ${barWidth}; background-color: #2D4224; transition: height 0.3s ease;" title="${escapeHtml(punto.mes)} · Ventas: ${escapeHtml(formatearMonedaCorta(punto.ventas))}"></div>` +
                            `<div class="sf-bar sf-bar-cobranza rounded-top-2" style="height: ${hc}%; width: ${barWidth}; background-color: #C59D52; transition: height 0.3s ease;" title="${escapeHtml(punto.mes)} · Cobranza: ${escapeHtml(formatearMonedaCorta(punto.cobranza))}"></div>` +
                            `</div>` +
                            `<span class="small text-muted fw-semibold mt-2" style="font-size: 0.75rem;">${escapeHtml(punto.mes)}</span>` +
                            `</div>`;
                    }).join("");
                }
            }

            const tbody = doc.getElementById("tbodyActividadReciente");
            if (tbody) {
                if (actividad.length === 0) {
                    tbody.innerHTML = '<tr><td colspan="6" class="py-4 text-center text-muted small">' +
                        '<i class="fa-regular fa-folder-open me-2" aria-hidden="true"></i>Sin movimientos registrados todavía.' +
                        '</td></tr>';
                } else {
                    tbody.innerHTML = actividad.map(function (mov) {
                        const badge = String(mov.estado || "").toLowerCase() === "aplicado"
                            ? '<span class="badge bg-success-subtle text-success px-3 py-1 rounded-pill fw-medium"><i class="fa-solid fa-circle-check me-1 small" aria-hidden="true"></i> Aplicado</span>'
                            : `<span class="badge bg-warning-subtle text-warning-emphasis px-3 py-1 rounded-pill fw-medium"><i class="fa-solid fa-clock me-1 small" aria-hidden="true"></i> ${escapeHtml(mov.estado || "Firmado")}</span>`;

                        const folioDestino = mov.folioContrato || (String(mov.referencia || "").startsWith("CTR-") ? mov.referencia : null);
                        const referenciaHtml = folioDestino
                            ? `<a href="/Contratos/Detalle?folio=${encodeURIComponent(folioDestino)}" class="badge bg-light text-dark border px-2 py-1 fw-medium text-decoration-none" style="font-family: monospace;" title="Ver detalle del contrato ${escapeHtml(folioDestino)}">${escapeHtml(mov.referencia)} <i class="fa-solid fa-arrow-up-right-from-square ms-1 small text-muted" aria-hidden="true"></i></a>`
                            : `<span class="badge bg-light text-dark border px-2 py-1 fw-medium" style="font-family: monospace;">${escapeHtml(mov.referencia)}</span>`;

                        return `<tr>` +
                            `<td class="py-3"><div class="d-flex align-items-center gap-2">` +
                            `<div class="rounded-circle d-flex align-items-center justify-content-center text-white fw-bold shadow-sm" style="width: 36px; height: 36px; min-width: 36px; background-color: #485635; font-size: 0.8rem;">${escapeHtml(mov.iniciales || "SF")}</div>` +
                            `<div><div class="fw-semibold text-dark">${escapeHtml(mov.cliente)}</div><div class="text-muted small" style="font-size: 0.75rem;">${escapeHtml(mov.email)}</div></div>` +
                            `</div></td>` +
                            `<td class="py-3 text-secondary">${escapeHtml(mov.movimiento)}</td>` +
                            `<td class="py-3">${referenciaHtml}</td>` +
                            `<td class="py-3 sf-money fw-bold text-dark">${escapeHtml(formatearMonedaExacta(mov.importe))}</td>` +
                            `<td class="py-3">${badge}</td>` +
                            `<td class="py-3 text-muted text-end small">${escapeHtml(mov.fechaRelativa)}</td>` +
                            `</tr>`;
                    }).join("");
                }
            }
        }

        async function cargar(rango = "6m") {
            if (!reportesApi) return;

            let session = null;
            if (authApi && typeof authApi.requireAuth === "function") {
                session = authApi.requireAuth({ client: apiClient, storage: deps.storage });
                if (!session) {
                    return;
                }
                actualizarEncabezado(session);
            }

            ocultarError();

            try {
                const data = await reportesApi.obtenerResumenDashboard({ rango });
                poblar(data);
            } catch (err) {
                mostrarError(err?.message || "No se pudo cargar la información del dashboard.");
            }
        }

        const selectRango = doc.getElementById("selectRangoGrafica");
        if (selectRango) {
            selectRango.addEventListener("change", function () {
                const rango = selectRango.value === "anio" ? "anio" : "6m";
                const subtituloGrafica = doc.getElementById("subtituloGrafica");
                if (subtituloGrafica) {
                    subtituloGrafica.textContent = rango === "anio"
                        ? "Comportamiento del año en curso."
                        : "Comportamiento de los últimos 6 meses.";
                }
                cargar(rango);
            });
        }

        const btnReintentar = doc.getElementById("btnReintentarDashboard");
        if (btnReintentar) {
            btnReintentar.addEventListener("click", function () {
                const rango = selectRango && selectRango.value === "anio" ? "anio" : "6m";
                cargar(rango);
            });
        }

        const rangoInicial = selectRango && selectRango.value === "anio" ? "anio" : "6m";
        cargar(rangoInicial);
        return { cargar, poblar, actualizarEncabezado };
    }

    return { formatearMonedaCorta, formatearMonedaExacta, calcularEscalaGrafica, init };
});
