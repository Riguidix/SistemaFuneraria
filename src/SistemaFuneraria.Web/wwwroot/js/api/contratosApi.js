/**
 * Opciones inyectables para {@link contratosApi}.
 * @typedef {Object} ContratosApiOptions
 * @property {import("./apiClient")} [client] Cliente HTTP alternativo (por defecto `apiClient`).
 * @property {Record<string, string|number|boolean|null|undefined>} [params] Parámetros de query string.
 * @property {Record<string, string>} [headers] Cabeceras adicionales.
 * @property {string} [token] Token JWT explícito.
 */

const CONTRATOS_ENDPOINT = "/api/v1/contratos";
const VENTAS_ADMIN_ENDPOINT = "/api/v1/ventas";

/**
 * Resuelve la instancia de `apiClient` compatible con navegador y Node.
 * @param {ContratosApiOptions} [opts]
 * @returns {import("./apiClient")}
 */
function resolveContratosClient(opts) {
    if (opts && opts.client) {
        return opts.client;
    }
    if (typeof window !== "undefined" && window.apiClient) {
        return window.apiClient;
    }
    if (typeof require === "function") {
        return require("./apiClient.js");
    }
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de contratosApi.js.");
}

/**
 * Módulo de API para la cartera de contratos, detalle por folio/UUID y amortización (SF-069).
 */
const contratosApi = {
    /**
     * Obtiene los 4 KPIs agregados y la tabla filtrada/paginada de contratos (SF-068 / SF-069).
     * @param {{ buscar?: string, tab?: string, page?: number, pageSize?: number }} [filtros]
     * @param {ContratosApiOptions} [opts]
     */
    obtenerCartera: (filtros = {}, opts = {}) =>
        resolveContratosClient(opts).get(CONTRATOS_ENDPOINT, {
            ...opts,
            params: {
                ...(opts.params || {}),
                buscar: filtros.buscar,
                tab: filtros.tab ?? "Todos",
                page: filtros.page ?? 1,
                pageSize: filtros.pageSize ?? 20
            }
        }),

    /**
     * Localiza el expediente completo de un contrato por UUID o folio comercial CTR-YYYY-XXXX (SF-069).
     * @param {string} folioOrId UUID o folio comercial.
     * @param {ContratosApiOptions} [opts]
     */
    obtenerDetalleContrato: (folioOrId, opts = {}) =>
        resolveContratosClient(opts).get(`${CONTRATOS_ENDPOINT}/${encodeURIComponent(folioOrId)}`, opts),

    /**
     * Obtiene las N cuotas persistidas de la tabla de amortización de un contrato (SF-067 / SF-069).
     * @param {string} id UUID de la venta/contrato.
     * @param {ContratosApiOptions} [opts]
     */
    obtenerTablaAmortizacion: (id, opts = {}) =>
        resolveContratosClient(opts).get(`${CONTRATOS_ENDPOINT}/${encodeURIComponent(id)}/amortizacion`, opts),

    /**
     * Ejecuta cambio de estado administrativo sobre el contrato (SF-063 / SF-069).
     * @param {string} id UUID del contrato/venta.
     * @param {{ estado: string, motivo: string }} payload Estado objetivo y motivo >= 10 caracteres.
     * @param {ContratosApiOptions} [opts]
     */
    cambiarEstadoAdmin: (id, payload, opts = {}) => {
        const estado = payload?.estado;
        const motivo = payload?.motivo;
        if (String(estado || "").trim().toLowerCase() === "cancelado") {
            return resolveContratosClient(opts).delete(`${VENTAS_ADMIN_ENDPOINT}/${encodeURIComponent(id)}`, {
                ...opts,
                params: { ...(opts.params || {}), motivo }
            });
        }
        return resolveContratosClient(opts).patch(`${VENTAS_ADMIN_ENDPOINT}/${encodeURIComponent(id)}/estado`, { estado, motivo }, opts);
    }
};

if (typeof window !== "undefined") {
    window.contratosApi = contratosApi;
}
if (typeof module !== "undefined" && module.exports) {
    module.exports = contratosApi;
}
