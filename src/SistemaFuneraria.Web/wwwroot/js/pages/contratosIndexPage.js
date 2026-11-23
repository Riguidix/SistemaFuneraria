/**
 * contratosIndexPage.js - Conector real para /Contratos/Index (SF-070).
 * Conecta 4 KPI cards, búsqueda, tabs Todos/Activos/Vencidos/Liquidados y acción Cobrar -> /Caja.
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.contratosIndexPage = api;
        if (typeof document !== "undefined") {
            document.addEventListener("DOMContentLoaded", function () {
                api.init();
            });
        }
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    function formatearMonedaMXN(monto) {
        const num = Number(monto);
        const seguro = Number.isFinite(num) ? num : 0;
        return "$" + seguro.toLocaleString("es-MX", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function obtenerBadgeEstadoCartera(estado) {
        const norm = String(estado || "").trim().toLowerCase();
        if (norm === "vencido") {
            return {
                texto: "Vencido",
                clase: "badge bg-danger-subtle text-danger fw-semibold px-2 py-1 rounded-pill"
            };
        }
        if (norm === "en revisión" || norm === "enrevision") {
            return {
                texto: "En revisión",
                clase: "badge bg-warning-subtle text-warning-emphasis fw-semibold px-2 py-1 rounded-pill"
            };
        }
        if (norm === "cancelado") {
            return {
                texto: "Cancelado",
                clase: "badge bg-secondary-subtle text-secondary fw-semibold px-2 py-1 rounded-pill"
            };
        }
        if (norm === "liquidado") {
            return {
                texto: "Liquidado",
                clase: "badge bg-success-subtle text-success fw-semibold px-2 py-1 rounded-pill"
            };
        }
        return {
            texto: "Al corriente",
            clase: "badge bg-success-subtle text-success fw-semibold px-2 py-1 rounded-pill"
        };
    }

    function escapeHtml(text) {
        return String(text ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function renderizarFilaContrato(item) {
        const folio = item.folioContrato || "-";
        const cliente = item.clienteNombre || "-";
        const lote = item.loteClave || "-";
        const saldo = formatearMonedaMXN(item.saldoInsoluto);
        const proximo = item.proximoPagoTexto || "-";
        const badge = obtenerBadgeEstadoCartera(item.estado);
        const esCobrar = String(item.accionSugerida || "").toLowerCase() === "cobrar"
            || String(item.estado || "").toLowerCase() === "vencido";

        const accionHtml = esCobrar
            ? `<a href="/Caja?contrato=${encodeURIComponent(folio)}" class="btn btn-sm btn-outline-danger rounded-3 px-3 py-1 fw-medium shadow-sm"><i class="fa-solid fa-receipt me-1" aria-hidden="true"></i> Cobrar</a>`
            : `<a href="/Contratos/Detalle?folio=${encodeURIComponent(folio)}" class="text-decoration-none fw-semibold small" style="color: var(--sf-primary-olive, #2D4224);">Detalle <i class="fa-solid fa-chevron-right ms-1" aria-hidden="true"></i></a>`;

        return `<tr class="fila-contrato">` +
            `<td class="ps-4"><span class="fw-semibold text-dark">${escapeHtml(folio)}</span></td>` +
            `<td><div class="fw-bold text-dark">${escapeHtml(cliente)}</div><div class="text-muted small">Titular principal</div></td>` +
            `<td><span class="badge bg-light text-dark border px-2 py-1">${escapeHtml(lote)}</span></td>` +
            `<td class="text-end"><span class="sf-money fw-bold text-dark">${escapeHtml(saldo)}</span></td>` +
            `<td><span class="text-dark small">${escapeHtml(proximo)}</span></td>` +
            `<td><span class="${badge.clase}">${escapeHtml(badge.texto)}</span></td>` +
            `<td class="text-end pe-4">${accionHtml}</td>` +
            `</tr>`;
    }

    function normalizarTab(tab) {
        const norm = String(tab || "todos").trim().toLowerCase();
        if (norm === "activos") return "Activos";
        if (norm === "vencidos") return "Vencidos";
        if (norm === "liquidados") return "Liquidados";
        return "Todos";
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const contratosApi = deps.contratosApi ?? win?.contratosApi;
        const sfUi = deps.SfUi ?? win?.SfUi;

        if (!doc) return null;

        const tbody = doc.getElementById("tbodyContratos");
        const emptyState = doc.getElementById("emptyStateContratos");
        const errorState = doc.getElementById("alertaErrorContratos");
        const inputBuscar = doc.getElementById("inputBuscarContratos");
        const tabs = Array.from(doc.querySelectorAll(".btn-tab-filtro"));

        const elTotalActivos = doc.getElementById("kpiTotalActivos");
        const elMontoCartera = doc.getElementById("kpiMontoCartera");
        const elTotalAlCorriente = doc.getElementById("kpiTotalAlCorriente");
        const elPorcentajeAlCorriente = doc.getElementById("kpiPorcentajeAlCorriente");
        const elTotalConAtraso = doc.getElementById("kpiTotalConAtraso");
        const elMontoPorRecuperar = doc.getElementById("kpiMontoPorRecuperar");
        const elTotalLiquidados = doc.getElementById("kpiTotalLiquidados");

        const params = new URLSearchParams(win?.location?.search || "");
        let tabActual = normalizarTab(params.get("tab") || "Todos");

        function marcarTabVisual(tabNombre) {
            const norm = tabNombre.toLowerCase();
            tabs.forEach(function (btn) {
                const btnTab = (btn.getAttribute("data-filtro-estado") || "").toLowerCase();
                if (btnTab === norm) {
                    btn.classList.add("active");
                    btn.classList.remove("text-muted");
                } else {
                    btn.classList.remove("active");
                    btn.classList.add("text-muted");
                }
            });
        }

        function actualizarKpis(kpis) {
            if (!kpis) return;
            if (elTotalActivos) elTotalActivos.textContent = String(kpis.totalActivos ?? 0);
            if (elMontoCartera) elMontoCartera.textContent = `${formatearMonedaMXN(kpis.montoEnCartera ?? 0)} en cartera`;
            if (elTotalAlCorriente) elTotalAlCorriente.textContent = String(kpis.totalAlCorriente ?? 0);
            if (elPorcentajeAlCorriente) elPorcentajeAlCorriente.textContent = `${Number(kpis.porcentajeAlCorriente ?? 0).toFixed(1)}%`;
            if (elTotalConAtraso) elTotalConAtraso.textContent = String(kpis.totalConAtraso ?? 0);
            if (elMontoPorRecuperar) elMontoPorRecuperar.textContent = `${formatearMonedaMXN(kpis.montoPorRecuperar ?? 0)} por recuperar`;
            if (elTotalLiquidados) elTotalLiquidados.textContent = String(kpis.totalLiquidadosAnio ?? 0);
        }

        async function cargarCartera() {
            if (!contratosApi) return;

            if (authApi && apiClient) {
                const storage = deps.storage ?? (typeof sessionStorage !== "undefined" ? sessionStorage : null);
                const token = storage ? storage.getItem(apiClient.TOKEN_STORAGE_KEY) : null;
                const session = authApi.getSession({ client: apiClient, storage });
                if (!token || !session) {
                    win?.location?.assign?.("/Cuenta/Login");
                    return;
                }
            }

            if (errorState) errorState.classList.add("d-none");

            try {
                const res = await contratosApi.obtenerCartera({
                    buscar: inputBuscar ? inputBuscar.value.trim() : "",
                    tab: tabActual,
                    page: 1,
                    pageSize: 50
                });

                actualizarKpis(res?.kpis);
                const items = Array.isArray(res?.items) ? res.items : [];
                if (tbody) {
                    tbody.innerHTML = items.map(renderizarFilaContrato).join("");
                }
                if (emptyState) {
                    emptyState.classList.toggle("d-none", items.length > 0);
                }
            } catch (err) {
                if (errorState) {
                    errorState.textContent = err?.message || "No fue posible cargar la cartera de contratos.";
                    errorState.classList.remove("d-none");
                } else if (sfUi && typeof sfUi.showToast === "function") {
                    sfUi.showToast({
                        type: "error",
                        title: "Error de cartera",
                        message: "No fue posible consultar los contratos."
                    });
                }
            }
        }

        let debounceTimer = null;
        if (inputBuscar) {
            inputBuscar.addEventListener("input", function () {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(cargarCartera, 220);
            });
        }

        tabs.forEach(function (btn) {
            btn.addEventListener("click", function () {
                tabActual = normalizarTab(btn.getAttribute("data-filtro-estado"));
                marcarTabVisual(tabActual);
                cargarCartera();
            });
        });

        marcarTabVisual(tabActual);
        cargarCartera();

        return {
            cargarCartera,
            actualizarKpis
        };
    }

    return {
        formatearMonedaMXN,
        obtenerBadgeEstadoCartera,
        renderizarFilaContrato,
        init
    };
});
