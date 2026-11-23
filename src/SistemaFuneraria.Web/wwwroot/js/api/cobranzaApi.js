/**
 * Opciones inyectables para {@link cobranzaApi}.
 * @typedef {Object} CobranzaApiOptions
 * @property {import("./apiClient")} [client] Cliente HTTP alternativo (por defecto `apiClient`).
 * @property {Record<string, string|number|boolean|null|undefined>} [params] Parámetros de query string.
 * @property {Record<string, string>} [headers] Cabeceras adicionales.
 * @property {string} [token] Token JWT explícito.
 */

const COBRANZA_ENDPOINT = "/api/v1/cobranza";

function resolveCobranzaClient(opts) {
    if (opts && opts.client) {
        return opts.client;
    }
    if (typeof window !== "undefined" && window.apiClient) {
        return window.apiClient;
    }
    if (typeof require === "function") {
        return require("./apiClient.js");
    }
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de cobranzaApi.js.");
}

/**
 * Genera una clave de idempotencia UUID v4 mediante `crypto.randomUUID()` (SF-079).
 * @returns {string}
 */
function generarClaveIdempotencia() {
    if (typeof globalThis !== "undefined" && globalThis.crypto && typeof globalThis.crypto.randomUUID === "function") {
        return globalThis.crypto.randomUUID();
    }
    if (typeof require === "function") {
        try {
            return require("crypto").randomUUID();
        } catch {
            // Fallback RFC 4122
        }
    }
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
        const r = Math.floor(Math.random() * 16);
        const v = c === "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
    });
}

/**
 * Módulo de API para Caja y Cobranza (SF-079).
 */
const cobranzaApi = {
    generarClaveIdempotencia,

    /**
     * Obtiene el corte del día en vivo y el conteo de pagos vencidos (SF-078).
     * @param {CobranzaApiOptions} [opts]
     */
    obtenerResumenCaja: (opts = {}) =>
        resolveCobranzaClient(opts).get(`${COBRANZA_ENDPOINT}/resumen-caja`, opts),

    /**
     * Localiza un contrato en ventanilla de Caja por folio CTR-YYYY-XXXX o UUID (SF-078).
     * @param {string} folioOrId
     * @param {CobranzaApiOptions} [opts]
     */
    obtenerContratoParaCaja: (folioOrId, opts = {}) =>
        resolveCobranzaClient(opts).get(`${COBRANZA_ENDPOINT}/contrato/${encodeURIComponent(folioOrId)}`, opts),

    /**
     * Aplica el pago de la siguiente mensualidad exigible enviando `X-Idempotency-Key` (SF-079).
     * @param {{ folioContrato: string, importeRecibido: number, metodoPago: string, referencia?: string|null, numeroCuota?: number|null, claveIdempotencia?: string }} payload
     * @param {CobranzaApiOptions} [opts]
     */
    aplicarPagoMensualidad: (payload, opts = {}) => {
        const clave = payload?.claveIdempotencia || generarClaveIdempotencia();
        const body = {
            ...payload,
            claveIdempotencia: clave
        };
        const headers = {
            ...(opts.headers || {}),
            "X-Idempotency-Key": clave
        };
        return resolveCobranzaClient(opts).post(`${COBRANZA_ENDPOINT}/mensualidad`, body, { ...opts, headers });
    },

    /**
     * Aplica un abono extraordinario 100% a capital enviando `X-Idempotency-Key` (SF-087).
     * @param {{ folioContrato: string, montoAbono: number, metodoPago: string, referencia?: string|null, claveIdempotencia?: string }} payload
     * @param {CobranzaApiOptions} [opts]
     */
    aplicarAbonoCapital: (payload, opts = {}) => {
        const clave = payload?.claveIdempotencia || generarClaveIdempotencia();
        const body = {
            ...payload,
            claveIdempotencia: clave
        };
        const headers = {
            ...(opts.headers || {}),
            "X-Idempotency-Key": clave
        };
        return resolveCobranzaClient(opts).post(`${COBRANZA_ENDPOINT}/abono-capital`, body, { ...opts, headers });
    },

    /**
     * Aplica la liquidación anticipada con descuento del 10% sobre intereses futuros enviando `X-Idempotency-Key` (SF-087).
     * @param {{ folioContrato: string, importeRecibido: number, metodoPago: string, referencia?: string|null, claveIdempotencia?: string }} payload
     * @param {CobranzaApiOptions} [opts]
     */
    aplicarLiquidacion: (payload, opts = {}) => {
        const clave = payload?.claveIdempotencia || generarClaveIdempotencia();
        const body = {
            ...payload,
            claveIdempotencia: clave
        };
        const headers = {
            ...(opts.headers || {}),
            "X-Idempotency-Key": clave
        };
        return resolveCobranzaClient(opts).post(`${COBRANZA_ENDPOINT}/liquidacion`, body, { ...opts, headers });
    },

    /**
     * Consulta un recibo oficial PAG-YYYY-XXXXXX por su folio (SF-078 / SF-082).
     * @param {string} folio
     * @param {CobranzaApiOptions} [opts]
     */
    obtenerRecibo: (folio, opts = {}) =>
        resolveCobranzaClient(opts).get(`${COBRANZA_ENDPOINT}/recibos/${encodeURIComponent(folio)}`, opts)
};

if (typeof window !== "undefined") {
    window.cobranzaApi = cobranzaApi;
}
if (typeof module !== "undefined" && module.exports) {
    module.exports = cobranzaApi;
}
