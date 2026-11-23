/**
 * Módulo de API para Configuración de Sucursal y Usuarios (SF-091).
 */
function resolveConfigClient(opts) {
    if (opts && opts.client) return opts.client;
    if (typeof window !== "undefined" && window.apiClient) return window.apiClient;
    if (typeof require === "function") return require("./apiClient.js");
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de configuracionApi.js.");
}

const CONFIG_BASE = "/api/v1/configuracion";

const configuracionApi = {
    obtenerSucursal: (opts = {}) =>
        resolveConfigClient(opts).get(`${CONFIG_BASE}/sucursal`, opts),

    actualizarSucursal: (payload, opts = {}) =>
        resolveConfigClient(opts).put(`${CONFIG_BASE}/sucursal`, payload, opts),

    listarUsuarios: (opts = {}) =>
        resolveConfigClient(opts).get(`${CONFIG_BASE}/usuarios`, opts),

    crearUsuario: (payload, opts = {}) =>
        resolveConfigClient(opts).post(`${CONFIG_BASE}/usuarios`, payload, opts),

    actualizarUsuario: (id, payload, opts = {}) =>
        resolveConfigClient(opts).put(`${CONFIG_BASE}/usuarios/${encodeURIComponent(id)}`, payload, opts),

    cambiarEstadoUsuario: (id, payload, opts = {}) =>
        resolveConfigClient(opts).patch(`${CONFIG_BASE}/usuarios/${encodeURIComponent(id)}/estado`, payload, opts)
};

if (typeof window !== "undefined") {
    window.configuracionApi = configuracionApi;
}
if (typeof module !== "undefined" && module.exports) {
    module.exports = configuracionApi;
}
