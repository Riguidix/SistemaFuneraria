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

    function puedeCobrarRol(rol) {
        if (!rol) return true;
        return rol === "Administrador" || rol === "Cajero";
    }

    function renderizarFilaContrato(item, opciones = {}) {
        const folio = item.folioContrato || "-";
        const cliente = item.clienteNombre || "-";
        const lote = item.loteClave || "-";
        const saldo = formatearMonedaMXN(item.saldoInsoluto);
        const proximo = item.proximoPagoTexto || "-";
        const badge = obtenerBadgeEstadoCartera(item.estado);
        const esCobrar = String(item.accionSugerida || "").toLowerCase() === "cobrar"
            || String(item.estado || "").toLowerCase() === "vencido";
        const puedeCobrar = opciones.puedeCobrar !== undefined
            ? Boolean(opciones.puedeCobrar)
            : puedeCobrarRol(opciones.rol);

        const detalleUrl = `/Contratos/Detalle?folio=${encodeURIComponent(folio)}`;
        const folioHtml = folio !== "-"
            ? `<a href="${detalleUrl}" class="fw-semibold text-dark text-decoration-none" title="Ver expediente ${escapeHtml(folio)}">${escapeHtml(folio)}</a>`
            : `<span class="fw-semibold text-dark">-</span>`;

        const detalleLinkHtml = `<a href="${detalleUrl}" class="text-decoration-none fw-semibold small" style="color: var(--sf-primary-olive, #2D4224);">Detalle <i class="fa-solid fa-chevron-right ms-1" aria-hidden="true"></i></a>`;
        const accionHtml = (esCobrar && puedeCobrar)
            ? `<div class="d-inline-flex align-items-center justify-content-end gap-2">` +
              `<a href="/Caja?contrato=${encodeURIComponent(folio)}" class="btn btn-sm btn-outline-danger rounded-3 px-3 py-1 fw-medium shadow-sm"><i class="fa-solid fa-receipt me-1" aria-hidden="true"></i> Cobrar</a>` +
              detalleLinkHtml +
              `</div>`
            : detalleLinkHtml;

        return `<tr class="fila-contrato">` +
            `<td class="ps-4">${folioHtml}</td>` +
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

        const hasExplicitStorage = deps.storage !== undefined;
        const storageOpts = apiClient
            ? (hasExplicitStorage ? { client: apiClient, storage: deps.storage } : { client: apiClient })
            : {};

        if (authApi && typeof authApi.applyRolePermissions === "function") {
            authApi.applyRolePermissions(doc, storageOpts);
        }

        const tbody = doc.getElementById("tbodyContratos");
        const emptyState = doc.getElementById("emptyStateContratos");
        const errorState = doc.getElementById("alertaErrorContratos");
        const errorMsgEl = doc.getElementById("mensajeErrorContratos");
        const btnReintentar = doc.getElementById("btnReintentarContratos");
        const inputBuscar = doc.getElementById("inputBuscarContratos");
        const tabs = Array.from(doc.querySelectorAll(".btn-tab-filtro"));

        const paginacionEl = doc.getElementById("paginacionContratos");
        const conteoPagEl = doc.getElementById("conteoPaginacionContratos");
        const indicadorPagEl = doc.getElementById("indicadorPaginaContratos");
        const btnAnterior = doc.getElementById("btnPaginaAnteriorContratos");
        const btnSiguiente = doc.getElementById("btnPaginaSiguienteContratos");

        const elTotalActivos = doc.getElementById("kpiTotalActivos");
        const elMontoCartera = doc.getElementById("kpiMontoCartera");
        const elTotalAlCorriente = doc.getElementById("kpiTotalAlCorriente");
        const elPorcentajeAlCorriente = doc.getElementById("kpiPorcentajeAlCorriente");
        const elTotalConAtraso = doc.getElementById("kpiTotalConAtraso");
        const elMontoPorRecuperar = doc.getElementById("kpiMontoPorRecuperar");
        const elTotalLiquidados = doc.getElementById("kpiTotalLiquidados");

        const params = new URLSearchParams(win?.location?.search || "");
        let tabActual = normalizarTab(params.get("tab") || "Todos");
        const buscarQuery = (params.get("buscar") || "").trim();
        if (inputBuscar && buscarQuery) {
            inputBuscar.value = buscarQuery;
        }

        let paginaActual = 1;
        const tamanoPagina = 10;
        let totalRegistros = 0;

        function verificarAcceso() {
            if (!authApi || !apiClient) {
                return { autorizado: true, session: null };
            }

            const token = typeof authApi.getToken === "function"
                ? authApi.getToken(storageOpts)
                : (deps.storage ? deps.storage.getItem(apiClient.TOKEN_STORAGE_KEY) : null);
            const session = typeof authApi.getSession === "function"
                ? authApi.getSession(storageOpts)
                : null;
            const expirado = typeof authApi.isTokenExpired === "function" && token
                ? authApi.isTokenExpired(token)
                : false;

            if (!token || !session || expirado) {
                if (typeof authApi.logout === "function") {
                    authApi.logout({ ...storageOpts, skipRedirect: true });
                }
                let destino = "/Cuenta/Login";
                const path = win?.location?.pathname || "";
                if (path && path !== "/" && !path.toLowerCase().startsWith("/cuenta/")) {
                    destino = `/Cuenta/Login?returnUrl=${encodeURIComponent(path + (win?.location?.search || ""))}`;
                }
                win?.location?.assign?.(destino);
                return { autorizado: false, session: null };
            }

            return { autorizado: true, session };
        }

        function marcarTabVisual(tabNombre) {
            const norm = tabNombre.toLowerCase();
            tabs.forEach(function (btn) {
                const btnTab = (btn.getAttribute("data-filtro-estado") || "").toLowerCase();
                const esActivo = btnTab === norm;
                btn.setAttribute("aria-pressed", esActivo ? "true" : "false");
                if (esActivo) {
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

        function actualizarPaginacion(total, pagina, tamano) {
            const totalSeg = Math.max(0, Number(total) || 0);
            const tamSeg = Math.max(1, Number(tamano) || tamanoPagina);
            const totalPaginas = Math.max(1, Math.ceil(totalSeg / tamSeg));
            const pagSeg = Math.min(Math.max(1, Number(pagina) || 1), totalPaginas);

            if (paginacionEl) {
                paginacionEl.classList.toggle("d-none", totalSeg === 0);
            }

            if (totalSeg > 0) {
                const desde = (pagSeg - 1) * tamSeg + 1;
                const hasta = Math.min(pagSeg * tamSeg, totalSeg);
                if (conteoPagEl) {
                    conteoPagEl.textContent = `Mostrando ${desde}–${hasta} de ${totalSeg} contratos`;
                }
                if (indicadorPagEl) {
                    indicadorPagEl.textContent = `Página ${pagSeg} de ${totalPaginas}`;
                }
            }

            if (btnAnterior) btnAnterior.disabled = pagSeg <= 1;
            if (btnSiguiente) btnSiguiente.disabled = pagSeg >= totalPaginas;
        }

        async function cargarCartera() {
            if (!contratosApi) return;

            const acceso = verificarAcceso();
            if (!acceso.autorizado) return;

            const rolActual = acceso.session?.rol || "";
            const puedeCobrar = puedeCobrarRol(rolActual);

            if (errorState) errorState.classList.add("d-none");

            try {
                const res = await contratosApi.obtenerCartera({
                    buscar: inputBuscar ? inputBuscar.value.trim() : "",
                    tab: tabActual,
                    page: paginaActual,
                    pageSize: tamanoPagina
                });

                actualizarKpis(res?.kpis);
                const items = Array.isArray(res?.items) ? res.items : [];
                totalRegistros = Number(res?.totalRegistros ?? items.length) || 0;
                paginaActual = Number(res?.pagina ?? paginaActual) || 1;
                const tamActual = Number(res?.tamanoPagina ?? tamanoPagina) || tamanoPagina;

                if (tbody) {
                    tbody.innerHTML = items.map(function (item) {
                        return renderizarFilaContrato(item, { puedeCobrar });
                    }).join("");
                }
                if (emptyState) {
                    emptyState.classList.toggle("d-none", items.length > 0);
                }
                actualizarPaginacion(totalRegistros, paginaActual, tamActual);
            } catch (err) {
                if (tbody) tbody.innerHTML = "";
                if (emptyState) emptyState.classList.add("d-none");
                if (paginacionEl) paginacionEl.classList.add("d-none");

                const mensaje = err?.message || "No fue posible cargar la cartera de contratos.";
                if (errorState) {
                    if (errorMsgEl) {
                        errorMsgEl.textContent = mensaje;
                    } else {
                        errorState.textContent = mensaje;
                    }
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
                debounceTimer = setTimeout(function () {
                    paginaActual = 1;
                    cargarCartera();
                }, 220);
            });
        }

        tabs.forEach(function (btn) {
            btn.addEventListener("click", function () {
                tabActual = normalizarTab(btn.getAttribute("data-filtro-estado"));
                paginaActual = 1;
                marcarTabVisual(tabActual);
                cargarCartera();
            });
        });

        if (btnAnterior) {
            btnAnterior.addEventListener("click", function () {
                if (paginaActual > 1) {
                    paginaActual -= 1;
                    cargarCartera();
                }
            });
        }

        if (btnSiguiente) {
            btnSiguiente.addEventListener("click", function () {
                const totalPaginas = Math.max(1, Math.ceil(totalRegistros / tamanoPagina));
                if (paginaActual < totalPaginas) {
                    paginaActual += 1;
                    cargarCartera();
                }
            });
        }

        if (btnReintentar) {
            btnReintentar.addEventListener("click", function () {
                cargarCartera();
            });
        }

        marcarTabVisual(tabActual);
        cargarCartera();

        return {
            cargarCartera,
            actualizarKpis,
            actualizarPaginacion
        };
    }

    return {
        formatearMonedaMXN,
        obtenerBadgeEstadoCartera,
        puedeCobrarRol,
        renderizarFilaContrato,
        normalizarTab,
        init
    };
});
