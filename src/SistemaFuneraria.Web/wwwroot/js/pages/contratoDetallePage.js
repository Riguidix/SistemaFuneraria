/**
 * contratoDetallePage.js - Conector real para /Contratos/Detalle (SF-064 y gestión Admin SF-063).
 * Hidrata contratos de Contado vía GET /api/v1/ventas/{id} y conecta PATCH/DELETE Admin.
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.contratoDetallePage = api;
        if (typeof document !== "undefined") {
            document.addEventListener("DOMContentLoaded", function () {
                api.init();
            });
        }
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    function formatearMonedaMXN(monto) {
        const num = Number(monto);
        const seguro = Number.isFinite(num) ? num : 0;
        return "$" + seguro.toLocaleString("es-MX", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function formatearFecha(fechaIso) {
        if (!fechaIso) return "-";
        const fecha = new Date(fechaIso);
        if (Number.isNaN(fecha.getTime())) return String(fechaIso);
        return new Intl.DateTimeFormat("es-MX", {
            day: "2-digit",
            month: "short",
            year: "numeric",
            timeZone: "UTC"
        }).format(fecha);
    }

    function obtenerBadgeEstadoVenta(estado) {
        const norm = String(estado || "").trim().toLowerCase();
        if (norm === "liquidado" || norm === "activo" || norm === "al corriente") {
            return {
                texto: norm === "liquidado" ? "Liquidado" : (norm === "activo" ? "Activo" : estado),
                clase: "badge rounded-pill bg-success-subtle text-success border border-success-subtle px-3 py-1 fw-medium"
            };
        }
        if (norm === "cancelado") {
            return {
                texto: "Cancelado",
                clase: "badge rounded-pill bg-danger-subtle text-danger border border-danger-subtle px-3 py-1 fw-medium"
            };
        }
        return {
            texto: norm === "enrevision" || norm === "en revisión" ? "En revisión" : (estado || "En revisión"),
            clase: "badge rounded-pill bg-warning-subtle text-warning-emphasis border border-warning-subtle px-3 py-1 fw-medium"
        };
    }

    function validarCambioEstadoAdmin(estado, motivo) {
        const permitidos = ["Activo", "EnRevision", "Cancelado"];
        if (!permitidos.includes(estado)) {
            return "Selecciona un estado administrativo válido.";
        }
        const texto = String(motivo ?? "").trim();
        if (texto.length < 10) {
            return "El motivo administrativo es obligatorio y debe tener al menos 10 caracteres.";
        }
        return null;
    }

    function limpiarPrefijoError(mensaje) {
        return String(mensaje ?? "").replace(/^\[(CONFLICTO|NO_ENCONTRADO)\]\s*/i, "");
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const ventasApi = deps.ventasApi ?? win?.ventasApi;
        const contratosApi = deps.contratosApi ?? win?.contratosApi;
        const bootstrapObj = deps.bootstrap ?? win?.bootstrap;
        const sfUi = deps.SfUi ?? win?.SfUi;

        if (!doc) return null;

        if (authApi) {
            authApi.applyRolePermissions(doc);
        }

        const contenidoPrincipal = doc.getElementById("contratoContenidoPrincipal");
        const contratoNoEncontrado = doc.getElementById("contratoNoEncontrado");

        const elFolio = doc.getElementById("detalleFolioContrato");
        const elTitular = doc.getElementById("detalleTitularContrato");
        const elLote = doc.getElementById("detalleLoteContrato");
        const badgeEstado = doc.getElementById("badgeEstadoContrato");
        const elPrecio = doc.getElementById("detallePrecioPactado");
        const elSubPrecio = doc.getElementById("detalleSubtituloPrecio");
        const elSaldo = doc.getElementById("detalleSaldoInsoluto");
        const elMensualidad = doc.getElementById("detalleMensualidad");
        const elSubMensualidad = doc.getElementById("detalleSubtituloMensualidad");
        const elPlazo = doc.getElementById("detallePlazoTasa");
        const elSubTasa = doc.getElementById("detalleSubtituloTasa");
        const btnIrACaja = doc.getElementById("btnIrACajaContrato");

        const seccionAmortizacion = doc.getElementById("seccionTablaAmortizacion");
        const badgeConteoAmortizacion = doc.getElementById("badgeConteoAmortizacion");
        const tbodyAmortizacion = doc.getElementById("tbodyAmortizacion");
        const comprobanteCard = doc.getElementById("comprobanteContadoCard");
        const compFolioContrato = doc.getElementById("comprobanteFolioContrato");
        const compFolioPago = doc.getElementById("comprobanteFolioPago");
        const compFecha = doc.getElementById("comprobanteFechaVenta");
        const compPrecio = doc.getElementById("comprobantePrecioPagado");

        const modalEl = doc.getElementById("modalAdminEstadoContrato");
        const alertaModal = doc.getElementById("alertaModalAdminContrato");
        const modalFolio = doc.getElementById("modalAdminFolioContrato");
        const selectEstado = doc.getElementById("selectNuevoEstadoAdmin");
        const motivoInput = doc.getElementById("motivoCancelacionAdmin");
        const btnGuardar = doc.getElementById("btnGuardarEstadoAdmin");

        const params = new URLSearchParams(win?.location?.search || "");
        const ventaIdParam = (params.get("id") || "").trim();
        const folioParam = (params.get("folio") || "").trim();
        const identificadorParam = ventaIdParam || folioParam;
        let contratoVentaId = ventaIdParam;

        function mostrarAlertaModal(msg) {
            if (!alertaModal) return;
            alertaModal.textContent = limpiarPrefijoError(msg);
            alertaModal.classList.remove("d-none");
        }

        function ocultarAlertaModal() {
            if (!alertaModal) return;
            alertaModal.classList.add("d-none");
            alertaModal.textContent = "";
        }

        function actualizarBadgeEstado(estadoTexto) {
            if (!badgeEstado) return;
            const badge = obtenerBadgeEstadoVenta(estadoTexto);
            badgeEstado.textContent = badge.texto;
            badgeEstado.className = badge.clase;
        }

        function badgeCuotaHtml(estado) {
            const norm = String(estado || "").toLowerCase();
            if (norm === "pagada") {
                return '<span class="badge bg-success-subtle text-success rounded-pill px-2 py-1 small">Pagada</span>';
            }
            if (norm === "vencida") {
                return '<span class="badge bg-danger-subtle text-danger rounded-pill px-2 py-1 small">Vencida</span>';
            }
            return '<span class="badge bg-light text-muted border rounded-pill px-2 py-1 small">Pendiente</span>';
        }

        function poblarDesdeApi(dto) {
            if (!dto) return;
            if (dto.ventaId) {
                contratoVentaId = String(dto.ventaId);
            }

            const precioStr = formatearMonedaMXN(dto.precioPactado);
            const esContado = String(dto.modalidad || "").toLowerCase() === "contado";

            if (elFolio) elFolio.textContent = dto.folioContrato || "-";
            if (modalFolio) modalFolio.textContent = dto.folioContrato || "-";
            if (elTitular) elTitular.textContent = dto.clienteNombre || "-";
            if (elLote) elLote.textContent = dto.loteClave || "-";
            actualizarBadgeEstado(dto.estadoVisual || dto.estado);

            if (elPrecio) elPrecio.textContent = precioStr;

            if (esContado) {
                if (elSubPrecio) elSubPrecio.textContent = "Modalidad Contado (100%)";
                if (elSaldo) elSaldo.textContent = "$0.00";
                if (elMensualidad) elMensualidad.textContent = precioStr;
                if (elSubMensualidad) elSubMensualidad.textContent = "Pago único liquidado";
                if (elPlazo) elPlazo.textContent = "Contado";
                if (elSubTasa) elSubTasa.textContent = "0% interés · Comprobante: " + (dto.folioPago || "-");
                if (seccionAmortizacion) seccionAmortizacion.classList.add("d-none");
                if (btnIrACaja) btnIrACaja.classList.add("d-none");
                if (comprobanteCard) comprobanteCard.classList.remove("d-none");
                if (compFolioContrato) compFolioContrato.textContent = dto.folioContrato || "-";
                if (compFolioPago) compFolioPago.textContent = dto.folioPago || "-";
                if (compFecha) compFecha.textContent = formatearFecha(dto.fechaVenta);
                if (compPrecio) compPrecio.textContent = precioStr;
            } else {
                const fin = dto.financiamiento || {};
                const cuotas = Array.isArray(dto.amortizacion) ? dto.amortizacion : [];
                const pctEng = Number(fin.porcentajeEnganche ?? 0);
                const tasaPct = Number(fin.tasaInteresAnual ?? 0) * 100;
                const proxima = cuotas.find(function (c) {
                    const st = String(c.estado || "").toLowerCase();
                    return st === "vencida" || st === "pendiente";
                });

                if (elSubPrecio) {
                    elSubPrecio.textContent = `Enganche: ${formatearMonedaMXN(fin.montoEnganche)} (${pctEng.toFixed(0)}%)`;
                }
                if (elSaldo) elSaldo.textContent = formatearMonedaMXN(fin.saldoInsoluto ?? 0);
                if (elMensualidad) elMensualidad.textContent = formatearMonedaMXN(fin.cuotaMensualActual ?? 0);
                if (elSubMensualidad) {
                    elSubMensualidad.textContent = proxima
                        ? `Próximo vencimiento: ${formatearFecha(proxima.fechaVencimiento)}`
                        : "Calendario liquidado";
                }
                if (elPlazo) elPlazo.textContent = `${fin.plazoMeses || cuotas.length || "-"} meses`;
                if (elSubTasa) elSubTasa.textContent = `Tasa anual fija: ${tasaPct.toFixed(0)}%`;

                const esCerrado = ["liquidado", "cancelado"].includes(String(dto.estado || "").toLowerCase());
                if (btnIrACaja) {
                    btnIrACaja.setAttribute("href", `/Caja?contrato=${encodeURIComponent(dto.folioContrato || "")}`);
                    btnIrACaja.classList.toggle("d-none", esCerrado);
                }
                if (comprobanteCard) comprobanteCard.classList.add("d-none");
                if (seccionAmortizacion) seccionAmortizacion.classList.remove("d-none");
                if (badgeConteoAmortizacion) {
                    badgeConteoAmortizacion.innerHTML = `<i class="fa-solid fa-file-invoice-dollar me-1 text-success" aria-hidden="true"></i> ${cuotas.length} Cuotas calculadas`;
                }
                if (tbodyAmortizacion && cuotas.length > 0) {
                    tbodyAmortizacion.innerHTML = cuotas.map(function (c) {
                        const saldoClass = Number(c.saldoFinal) === 0 ? "text-success" : "text-dark";
                        return `<tr>` +
                            `<td class="ps-3 fw-bold text-muted">${c.numeroCuota}</td>` +
                            `<td class="text-dark">${formatearFecha(c.fechaVencimiento)}</td>` +
                            `<td class="text-end text-muted">${formatearMonedaMXN(c.saldoInicial)}</td>` +
                            `<td class="text-end text-dark">${formatearMonedaMXN(c.capital)}</td>` +
                            `<td class="text-end text-muted">${formatearMonedaMXN(c.interes)}</td>` +
                            `<td class="text-end fw-bold text-dark">${formatearMonedaMXN(c.cuotaTotal)}</td>` +
                            `<td class="text-end fw-medium ${saldoClass}">${formatearMonedaMXN(c.saldoFinal)}</td>` +
                            `<td class="text-center pe-3">${badgeCuotaHtml(c.estado)}</td>` +
                            `</tr>`;
                    }).join("");
                }
            }
        }

        async function cargarPorId() {
            if (!identificadorParam || (!contratosApi && !ventasApi)) return;

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
                const dto = contratosApi
                    ? await contratosApi.obtenerDetalleContrato(identificadorParam)
                    : await ventasApi.obtenerVentaPorId(ventaIdParam);
                poblarDesdeApi(dto);
            } catch (err) {
                if (err?.status === 404) {
                    if (contenidoPrincipal) contenidoPrincipal.classList.add("d-none");
                    if (contratoNoEncontrado) contratoNoEncontrado.classList.remove("d-none");
                } else if (sfUi && typeof sfUi.showToast === "function") {
                    sfUi.showToast({
                        type: "error",
                        title: "Error al cargar contrato",
                        message: "No fue posible consultar el expediente del contrato."
                    });
                }
            }
        }

        if (modalEl) {
            modalEl.addEventListener("hidden.bs.modal", function () {
                ocultarAlertaModal();
                if (motivoInput) motivoInput.value = "";
            });
        }

        if (btnGuardar) {
            btnGuardar.addEventListener("click", async function () {
                ocultarAlertaModal();
                const nuevoEstado = selectEstado ? selectEstado.value : "Activo";
                const motivo = motivoInput ? motivoInput.value.trim() : "";

                const errorValidacion = validarCambioEstadoAdmin(nuevoEstado, motivo);
                if (errorValidacion) {
                    mostrarAlertaModal(errorValidacion);
                    return;
                }

                if (!contratoVentaId || !ventasApi) {
                    actualizarBadgeEstado(nuevoEstado);
                    if (modalEl && bootstrapObj?.Modal) {
                        bootstrapObj.Modal.getOrCreateInstance(modalEl).hide();
                    }
                    return;
                }

                const htmlOrig = btnGuardar.innerHTML;
                btnGuardar.disabled = true;
                btnGuardar.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-1" aria-hidden="true"></i> Guardando...';

                try {
                    if (nuevoEstado === "Cancelado") {
                        await ventasApi.cancelarVenta(contratoVentaId, motivo);
                    } else {
                        await ventasApi.cambiarEstadoVenta(contratoVentaId, nuevoEstado, motivo);
                    }

                    actualizarBadgeEstado(nuevoEstado);
                    if (modalEl && bootstrapObj?.Modal) {
                        bootstrapObj.Modal.getOrCreateInstance(modalEl).hide();
                    }
                    if (sfUi && typeof sfUi.showToast === "function") {
                        sfUi.showToast({
                            type: "success",
                            title: "Estado actualizado",
                            message: "El estado del contrato fue actualizado correctamente."
                        });
                    }
                } catch (err) {
                    mostrarAlertaModal(err?.message || "No fue posible actualizar el estado del contrato.");
                } finally {
                    btnGuardar.disabled = false;
                    btnGuardar.innerHTML = htmlOrig;
                }
            });
        }

        cargarPorId();

        return {
            cargarPorId,
            poblarDesdeApi
        };
    }

    return {
        formatearMonedaMXN,
        formatearFecha,
        obtenerBadgeEstadoVenta,
        validarCambioEstadoAdmin,
        init
    };
});
