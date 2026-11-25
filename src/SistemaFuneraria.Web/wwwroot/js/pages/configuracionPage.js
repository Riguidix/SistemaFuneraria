/**
 * configuracionPage.js - Conector real para /Configuracion/Index (SF-091 / SF-092).
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.configuracionPage = api;
        if (typeof document !== "undefined") {
            document.addEventListener("DOMContentLoaded", function () {
                api.init();
            });
        }
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    const ROLES_PERMITIDOS = ["Administrador"];
    const ROLES_USUARIO_VALIDOS = ["Administrador", "Vendedor", "Cajero"];
    const REGEX_CORREO = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    function escapeHtml(text) {
        return String(text ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function obtenerIniciales(nombre) {
        const partes = String(nombre || "").trim().split(/\s+/).filter(Boolean);
        if (partes.length === 0) return "US";
        if (partes.length === 1) return partes[0].slice(0, 2).toUpperCase();
        return (partes[0][0] + partes[1][0]).toUpperCase();
    }

    function validarFormularioUsuario(datos = {}, esEdicion = false) {
        const nombre = String(datos.nombre ?? "").trim();
        const correo = String(datos.correo ?? "").trim();
        const rol = String(datos.rol ?? "").trim();
        const password = String(datos.password ?? "");

        if (!nombre) {
            return "El nombre completo del colaborador es obligatorio.";
        }
        if (!esEdicion && (!correo || !REGEX_CORREO.test(correo))) {
            return "Ingresa un correo electrónico válido.";
        }
        if (!ROLES_USUARIO_VALIDOS.includes(rol)) {
            return "Selecciona un rol operativo válido (Administrador, Vendedor o Cajero).";
        }
        if (!esEdicion && password.length < 6) {
            return "La contraseña es obligatoria y debe tener al menos 6 caracteres.";
        }
        if (esEdicion && password.length > 0 && password.length < 6) {
            return "Si cambias la contraseña, debe tener al menos 6 caracteres.";
        }
        return null;
    }

    function limpiarPrefijoError(msg) {
        return String(msg || "").replace(/^\[(CONFLICTO|NO_ENCONTRADO)\]\s*/i, "");
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const configuracionApi = deps.configuracionApi ?? win?.configuracionApi;
        const bootstrapObj = deps.bootstrap ?? win?.bootstrap;
        const sfUi = deps.SfUi ?? win?.SfUi;

        if (!doc) return null;

        const hasExplicitStorage = deps.storage !== undefined;
        const storageOpts = apiClient
            ? (hasExplicitStorage ? { client: apiClient, storage: deps.storage } : { client: apiClient })
            : {};

        const principal = doc.getElementById("configContenidoPrincipal");
        const accesoRestringido = doc.getElementById("configAccesoRestringido");
        const btnVolver403 = doc.getElementById("btnVolver403Config");
        const alertaError = doc.getElementById("alertaErrorConfig");
        const mensajeError = doc.getElementById("mensajeErrorConfig");
        const btnReintentar = doc.getElementById("btnReintentarConfig");

        function verificarAcceso() {
            if (!authApi || !apiClient) {
                return { autorizado: true, redirigido: false, session: null };
            }

            const token = typeof authApi.getToken === "function"
                ? authApi.getToken(storageOpts)
                : (deps.storage ? deps.storage.getItem(apiClient.TOKEN_STORAGE_KEY) : null);
            const session = typeof authApi.getSession === "function"
                ? authApi.getSession(storageOpts)
                : null;
            const expirado = typeof authApi.isTokenExpired === "function" && token
                ? authApi.isTokenExpired(token)
                : false;

            if (!token || !session || expirado) {
                if (typeof authApi.logout === "function") {
                    authApi.logout({ ...storageOpts, skipRedirect: true });
                }
                let destino = "/Cuenta/Login";
                const path = win?.location?.pathname || "";
                if (path && path !== "/" && !path.toLowerCase().startsWith("/cuenta/")) {
                    destino = `/Cuenta/Login?returnUrl=${encodeURIComponent(path + (win?.location?.search || ""))}`;
                }
                win?.location?.assign?.(destino);
                return { autorizado: false, redirigido: true, session: null };
            }

            if (session.rol && !ROLES_PERMITIDOS.includes(session.rol)) {
                if (btnVolver403) {
                    const rutaInicio = typeof authApi.getDefaultRouteForRole === "function"
                        ? authApi.getDefaultRouteForRole(session.rol)
                        : "/Resumen";
                    btnVolver403.setAttribute("href", rutaInicio);
                }
                if (principal) principal.classList.add("d-none");
                if (alertaError) alertaError.classList.add("d-none");
                if (accesoRestringido) accesoRestringido.classList.remove("d-none");
                return { autorizado: false, redirigido: false, session };
            }

            return { autorizado: true, redirigido: false, session };
        }

        const accInicial = verificarAcceso();
        if (!accInicial.autorizado) {
            return { autorizado: false, redirigido: accInicial.redirigido, session: accInicial.session };
        }

        const navButtons = Array.from(doc.querySelectorAll(".sf-config-nav-item[data-target]"));
        const panels = Array.from(doc.querySelectorAll(".sf-config-panel"));

        const inputNombre = doc.getElementById("inputNombreComercial");
        const inputTelefono = doc.getElementById("inputTelefono");
        const inputCorreo = doc.getElementById("inputCorreo");
        const inputDomicilio = doc.getElementById("inputDomicilio");
        const inputCiudad = doc.getElementById("inputCiudad");
        const inputCp = doc.getElementById("inputCodigoPostal");
        const switchRec = doc.getElementById("switchRecordatorios");
        const switchRes = doc.getElementById("switchResumenCaja");
        const btnGuardar = doc.getElementById("btnGuardarCambios");
        const tbodyUsuarios = doc.getElementById("tbodyUsuariosConfig");

        const btnNuevoUsuario = doc.getElementById("btnNuevoUsuarioConfig");
        const modalUsuario = doc.getElementById("modalUsuarioConfig");
        const modalUsuarioLabel = doc.getElementById("modalUsuarioConfigLabel");
        const alertaModalUsuario = doc.getElementById("alertaModalUsuarioConfig");
        const inputUserId = doc.getElementById("usuarioConfigId");
        const inputUserNombre = doc.getElementById("usuarioConfigNombre");
        const inputUserCorreo = doc.getElementById("usuarioConfigCorreo");
        const selectUserRol = doc.getElementById("usuarioConfigRol");
        const inputUserPassword = doc.getElementById("usuarioConfigPassword");
        const ayudaUserPassword = doc.getElementById("ayudaPasswordUsuarioConfig");
        const btnGuardarUsuario = doc.getElementById("btnGuardarUsuarioConfig");

        let usuariosCargados = [];

        function notificar(tipo, mensaje) {
            if (sfUi && typeof sfUi.showToast === "function") {
                sfUi.showToast({ type: tipo, title: tipo === "error" ? "Error" : "Configuración", message: limpiarPrefijoError(mensaje) });
            }
        }

        function mostrarError(msg) {
            const texto = limpiarPrefijoError(msg) || "No fue posible cargar la configuración del sistema.";
            if (alertaError) {
                if (mensajeError) {
                    mensajeError.textContent = texto;
                } else {
                    alertaError.textContent = texto;
                }
                alertaError.classList.remove("d-none");
            }
        }

        function ocultarError() {
            if (alertaError) alertaError.classList.add("d-none");
        }

        function mostrarErrorModalUsuario(msg) {
            if (!alertaModalUsuario) return;
            alertaModalUsuario.textContent = limpiarPrefijoError(msg);
            alertaModalUsuario.classList.remove("d-none");
        }

        function ocultarErrorModalUsuario() {
            if (!alertaModalUsuario) return;
            alertaModalUsuario.classList.add("d-none");
            alertaModalUsuario.textContent = "";
        }

        function activarSeccion(targetId) {
            navButtons.forEach(function (btn) {
                const isTarget = btn.getAttribute("data-target") === targetId;
                const icon = btn.querySelector(".sf-nav-icon");
                btn.classList.toggle("active", isTarget);
                if (isTarget) {
                    btn.setAttribute("aria-current", "page");
                } else if (typeof btn.removeAttribute === "function") {
                    btn.removeAttribute("aria-current");
                }
                if (icon) {
                    icon.classList.toggle("text-success", isTarget);
                    icon.classList.toggle("text-muted", !isTarget);
                }
            });
            panels.forEach(function (panel) {
                panel.classList.toggle("d-none", panel.id !== targetId);
            });
            if (btnGuardar) {
                btnGuardar.classList.toggle("d-none", targetId !== "panel-datos-sucursal");
            }
        }

        navButtons.forEach(function (btn) {
            btn.addEventListener("click", function () {
                activarSeccion(btn.getAttribute("data-target"));
            });
        });

        function abrirModalNuevoUsuario() {
            ocultarErrorModalUsuario();
            if (modalUsuarioLabel) modalUsuarioLabel.textContent = "Nuevo usuario";
            if (inputUserId) inputUserId.value = "";
            if (inputUserNombre) inputUserNombre.value = "";
            if (inputUserCorreo) {
                inputUserCorreo.value = "";
                inputUserCorreo.readOnly = false;
            }
            if (selectUserRol) selectUserRol.value = "Vendedor";
            if (inputUserPassword) {
                inputUserPassword.value = "";
                inputUserPassword.required = true;
            }
            if (ayudaUserPassword) ayudaUserPassword.textContent = "Obligatoria al dar de alta (mínimo 6 caracteres).";
            if (modalUsuario && bootstrapObj?.Modal) {
                bootstrapObj.Modal.getOrCreateInstance(modalUsuario).show();
            }
        }

        function abrirModalEditarUsuario(user) {
            if (!user) return;
            ocultarErrorModalUsuario();
            if (modalUsuarioLabel) modalUsuarioLabel.textContent = "Editar usuario";
            if (inputUserId) inputUserId.value = user.id || "";
            if (inputUserNombre) inputUserNombre.value = user.nombre || "";
            if (inputUserCorreo) {
                inputUserCorreo.value = user.correo || "";
                inputUserCorreo.readOnly = true;
            }
            if (selectUserRol) selectUserRol.value = ROLES_USUARIO_VALIDOS.includes(user.rol) ? user.rol : "Vendedor";
            if (inputUserPassword) {
                inputUserPassword.value = "";
                inputUserPassword.required = false;
            }
            if (ayudaUserPassword) ayudaUserPassword.textContent = "Déjala vacía si no deseas cambiar la contraseña.";
            if (modalUsuario && bootstrapObj?.Modal) {
                bootstrapObj.Modal.getOrCreateInstance(modalUsuario).show();
            }
        }

        function poblarSucursal(data) {
            if (!data) return;
            if (inputNombre) inputNombre.value = data.nombreComercial || "";
            if (inputTelefono) inputTelefono.value = data.telefonoAtencion || "";
            if (inputCorreo) inputCorreo.value = data.correoOperativo || "";
            if (inputDomicilio) inputDomicilio.value = data.direccion || "";
            if (inputCiudad) inputCiudad.value = data.ciudad || "";
            if (inputCp) inputCp.value = data.codigoPostal || "";
            if (switchRec) switchRec.checked = Boolean(data.recordatoriosPago);
            if (switchRes) switchRes.checked = Boolean(data.resumenDiarioCaja);
        }

        function poblarUsuarios(lista) {
            if (!tbodyUsuarios || !Array.isArray(lista)) return;
            usuariosCargados = lista;

            if (lista.length === 0) {
                tbodyUsuarios.innerHTML = '<tr><td colspan="5" class="text-center py-4 text-muted small">No hay usuarios registrados.</td></tr>';
                return;
            }

            tbodyUsuarios.innerHTML = lista.map(function (u) {
                const ini = obtenerIniciales(u.nombre);
                const badgeEstado = u.activo && !u.bloqueado
                    ? '<span class="badge bg-success-subtle text-success px-3 py-1 rounded-pill fw-medium"><i class="fa-solid fa-circle me-1" style="font-size: 0.45rem;" aria-hidden="true"></i> Activo</span>'
                    : '<span class="badge bg-danger-subtle text-danger px-3 py-1 rounded-pill fw-medium"><i class="fa-solid fa-circle me-1" style="font-size: 0.45rem;" aria-hidden="true"></i> Inactivo</span>';
                const idSafe = escapeHtml(u.id);
                const nombreSafe = escapeHtml(u.nombre);
                return `<tr>` +
                    `<td class="py-3"><div class="d-flex align-items-center gap-2">` +
                    `<div class="rounded-circle d-flex align-items-center justify-content-center text-white fw-bold shadow-sm" style="width: 36px; height: 36px; min-width: 36px; background-color: #485635; font-size: 0.8rem;" aria-hidden="true">${escapeHtml(ini)}</div>` +
                    `<div><div class="fw-semibold text-dark">${nombreSafe}</div><div class="text-muted small">${escapeHtml(u.correo)}</div></div>` +
                    `</div></td>` +
                    `<td class="py-3"><span class="badge bg-light text-dark border px-3 py-1 rounded-pill fw-medium">${escapeHtml(u.rol)}</span></td>` +
                    `<td class="py-3">${badgeEstado}</td>` +
                    `<td class="py-3 text-muted small">${u.bloqueado ? "Bloqueado" : "Habilitado"}</td>` +
                    `<td class="py-3 text-end">` +
                    `<button type="button" class="btn btn-sm btn-outline-secondary rounded-3 px-2 py-1 me-1 btn-editar-usuario" data-user-id="${idSafe}" aria-label="Editar usuario ${nombreSafe}" title="Editar usuario">` +
                    `<i class="fa-solid fa-pen-to-square" aria-hidden="true"></i></button>` +
                    `<button type="button" class="btn btn-sm ${u.activo ? "btn-outline-danger" : "btn-outline-success"} rounded-3 px-2 py-1 btn-toggle-usuario" data-user-id="${idSafe}" data-user-activo="${u.activo ? "true" : "false"}" aria-label="${u.activo ? "Desactivar" : "Activar"} usuario ${nombreSafe}" title="${u.activo ? "Desactivar" : "Activar"}">` +
                    `<i class="fa-solid ${u.activo ? "fa-user-slash" : "fa-user-check"}" aria-hidden="true"></i></button>` +
                    `</td></tr>`;
            }).join("");

            if (typeof tbodyUsuarios.querySelectorAll === "function") {
                tbodyUsuarios.querySelectorAll(".btn-editar-usuario").forEach(function (btn) {
                    btn.addEventListener("click", function () {
                        const userId = btn.getAttribute("data-user-id");
                        const user = usuariosCargados.find((x) => String(x.id) === String(userId));
                        abrirModalEditarUsuario(user);
                    });
                });

                tbodyUsuarios.querySelectorAll(".btn-toggle-usuario").forEach(function (btn) {
                    btn.addEventListener("click", async function () {
                        const userId = btn.getAttribute("data-user-id");
                        const activoActual = btn.getAttribute("data-user-activo") === "true";
                        try {
                            await configuracionApi.cambiarEstadoUsuario(userId, { activo: !activoActual, desbloquear: true });
                            notificar("success", activoActual ? "Usuario desactivado correctamente." : "Usuario activado correctamente.");
                            await cargarDatos();
                        } catch (err) {
                            notificar("error", err?.message || "No se pudo cambiar el estado del usuario.");
                        }
                    });
                });
            }
        }

        async function cargarDatos() {
            const acc = verificarAcceso();
            if (!acc.autorizado || !configuracionApi) return;
            ocultarError();

            try {
                const [sucursal, usuarios] = await Promise.all([
                    configuracionApi.obtenerSucursal(),
                    configuracionApi.listarUsuarios()
                ]);
                poblarSucursal(sucursal);
                poblarUsuarios(usuarios);
            } catch (err) {
                if (tbodyUsuarios && doc.getElementById("filaCargandoUsuariosConfig")) {
                    tbodyUsuarios.innerHTML = "";
                }
                mostrarError(err?.message || "No fue posible cargar la configuración del sistema.");
            }
        }

        if (btnNuevoUsuario) {
            btnNuevoUsuario.addEventListener("click", abrirModalNuevoUsuario);
        }

        if (modalUsuario) {
            modalUsuario.addEventListener("hidden.bs.modal", function () {
                ocultarErrorModalUsuario();
            });
        }

        if (btnGuardarUsuario) {
            btnGuardarUsuario.addEventListener("click", async function () {
                ocultarErrorModalUsuario();
                const id = inputUserId?.value?.trim() || "";
                const nombre = inputUserNombre?.value?.trim() || "";
                const correo = inputUserCorreo?.value?.trim() || "";
                const rol = selectUserRol?.value || "Vendedor";
                const password = inputUserPassword?.value || "";
                const esEdicion = Boolean(id);

                const errValid = validarFormularioUsuario({ nombre, correo, rol, password }, esEdicion);
                if (errValid) {
                    mostrarErrorModalUsuario(errValid);
                    return;
                }
                if (!configuracionApi) return;

                const htmlOrig = btnGuardarUsuario.innerHTML;
                btnGuardarUsuario.disabled = true;
                btnGuardarUsuario.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Guardando...';

                try {
                    if (esEdicion) {
                        await configuracionApi.actualizarUsuario(id, {
                            nombre,
                            rol,
                            nuevaPassword: password ? password : null
                        });
                        notificar("success", "Usuario actualizado correctamente.");
                    } else {
                        await configuracionApi.crearUsuario({
                            nombre,
                            correo,
                            password,
                            rol
                        });
                        notificar("success", "Usuario creado correctamente.");
                    }

                    if (modalUsuario && bootstrapObj?.Modal) {
                        bootstrapObj.Modal.getOrCreateInstance(modalUsuario).hide();
                    }
                    await cargarDatos();
                } catch (err) {
                    mostrarErrorModalUsuario(err?.message || "No fue posible guardar el usuario.");
                } finally {
                    btnGuardarUsuario.disabled = false;
                    btnGuardarUsuario.innerHTML = htmlOrig;
                }
            });
        }

        if (btnReintentar) {
            btnReintentar.addEventListener("click", cargarDatos);
        }

        if (btnGuardar) {
            btnGuardar.addEventListener("click", async function () {
                if (!configuracionApi) return;
                const nombreComercial = inputNombre?.value?.trim() || "";
                const correoOperativo = inputCorreo?.value?.trim() || "";
                if (!nombreComercial || !correoOperativo) {
                    notificar("error", "El nombre comercial y el correo de la sucursal son obligatorios.");
                    return;
                }
                try {
                    const res = await configuracionApi.actualizarSucursal({
                        nombreComercial,
                        telefonoAtencion: inputTelefono?.value?.trim() || "",
                        correoOperativo,
                        direccion: inputDomicilio?.value?.trim() || "",
                        ciudad: inputCiudad?.value?.trim() || "",
                        codigoPostal: inputCp?.value?.trim() || "",
                        recordatoriosPago: Boolean(switchRec?.checked),
                        resumenDiarioCaja: Boolean(switchRes?.checked)
                    });
                    poblarSucursal(res);
                    notificar("success", "Cambios guardados con éxito.");
                } catch (err) {
                    notificar("error", err?.message || "No fue posible guardar la configuración.");
                }
            });
        }

        cargarDatos();
        return { cargarDatos, poblarSucursal, poblarUsuarios, activarSeccion };
    }

    return { obtenerIniciales, validarFormularioUsuario, init };
});
