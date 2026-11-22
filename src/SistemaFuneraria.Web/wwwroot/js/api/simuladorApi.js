/**
 * Opciones inyectables para simuladorApi (SF-050).
 * @typedef {Object} SimuladorApiOptions
 * @property {import("./apiClient")} [client] Cliente HTTP alternativo (por defecto apiClient).
 * @property {Record<string, string|number|boolean|null|undefined>} [params] Parámetros de query string.
 * @property {Record<string, string>} [headers] Cabeceras adicionales.
 * @property {string} [token] Token JWT explícito.
 */

const SIMULADOR_ENDPOINT = "/api/v1/simulador";

/**
 * Resuelve la instancia de apiClient compatible con navegador y Node.
 * @param {SimuladorApiOptions} [opts]
 * @returns {import("./apiClient")}
 */
function resolveClient(opts) {
    if (opts && opts.client) return opts.client;
    if (typeof window !== "undefined" && window.apiClient) return window.apiClient;
    if (typeof require === "function") return require("./apiClient.js");
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de simuladorApi.js.");
}

/**
 * Módulo de API para simulación de esquemas de financiamiento funerario (SF-050).
 */
const simuladorApi = {
    /**
     * Consulta el catálogo oficial de porcentajes de enganche y opciones de plazos (Captura 7).
     * @param {SimuladorApiOptions} [opts]
     */
    obtenerPlanes: (opts = {}) =>
        resolveClient(opts).get(`${SIMULADOR_ENDPOINT}/planes`, opts),

    /**
     * Ejecuta la simulación de cotización comercial para un lote en modalidad Contado o Crédito (CP-10).
     * @param {{ loteId?: string, gavetas?: number, modalidad: string, porcentajeEnganche: number, plazoMeses: number, fechaPrimerPago?: string }} payload
     * @param {SimuladorApiOptions} [opts]
     */
    cotizar: (payload, opts = {}) =>
        resolveClient(opts).post(`${SIMULADOR_ENDPOINT}/cotizar`, payload, opts)
};

if (typeof window !== "undefined") {
    window.simuladorApi = simuladorApi;
}
if (typeof module !== "undefined" && module.exports) {
    module.exports = simuladorApi;
}
