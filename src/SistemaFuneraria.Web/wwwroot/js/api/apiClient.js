const DEFAULT_BASE_URL = "http://localhost:5180";
const TOKEN_STORAGE_KEY = "sf_token";
const SESSION_STORAGE_KEY = "sf_session";

/**
 * Opciones configurables para una petición HTTP en {@link apiClient}.
 * @typedef {Object} RequestOptions
 * @property {Record<string, string|number|boolean|null|undefined>} [params] Parámetros de query string (?clave=valor).
 * @property {Record<string, string>} [headers] Cabeceras HTTP adicionales.
 * @property {string} [token] Token JWT explícito (si se omite, se lee de sessionStorage).
 * @property {AbortSignal} [signal] Señal de AbortController para cancelar la petición.
 * @property {string} [baseUrl] URL base alternativa de la API.
 * @property {typeof fetch} [fetchImpl] Implementación de fetch inyectable para pruebas.
 * @property {Storage} [storage] Almacenamiento inyectable para pruebas (por defecto sessionStorage).
 */

/**
 * Error tipado emitido cuando la API responde con un código HTTP >= 400 (RFC 7807 ProblemDetails).
 * @extends Error
 */
class ApiError extends Error {
    /**
     * @param {string} message Mensaje legible del error (prioriza `detail` o `title` de ProblemDetails).
     * @param {{ status?: number, title?: string|null, detail?: string|null, errors?: Record<string, string[]>|null, payload?: any }} [meta]
     */
    constructor(message, { status = 0, title = null, detail = null, errors = null, payload = null } = {}) {
        super(message);
        this.name = "ApiError";
        this.status = status;
        this.title = title;
        this.detail = detail;
        this.errors = errors;
        this.payload = payload;
    }
}

/**
 * Resuelve la URL base de la API desde `overrideBaseUrl`, `window.__API_BASE_URL__`,
 * `<meta name="api-base-url">` o el valor por defecto `http://localhost:5180`.
 * @param {string} [overrideBaseUrl]
 * @returns {string}
 */
function resolveBaseUrl(overrideBaseUrl) {
    if (overrideBaseUrl) {
        return overrideBaseUrl;
    }

    if (typeof window !== "undefined") {
        if (window.__API_BASE_URL__) {
            return window.__API_BASE_URL__;
        }

        const meta = typeof document !== "undefined" ?
            document.querySelector('meta[name="api-base-url"]') :
            null;
        if (meta && meta.content) {
            return meta.content;
        }
    }

    return DEFAULT_BASE_URL;
}

/**
 * Construye la URL absoluta normalizando la ruta y serializando query params no vacíos.
 * @param {string} endpoint Ruta relativa (ej. `"/api/v1/clientes"`).
 * @param {RequestOptions["params"]} [params]
 * @param {string} [baseUrl]
 * @returns {string}
 */
function buildUrl(endpoint, params, baseUrl) {
    const normalizedPath = endpoint.startsWith("/") ? endpoint : `/${endpoint}`;
    const url = new URL(normalizedPath, resolveBaseUrl(baseUrl));

    if (params && typeof params === "object") {
        for (const [key, value] of Object.entries(params)) {
            if (value !== undefined && value !== null && value !== "") {
                url.searchParams.append(key, String(value));
            }
        }
    }

    return url.toString();
}

/**
 * @param {Storage} [storage]
 * @returns {string|null}
 */
function getStoredToken(storage) {
    if (storage) {
        return storage.getItem(TOKEN_STORAGE_KEY);
    }

    const sessionToken = typeof sessionStorage !== "undefined" ? sessionStorage.getItem(TOKEN_STORAGE_KEY) : null;
    if (sessionToken) {
        return sessionToken;
    }

    return typeof localStorage !== "undefined" ? localStorage.getItem(TOKEN_STORAGE_KEY) : null;
}

/**
 * @param {Storage} [storage]
 * @returns {void}
 */
function clearStoredSession(storage) {
    if (storage) {
        storage.removeItem(TOKEN_STORAGE_KEY);
        storage.removeItem(SESSION_STORAGE_KEY);
        return;
    }

    if (typeof sessionStorage !== "undefined") {
        sessionStorage.removeItem(TOKEN_STORAGE_KEY);
        sessionStorage.removeItem(SESSION_STORAGE_KEY);
    }

    if (typeof localStorage !== "undefined") {
        localStorage.removeItem(TOKEN_STORAGE_KEY);
        localStorage.removeItem(SESSION_STORAGE_KEY);
    }
}

/**
 * Construye las cabeceras HTTP agregando `Content-Type: application/json` y `Authorization: Bearer` si aplica.
 * @param {RequestOptions} [options]
 * @param {boolean} [hasJsonBody]
 * @returns {Record<string, string>}
 */
function buildHeaders(options = {}, hasJsonBody = false) {
    const headers = { ...(options.headers ?? {}) };

    if (hasJsonBody && !headers["Content-Type"]) {
        headers["Content-Type"] = "application/json";
    }

    const token = options.token ?? getStoredToken(options.storage);
    if (token) {
        headers.Authorization = `Bearer ${token}`;
    }

    return headers;
}

/**
 * Parsea la respuesta HTTP manejando `204 No Content`, JSON o texto plano sin lanzar error si el cuerpo viene vacío.
 * @param {Response} response
 * @returns {Promise<any>}
 */
