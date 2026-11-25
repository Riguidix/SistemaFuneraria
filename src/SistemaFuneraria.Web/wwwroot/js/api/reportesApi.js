/**
 * Módulo de API para Dashboard Resumen y Reportes Analíticos (SF-088).
 */
function resolveReportesClient(opts) {
    if (opts && opts.client) return opts.client;
    if (typeof window !== "undefined" && window.apiClient) return window.apiClient;
    if (typeof require === "function") return require("./apiClient.js");
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de reportesApi.js.");
}

const reportesApi = {
    obtenerResumenDashboard: (filtros = {}, opts = {}) => {
        const esOptsLegacy = filtros && (filtros.client || filtros.fetchImpl || filtros.storage || filtros.headers || filtros.signal || filtros.baseUrl || filtros.token);
        const opciones = esOptsLegacy ? filtros : opts;
        const rango = esOptsLegacy ? opciones?.params?.rango : filtros?.rango;
        return resolveReportesClient(opciones).get("/api/v1/dashboard/resumen", {
            ...opciones,
            params: {
                ...(opciones.params || {}),
                ...(rango ? { rango } : {})
            }
        });
    },

    obtenerIndicadores: (filtros = {}, opts = {}) =>
        resolveReportesClient(opts).get("/api/v1/reportes/indicadores", {
            ...opts,
            params: {
                ...(opts.params || {}),
                periodo: filtros.periodo,
                sucursal: filtros.sucursal
            }
        })
};

if (typeof window !== "undefined") {
    window.reportesApi = reportesApi;
}
if (typeof module !== "undefined" && module.exports) {
    module.exports = reportesApi;
}
