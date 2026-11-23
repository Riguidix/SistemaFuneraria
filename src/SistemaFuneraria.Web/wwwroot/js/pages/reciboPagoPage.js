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

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const cobranzaApi = deps.cobranzaApi ?? win?.cobranzaApi;

        if (!doc) return null;

        const principal = doc.getElementById("reciboContenidoPrincipal");
        const noEncontrado = doc.getElementById("reciboNoEncontrado");

        const params = new URLSearchParams(win?.location?.search || "");
        const folio = (params.get("folio") || "").trim();

        function poblar(dto) {
            if (!dto) return;
            const set = function (id, val) {
                const el = doc.getElementById(id);
                if (el) el.textContent = val;
            };
            set("reciboFolioPago", dto.folioPago || "-");
            set("reciboFechaPago", formatearFechaHora(dto.fechaPago));
            set("reciboFolioContrato", dto.folioContrato || "-");
            set("reciboClienteNombre", dto.clienteNombre || "-");
            set("reciboLoteClave", dto.loteClave || "-");
            set("reciboConcepto", dto.concepto || "-");
            set("reciboMetodoPago", dto.metodoPago || "-");
            set("reciboReferencia", dto.referencia || "N/A");
            set("reciboSaldoAnterior", formatearMonedaMXN(dto.saldoAnterior));
            set("reciboMontoCapital", formatearMonedaMXN(dto.montoCapital));
            set("reciboMontoInteres", formatearMonedaMXN(dto.montoInteres));
            set("reciboMontoTotal", formatearMonedaMXN(dto.montoTotal));
            set("reciboSaldoPosterior", formatearMonedaMXN(dto.saldoPosterior));
        }

        async function cargar() {
            if (!folio || !cobranzaApi) return;

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
                const dto = await cobranzaApi.obtenerRecibo(folio);
                poblar(dto);
            } catch (err) {
                if (err?.status === 404) {
                    if (principal) principal.classList.add("d-none");
                    if (noEncontrado) noEncontrado.classList.remove("d-none");
                }
            }
        }

        cargar();
        return { cargar, poblar };
    }

    return {
        formatearMonedaMXN,
        init
    };
});
