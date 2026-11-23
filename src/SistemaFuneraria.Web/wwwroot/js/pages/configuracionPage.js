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

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const configuracionApi = deps.configuracionApi ?? win?.configuracionApi;
        const sfUi = deps.SfUi ?? win?.SfUi;

        if (!doc) return null;

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

        function notificar(tipo, mensaje) {
            if (sfUi && typeof sfUi.showToast === "function") {
                sfUi.showToast({ type: tipo, title: tipo === "error" ? "Error" : "Configuración", message: mensaje });
            }
        }

        function activarSeccion(targetId) {
            navButtons.forEach(function (btn) {
                const isTarget = btn.getAttribute("data-target") === targetId;
                const icon = btn.querySelector(".sf-nav-icon");
                btn.classList.toggle("active", isTarget);
                if (icon) {
                    icon.classList.toggle("text-success", isTarget);
                    icon.classList.toggle("text-muted", !isTarget);
                }
            });
            panels.forEach(function (panel) {
                panel.classList.toggle("d-none", panel.id !== targetId);
            });
        }

        navButtons.forEach(function (btn) {
            btn.addEventListener("click", function () {
                activarSeccion(btn.getAttribute("data-target"));
            });
        });

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
            tbodyUsuarios.innerHTML = lista.map(function (u) {
                const ini = obtenerIniciales(u.nombre);
                const badgeEstado = u.activo && !u.bloqueado
                    ? '<span class="badge bg-success-subtle text-success px-3 py-1 rounded-pill fw-medium"><i class="fa-solid fa-circle me-1" style="font-size: 0.45rem;"></i> Activo</span>'
                    : '<span class="badge bg-danger-subtle text-danger px-3 py-1 rounded-pill fw-medium"><i class="fa-solid fa-circle me-1" style="font-size: 0.45rem;"></i> Inactivo</span>';
                return `<tr>` +
                    `<td class="py-3"><div class="d-flex align-items-center gap-2">` +
                    `<div class="rounded-circle d-flex align-items-center justify-content-center text-white fw-bold shadow-sm" style="width: 36px; height: 36px; min-width: 36px; background-color: #485635; font-size: 0.8rem;">${escapeHtml(ini)}</div>` +
                    `<div><div class="fw-semibold text-dark">${escapeHtml(u.nombre)}</div><div class="text-muted small">${escapeHtml(u.correo)}</div></div>` +
                    `</div></td>` +
                    `<td class="py-3"><span class="badge bg-light text-dark border px-3 py-1 rounded-pill fw-medium">${escapeHtml(u.rol)}</span></td>` +
                    `<td class="py-3">${badgeEstado}</td>` +
                    `<td class="py-3 text-muted small">${u.bloqueado ? "Bloqueado" : "Habilitado"}</td>` +
                    `<td class="py-3 text-end">` +
                    `<button type="button" class="btn btn-sm btn-outline-secondary rounded-3 px-2 py-1 me-1 btn-toggle-usuario" data-user-id="${escapeHtml(u.id)}" data-user-activo="${u.activo ? "true" : "false"}" title="${u.activo ? "Desactivar" : "Activar"}">` +
                    `<i class="fa-solid ${u.activo ? "fa-user-slash" : "fa-user-check"}"></i></button>` +
                    `</td></tr>`;
            }).join("");

            tbodyUsuarios.querySelectorAll(".btn-toggle-usuario").forEach(function (btn) {
                btn.addEventListener("click", async function () {
                    const userId = btn.getAttribute("data-user-id");
                    const activoActual = btn.getAttribute("data-user-activo") === "true";
                    try {
                        await configuracionApi.cambiarEstadoUsuario(userId, { activo: !activoActual, desbloquear: true });
                        await cargarDatos();
                    } catch (err) {
                        notificar("error", err?.message || "No se pudo cambiar el estado del usuario.");
                    }
                });
            });
        }

        async function cargarDatos() {
            if (!configuracionApi) return;
            if (authApi && apiClient) {
                const storage = deps.storage ?? (typeof sessionStorage !== "undefined" ? sessionStorage : null);
                const token = storage ? storage.getItem(apiClient.TOKEN_STORAGE_KEY) : null;
                const session = authApi.getSession({ client: apiClient, storage });
                if (!token || !session) {
                    win?.location?.assign?.("/Cuenta/Login");
                    return;
                }
            }
            try {
                const [sucursal, usuarios] = await Promise.all([
                    configuracionApi.obtenerSucursal(),
                    configuracionApi.listarUsuarios()
                ]);
                poblarSucursal(sucursal);
                poblarUsuarios(usuarios);
            } catch {
                // Mantiene fallback SSR si hay 403/offline
            }
        }

        if (btnGuardar) {
            btnGuardar.addEventListener("click", async function () {
                if (!configuracionApi) return;
                try {
                    const res = await configuracionApi.actualizarSucursal({
                        nombreComercial: inputNombre?.value?.trim() || "",
                        telefonoAtencion: inputTelefono?.value?.trim() || "",
                        correoOperativo: inputCorreo?.value?.trim() || "",
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
        return { cargarDatos, poblarSucursal, poblarUsuarios };
    }

    return { obtenerIniciales, init };
});
