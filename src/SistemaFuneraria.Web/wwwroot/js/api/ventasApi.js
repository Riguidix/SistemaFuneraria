/**
 * Opciones inyectables para {@link ventasApi}.
 * @typedef {Object} VentasApiOptions
 * @property {import("./apiClient")} [client] Cliente HTTP alternativo (por defecto `apiClient`).
 * @property {Record<string, string|number|boolean|null|undefined>} [params] Parámetros de query string.
 * @property {Record<string, string>} [headers] Cabeceras adicionales.
 * @property {string} [token] Token JWT explícito.
 */

const VENTAS_ENDPOINT = "/api/v1/ventas";

/**
 * Resuelve la instancia de `apiClient` compatible con navegador y Node.
 * @param {VentasApiOptions} [opts]
 * @returns {import("./apiClient")}
 */
function resolveClient(opts) {
    if (opts && opts.client) {
        return opts.client;
    }
    if (typeof window !== "undefined" && window.apiClient) {
        return window.apiClient;
    }
    if (typeof require === "function") {
        return require("./apiClient.js");
    }
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de ventasApi.js.");
}

/**
 * Módulo de API para el registro de ventas y contratos comerciales (SF-061).
 */
const ventasApi = {
    /**
     * Registra una venta en modalidad de contado con pago único y liquidación inmediata (CP-02 / ST-01).
     * @param {Object} payload Datos de la venta ({ clienteId, loteId, metodoPago, referenciaPago }).
     * @param {VentasApiOptions} [opts]
     */
    registrarVentaContado: (payload, opts = {}) =>
        resolveClient(opts).post(`${VENTAS_ENDPOINT}/contado`, payload, opts),

    /**
     * Registra una venta en modalidad financiada a crédito (SF-067 / Milestone 7).
     * @param {Object} payload Datos de la venta financiada.
     * @param {VentasApiOptions} [opts]
     */
    registrarVentaFinanciada: (payload, opts = {}) =>
        resolveClient(opts).post(`${VENTAS_ENDPOINT}/financiada`, payload, opts),

    /**
     * Obtiene el resumen del contrato comercial y comprobante de venta por ID.
     * @param {string} id UUID de la venta.
     * @param {VentasApiOptions} [opts]
     */
    obtenerVentaPorId: (id, opts = {}) =>
        resolveClient(opts).get(`${VENTAS_ENDPOINT}/${encodeURIComponent(id)}`, opts),

    /**
     * Cambia administrativamente el estado de un contrato comercial (SF-063).
     * @param {string} id UUID de la venta.
     * @param {string|number} estado Nuevo estado (Activo, EnRevision, Cancelado).
     * @param {string} motivo Motivo obligatorio (mínimo 10 caracteres).
     * @param {VentasApiOptions} [opts]
     */
    cambiarEstadoVenta: (id, estado, motivo, opts = {}) =>
        resolveClient(opts).patch(`${VENTAS_ENDPOINT}/${encodeURIComponent(id)}/estado`, { estado, motivo }, opts),

    /**
     * Realiza la baja lógica o cancelación administrativa liberando el lote (SF-063).
     * @param {string} id UUID de la venta.
     * @param {string} motivo Motivo obligatorio de cancelación (mínimo 10 caracteres).
     * @param {VentasApiOptions} [opts]
     */
    cancelarVenta: (id, motivo, opts = {}) =>
        resolveClient(opts).delete(`${VENTAS_ENDPOINT}/${encodeURIComponent(id)}`, { ...opts, params: { ...(opts.params || {}), motivo } })
};

if (typeof window !== "undefined") {
    window.ventasApi = ventasApi;
}
if (typeof module !== "undefined" && module.exports) {
    module.exports = ventasApi;
}
