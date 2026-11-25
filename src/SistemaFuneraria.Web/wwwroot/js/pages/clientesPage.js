/**
 * Módulo de página para /Clientes (PR 1: conexión CRUD completa contra clientesApi.js).
 * Exporta funciones puras y un inicializador compatible con navegador (UMD) y pruebas unitarias en Node.
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.clientesPage = api;
        if (typeof document !== "undefined") {
            document.addEventListener("DOMContentLoaded", function () {
                api.init();
            });
        }
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    const ROLES_PERMITIDOS = ["Administrador", "Vendedor"];
    const REGEX_TELEFONO = /^[0-9]{10}$/;
    const REGEX_CURP = /^[A-Z0-9]{18}$/;
    const REGEX_CORREO = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    const PAGE_SIZE = 10;

    /**
     * Formatea montos en pesos mexicanos ($#,##0.00).
     * @param {number|string} monto
     * @returns {string}
     */
    function formatearMonedaMXN(monto) {
        const numero = Number(monto);
        const seguro = Number.isFinite(numero) ? numero : 0;
        return new Intl.NumberFormat("es-MX", {
            style: "currency",
            currency: "MXN",
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(seguro);
    }

    /**
     * Formatea un teléfono de 10 dígitos como "XX XXXX XXXX".
     * @param {string|null|undefined} telefono
     * @returns {string}
     */
    function formatearTelefono(telefono) {
        if (!telefono || typeof telefono !== "string") {
            return "";
        }
        const limpio = telefono.replace(/\s+/g, "");
        if (/^[0-9]{10}$/.test(limpio)) {
            return `${limpio.slice(0, 2)} ${limpio.slice(2, 6)} ${limpio.slice(6, 10)}`;
        }
        return telefono.trim();
    }

    /**
     * Formatea una fecha ISO en formato corto es-MX o "Sin pagos" si no existe.
     * @param {string|Date|null|undefined} fechaIso
     * @returns {string}
     */
    function formatearFechaCorta(fechaIso) {
        if (!fechaIso) {
            return "Sin pagos";
        }
        const fecha = fechaIso instanceof Date ? fechaIso : new Date(fechaIso);
        if (Number.isNaN(fecha.getTime())) {
            return "Sin pagos";
        }
        return new Intl.DateTimeFormat("es-MX", {
            day: "2-digit",
            month: "short",
            year: "numeric",
            timeZone: "UTC"
        }).format(fecha);
    }

    /**
     * Devuelve la clase CSS del badge de estado de cartera.
     * @param {string|null|undefined} estado
     * @returns {string}
     */
    function obtenerClaseBadgeEstado(estado) {
        const norm = (estado || "").trim().toLowerCase();
        if (norm === "pago vencido") {
            return "badge-pago-vencido";
        }
        if (norm === "en revisión" || norm === "en revision") {
            return "badge-en-revision";
        }
        return "badge-al-corriente";
    }

    /**
     * Normaliza los campos de entrada del formulario de cliente.
     * @param {Object} datos
     * @returns {{ nombre: string, apellidos: string, curp: string, telefono: string, correo: string, direccion: string }}
     */
    function normalizarDatosCliente(datos = {}) {
        return {
            nombre: String(datos.nombre ?? "").trim(),
            apellidos: String(datos.apellidos ?? "").trim(),
            telefono: String(datos.telefono ?? "").replace(/\s+/g, "").trim(),
            curp: String(datos.curp ?? "").trim().toUpperCase(),
            correo: String(datos.correo ?? "").trim().toLowerCase(),
            direccion: String(datos.direccion ?? "").trim()
        };
    }

    /**
     * Valida los datos del formulario de creación/edición de cliente.
     * Retorna un mensaje de error legible o null si es válido.
     * @param {Object} datos
     * @returns {string|null}
     */
    function validarFormularioCliente(datos) {
        const norm = normalizarDatosCliente(datos);

        if (!norm.nombre) {
            return "El nombre del cliente es obligatorio.";
        }
        if (norm.nombre.length > 100) {
            return "El nombre no puede exceder los 100 caracteres.";
        }
        if (!norm.apellidos) {
            return "Los apellidos del cliente son obligatorios.";
        }
        if (norm.apellidos.length > 100) {
            return "Los apellidos no pueden exceder los 100 caracteres.";
        }
        if (!REGEX_TELEFONO.test(norm.telefono)) {
            return "El teléfono debe contener exactamente 10 dígitos numéricos.";
        }
        if (!REGEX_CURP.test(norm.curp)) {
            return "La CURP debe contener exactamente 18 caracteres alfanuméricos.";
        }
        if (!norm.correo || !REGEX_CORREO.test(norm.correo) || norm.correo.length > 255) {
            return "Ingresa un correo electrónico válido.";
        }
        if (!norm.direccion) {
            return "La dirección del cliente es obligatoria.";
        }

        return null;
    }

    /**
     * Calcula los conteos para las filter pills de estado de cartera.
     * @param {Array<Object>} lista
     * @returns {{ todos: number, alCorriente: number, pagoVencido: number, enRevision: number }}
     */
    function calcularResumenPorEstado(lista) {
        const items = Array.isArray(lista) ? lista : [];
        let alCorriente = 0;
        let pagoVencido = 0;
        let enRevision = 0;

        for (const c of items) {
            const est = (c?.estadoCartera || "").trim().toLowerCase();
            if (est === "pago vencido") {
                pagoVencido++;
            } else if (est === "en revisión" || est === "en revision") {
                enRevision++;
            } else {
                alCorriente++;
            }
        }

        return {
            todos: items.length,
            alCorriente,
            pagoVencido,
            enRevision
        };
    }

    /**
     * Filtra por pill de estado y ordena clientes sin mutar el arreglo original.
     * @param {Array<Object>} lista
     * @param {{ estado?: string, columna?: string|null, ascendente?: boolean }} [criterio]
     * @returns {Array<Object>}
     */
    function filtrarYOrdenarClientes(lista, criterio = {}) {
        const items = Array.isArray(lista) ? lista : [];
        const estadoObjetivo = (criterio.estado || "todos").trim().toLowerCase();
        const columna = criterio.columna || null;
        const asc = criterio.ascendente !== false;

        let resultado = items.filter((c) => {
            if (!c) return false;
            if (estadoObjetivo === "todos") return true;
            const estadoCliente = (c.estadoCartera || "Al corriente").trim().toLowerCase();
            return estadoCliente === estadoObjetivo;
        });

        if (columna) {
            resultado = [...resultado].sort((a, b) => {
                if (columna === "saldo") {
                    const numA = Number(a.saldoActual) || 0;
                    const numB = Number(b.saldoActual) || 0;
                    return asc ? numA - numB : numB - numA;
                }
                if (columna === "fecha") {
                    const timeA = a.ultimoPago ? new Date(a.ultimoPago).getTime() || 0 : 0;
                    const timeB = b.ultimoPago ? new Date(b.ultimoPago).getTime() || 0 : 0;
                    return asc ? timeA - timeB : timeB - timeA;
                }
                const nombreA = `${a.nombre || ""} ${a.apellidos || ""}`.trim();
                const nombreB = `${b.nombre || ""} ${b.apellidos || ""}`.trim();
                return asc
                    ? nombreA.localeCompare(nombreB, "es", { numeric: true })
                    : nombreB.localeCompare(nombreA, "es", { numeric: true });
            });
        }

        return resultado;
    }

    function textoContratos(cantidad) {
        const num = Number(cantidad) || 0;
        if (num === 1) return "1 activo";
        return `${num} activos`;
    }

    function obtenerInicialesCliente(cliente) {
        if (cliente?.iniciales) return cliente.iniciales;
        const n = (cliente?.nombre || "").trim();
        const a = (cliente?.apellidos || "").trim();
        return `${n[0] || "C"}${a[0] || "L"}`.toUpperCase();
    }

    /**
     * Valida sesión JWT y rol autorizado antes de cargar la página.
     * @param {{ authApi: any, apiClient: any, storage?: Storage|null, navigate?: (url: string) => void }} deps
     * @returns {{ autorizado: boolean, redirigido?: boolean, motivo?: string, session?: Object|null }}
     */
    function verificarAcceso(deps) {
        const authApi = deps.authApi;
        const apiClient = deps.apiClient;
        const hasExplicitStorage = deps.storage !== undefined;
        const storageOpts = hasExplicitStorage
            ? { client: apiClient, storage: deps.storage }
            : { client: apiClient };
        const navigate = deps.navigate || ((url) => {
            if (typeof window !== "undefined") {
                window.location.assign(url);
            }
        });

        const token = typeof authApi.getToken === "function"
            ? authApi.getToken(storageOpts)
            : (deps.storage ? deps.storage.getItem(apiClient.TOKEN_STORAGE_KEY) : null);
        const session = authApi.getSession(storageOpts);
        const expirado = typeof authApi.isTokenExpired === "function" && token
            ? authApi.isTokenExpired(token)
            : false;

        if (!token || !session || expirado) {
            let destino = "/Cuenta/Login";
            if (typeof window !== "undefined" && window.location?.pathname) {
                const path = window.location.pathname;
                if (path !== "/" && !path.toLowerCase().startsWith("/cuenta/")) {
                    destino = `/Cuenta/Login?returnUrl=${encodeURIComponent(path + (window.location.search || ""))}`;
                }
            }
            navigate(destino);
            return { autorizado: false, redirigido: true, motivo: "SIN_SESION", session: null };
        }

        if (!ROLES_PERMITIDOS.includes(session.rol)) {
            return { autorizado: false, redirigido: false, motivo: "ROL_NO_AUTORIZADO", session };
        }

        return { autorizado: true, redirigido: false, session };
    }

    /**
     * Inicializa el controlador completo de la vista /Clientes.
     * @param {Object} [deps] Dependencias inyectables para navegador o pruebas.
     */
    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const clientesApi = deps.clientesApi ?? win?.clientesApi;
        const bootstrapObj = deps.bootstrap ?? win?.bootstrap;
        const sfUi = deps.SfUi ?? win?.SfUi;

        if (!doc || !authApi || !apiClient || !clientesApi) {
            return null;
        }

        const contenidoPrincipal = doc.getElementById("clientesContenidoPrincipal");
        const accesoRestringidoEl = doc.getElementById("clientesAccesoRestringido");
        const btnAbrirNuevoCliente = doc.getElementById("btnAbrirNuevoCliente");
        const tbody = doc.getElementById("tbodyClientes");

        if (!tbody) {
            return null;
        }

        function mostrarAccesoRestringido(session) {
            authApi.applyRolePermissions(doc);
            if (contenidoPrincipal) contenidoPrincipal.classList.add("d-none");
            if (btnAbrirNuevoCliente) {
                btnAbrirNuevoCliente.classList.remove("d-inline-flex");
                btnAbrirNuevoCliente.classList.add("d-none");
            }
            const btnVolver = doc.getElementById("btnVolver403Clientes");
            if (btnVolver) {
                const rutaDestino = typeof authApi.getDefaultRouteForRole === "function"
                    ? authApi.getDefaultRouteForRole(session?.rol)
                    : "/Caja";
                btnVolver.setAttribute("href", rutaDestino);
            }
            if (accesoRestringidoEl) accesoRestringidoEl.classList.remove("d-none");
        }

        const estadoAcceso = verificarAcceso({
            authApi,
            apiClient,
            storage: deps.storage,
            navigate: deps.navigate
        });

        if (!estadoAcceso.autorizado) {
            if (!estadoAcceso.redirigido) {
                mostrarAccesoRestringido(estadoAcceso.session);
            }
            return { autorizado: false, motivo: estadoAcceso.motivo };
        }

        authApi.applyRolePermissions(doc);

        // Referencias DOM
        const inputBuscar = doc.getElementById("inputBuscarClientes");
        const btnClearSearch = doc.getElementById("btnClearSearch");
        const btnResetFiltros = doc.getElementById("btnRestablecerFiltros");
        const btnReintentar = doc.getElementById("btnReintentarClientes");
        const btnExportar = doc.getElementById("btnExportarDirectorio");
        const filaSinResultados = doc.getElementById("filaSinResultados");
        const filaErrorClientes = doc.getElementById("filaErrorClientes");
        const mensajeErrorClientes = doc.getElementById("mensajeErrorClientes");
        const contadorResultados = doc.getElementById("contadorResultados");
        const pagDesde = doc.getElementById("pagDesde");
        const pagHasta = doc.getElementById("pagHasta");
        const pagTotal = doc.getElementById("pagTotal");
        const paginacionUl = doc.getElementById("paginacionClientes");
        const filterPills = doc.querySelectorAll(".filter-pill");
        const sortHeaders = doc.querySelectorAll(".th-sortable");

        // Badges de conteo
        const badgeTodos = doc.getElementById("badgeTotalTodos");
        const badgeAlCorriente = doc.getElementById("badgeTotalAlCorriente");
        const badgePagoVencido = doc.getElementById("badgeTotalPagoVencido");
        const badgeEnRevision = doc.getElementById("badgeTotalEnRevision");

        // Modal Nuevo Cliente
        const modalNuevoEl = doc.getElementById("modalNuevoCliente");
        const formNuevoCliente = doc.getElementById("formNuevoCliente");
        const modalNuevoError = doc.getElementById("modalNuevoClienteError");
        const btnGuardarNuevo = doc.getElementById("btnGuardarClienteModal");

        // Drawer Expediente
        const drawerEl = doc.getElementById("drawerExpedienteCliente");
        const drawerIniciales = doc.getElementById("drawerClienteIniciales");
        const drawerNombre = doc.getElementById("drawerClienteNombre");
        const drawerFechaAlta = doc.getElementById("drawerClienteFechaAlta");
        const drawerAlert = doc.getElementById("drawerClienteAlert");
        const drawerContratos = doc.getElementById("drawerClienteContratos");
        const drawerSaldo = doc.getElementById("drawerClienteSaldo");
        const drawerUltimoPago = doc.getElementById("drawerClienteUltimoPago");
        const drawerEstado = doc.getElementById("drawerClienteEstado");
        const formEditarCliente = doc.getElementById("formEditarCliente");
        const drawerEditId = doc.getElementById("drawerEditId");
        const drawerEditNombre = doc.getElementById("drawerEditNombre");
        const drawerEditApellidos = doc.getElementById("drawerEditApellidos");
        const drawerEditTelefono = doc.getElementById("drawerEditTelefono");
        const drawerEditCurp = doc.getElementById("drawerEditCurp");
        const drawerEditCorreo = doc.getElementById("drawerEditCorreo");
        const drawerEditDireccion = doc.getElementById("drawerEditDireccion");
        const btnGuardarEdicion = doc.getElementById("btnGuardarEdicionCliente");
        const btnEliminarCliente = doc.getElementById("btnEliminarCliente");
        const panelConfirmarEliminacion = doc.getElementById("panelConfirmarEliminacion");
        const btnConfirmarEliminar = doc.getElementById("btnConfirmarEliminarCliente");
        const btnCancelarEliminar = doc.getElementById("btnCancelarEliminarCliente");

        // Estado interno
        let clientesCargados = [];
        let filtroEstadoActivo = "todos";
        let ordenActual = { columna: null, ascendente: true };
        let paginaActual = 1;
        let secuenciaCarga = 0;
        let debounceTimer = null;
        let clienteSeleccionadoId = null;

        function notificarToast(type, title, message) {
            if (sfUi && typeof sfUi.showToast === "function") {
                sfUi.showToast({ type, title, message });
            }
        }

        function mostrarAlertaElemento(el, tipo, mensaje) {
            if (!el) return;
            const textoLimpio = String(mensaje ?? "").replace(/^\[(CONFLICTO|NO_ENCONTRADO)\]\s*/i, "");
            el.className = `alert alert-${tipo} rounded-3 small py-2 px-3 mb-3`;
            el.textContent = textoLimpio;
        }

        function ocultarAlertaElemento(el) {
            if (!el) return;
            el.className = "alert d-none rounded-3 small py-2 px-3 mb-3";
            el.textContent = "";
        }

        function alternarSkeleton(visible) {
            const skeletons = tbody.querySelectorAll(".skeleton-row");
            skeletons.forEach((row) => {
                row.style.display = visible ? "" : "none";
            });
        }

        function limpiarFilasDatos() {
            tbody.querySelectorAll("tr.cliente-data-row").forEach((row) => row.remove());
        }

        function actualizarBadges(lista) {
            const conteos = calcularResumenPorEstado(lista);
            if (badgeTodos) badgeTodos.textContent = String(conteos.todos);
            if (badgeAlCorriente) badgeAlCorriente.textContent = String(conteos.alCorriente);
            if (badgePagoVencido) badgePagoVencido.textContent = String(conteos.pagoVencido);
            if (badgeEnRevision) badgeEnRevision.textContent = String(conteos.enRevision);
        }

        function crearCeldaTexto(texto, className = "") {
            const td = doc.createElement("td");
            if (className) td.className = className;
            td.textContent = texto;
            return td;
        }

        function construirFilaCliente(cliente) {
            const nombreCompleto = `${cliente.nombre || ""} ${cliente.apellidos || ""}`.trim();
            const estadoTexto = cliente.estadoCartera || "Al corriente";
            const iniciales = obtenerInicialesCliente(cliente);

            const tr = doc.createElement("tr");
            tr.className = "cliente-data-row";
            tr.setAttribute("data-id", cliente.id || "");
            tr.setAttribute("data-curp", cliente.curp || "");
            tr.setAttribute("data-estado", estadoTexto.toLowerCase());
            tr.setAttribute("data-nombre", nombreCompleto);

            // Columna 1: Avatar + Nombre + Fecha alta
            const tdCliente = doc.createElement("td");
            tdCliente.className = "ps-4";
            const wrapper = doc.createElement("div");
            wrapper.className = "d-flex align-items-center gap-3";

            const avatar = doc.createElement("div");
            avatar.className = "avatar-funeraria";
            avatar.setAttribute("aria-hidden", "true");
            avatar.textContent = iniciales;

            const info = doc.createElement("div");
            const divNombre = doc.createElement("div");
            divNombre.className = "fw-bold text-dark nombre-cliente";
            divNombre.textContent = nombreCompleto;

            const divAlta = doc.createElement("div");
            divAlta.className = "text-muted small";
            divAlta.textContent = cliente.fechaAltaFormateada || "Cliente registrado";

            info.append(divNombre, divAlta);
            wrapper.append(avatar, info);
            tdCliente.appendChild(wrapper);

            // Columna 2: Contacto
            const tdContacto = doc.createElement("td");
            const divTel = doc.createElement("div");
            divTel.className = "text-dark";
            divTel.textContent = formatearTelefono(cliente.telefono);
            const divCorreo = doc.createElement("div");
            divCorreo.className = "text-muted small";
            divCorreo.textContent = cliente.correo || "";
            tdContacto.append(divTel, divCorreo);

            // Columna 3: Contratos
            const tdContratos = crearCeldaTexto(textoContratos(cliente.contratosActivos));

            // Columna 4: Saldo actual
            const tdSaldo = crearCeldaTexto(
                formatearMonedaMXN(cliente.saldoActual),
                "text-end fw-bold formato-moneda text-tabular"
            );

            // Columna 5: Último pago
            const tdUltimoPago = crearCeldaTexto(formatearFechaCorta(cliente.ultimoPago));

            // Columna 6: Estado
            const tdEstado = doc.createElement("td");
            const badge = doc.createElement("span");
            badge.className = `badge badge-estado ${obtenerClaseBadgeEstado(estadoTexto)}`;
            badge.textContent = estadoTexto;
            tdEstado.appendChild(badge);

            // Columna 7: Acción Abrir
            const tdAccion = doc.createElement("td");
            tdAccion.className = "text-end pe-4";
            const btnAbrir = doc.createElement("button");
            btnAbrir.type = "button";
            btnAbrir.className = "btn btn-link p-0 text-decoration-none funeraria-action-link btn-abrir-expediente";
            btnAbrir.setAttribute("data-cliente-id", cliente.id || "");
            btnAbrir.setAttribute("title", `Abrir expediente de ${nombreCompleto}`);

            const spanTexto = doc.createElement("span");
            spanTexto.textContent = "Abrir";
            const icono = doc.createElement("i");
            icono.className = "fa-solid fa-chevron-right ms-1 small";
            icono.setAttribute("aria-hidden", "true");

            btnAbrir.append(spanTexto, icono);
            btnAbrir.addEventListener("click", function () {
                abrirExpediente(cliente.id);
            });
            tdAccion.appendChild(btnAbrir);

            tr.append(tdCliente, tdContacto, tdContratos, tdSaldo, tdUltimoPago, tdEstado, tdAccion);
            return tr;
        }

        function renderizarPaginacion(totalPaginas) {
            if (!paginacionUl) return;
            paginacionUl.innerHTML = "";

            const crearBotonPagina = (label, paginaDestino, deshabilitado, activo, ariaLabel, iconClass = null) => {
                const li = doc.createElement("li");
                const btn = doc.createElement("button");
                btn.type = "button";
                btn.className = `page-btn${activo ? " active" : ""}${deshabilitado ? " disabled" : ""}`;
                if (ariaLabel) btn.setAttribute("aria-label", ariaLabel);
                if (activo) btn.setAttribute("aria-current", "page");
                btn.disabled = deshabilitado;

                if (iconClass) {
                    const icon = doc.createElement("i");
                    icon.className = iconClass;
                    icon.setAttribute("aria-hidden", "true");
                    btn.appendChild(icon);
                } else {
                    btn.textContent = label;
                }

                if (!deshabilitado && !activo) {
                    btn.addEventListener("click", function () {
                        paginaActual = paginaDestino;
                        renderizarTabla();
                    });
                }

                li.appendChild(btn);
                return li;
            };

            paginacionUl.appendChild(
                crearBotonPagina("", paginaActual - 1, paginaActual <= 1, false, "Página anterior", "fa-solid fa-chevron-left small")
            );

            for (let p = 1; p <= totalPaginas; p++) {
                paginacionUl.appendChild(crearBotonPagina(String(p), p, false, p === paginaActual, `Página ${p}`));
            }

            paginacionUl.appendChild(
                crearBotonPagina("", paginaActual + 1, paginaActual >= totalPaginas, false, "Página siguiente", "fa-solid fa-chevron-right small")
            );
        }

        function renderizarTabla() {
            alternarSkeleton(false);
            if (filaErrorClientes) filaErrorClientes.style.display = "none";
            limpiarFilasDatos();

            const filtrados = filtrarYOrdenarClientes(clientesCargados, {
                estado: filtroEstadoActivo,
                columna: ordenActual.columna,
                ascendente: ordenActual.ascendente
            });

            const totalFiltrados = filtrados.length;
            const totalPaginas = Math.max(1, Math.ceil(totalFiltrados / PAGE_SIZE));
            if (paginaActual > totalPaginas) {
                paginaActual = totalPaginas;
            }

            const inicio = (paginaActual - 1) * PAGE_SIZE;
            const visibles = filtrados.slice(inicio, inicio + PAGE_SIZE);

            if (filaSinResultados) {
                filaSinResultados.style.display = totalFiltrados === 0 ? "" : "none";
            }

            const anchor = filaSinResultados || null;
            for (const cliente of visibles) {
                const fila = construirFilaCliente(cliente);
                tbody.insertBefore(fila, anchor);
            }

            const desde = totalFiltrados === 0 ? 0 : inicio + 1;
            const hasta = totalFiltrados === 0 ? 0 : inicio + visibles.length;

            if (contadorResultados) {
                contadorResultados.textContent = `Mostrando ${visibles.length} de ${totalFiltrados} clientes`;
            }
            if (pagDesde) pagDesde.textContent = String(desde);
            if (pagHasta) pagHasta.textContent = String(hasta);
            if (pagTotal) pagTotal.textContent = String(totalFiltrados);

            renderizarPaginacion(totalPaginas);
        }

        async function cargarClientes(termino = "") {
            const requestId = ++secuenciaCarga;
            if (filaSinResultados) filaSinResultados.style.display = "none";
            if (filaErrorClientes) filaErrorClientes.style.display = "none";
            limpiarFilasDatos();
            alternarSkeleton(true);
            if (contadorResultados) {
                contadorResultados.textContent = "Cargando clientes...";
            }

            try {
                const resultado = await clientesApi.listar(termino || null);
                if (requestId !== secuenciaCarga) return;

                clientesCargados = Array.isArray(resultado) ? resultado : [];
                actualizarBadges(clientesCargados);
                renderizarTabla();
            } catch (err) {
                if (requestId !== secuenciaCarga) return;
                alternarSkeleton(false);
                limpiarFilasDatos();

                if (err?.status === 403) {
                    mostrarAccesoRestringido();
                    return;
                }

                if (mensajeErrorClientes) {
                    mensajeErrorClientes.textContent = err?.message || "No fue posible consultar los clientes.";
                }
                if (filaErrorClientes) {
                    filaErrorClientes.style.display = "";
                }
                if (contadorResultados) {
                    contadorResultados.textContent = "Error al cargar clientes";
                }
            }
        }

        function poblarDrawer(cliente) {
            const nombreCompleto = `${cliente.nombre || ""} ${cliente.apellidos || ""}`.trim();
            const estadoTexto = cliente.estadoCartera || "Al corriente";

            if (drawerIniciales) drawerIniciales.textContent = obtenerInicialesCliente(cliente);
            if (drawerNombre) drawerNombre.textContent = nombreCompleto || "Expediente del cliente";
            if (drawerFechaAlta) drawerFechaAlta.textContent = cliente.fechaAltaFormateada || "Cliente registrado";
            if (drawerContratos) drawerContratos.textContent = textoContratos(cliente.contratosActivos);
            if (drawerSaldo) drawerSaldo.textContent = formatearMonedaMXN(cliente.saldoActual);
            if (drawerUltimoPago) drawerUltimoPago.textContent = formatearFechaCorta(cliente.ultimoPago);
            if (drawerEstado) {
                drawerEstado.className = `badge badge-estado ${obtenerClaseBadgeEstado(estadoTexto)}`;
                drawerEstado.textContent = estadoTexto;
            }

            if (drawerEditId) drawerEditId.value = cliente.id || "";
            if (drawerEditNombre) drawerEditNombre.value = cliente.nombre || "";
            if (drawerEditApellidos) drawerEditApellidos.value = cliente.apellidos || "";
            if (drawerEditTelefono) drawerEditTelefono.value = cliente.telefono || "";
            if (drawerEditCurp) drawerEditCurp.value = cliente.curp || "";
            if (drawerEditCorreo) drawerEditCorreo.value = cliente.correo || "";
            if (drawerEditDireccion) drawerEditDireccion.value = cliente.direccion || "";

            const btnNuevaVenta = doc.getElementById("btnDrawerNuevaVenta");
            if (btnNuevaVenta) {
                btnNuevaVenta.setAttribute("href", cliente.id ? `/Ventas/Nueva?clienteId=${encodeURIComponent(cliente.id)}` : "/Ventas/Nueva");
            }
            const btnVerContratos = doc.getElementById("btnDrawerVerContratos");
            if (btnVerContratos) {
                const filtroContrato = cliente.curp || nombreCompleto;
                btnVerContratos.setAttribute("href", filtroContrato ? `/Contratos?buscar=${encodeURIComponent(filtroContrato)}` : "/Contratos");
            }
        }

        async function abrirExpediente(id) {
            if (!id || !drawerEl) return;
            clienteSeleccionadoId = id;
            ocultarAlertaElemento(drawerAlert);
            if (panelConfirmarEliminacion) {
                panelConfirmarEliminacion.classList.add("d-none");
            }

            const local = clientesCargados.find((c) => c.id === id);
            if (local) {
                poblarDrawer(local);
            }

            authApi.applyRolePermissions(drawerEl);

            if (bootstrapObj?.Offcanvas) {
                bootstrapObj.Offcanvas.getOrCreateInstance(drawerEl).show();
            }

            try {
                const detalle = await clientesApi.obtenerPorId(id);
                if (clienteSeleccionadoId === id && detalle) {
                    poblarDrawer(detalle);
                }
            } catch (err) {
                if (clienteSeleccionadoId === id) {
                    mostrarAlertaElemento(drawerAlert, "danger", err?.message || "No se pudo actualizar el detalle del cliente.");
                }
            }
        }

        // Búsqueda con debounce
        if (inputBuscar) {
            inputBuscar.addEventListener("input", function () {
                const termino = inputBuscar.value.trim();
                if (btnClearSearch) {
                    btnClearSearch.style.display = termino.length > 0 ? "block" : "none";
                }
                if (debounceTimer) clearTimeout(debounceTimer);
                debounceTimer = setTimeout(function () {
                    paginaActual = 1;
                    cargarClientes(termino);
                }, 250);
            });
        }

        if (btnClearSearch && inputBuscar) {
            btnClearSearch.addEventListener("click", function () {
                inputBuscar.value = "";
                btnClearSearch.style.display = "none";
                inputBuscar.focus();
                paginaActual = 1;
                cargarClientes("");
            });
        }

        function restablecerFiltros() {
            if (inputBuscar) inputBuscar.value = "";
            if (btnClearSearch) btnClearSearch.style.display = "none";
            filtroEstadoActivo = "todos";
            paginaActual = 1;
            filterPills.forEach((pill) => {
                const isTodos = pill.getAttribute("data-filter") === "todos";
                pill.classList.toggle("active", isTodos);
                pill.setAttribute("aria-pressed", isTodos ? "true" : "false");
            });
            cargarClientes("");
        }

        if (btnResetFiltros) {
            btnResetFiltros.addEventListener("click", restablecerFiltros);
        }

        if (btnReintentar) {
            btnReintentar.addEventListener("click", function () {
                cargarClientes(inputBuscar ? inputBuscar.value.trim() : "");
            });
        }

        // Pills de estado
        filterPills.forEach((pill) => {
            pill.addEventListener("click", function () {
                filterPills.forEach((p) => {
                    p.classList.remove("active");
                    p.setAttribute("aria-pressed", "false");
                });
                this.classList.add("active");
                this.setAttribute("aria-pressed", "true");
                filtroEstadoActivo = this.getAttribute("data-filter") || "todos";
                paginaActual = 1;
                renderizarTabla();
            });
        });

        // Ordenamiento accesible por cabeceras (clic y teclado)
        function toggleOrdenCabecera(th) {
            const col = th.getAttribute("data-sort-col");
            if (ordenActual.columna === col) {
                ordenActual.ascendente = !ordenActual.ascendente;
            } else {
                ordenActual.columna = col;
                ordenActual.ascendente = true;
            }

            sortHeaders.forEach((header) => {
                header.setAttribute("aria-sort", "none");
                const icon = header.querySelector(".sort-icon");
                if (icon) icon.className = "fa-solid fa-sort sort-icon";
            });
            th.setAttribute("aria-sort", ordenActual.ascendente ? "ascending" : "descending");
            const currentIcon = th.querySelector(".sort-icon");
            if (currentIcon) {
                currentIcon.className = ordenActual.ascendente
                    ? "fa-solid fa-sort-up sort-icon text-dark"
                    : "fa-solid fa-sort-down sort-icon text-dark";
            }

            paginaActual = 1;
            renderizarTabla();
        }

        sortHeaders.forEach((th) => {
            th.addEventListener("click", function () {
                toggleOrdenCabecera(this);
            });
            th.addEventListener("keydown", function (e) {
                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    toggleOrdenCabecera(this);
                }
            });
        });

        // Exportación CSV real
        if (btnExportar) {
            btnExportar.addEventListener("click", async function () {
                const originalHtml = btnExportar.innerHTML;
                btnExportar.disabled = true;
                btnExportar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1" aria-hidden="true"></i> <span>Exportando...</span>';

                try {
                    await clientesApi.exportarDirectorio();
                    notificarToast("success", "Directorio exportado", "El archivo CSV del padrón de clientes se descargó correctamente.");
                } catch (err) {
                    notificarToast("error", "Error de exportación", "No fue posible exportar el directorio de clientes.");
                } finally {
                    btnExportar.disabled = false;
                    btnExportar.innerHTML = originalHtml;
                }
            });
        }

        // Alta en Modal Nuevo Cliente
        if (modalNuevoEl && formNuevoCliente) {
            modalNuevoEl.addEventListener("hidden.bs.modal", function () {
                formNuevoCliente.reset();
                ocultarAlertaElemento(modalNuevoError);
            });

            formNuevoCliente.addEventListener("submit", async function (e) {
                e.preventDefault();
                ocultarAlertaElemento(modalNuevoError);

                const datos = {
                    nombre: doc.getElementById("modalClienteNombre")?.value,
                    apellidos: doc.getElementById("modalClienteApellidos")?.value,
                    telefono: doc.getElementById("modalClienteTelefono")?.value,
                    curp: doc.getElementById("modalClienteCurp")?.value,
                    correo: doc.getElementById("modalClienteEmail")?.value,
                    direccion: doc.getElementById("modalClienteDireccion")?.value
                };

                const errorValidacion = validarFormularioCliente(datos);
                if (errorValidacion) {
                    mostrarAlertaElemento(modalNuevoError, "danger", errorValidacion);
                    return;
                }

                const payload = normalizarDatosCliente(datos);
                const textoOriginal = btnGuardarNuevo ? btnGuardarNuevo.innerHTML : "";
                if (btnGuardarNuevo) {
                    btnGuardarNuevo.disabled = true;
                    btnGuardarNuevo.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1" aria-hidden="true"></i> Guardando...';
                }

                try {
                    await clientesApi.registrar(payload);
                    if (bootstrapObj?.Modal) {
                        bootstrapObj.Modal.getOrCreateInstance(modalNuevoEl).hide();
                    }
                    formNuevoCliente.reset();
                    notificarToast("success", "Cliente registrado", "El nuevo cliente fue guardado en el directorio.");
                    await cargarClientes(inputBuscar ? inputBuscar.value.trim() : "");
                } catch (err) {
                    mostrarAlertaElemento(modalNuevoError, "danger", err?.message || "No se pudo registrar el cliente.");
                } finally {
                    if (btnGuardarNuevo) {
                        btnGuardarNuevo.disabled = false;
                        btnGuardarNuevo.innerHTML = textoOriginal || "Guardar cliente";
                    }
                }
            });
        }

        // Edición en Drawer Expediente
        if (formEditarCliente) {
            formEditarCliente.addEventListener("submit", async function (e) {
                e.preventDefault();
                ocultarAlertaElemento(drawerAlert);

                const id = drawerEditId?.value;
                if (!id) return;

                const datos = {
                    nombre: drawerEditNombre?.value,
                    apellidos: drawerEditApellidos?.value,
                    telefono: drawerEditTelefono?.value,
                    curp: drawerEditCurp?.value,
                    correo: drawerEditCorreo?.value,
                    direccion: drawerEditDireccion?.value
                };

                const errorValidacion = validarFormularioCliente(datos);
                if (errorValidacion) {
                    mostrarAlertaElemento(drawerAlert, "danger", errorValidacion);
                    return;
                }

                const payload = normalizarDatosCliente(datos);
                const textoOriginal = btnGuardarEdicion ? btnGuardarEdicion.innerHTML : "";
                if (btnGuardarEdicion) {
                    btnGuardarEdicion.disabled = true;
                    btnGuardarEdicion.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1" aria-hidden="true"></i> Guardando...';
                }

                try {
                    await clientesApi.actualizar(id, payload);
                    mostrarAlertaElemento(drawerAlert, "success", "Los datos del cliente se actualizaron correctamente.");
                    notificarToast("success", "Expediente actualizado", "Los cambios del cliente fueron guardados.");
                    await cargarClientes(inputBuscar ? inputBuscar.value.trim() : "");
                    const actualizado = clientesCargados.find((c) => c.id === id);
                    if (actualizado) {
                        poblarDrawer(actualizado);
                    }
                } catch (err) {
                    mostrarAlertaElemento(drawerAlert, "danger", err?.message || "No se pudieron guardar los cambios.");
                } finally {
                    if (btnGuardarEdicion) {
                        btnGuardarEdicion.disabled = false;
                        btnGuardarEdicion.innerHTML = textoOriginal || "Guardar cambios";
                    }
                }
            });
        }

        // Eliminación Admin en Drawer
        if (btnEliminarCliente && panelConfirmarEliminacion) {
            btnEliminarCliente.addEventListener("click", function () {
                ocultarAlertaElemento(drawerAlert);
                panelConfirmarEliminacion.classList.remove("d-none");
            });
        }

        if (btnCancelarEliminar && panelConfirmarEliminacion) {
            btnCancelarEliminar.addEventListener("click", function () {
                panelConfirmarEliminacion.classList.add("d-none");
            });
        }

        if (btnConfirmarEliminar) {
            btnConfirmarEliminar.addEventListener("click", async function () {
                const id = drawerEditId?.value;
                if (!id) return;

                ocultarAlertaElemento(drawerAlert);
                const textoOriginal = btnConfirmarEliminar.innerHTML;
                btnConfirmarEliminar.disabled = true;
                btnConfirmarEliminar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1" aria-hidden="true"></i> Eliminando...';

                try {
                    await clientesApi.eliminar(id);
                    if (panelConfirmarEliminacion) {
                        panelConfirmarEliminacion.classList.add("d-none");
                    }
                    if (drawerEl && bootstrapObj?.Offcanvas) {
                        bootstrapObj.Offcanvas.getOrCreateInstance(drawerEl).hide();
                    }
                    notificarToast("success", "Cliente eliminado", "El cliente fue eliminado del padrón.");
                    await cargarClientes(inputBuscar ? inputBuscar.value.trim() : "");
                } catch (err) {
                    if (panelConfirmarEliminacion) {
                        panelConfirmarEliminacion.classList.add("d-none");
                    }
                    mostrarAlertaElemento(drawerAlert, "danger", err?.message || "No se pudo eliminar el cliente.");
                } finally {
                    btnConfirmarEliminar.disabled = false;
                    btnConfirmarEliminar.innerHTML = textoOriginal || "Sí, eliminar";
                }
            });
        }

        // Carga inicial desde API
        cargarClientes("");

        return {
            autorizado: true,
            recargar: () => cargarClientes(inputBuscar ? inputBuscar.value.trim() : ""),
            abrirExpediente
        };
    }

    return {
        PAGE_SIZE,
        ROLES_PERMITIDOS,
        formatearMonedaMXN,
        formatearTelefono,
        formatearFechaCorta,
        obtenerClaseBadgeEstado,
        normalizarDatosCliente,
        validarFormularioCliente,
        calcularResumenPorEstado,
        filtrarYOrdenarClientes,
        verificarAcceso,
        init
    };
});
