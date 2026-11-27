/**
 * Opciones inyectables para {@link lotesApi}.
 * @typedef {Object} LotesApiOptions
 * @property {import("./apiClient")} [client] Cliente HTTP alternativo (por defecto `apiClient`).
 * @property {Record<string, string|number|boolean|null|undefined>} [params] Parámetros de query string.
 * @property {Record<string, string>} [headers] Cabeceras adicionales.
 * @property {string} [token] Token JWT explícito.
 */

const LOTES_ENDPOINT = "/api/v1/lotes";

/**
 * Resuelve la instancia de `apiClient` compatible con navegador y Node.
 * @param {LotesApiOptions} [opts]
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
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de lotesApi.js.");
}

/**
 * Formatea un monto numérico a moneda mexicana según cultura es-MX ($120,000.00).
 * @param {number} monto
 * @returns {string}
 */
function formatearMonedaMXN(monto) {
    const formateador = new Intl.NumberFormat("es-MX", {
        style: "currency",
        currency: "MXN",
        minimumFractionDigits: 2
    });
    return formateador.format(monto || 0);
}

/**
 * Obtiene la configuración visual y acción contextual según el estado del lote (Figma Captura 2).
 * @param {string|number} estado
 * @returns {{ clase: string, texto: string, accion: string }}
 */
function obtenerMetadatosEstadoLote(estado) {
    const normalizado = String(estado || "").trim().toLowerCase();
    switch (normalizado) {
        case "disponible":
        case "1":
            return { clase: "badge-sf-success", texto: "Disponible", accion: "Cotizar" };
        case "reservado":
        case "2":
            return { clase: "badge-sf-warning", texto: "Reservado", accion: "Ver detalle" };
        case "vendido":
        case "3":
            return { clase: "badge-sf-neutral", texto: "Vendido", accion: "Ver contrato" };
        case "mantenimiento":
        case "4":
            return { clase: "badge-sf-maintenance", texto: "Mantenimiento", accion: "Ver detalle" };
        default:
            return { clase: "badge-sf-neutral", texto: String(estado || "Desconocido"), accion: "Ver detalle" };
    }
}

/**
 * Renderiza el HTML de badge para el estado de un lote funerario.
 * @param {string|number} estado
 * @returns {string}
 */
function renderizarBadgeEstadoLote(estado) {
    const meta = obtenerMetadatosEstadoLote(estado);
    return `<span class="badge badge-estado ${meta.clase}">${meta.texto}</span>`;
}

/**
 * Módulo de API para el catálogo y gestión de lotes funerarios (SF-043).
 */
const lotesApi = {
    formatearMonedaMXN,
    renderizarBadgeEstadoLote,
    obtenerMetadatosEstadoLote,

    /**
     * Consulta el catálogo paginado de lotes con filtros sanitizados y resumen de KPIs.
     * @param {Object} [filtros]
     * @param {LotesApiOptions} [opts]
     */
    obtenerCatalogo: (filtros = {}, opts = {}) => {
        const params = { ...(opts.params || {}) };
        if (filtros.buscar && typeof filtros.buscar === "string" && filtros.buscar.trim()) {
            params.buscar = filtros.buscar.trim();
        }
        if (filtros.seccion && typeof filtros.seccion === "string") {
            const sec = filtros.seccion.trim();
            if (sec && !/^todas(\s+las\s+secciones)?$/i.test(sec)) {
                params.seccion = sec;
            }
        }
        if (filtros.estado && typeof filtros.estado === "string") {
            const est = filtros.estado.trim();
            if (est && !/^todos(\s+los\s+estados)?$/i.test(est)) {
                params.estado = est;
            }
        }
        if (filtros.gavetas !== undefined && filtros.gavetas !== null && filtros.gavetas !== "") {
            params.gavetas = filtros.gavetas;
        }
        if (filtros.page) params.page = filtros.page;
        if (filtros.pageSize) params.pageSize = filtros.pageSize;

        return resolveClient(opts).get(LOTES_ENDPOINT, { ...opts, params });
    },

    /**
     * Obtiene los detalles de un lote funerario por ID.
     */
    obtenerLotePorId: (id, opts = {}) =>
        resolveClient(opts).get(`${LOTES_ENDPOINT}/${encodeURIComponent(id)}`, opts),

    /**
     * Obtiene los detalles técnicos de un lote funerario por su clave (SF-046).
     */
    obtenerLotePorClave: (clave, opts = {}) =>
        resolveClient(opts).get(`${LOTES_ENDPOINT}/clave/${encodeURIComponent(clave)}`, opts),

    /**
     * Obtiene la vista previa de la siguiente clave física, andador y fila disponibles para una sección.
     */
    obtenerSiguienteUbicacion: (seccion, opts = {}) =>
        resolveClient(opts).get(`${LOTES_ENDPOINT}/siguiente-ubicacion`, {
            ...opts,
            params: { ...(opts.params || {}), seccion }
        }),

    /**
     * Crea un nuevo lote (exclusivo Administrador).
     */
    crearLote: (payload, opts = {}) =>
        resolveClient(opts).post(LOTES_ENDPOINT, payload, opts),

    /**
     * Actualiza información de un lote existente (exclusivo Administrador).
     */
    actualizarLote: (id, payload, opts = {}) =>
        resolveClient(opts).put(`${LOTES_ENDPOINT}/${encodeURIComponent(id)}`, payload, opts),

    /**
     * Actualiza únicamente el estado operativo de un lote (exclusivo Administrador).
     */
    cambiarEstadoLote: (id, estado, opts = {}) =>
        resolveClient(opts).patch(`${LOTES_ENDPOINT}/${encodeURIComponent(id)}/estado`, { estado }, opts)
};

if (typeof window !== "undefined") {
    window.lotesApi = lotesApi;
}
if (typeof module !== "undefined" && module.exports) {
    module.exports = lotesApi;
}
