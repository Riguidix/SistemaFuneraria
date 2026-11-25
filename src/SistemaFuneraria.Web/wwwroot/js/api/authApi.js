/**
 * Perfil de sesión almacenado tras autenticar contra `SistemaFuneraria.Api`.
 * @typedef {Object} AuthSession
 * @property {string} usuarioId
 * @property {string} nombreCompleto
 * @property {string} iniciales
 * @property {string} correo
 * @property {"Administrador"|"Vendedor"|"Cajero"|string} rol
 * @property {string} sucursal
 */

/**
 * Opciones inyectables para pruebas unitarias y configuración de sesión de {@link authApi}.
 * @typedef {Object} AuthOptions
 * @property {import("./apiClient")} [client] Cliente HTTP alternativo (por defecto `apiClient`).
 * @property {Storage} [storage] Almacenamiento alternativo (por defecto `sessionStorage` o `localStorage` si `rememberMe`).
 * @property {boolean} [rememberMe] Persiste la sesión en `localStorage` cuando es `true`.
 * @property {boolean} [skipRedirect] Evita redirección al llamar `logout()` o `requireAuth()` en pruebas.
 */

const LOGIN_PATH = "/Cuenta/Login";
const ROL_ADMINISTRADOR = "Administrador";

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

    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de authApi.js.");
}

function resolveWriteStorage(opts) {
    if (opts && opts.storage) {
        return opts.storage;
    }

    if (opts && opts.rememberMe && typeof localStorage !== "undefined") {
        return localStorage;
    }

    return typeof sessionStorage !== "undefined" ? sessionStorage : null;
}

function resolveStoragesToRead(opts) {
    if (opts && opts.storage) {
        return [opts.storage];
    }

    const storages = [];
    if (typeof sessionStorage !== "undefined") storages.push(sessionStorage);
    if (typeof localStorage !== "undefined") storages.push(localStorage);
    return storages;
}

function clearOtherStorage(activeStorage, client) {
    if (typeof sessionStorage !== "undefined" && activeStorage !== sessionStorage) {
        sessionStorage.removeItem(client.TOKEN_STORAGE_KEY);
        sessionStorage.removeItem(client.SESSION_STORAGE_KEY);
    }
    if (typeof localStorage !== "undefined" && activeStorage !== localStorage) {
        localStorage.removeItem(client.TOKEN_STORAGE_KEY);
        localStorage.removeItem(client.SESSION_STORAGE_KEY);
    }
}

function saveSession(data, client, storage) {
    const session = {
        usuarioId: data.usuarioId ?? "",
        nombreCompleto: data.nombreCompleto ?? "",
        iniciales: data.iniciales ?? "",
        correo: data.correo ?? "",
        rol: data.rol ?? "",
        sucursal: data.sucursal ?? ""
    };

    if (storage) {
        clearOtherStorage(storage, client);
        if (data.token) {
            storage.setItem(client.TOKEN_STORAGE_KEY, data.token);
        }
        storage.setItem(client.SESSION_STORAGE_KEY, JSON.stringify(session));
    }

    return session;
}

function buildRequireAuthRedirectUrl(loc) {
    const pathname = String(loc?.pathname ?? "");
    const lowerPath = pathname.toLowerCase();
    if (!pathname || pathname === "/" || lowerPath.startsWith("/cuenta/")) {
        return LOGIN_PATH;
    }

    return `${LOGIN_PATH}?returnUrl=${encodeURIComponent(pathname + (loc?.search ?? ""))}`;
}

function decodeJwtPayload(token) {
    if (!token || typeof token !== "string") {
        return null;
    }

    const parts = token.split(".");
    if (parts.length !== 3) {
        return null;
    }

    try {
        const base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
        const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), "=");
        const json = typeof atob === "function"
            ? atob(padded)
            : Buffer.from(padded, "base64").toString("utf8");
        return JSON.parse(json);
    } catch {
        return null;
    }
}

/**
 * Cliente de autenticación, guardia de sesión y control de visibilidad RBAC en UI (`SF-024`).
 */
