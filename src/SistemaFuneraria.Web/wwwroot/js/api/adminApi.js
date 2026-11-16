/**
 * Opciones inyectables para {@link adminApi}.
 * @typedef {Object} AdminApiOptions
 * @property {import("./apiClient")} [client] Cliente HTTP alternativo (por defecto `apiClient`).
 */

const ADMIN_USUARIOS_ENDPOINT = "/api/v1/admin/usuarios";

/**
 * Resuelve la instancia de `apiClient` compatible con navegador y Node.
 * @param {AdminApiOptions} [opts]
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
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de adminApi.js.");
}

/**
 * Módulo de API para administración de usuarios y roles (`SF-029`).
 * Delega todas las peticiones a `apiClient` (JWT Bearer + RFC 7807 ProblemDetails).
 */
const adminApi = {
    /**
     * Obtiene la lista completa de usuarios operativos del sistema.
     * @param {AdminApiOptions} [opts]
     * @returns {Promise<Array<Object>>}
     */
    listarUsuarios: (opts = {}) => resolveClient(opts).get(ADMIN_USUARIOS_ENDPOINT, opts),

    /**
     * Registra un nuevo usuario en el sistema.
     * @param {{ nombreCompleto: string, correo: string, password: string, rol: string, sucursal?: string }} data
     * @param {AdminApiOptions} [opts]
     * @returns {Promise<Object>}
     */
    crearUsuario: (data, opts = {}) => resolveClient(opts).post(ADMIN_USUARIOS_ENDPOINT, data, opts),

    /**
     * Actualiza información de perfil, rol o desbloquea una cuenta.
     * @param {string} id Identificador del usuario.
     * @param {{ nombreCompleto?: string, rol?: string, sucursal?: string, desbloquear?: boolean }} data
     * @param {AdminApiOptions} [opts]
     * @returns {Promise<Object>}
     */
    actualizarUsuario: (id, data, opts = {}) =>
        resolveClient(opts).put(`${ADMIN_USUARIOS_ENDPOINT}/${encodeURIComponent(id)}`, data, opts),

    /**
     * Elimina un usuario del sistema (prohíbe auto-eliminación en el servidor).
     * @param {string} id Identificador del usuario.
     * @param {AdminApiOptions} [opts]
     * @returns {Promise<null>}
     */
    eliminarUsuario: (id, opts = {}) =>
        resolveClient(opts).delete(`${ADMIN_USUARIOS_ENDPOINT}/${encodeURIComponent(id)}`, opts)
};

if (typeof window !== "undefined") {
    window.adminApi = adminApi;
}

if (typeof module !== "undefined" && module.exports) {
    module.exports = adminApi;
}
