/**
 * Opciones inyectables para {@link clientesApi}.
 * @typedef {Object} ClientesApiOptions
 * @property {import("./apiClient")} [client] Cliente HTTP alternativo (por defecto `apiClient`).
 * @property {Record<string, string|number|boolean|null|undefined>} [params] Parámetros de query string.
 * @property {Record<string, string>} [headers] Cabeceras adicionales.
 * @property {string} [token] Token JWT explícito.
 */

const CLIENTES_ENDPOINT = "/api/v1/clientes";

/**
 * Resuelve la instancia de `apiClient` compatible con navegador y Node.
 * @param {ClientesApiOptions} [opts]
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
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de clientesApi.js.");
}

/**
 * Sanitiza automáticamente los campos del cliente antes del envío (SF-033, RF-02, RNF-01).
 * Normaliza el teléfono a 10 dígitos continuos sin espacios y la CURP a mayúsculas.
 * @param {Object} payload Datos del cliente.
 * @returns {Object} Payload sanitizado.
 */
function sanitizarPayload(payload) {
    if (!payload || typeof payload !== "object") {
        return payload;
    }

    const clon = { ...payload };

    if (typeof clon.telefono === "string") {
        clon.telefono = clon.telefono.replace(/\s+/g, "");
    }

    if (typeof clon.curp === "string") {
        clon.curp = clon.curp.trim().toUpperCase();
    }

    return clon;
}

/**
 * Módulo de API para la gestión del ciclo de vida de clientes (SF-033).
 * Consume los endpoints REST de `/api/v1/clientes` con sanitización ergonómica y soporte para exportación.
 */
const clientesApi = {
    /**
     * Helper de sanitización expuesto para validación y pruebas.
     */
    _sanitizarPayload: sanitizarPayload,

    /**
     * Lista clientes con soporte de filtrado reactivo por nombre, teléfono o CURP (Captura 3).
     * @param {string|null} [filtro] Término de búsqueda opcional.
     * @param {ClientesApiOptions} [opts]
     * @returns {Promise<Array<Object>>}
     */
    listar: (filtro, opts = {}) => {
        const params = { ...(opts.params || {}) };
        if (filtro && typeof filtro === "string" && filtro.trim() !== "") {
            params.filtro = filtro.trim();
        }
        return resolveClient(opts).get(CLIENTES_ENDPOINT, { ...opts, params });
    },

    /**
     * Consulta los detalles de un cliente a partir de su ID único.
     * @param {string} id GUID del cliente.
     * @param {ClientesApiOptions} [opts]
     * @returns {Promise<Object>}
     */
    obtenerPorId: (id, opts = {}) =>
        resolveClient(opts).get(`${CLIENTES_ENDPOINT}/${encodeURIComponent(id)}`, opts),

    /**
     * Registra un nuevo cliente con sanitización automática de teléfono y CURP (Captura 4).
     * @param {{ nombre: string, apellidos: string, curp: string, telefono: string, correo: string, direccion?: string }} payload
     * @param {ClientesApiOptions} [opts]
     * @returns {Promise<Object>}
     */
    registrar: (payload, opts = {}) =>
        resolveClient(opts).post(CLIENTES_ENDPOINT, sanitizarPayload(payload), opts),

    /**
     * Actualiza la información de un cliente existente con sanitización automática.
     * @param {string} id GUID del cliente.
     * @param {{ nombre?: string, apellidos?: string, curp?: string, telefono?: string, correo?: string, direccion?: string }} payload
     * @param {ClientesApiOptions} [opts]
     * @returns {Promise<Object>}
     */
    actualizar: (id, payload, opts = {}) =>
        resolveClient(opts).put(`${CLIENTES_ENDPOINT}/${encodeURIComponent(id)}`, sanitizarPayload(payload), opts),

    /**
     * Elimina un cliente del padrón (exclusivo para rol Administrador).
     * @param {string} id GUID del cliente.
     * @param {ClientesApiOptions} [opts]
     * @returns {Promise<null>}
     */
    eliminar: (id, opts = {}) =>
        resolveClient(opts).delete(`${CLIENTES_ENDPOINT}/${encodeURIComponent(id)}`, opts),

    /**
     * Descarga el padrón completo de clientes en formato CSV UTF-8 con BOM (Captura 3).
     * @param {ClientesApiOptions} [opts]
     * @returns {Promise<string>} Contenido CSV exportado.
     */
    exportarDirectorio: async (opts = {}) => {
        const client = resolveClient(opts);
        const csvContent = await client.get(`${CLIENTES_ENDPOINT}/exportar`, opts);

        if (typeof window !== "undefined" && typeof document !== "undefined" && typeof Blob !== "undefined") {
            const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.setAttribute("href", url);
            link.setAttribute("download", "directorio-clientes.csv");
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
        }

        return csvContent;
    }
};

if (typeof window !== "undefined") {
    window.clientesApi = clientesApi;
}

if (typeof module !== "undefined" && module.exports) {
    module.exports = clientesApi;
}
