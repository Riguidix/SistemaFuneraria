/**
 * paso1Cliente.js - Lógica interactiva del Paso 1 del Wizard (SF-052).
 * Conecta el buscador y la captura inline de cliente contra clientesApi.js y wizardState.js.
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.paso1Cliente = api;
        if (typeof document !== "undefined") {
            if (document.readyState === "loading") {
                document.addEventListener("DOMContentLoaded", function () {
                    api.init();
                });
            } else {
                api.init();
            }
        }
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    const ROLES_PERMITIDOS = ["Administrador", "Vendedor"];
    const REGEX_TELEFONO = /^[0-9]{10}$/;
    const REGEX_CURP = /^[A-Z0-9]{18}$/;
    const REGEX_CORREO = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;

    function normalizarClientePaso1(datos = {}) {
        return {
            id: datos.id || null,
            nombre: String(datos.nombre ?? "").trim(),
            apellidos: String(datos.apellidos ?? "").trim(),
            curp: String(datos.curp ?? "").trim().toUpperCase(),
            telefono: String(datos.telefono ?? "").replace(/\D/g, ""),
            correo: String(datos.correo ?? datos.email ?? "").trim().toLowerCase(),
            direccion: String(datos.direccion ?? "").trim()
        };
    }

    function validarPaso1Cliente(datos) {
        const norm = normalizarClientePaso1(datos);
        if (!norm.nombre || !norm.apellidos) {
            return "Por favor completa el Nombre(s) y Apellidos del titular.";
        }
        if (!REGEX_CURP.test(norm.curp)) {
            return "La CURP debe contener exactamente 18 caracteres alfanuméricos.";
        }
        if (!REGEX_TELEFONO.test(norm.telefono)) {
            return "El teléfono debe contener exactamente 10 dígitos numéricos.";
        }
        if (!norm.correo || !REGEX_CORREO.test(norm.correo)) {
            return "El correo electrónico no tiene un formato válido.";
        }
        if (!norm.direccion) {
            return "La dirección del cliente es obligatoria.";
        }
        return null;
    }

    function verificarAccesoWizard(deps) {
        const authApi = deps.authApi;
        const apiClient = deps.apiClient;
        const storage = deps.storage !== undefined
            ? deps.storage
            : (typeof sessionStorage !== "undefined" ? sessionStorage : null);
        const navigate = deps.navigate || ((url) => {
            if (typeof window !== "undefined") {
                window.location.assign(url);
            }
        });

        const token = storage ? storage.getItem(apiClient.TOKEN_STORAGE_KEY) : null;
        const session = authApi.getSession({ client: apiClient, storage });

        if (!token || !session) {
            navigate("/Cuenta/Login");
            return { autorizado: false, redirigido: true, motivo: "SIN_SESION", session: null };
        }

        if (!ROLES_PERMITIDOS.includes(session.rol)) {
            return { autorizado: false, redirigido: false, motivo: "ROL_NO_AUTORIZADO", session };
        }

        return { autorizado: true, redirigido: false, session };
    }

    function limpiarPrefijoError(mensaje) {
        return String(mensaje ?? "").replace(/^\[(CONFLICTO|NO_ENCONTRADO)\]\s*/i, "");
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const clientesApi = deps.clientesApi ?? win?.clientesApi;
        const wizardState = deps.wizardState ?? win?.wizardState;

        if (!doc) return null;

        const form = doc.getElementById("formPaso1Cliente");
        const btnContinuar = doc.getElementById("btnContinuarPaso1");
        if (!form || !btnContinuar) return null;

        // Guardia de sesión y rol del Wizard
        if (authApi && apiClient) {
            const acceso = verificarAccesoWizard({
                authApi,
                apiClient,
                storage: deps.storage,
                navigate: deps.navigate
            });

            if (!acceso.autorizado) {
                if (!acceso.redirigido) {
                    const principal = doc.getElementById("wizardContenidoPrincipal");
                    const restringido = doc.getElementById("wizardAccesoRestringido");
                    const draftBadge = doc.getElementById("draftBadge");
                    if (principal) principal.classList.add("d-none");
                    if (draftBadge) draftBadge.classList.add("d-none");
                    if (restringido) restringido.classList.remove("d-none");
                }
                return { autorizado: false, motivo: acceso.motivo };
            }
        }

        const inputBuscar = doc.getElementById("inputBuscarCliente");
        const btnLimpiar = doc.getElementById("btnLimpiarBusqueda");
        const dropdownResultados = doc.getElementById("resultadosBusquedaCliente");
        const badgeSeleccionado = doc.getElementById("badgeClienteSeleccionado");
        const hdnClienteId = doc.getElementById("hdnClienteId");

        const txtNombre = doc.getElementById("txtClienteNombre");
        const txtApellidos = doc.getElementById("txtClienteApellidos");
        const txtCurp = doc.getElementById("txtClienteCurp");
        const txtTelefono = doc.getElementById("txtClienteTelefono");
        const txtEmail = doc.getElementById("txtClienteEmail");
        const txtDireccion = doc.getElementById("txtClienteDireccion");
        const alertaValidacion = doc.getElementById("paso1AlertaValidacion");

        let debounceTimer = null;
        let secuenciaBusqueda = 0;

        function mostrarError(mensaje) {
            if (!alertaValidacion) return;
            alertaValidacion.textContent = limpiarPrefijoError(mensaje);
            alertaValidacion.classList.remove("d-none");
        }

        function ocultarError() {
            if (!alertaValidacion) return;
            alertaValidacion.classList.add("d-none");
            alertaValidacion.textContent = "";
        }

        function marcarClienteExistente(id) {
            if (hdnClienteId) hdnClienteId.value = id || "";
            if (badgeSeleccionado) {
                badgeSeleccionado.classList.toggle("d-none", !id);
            }
        }

        function poblarCampos(cliente) {
            if (txtNombre) txtNombre.value = cliente.nombre || "";
            if (txtApellidos) txtApellidos.value = cliente.apellidos || "";
            if (txtCurp) txtCurp.value = cliente.curp || "";
            if (txtTelefono) txtTelefono.value = cliente.telefono || "";
            if (txtEmail) txtEmail.value = cliente.correo || cliente.email || "";
            if (txtDireccion) txtDireccion.value = cliente.direccion || "";
            marcarClienteExistente(cliente.id || "");
        }

        function renderizarResultados(lista) {
            if (!dropdownResultados) return;
            dropdownResultados.innerHTML = "";

            if (!Array.isArray(lista) || lista.length === 0) {
                const vacio = doc.createElement("div");
                vacio.className = "list-group-item text-muted small py-2";
                vacio.textContent = "Sin clientes encontrados. Puedes capturarlo abajo.";
                dropdownResultados.appendChild(vacio);
                dropdownResultados.classList.remove("d-none");
                return;
            }

            for (const c of lista) {
                const btn = doc.createElement("button");
                btn.type = "button";
                btn.className = "list-group-item list-group-item-action py-2 d-flex justify-content-between align-items-center";
                btn.setAttribute("data-cliente-id", c.id || "");

                const colInfo = doc.createElement("div");
                const nombreEl = doc.createElement("div");
                nombreEl.className = "fw-semibold small text-dark";
                nombreEl.textContent = `${c.nombre || ""} ${c.apellidos || ""}`.trim();

                const metaEl = doc.createElement("div");
                metaEl.className = "text-muted";
                metaEl.style.fontSize = "0.75rem";
                metaEl.textContent = `CURP: ${c.curp || "-"} · Tel: ${c.telefono || "-"}`;

                colInfo.append(nombreEl, metaEl);

                const badge = doc.createElement("span");
                badge.className = "badge bg-light text-secondary border";
                badge.textContent = "Seleccionar";

                btn.append(colInfo, badge);
                btn.addEventListener("click", function () {
                    poblarCampos(c);
                    if (inputBuscar) {
                        inputBuscar.value = `${c.nombre || ""} ${c.apellidos || ""}`.trim();
                    }
                    dropdownResultados.classList.add("d-none");
                    ocultarError();
                });

                dropdownResultados.appendChild(btn);
            }

            dropdownResultados.classList.remove("d-none");
        }

        // Restaurar borrador si existe
        if (wizardState) {
            const borrador = wizardState.getDraft();
            if (borrador?.cliente) {
                poblarCampos(borrador.cliente);
            }
        }

        // Si el usuario modifica la CURP manualmente tras elegir un cliente, desvinculamos el ID para no mezclar titulares
        if (txtCurp) {
            txtCurp.addEventListener("input", function () {
                if (hdnClienteId && hdnClienteId.value) {
                    marcarClienteExistente("");
                }
            });
        }

        // Buscador reactivo contra clientesApi.listar(query)
        if (inputBuscar && dropdownResultados && clientesApi) {
            inputBuscar.addEventListener("input", function () {
                const query = inputBuscar.value.trim();
                if (debounceTimer) clearTimeout(debounceTimer);

                if (query.length < 2) {
                    dropdownResultados.classList.add("d-none");
                    dropdownResultados.innerHTML = "";
                    return;
                }

                debounceTimer = setTimeout(async function () {
                    const reqId = ++secuenciaBusqueda;
                    try {
                        const resultados = await clientesApi.listar(query);
                        if (reqId !== secuenciaBusqueda) return;
                        renderizarResultados(resultados);
                    } catch (err) {
                        if (reqId !== secuenciaBusqueda) return;
                        dropdownResultados.innerHTML = "";
                        const errEl = doc.createElement("div");
                        errEl.className = "list-group-item text-danger small py-2";
                        errEl.textContent = limpiarPrefijoError(err?.message || "No se pudo buscar clientes.");
                        dropdownResultados.appendChild(errEl);
                        dropdownResultados.classList.remove("d-none");
                    }
                }, 250);
            });

            doc.addEventListener("click", function (e) {
                if (!inputBuscar.contains(e.target) && !dropdownResultados.contains(e.target)) {
                    dropdownResultados.classList.add("d-none");
                }
            });
        }

        if (btnLimpiar) {
            btnLimpiar.addEventListener("click", function () {
                if (inputBuscar) inputBuscar.value = "";
                if (dropdownResultados) {
                    dropdownResultados.classList.add("d-none");
                    dropdownResultados.innerHTML = "";
                }
                poblarCampos({ id: "", nombre: "", apellidos: "", curp: "", telefono: "", correo: "", direccion: "" });
                ocultarError();
            });
        }

        // Envío del Paso 1: si no tiene clienteId, registra en clientesApi antes de avanzar
        form.addEventListener("submit", async function (e) {
            e.preventDefault();
            ocultarError();

            const raw = {
                id: hdnClienteId?.value || null,
                nombre: txtNombre?.value,
                apellidos: txtApellidos?.value,
                curp: txtCurp?.value,
                telefono: txtTelefono?.value,
                correo: txtEmail?.value,
                direccion: txtDireccion?.value
            };

            const error = validarPaso1Cliente(raw);
            if (error) {
                mostrarError(error);
                return;
            }

            const clienteNorm = normalizarClientePaso1(raw);
            let clienteId = clienteNorm.id;

            const htmlOriginal = btnContinuar.innerHTML;
            if (!clienteId && clientesApi) {
                btnContinuar.disabled = true;
                btnContinuar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-2" aria-hidden="true"></i> Guardando titular...';

                try {
                    const respuesta = await clientesApi.registrar({
                        nombre: clienteNorm.nombre,
                        apellidos: clienteNorm.apellidos,
                        curp: clienteNorm.curp,
                        telefono: clienteNorm.telefono,
                        correo: clienteNorm.correo,
                        direccion: clienteNorm.direccion
                    });
                    clienteId = respuesta?.id || respuesta || null;
                    marcarClienteExistente(clienteId);
                } catch (err) {
                    mostrarError(err?.message || "No se pudo registrar el cliente. Verifica CURP y correo.");
                    btnContinuar.disabled = false;
                    btnContinuar.innerHTML = htmlOriginal;
                    return;
                } finally {
                    btnContinuar.disabled = false;
                    btnContinuar.innerHTML = htmlOriginal;
                }
            }

            const clienteFinal = {
                id: clienteId,
                nombre: clienteNorm.nombre,
                apellidos: clienteNorm.apellidos,
                curp: clienteNorm.curp,
                telefono: clienteNorm.telefono,
                correo: clienteNorm.correo,
                email: clienteNorm.correo,
                direccion: clienteNorm.direccion
            };

            if (wizardState) {
                wizardState.updateDraft({ cliente: clienteFinal, paso: 2 });
            }

            if (win) {
                win.dispatchEvent(new CustomEvent("sf:paso1-completado", {
                    detail: { cliente: clienteFinal }
                }));
            }
        });

        return { autorizado: true };
    }

    return {
        ROLES_PERMITIDOS,
        normalizarClientePaso1,
        validarPaso1Cliente,
        verificarAccesoWizard,
        init
    };
});
