/**
 * Perfil de sesión almacenado en `sessionStorage` tras autenticar contra `SistemaFuneraria.Api`.
 * @typedef {Object} AuthSession
 * @property {string} usuarioId
 * @property {string} nombreCompleto
 * @property {string} iniciales
 * @property {string} correo
 * @property {"Administrador"|"Vendedor"|"Cajero"|string} rol
 * @property {string} sucursal
 */

/**
 * Opciones inyectables para pruebas unitarias de {@link authApi}.
 * @typedef {Object} AuthOptions
 * @property {import("./apiClient")} [client] Cliente HTTP alternativo (por defecto `apiClient`).
 * @property {Storage} [storage] Almacenamiento alternativo (por defecto `sessionStorage`).
 * @property {boolean} [skipRedirect] Evita redirección al llamar `logout()` en pruebas.
 */

const LOGIN_PATH = "/Cuenta/Login";
const ROL_ADMINISTRADOR = "Administrador";

/**
 * Resuelve la instancia de `apiClient` compatible con navegador y Node.
 * @param {AuthOptions} [opts]
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
    
    throw new Error("apiClient no está disponible. Asegúrate de cargar apiClient.js antes de authApi.js.");
}

/**
 * @param {AuthOptions} [opts]
 * @returns {Storage|null}
 */
function resolveStorage(opts) {
    if (opts && opts.storage) {
        return opts.storage;
    }
    
    return typeof sessionStorage !== "undefined" ? sessionStorage : null;
}

/**
 * Persiste el token JWT y los datos de perfil en `sessionStorage`.
 * @param {any} data Respuesta de `/api/v1/auth/login` (`LoginResponseDto`).
 * @param {import("./apiClient")} client
 * @param {Storage|null} storage
 * @returns {AuthSession}
 */
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
        if (data.token) {
            storage.setItem(client.TOKEN_STORAGE_KEY, data.token);
        }
        
        storage.setItem(client.SESSION_STORAGE_KEY, JSON.stringify(session));
    }

    return session;
}

/**
 * Cliente de autenticación y control de visibilidad RBAC en UI (`SF-024`).
 */
let authApi = {
    ROL_ADMINISTRADOR,

    /**
     * Autentica al usuario contra `POST /api/v1/auth/login` y guarda su JWT y perfil en `sessionStorage`.
     * @param {string} correo
     * @param {string} password
     * @param {AuthOptions} [opts]
     * @returns {Promise<any>} Respuesta `LoginResponseDto` del servidor.
     * @throws {Error} `ApiError` si las credenciales fallan (401) o la cuenta está bloqueada (423).
     */
    async login(correo, password, opts = {}) {
        const client = resolveClient(opts);
        const storage = resolveStorage(opts);
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
     * Cierra la sesión actual limpiando `sessionStorage` y redirigiendo a `/Cuenta/Login`.
     * @param {AuthOptions} [opts]
     * @returns {void}
     */
    logout(opts = {}) {
        const client = resolveClient(opts);
        const storage = resolveStorage(opts);
        
        if (storage) {
            storage.removeItem(client.TOKEN_STORAGE_KEY);
            storage.removeItem(client.SESSION_STORAGE_KEY);
        }
        
        if (!opts.skipRedirect && typeof window !== "undefined") {
            window.location.assign(LOGIN_PATH);
        }
    },

    /**
     * Obtiene el perfil autenticado actual desde `sessionStorage`, o `null` si no existe o está corrupto.
     * @param {AuthOptions} [opts]
     * @returns {AuthSession|null}
     */
    getSession(opts = {}) {
        const client = resolveClient(opts);
        const storage = resolveStorage(opts);
        
        if (!storage) {
            return null;
        }

        const raw = storage.getItem(client.SESSION_STORAGE_KEY);
        if (!raw) {
            return null;
        }

        try {
            return JSON.parse(raw);
        } catch {
            return null;
        }
    },

    /**
     * Aplica visibilidad RBAC declarativa sobre elementos marcados con `data-roles` (ej. `data-roles="Administrador"`).
     * Oculta la sección `ADMINISTRACIÓN` y acciones CRUD destructivas cuando el rol no está autorizado.
     * Nota: Es únicamente visibilidad UX; la autorización real se valida en `SistemaFuneraria.Api`.
     * @param {ParentNode|null} [root] Contenedor DOM a evaluar (por defecto `document`).
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
