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

    const ROLES_PERMITIDOS = ["Administrador"];

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

    function escaparCeldaCsv(valor) {
        const str = String(valor ?? "");
        if (/[",\r\n]/.test(str)) {
            return `"${str.replace(/"/g, '""')}"`;
        }
        return str;
    }

    function construirCsvReporte(reporte) {
        if (!reporte) return "";
        const filas = [
            ["Seccion", "Concepto", "Detalle", "Valor"],
            ["Resumen", "Periodo", "", reporte.periodo || ""],
            ["Resumen", "Sucursal", "", reporte.sucursal || ""],
            ["KPI", "Conversion de cotizacion", "%", `${Number(reporte.conversionCotizacion ?? 0).toFixed(1)}%`],
            ["KPI", "Ticket promedio", "MXN", Number(reporte.ticketPromedio ?? 0).toFixed(2)],
            ["KPI", "Indice de morosidad", "%", `${Number(reporte.indiceMorosidad ?? 0).toFixed(1)}%`]
        ];

        const tipos = Array.isArray(reporte.ventasPorTipoLote) ? reporte.ventasPorTipoLote : [];
        for (const t of tipos) {
            filas.push([
                "Ventas por tipo",
                t.tipoLote || "",
                `${Number(t.porcentaje ?? 0).toFixed(1)}% (${Number(t.cantidadVentas ?? 0)} ventas)`,
                Number(t.montoTotal ?? 0).toFixed(2)
            ]);
        }

        const meta = reporte.metaMensual || {};
        filas.push(["Meta mensual", "Acumulado", `${Number(meta.porcentajeAvance ?? 0).toFixed(1)}%`, Number(meta.acumulado ?? 0).toFixed(2)]);
        filas.push(["Meta mensual", "Objetivo mensual", "", Number(meta.objetivoMensual ?? 0).toFixed(2)]);

        return "\uFEFF" + filas.map((f) => f.map(escaparCeldaCsv).join(",")).join("\n");
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const reportesApi = deps.reportesApi ?? win?.reportesApi;

        if (!doc) return null;

        const hasExplicitStorage = deps.storage !== undefined;
        const storageOpts = apiClient
            ? (hasExplicitStorage ? { client: apiClient, storage: deps.storage } : { client: apiClient })
            : {};

        const principal = doc.getElementById("reportesContenidoPrincipal");
        const accesoRestringido = doc.getElementById("reportesAccesoRestringido");
        const btnVolver403 = doc.getElementById("btnVolver403Reportes");
        const alertaError = doc.getElementById("alertaErrorReportes");
        const mensajeError = doc.getElementById("mensajeErrorReportes");
        const btnReintentar = doc.getElementById("btnReintentarReportes");

        const selectPeriodo = doc.getElementById("selectPeriodo");
        const selectSucursal = doc.getElementById("selectSucursal");
        const btnDescargar = doc.getElementById("btnDescargarReporte");
        let ultimoReporte = null;

        function verificarAcceso() {
            if (!authApi || !apiClient) {
                return { autorizado: true, redirigido: false, session: null };
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
                return { autorizado: false, redirigido: true, session: null };
            }

            if (session.rol && !ROLES_PERMITIDOS.includes(session.rol)) {
                if (btnVolver403) {
                    const rutaInicio = typeof authApi.getDefaultRouteForRole === "function"
                        ? authApi.getDefaultRouteForRole(session.rol)
                        : "/Resumen";
                    btnVolver403.setAttribute("href", rutaInicio);
                }
                if (principal) principal.classList.add("d-none");
                if (alertaError) alertaError.classList.add("d-none");
                if (accesoRestringido) accesoRestringido.classList.remove("d-none");
                return { autorizado: false, redirigido: false, session };
            }

            return { autorizado: true, redirigido: false, session };
        }

        function mostrarError(msg) {
            const texto = String(msg || "No fue posible cargar los indicadores de reportes.").replace(/^\[(CONFLICTO|NO_ENCONTRADO)\]\s*/i, "");
            if (alertaError) {
                if (mensajeError) {
                    mensajeError.textContent = texto;
                } else {
                    alertaError.textContent = texto;
                }
                alertaError.classList.remove("d-none");
            }
        }

        function ocultarError() {
            if (alertaError) alertaError.classList.add("d-none");
        }

        function poblar(data) {
            if (!data) return;
            ultimoReporte = data;
            ocultarError();
            if (btnDescargar) btnDescargar.disabled = false;

            const set = function (id, val) {
                const el = doc.getElementById(id);
                if (el) el.textContent = val;
            };

            const morosidad = Number(data.indiceMorosidad ?? 0);
            set("kpiConversion", `${Number(data.conversionCotizacion ?? 0).toFixed(1)}%`);
            set("kpiTicketPromedio", formatearMonedaCorta(data.ticketPromedio));
            set("kpiIndiceMorosidad", `${morosidad.toFixed(1)}%`);

            const subMor = doc.getElementById("subtituloMorosidad");
            if (subMor) {
                const dentroMeta = morosidad <= 8.0;
                subMor.className = `${dentroMeta ? "text-success" : "text-danger"} small fw-semibold d-flex align-items-center`;
                subMor.innerHTML = dentroMeta
                    ? '<i class="fa-solid fa-circle-check me-1" aria-hidden="true"></i> Meta menor a 8.0%'
                    : '<i class="fa-solid fa-triangle-exclamation me-1" aria-hidden="true"></i> Por encima de meta (8.0%)';
            }

            const subMeta = doc.getElementById("subtituloMetaMensual");
            if (subMeta) {
                subMeta.textContent = `Avance comercial · ${data.periodo || "Periodo actual"}`;
            }

            const tipos = Array.isArray(data.ventasPorTipoLote) ? data.ventasPorTipoLote : [];
            const totalIngresos = tipos.reduce((acc, x) => acc + Number(x.montoTotal || 0), 0);
            const contenedorTipos = doc.getElementById("contenedorVentasTipoLote");
            if (contenedorTipos) {
                if (tipos.length === 0 || totalIngresos <= 0) {
                    contenedorTipos.innerHTML = '<div class="text-center py-4 text-muted small">Sin ventas registradas en este periodo.</div>';
                } else {
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
            }

            set("totalIngresosReportes", formatearMonedaCorta(totalIngresos));

            const meta = data.metaMensual || {};
            const pctMeta = Math.min(100, Math.max(0, Number(meta.porcentajeAvance ?? 0)));
            set("kpiMetaPorcentaje", `${pctMeta.toFixed(0)}%`);
            set("kpiMetaAcumulado", formatearMonedaCorta(meta.acumulado));
            set("kpiMetaObjetivo", formatearMonedaCorta(meta.objetivoMensual));

            const donutMeta = doc.getElementById("donutMetaMensual");
            if (donutMeta && donutMeta.style) {
                donutMeta.style.background = `conic-gradient(#2D4224 0% ${pctMeta}%, #E5E0D8 ${pctMeta}% 100%)`;
            }
        }

        async function cargar() {
            const acc = verificarAcceso();
            if (!acc.autorizado) return;
            if (!reportesApi) return;

            ocultarError();

            try {
                const data = await reportesApi.obtenerIndicadores({
                    periodo: selectPeriodo?.value || "mes-actual",
                    sucursal: selectSucursal?.value || "Jardines del Valle"
                });
                poblar(data);
            } catch (err) {
                ultimoReporte = null;
                if (btnDescargar) btnDescargar.disabled = true;
                const contenedorTipos = doc.getElementById("contenedorVentasTipoLote");
                if (contenedorTipos && doc.getElementById("estadoCargaReportes")) {
                    contenedorTipos.innerHTML = "";
                }
                mostrarError(err?.message || "No fue posible cargar los indicadores de reportes.");
            }
        }

        const accInicial = verificarAcceso();
        if (!accInicial.autorizado) {
            return { autorizado: false, redirigido: accInicial.redirigido, session: accInicial.session };
        }

        if (selectPeriodo) selectPeriodo.addEventListener("change", cargar);
        if (selectSucursal) selectSucursal.addEventListener("change", cargar);
        if (btnReintentar) btnReintentar.addEventListener("click", cargar);

        if (btnDescargar) {
            btnDescargar.addEventListener("click", function () {
                if (!ultimoReporte || typeof Blob === "undefined" || !win?.URL?.createObjectURL) return;
                const csv = construirCsvReporte(ultimoReporte);
                const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
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

    return { formatearMonedaCorta, escaparCeldaCsv, construirCsvReporte, init };
});