async function parsePayload(response) {
    if (response.status === 204) {
        return null;
    }

    const contentType = response.headers?.get?.("content-type") ?? "";
    if (contentType.includes("json")) {
        return response.json().catch(() => null);
    }

    const text = await response.text().catch(() => "");
    if (!text) {
        return null;
    }

    try {
        return JSON.parse(text);
    } catch {
        return text;
    }
}

/**
 * Determina de forma insensible a mayúsculas si la ruta corresponde a la página de Login.
 * @param {string} [pathname]
 * @returns {boolean}
 */
function isLoginPath(pathname) {
    return String(pathname || "").toLowerCase().startsWith("/cuenta/login");
}

/**
 * Construye la URL de redirección a Login preservando `returnUrl` para rutas internas no públicas.
 * @param {Location|{ pathname?: string, search?: string }} [loc]
 * @returns {string}
 */
function buildLoginRedirectUrl(loc) {
    const pathname = String(loc?.pathname || "");
    const lowerPath = pathname.toLowerCase();
    if (!pathname || pathname === "/" || lowerPath.startsWith("/cuenta/")) {
        return "/Cuenta/Login";
    }

    return `/Cuenta/Login?returnUrl=${encodeURIComponent(pathname + (loc.search || ""))}`;
}

/**
 * Ejecuta una petición HTTP contra `SistemaFuneraria.Api` con soporte de JWT y mapeo de `ProblemDetails`.
 * @param {string} endpoint Ruta del recurso (ej. `"/api/v1/clientes"`).
 * @param {"GET"|"POST"|"PUT"|"PATCH"|"DELETE"} method Verbo HTTP.
 * @param {RequestOptions} [options]
 * @param {any} [data] Cuerpo a serializar (ignorado en GET/HEAD).
 * @returns {Promise<any>}
 * @throws {ApiError} Cuando la respuesta HTTP tiene código >= 400.
 */
async function request(endpoint, method, options = {}, data = null) {
    const fetchImpl = options.fetchImpl ?? fetch;
    const url = buildUrl(endpoint, options.params, options.baseUrl);
    const canSendBody = data !== undefined && data !== null && method !== "GET" && method !== "HEAD";
    const isFormData = typeof FormData !== "undefined" && data instanceof FormData;

    const fetchConfig = {
        method,
        headers: buildHeaders(options, canSendBody && !isFormData),
        signal: options.signal
    };

    if (canSendBody) {
        fetchConfig.body = isFormData ? data : JSON.stringify(data);
    }

    const response = await fetchImpl(url, fetchConfig);
    const payload = await parsePayload(response);

    if (!response.ok) {
        if (response.status === 401) {
            clearStoredSession(options.storage);

            if (typeof window !== "undefined" && !isLoginPath(window.location.pathname)) {
                window.location.assign(buildLoginRedirectUrl(window.location));
            }
        }

        const detail = payload && typeof payload === "object" ? (payload.detail ?? payload.title) : null;
        const message = detail || (typeof payload === "string" && payload) || `Error HTTP ${response.status}`;

        throw new ApiError(message, {
            status: response.status,
            title: payload?.title ?? null,
            detail: payload?.detail ?? null,
            errors: payload?.errors ?? null,
            payload
        });
    }

    return payload;
}

/**
 * Cliente HTTP centralizado para consumir `SistemaFuneraria.Api`.
 */
const apiClient = {
    ApiError,
    TOKEN_STORAGE_KEY,
    SESSION_STORAGE_KEY,

    /**
     * Realiza una petición `GET`.
     * @param {string} endpoint Ruta del recurso (ej. `"/api/v1/clientes"`).
     * @param {RequestOptions} [opts]
     * @returns {Promise<any>}
     * @throws {ApiError}
     */
    get: (endpoint, opts = {}) => request(endpoint, "GET", opts),

    /**
     * Realiza una petición `POST` enviando `data` en JSON (o `FormData`).
     * @param {string} endpoint
     * @param {any} [data]
     * @param {RequestOptions} [opts]
     * @returns {Promise<any>}
     * @throws {ApiError}
     */
    post: (endpoint, data = null, opts = {}) => request(endpoint, "POST", opts, data),

    /**
     * Realiza una petición `PUT` enviando `data` en JSON.
     * @param {string} endpoint
     * @param {any} [data]
     * @param {RequestOptions} [opts]
     * @returns {Promise<any>}
     * @throws {ApiError}
     */
    put: (endpoint, data = null, opts = {}) => request(endpoint, "PUT", opts, data),

    /**
     * Realiza una petición `PATCH` enviando `data` en JSON.
     * @param {string} endpoint
     * @param {any} [data]
     * @param {RequestOptions} [opts]
     * @returns {Promise<any>}
     * @throws {ApiError}
     */
    patch: (endpoint, data = null, opts = {}) => request(endpoint, "PATCH", opts, data),

    /**
     * Realiza una petición `DELETE`.
     * @param {string} endpoint
     * @param {RequestOptions} [opts]
     * @returns {Promise<any>}
     * @throws {ApiError}
     */
    delete: (endpoint, opts = {}) => request(endpoint, "DELETE", opts)
};

if (typeof window !== "undefined") {
    window.apiClient = apiClient;
}

if (typeof module !== "undefined" && module.exports) {
    module.exports = apiClient;
}
