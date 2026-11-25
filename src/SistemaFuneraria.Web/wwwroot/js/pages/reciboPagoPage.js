/**
 * reciboPagoPage.js - Conector real de /Caja/Recibo?folio=PAG-YYYY-XXXXXX (SF-082).
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.reciboPagoPage = api;
        if (typeof document !== "undefined") {
            document.addEventListener("DOMContentLoaded", function () {
                api.init();
            });
        }
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    const ROLES_PERMITIDOS = ["Administrador", "Cajero"];

    function formatearMonedaMXN(monto) {
        const num = Number(monto);
        const seguro = Number.isFinite(num) ? num : 0;
        return "$" + seguro.toLocaleString("es-MX", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function formatearFechaHora(fechaIso) {
        if (!fechaIso) return "-";
        const fecha = new Date(fechaIso);
        if (Number.isNaN(fecha.getTime())) return String(fechaIso);
        return new Intl.DateTimeFormat("es-MX", {
            day: "2-digit",
            month: "short",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        }).format(fecha);
    }

    function formatearMetodoPago(metodo) {
        const raw = String(metodo || "").trim();
        const norm = raw.toLowerCase().replace(/\s+/g, "");
        if (norm === "transferenciabancaria" || norm === "transferencia") {
            return "Transferencia bancaria";
        }
        if (norm === "tarjeta" || norm === "tarjetacredito" || norm === "tarjetadebito") {
            return "Tarjeta de débito / crédito";
        }
        if (norm === "efectivo") {
            return "Efectivo";
        }
        return raw || "-";
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const cobranzaApi = deps.cobranzaApi ?? win?.cobranzaApi;

        if (!doc) return null;

        const hasExplicitStorage = deps.storage !== undefined;
        const storageOpts = apiClient
            ? (hasExplicitStorage ? { client: apiClient, storage: deps.storage } : { client: apiClient })
            : {};

        const principal = doc.getElementById("reciboContenidoPrincipal");
        const noEncontrado = doc.getElementById("reciboNoEncontrado");
        const accesoRestringido = doc.getElementById("reciboAccesoRestringido");
        const btnVolver403 = doc.getElementById("btnVolver403Recibo");
        const alertaError = doc.getElementById("alertaErrorRecibo");
        const mensajeError = doc.getElementById("mensajeErrorRecibo");
        const btnReintentar = doc.getElementById("btnReintentarRecibo");
        const lnkContrato = doc.getElementById("lnkContratoDesdeRecibo");

        const params = new URLSearchParams(win?.location?.search || "");
        const folio = (params.get("folio") || "").trim();

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
                if (noEncontrado) noEncontrado.classList.add("d-none");
                if (alertaError) alertaError.classList.add("d-none");
                if (accesoRestringido) accesoRestringido.classList.remove("d-none");
                return { autorizado: false, redirigido: false, session };
            }

            return { autorizado: true, redirigido: false, session };
        }

        function mostrarError(msg) {
            const texto = String(msg || "No fue posible consultar el recibo oficial.").replace(/^\[(CONFLICTO|NO_ENCONTRADO)\]\s*/i, "");
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

        function mostrarNoEncontrado() {
            ocultarError();
            if (principal) principal.classList.add("d-none");
            if (noEncontrado) noEncontrado.classList.remove("d-none");
        }

        function poblar(dto) {
            if (!dto) return;
            ocultarError();
            if (noEncontrado) noEncontrado.classList.add("d-none");
            if (principal) principal.classList.remove("d-none");

            const set = function (id, val) {
                const el = doc.getElementById(id);
                if (el) el.textContent = val;
            };
            set("reciboFolioPago", dto.folioPago || "-");
            set("reciboFechaPago", formatearFechaHora(dto.fechaPago));
            set("reciboFolioContrato", dto.folioContrato || "-");
            if (lnkContrato && dto.folioContrato) {
                lnkContrato.setAttribute("href", `/Contratos/Detalle?folio=${encodeURIComponent(dto.folioContrato)}`);
            }
            set("reciboClienteNombre", dto.clienteNombre || "-");
            set("reciboLoteClave", dto.loteClave || "-");
            set("reciboConcepto", dto.concepto || "-");
            set("reciboMetodoPago", formatearMetodoPago(dto.metodoPago));
            set("reciboReferencia", dto.referencia || "N/A");
            set("reciboSaldoAnterior", formatearMonedaMXN(dto.saldoAnterior));
            set("reciboMontoCapital", formatearMonedaMXN(dto.montoCapital));
            set("reciboMontoInteres", formatearMonedaMXN(dto.montoInteres));
            set("reciboMontoTotal", formatearMonedaMXN(dto.montoTotal));
            set("reciboSaldoPosterior", formatearMonedaMXN(dto.saldoPosterior));
        }

        async function cargar() {
            const acc = verificarAcceso();
            if (!acc.autorizado) return;

            if (!folio) {
                mostrarNoEncontrado();
                return;
            }

            if (!cobranzaApi) return;

            ocultarError();

            try {
                const dto = await cobranzaApi.obtenerRecibo(folio);
                poblar(dto);
            } catch (err) {
                if (err?.status === 404) {
                    mostrarNoEncontrado();
                } else {
                    mostrarError(err?.message || "No fue posible consultar el recibo oficial.");
                }
            }
        }

        if (btnReintentar) {
            btnReintentar.addEventListener("click", function () {
                cargar();
            });
        }

        cargar();
        return { cargar, poblar };
    }

    return {
        formatearMonedaMXN,
        formatearMetodoPago,
        init
    };
});