let authApi = {
    ROL_ADMINISTRADOR,

    /**
     * Autentica al usuario contra `POST /api/v1/auth/login` y guarda su JWT y perfil.
     * @param {string} correo
     * @param {string} password
     * @param {AuthOptions} [opts]
     * @returns {Promise<any>} Respuesta `LoginResponseDto` del servidor.
     */
    async login(correo, password, opts = {}) {
        const client = resolveClient(opts);
        const storage = resolveWriteStorage(opts);
        const response = await client.post("/api/v1/auth/login", { correo, password }, { storage });

        saveSession(response, client, storage);
        return response;
    },

    /**
     * Solicita recuperación de contraseña contra `POST /api/v1/auth/olvide-contrasena`.
     * @param {string} correo
     * @param {AuthOptions} [opts]
     * @returns {Promise<{ mensaje?: string }>}
     */
    async olvideContrasena(correo, opts = {}) {
        const client = resolveClient(opts);

        return client.post("/api/v1/auth/olvide-contrasena", { correo });
    },

    /**
     * Cierra la sesión actual limpiando almacenamiento y redirigiendo a `/Cuenta/Login`.
     * @param {AuthOptions} [opts]
     * @returns {void}
     */
    logout(opts = {}) {
        const client = resolveClient(opts);
        const storages = resolveStoragesToRead(opts);

        for (const storage of storages) {
            storage.removeItem(client.TOKEN_STORAGE_KEY);
            storage.removeItem(client.SESSION_STORAGE_KEY);
        }

        if (!opts.skipRedirect && typeof window !== "undefined") {
            window.location.assign(LOGIN_PATH);
        }
    },

    /**
     * Obtiene el perfil autenticado actual desde `sessionStorage` o `localStorage`, o `null` si no existe o está corrupto.
     * @param {AuthOptions} [opts]
     * @returns {AuthSession|null}
     */
    getSession(opts = {}) {
        const client = resolveClient(opts);
        const storages = resolveStoragesToRead(opts);

        for (const storage of storages) {
            const raw = storage.getItem(client.SESSION_STORAGE_KEY);
            if (!raw) {
                continue;
            }
            try {
                return JSON.parse(raw);
            } catch {
                return null;
            }
        }

        return null;
    },

    /**
     * Obtiene el token JWT almacenado para la sesión vigente.
     * @param {AuthOptions} [opts]
     * @returns {string|null}
     */
    getToken(opts = {}) {
        const client = resolveClient(opts);
        const storages = resolveStoragesToRead(opts);

        for (const storage of storages) {
            const token = storage.getItem(client.TOKEN_STORAGE_KEY);
            if (token) {
                return token;
            }
        }

        return null;
    },

    /**
     * Verifica si un JWT tiene fecha de expiración (`exp`) vencida.
     * Si la cadena no corresponde a un JWT estándar de 3 partes, retorna `false`.
     * @param {string|null|undefined} token
     * @param {number} [nowMs]
     * @returns {boolean}
     */
    isTokenExpired(token, nowMs = Date.now()) {
        if (!token || typeof token !== "string") {
            return true;
        }

        const payload = decodeJwtPayload(token);
        if (!payload || typeof payload.exp !== "number") {
            return false;
        }

        return payload.exp * 1000 <= nowMs;
    },

    /**
     * Devuelve la ruta de aterrizaje por defecto según el rol autenticado.
     * @param {string|null|undefined} rol
     * @returns {string}
     */
    getDefaultRouteForRole(rol) {
        return rol === "Cajero" ? "/Caja" : "/Resumen";
    },

    /**
     * Valida que `returnUrl` sea una ruta local segura y distinta a las pantallas de cuenta.
     * @param {string|null|undefined} rawUrl
     * @returns {string|null}
     */
    sanitizeReturnUrl(rawUrl) {
        if (typeof rawUrl !== "string" || rawUrl.length === 0) {
            return null;
        }

        if (!rawUrl.startsWith("/") || rawUrl.startsWith("//")) {
            return null;
        }

        if (rawUrl.includes("\\") || /[\r\n]/.test(rawUrl)) {
            return null;
        }

        const lower = rawUrl.toLowerCase();
        if (lower.startsWith("/cuenta/login") || lower.startsWith("/cuenta/olvidecontrasena")) {
            return null;
        }

        return rawUrl;
    },

    /**
     * Resuelve la ruta de redirección post-login priorizando un `returnUrl` válido o la ruta por rol.
     * @param {{ rol?: string }|null|undefined} session
     * @param {string|null|undefined} rawReturnUrl
     * @returns {string}
     */
    resolvePostLoginUrl(session, rawReturnUrl) {
        return authApi.sanitizeReturnUrl(rawReturnUrl) || authApi.getDefaultRouteForRole(session?.rol);
    },

    /**
     * Guardia centralizado de autenticación para páginas protegidas.
     * Si no hay sesión o el JWT expiró, limpia sesión y redirige a `/Cuenta/Login` preservando `returnUrl`.
     * @param {AuthOptions} [opts]
     * @returns {AuthSession|null}
     */
    requireAuth(opts = {}) {
        const session = authApi.getSession(opts);
        const token = authApi.getToken(opts);

        if (!session || !token || authApi.isTokenExpired(token)) {
            authApi.logout({ ...opts, skipRedirect: true });
            if (!opts.skipRedirect && typeof window !== "undefined") {
                window.location.assign(buildRequireAuthRedirectUrl(window.location));
            }
            return null;
        }

        return session;
    },

    /**
     * Aplica visibilidad RBAC declarativa sobre elementos marcados con `data-roles` (ej. `data-roles="Administrador"`).
     * @param {ParentNode|null} [root]
     * @param {AuthOptions} [opts]
     * @returns {void}
     */
    applyRolePermissions(root = null, opts = {}) {
        const scope = root ?? (typeof document !== "undefined" ? document : null);

        if (!scope || typeof scope.querySelectorAll !== "function") {
            return;
        }

        const session = authApi.getSession(opts);
        const currentRole = session?.rol ?? "";
        const elements = scope.querySelectorAll("[data-roles]");

        for (let el of elements) {
            const rawRoles = el.getAttribute("data-roles") ?? "";
            const allowedRoles = rawRoles
                .split(",")
                .map((r) => r.trim())
                .filter(Boolean);

            const isAllowed = currentRole !== "" && allowedRoles.includes(currentRole);

            if (isAllowed) {
                el.classList.remove("d-none");
                el.removeAttribute("aria-hidden");
            } else {
                el.classList.add("d-none");
                el.setAttribute("aria-hidden", "true");
            }
        }
    }
};

if (typeof window !== "undefined") {
    window.authApi = authApi;
}

if (typeof module !== "undefined" && module.exports) {
    module.exports = authApi;
}
