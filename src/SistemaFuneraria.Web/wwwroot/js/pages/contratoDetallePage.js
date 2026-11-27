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
        if (norm === "vencido") {
            return {
                texto: "Vencido",
                clase: "badge rounded-pill bg-danger-subtle text-danger border border-danger-subtle px-3 py-1 fw-medium"
            };
        }
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

    function escaparHtml(valor) {
        return String(valor ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function badgeTipoPagoHtml(tipo) {
        const norm = String(tipo || "").trim().toLowerCase();
        if (norm === "abonocapital") {
            return '<span class="badge bg-primary-subtle text-primary border border-primary-subtle rounded-pill px-2 py-1 small">Abono a capital</span>';
        }
        if (norm === "liquidacion") {
            return '<span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-2 py-1 small">Liquidación</span>';
        }
        if (norm === "contado") {
            return '<span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-2 py-1 small">Contado</span>';
        }
        return '<span class="badge bg-light text-dark border rounded-pill px-2 py-1 small">Mensualidad</span>';
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

        const hasExplicitStorage = deps.storage !== undefined;
        const storageOpts = apiClient
            ? (hasExplicitStorage ? { client: apiClient, storage: deps.storage } : { client: apiClient })
            : {};

        if (authApi && typeof authApi.applyRolePermissions === "function") {
            authApi.applyRolePermissions(doc, storageOpts);
        }

        const contenidoPrincipal = doc.getElementById("contratoContenidoPrincipal");
        const contratoNoEncontrado = doc.getElementById("contratoNoEncontrado");
        const alertaErrorDetalle = doc.getElementById("alertaErrorDetalleContrato");
        const mensajeErrorDetalle = doc.getElementById("mensajeErrorDetalleContrato");
        const btnReintentarDetalle = doc.getElementById("btnReintentarDetalleContrato");

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
        const btnAbrirAdmin = doc.getElementById("btnAbrirModalAdminEstado");

        const seccionAmortizacion = doc.getElementById("seccionTablaAmortizacion");
        const badgeConteoAmortizacion = doc.getElementById("badgeConteoAmortizacion");
        const tbodyAmortizacion = doc.getElementById("tbodyAmortizacion");
        const badgeConteoHistorialPagos = doc.getElementById("badgeConteoHistorialPagos");
        const tbodyHistorialPagos = doc.getElementById("tbodyHistorialPagos");
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
        const loteClaveParam = (params.get("loteClave") || "").trim();
        const identificadorParam = ventaIdParam || folioParam;
        let contratoVentaId = ventaIdParam;
        let rolActual = "";

        function verificarAcceso() {
            if (!authApi || !apiClient) {
                return { autorizado: true, session: null };
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
                return { autorizado: false, session: null };
            }

            return { autorizado: true, session };
        }

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

        function mostrarErrorDetalle(msg) {
            const texto = limpiarPrefijoError(msg) || "No fue posible consultar el expediente del contrato.";
            if (alertaErrorDetalle) {
                if (mensajeErrorDetalle) {
                    mensajeErrorDetalle.textContent = texto;
                } else {
                    alertaErrorDetalle.textContent = texto;
                }
                alertaErrorDetalle.classList.remove("d-none");
            }
        }

        function ocultarErrorDetalle() {
            if (alertaErrorDetalle) {
                alertaErrorDetalle.classList.add("d-none");
            }
        }

        function mostrarNoEncontrado() {
            ocultarErrorDetalle();
            if (contenidoPrincipal) contenidoPrincipal.classList.add("d-none");
            if (contratoNoEncontrado) contratoNoEncontrado.classList.remove("d-none");
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

            ocultarErrorDetalle();
            if (contratoNoEncontrado) contratoNoEncontrado.classList.add("d-none");
            if (contenidoPrincipal) contenidoPrincipal.classList.remove("d-none");

            const precioStr = formatearMonedaMXN(dto.precioPactado);
            const esContado = String(dto.modalidad || "").toLowerCase() === "contado";
            const estadoNorm = String(dto.estado || "").toLowerCase();
            const esCerrado = estadoNorm === "liquidado" || estadoNorm === "cancelado";
            const puedeIrACaja = !rolActual || rolActual === "Administrador" || rolActual === "Cajero";

            if (elFolio) elFolio.textContent = dto.folioContrato || "-";
            if (modalFolio) modalFolio.textContent = dto.folioContrato || "-";
            if (elTitular) elTitular.textContent = dto.clienteNombre || "-";
            if (elLote) {
                const clave = dto.loteClave || "-";
                const tieneSeccion = Boolean(dto.loteSeccion && dto.loteSeccion !== "N/D");
                const tieneGavetas = Number(dto.loteGavetas) > 0;
                if (tieneSeccion && tieneGavetas) {
                    elLote.textContent = `${clave} (${dto.loteSeccion}, ${dto.loteGavetas} gavetas)`;
                } else if (tieneSeccion) {
                    elLote.textContent = `${clave} (${dto.loteSeccion})`;
                } else {
                    elLote.textContent = clave;
                }
            }
            actualizarBadgeEstado(dto.estadoVisual || dto.estado);

            if (selectEstado && (dto.estado === "Activo" || dto.estado === "EnRevision")) {
                selectEstado.value = dto.estado;
            }
            if (btnAbrirAdmin) {
                btnAbrirAdmin.disabled = esCerrado;
                btnAbrirAdmin.title = esCerrado
                    ? "No se puede modificar el estado de un contrato liquidado o cancelado."
                    : "Panel exclusivo de administración";
            }

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

                if (btnIrACaja) {
                    btnIrACaja.setAttribute("href", `/Caja?contrato=${encodeURIComponent(dto.folioContrato || "")}`);
                    btnIrACaja.classList.toggle("d-none", esCerrado || !puedeIrACaja);
                }
                if (comprobanteCard) comprobanteCard.classList.add("d-none");
                if (seccionAmortizacion) seccionAmortizacion.classList.remove("d-none");
                if (badgeConteoAmortizacion) {
                    badgeConteoAmortizacion.innerHTML = `<i class="fa-solid fa-file-invoice-dollar me-1 text-success" aria-hidden="true"></i> ${cuotas.length} Cuotas calculadas`;
                }
                if (tbodyAmortizacion) {
                    if (cuotas.length > 0) {
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
                    } else {
                        tbodyAmortizacion.innerHTML = `<tr><td colspan="8" class="text-center py-4 text-muted">No hay cuotas registradas para este contrato.</td></tr>`;
                    }
                }
            }

            renderizarHistorialPagos(dto.historialPagos);
        }

        function renderizarHistorialPagos(historialPagos) {
            const pagos = Array.isArray(historialPagos) ? historialPagos : [];
            if (badgeConteoHistorialPagos) {
                const etiqueta = pagos.length === 1 ? "1 pago registrado" : `${pagos.length} pagos registrados`;
                badgeConteoHistorialPagos.innerHTML = `<i class="fa-solid fa-receipt me-1 text-success" aria-hidden="true"></i> ${etiqueta}`;
            }
            if (!tbodyHistorialPagos) return;
            if (pagos.length === 0) {
                tbodyHistorialPagos.innerHTML = `<tr><td colspan="8" class="text-center py-4 text-muted">Aún no se registran pagos aplicados a este contrato.</td></tr>`;
                return;
            }
            tbodyHistorialPagos.innerHTML = pagos.map(function (p) {
                const folioSeguro = escaparHtml(p.folio || "-");
                const metodoSeguro = escaparHtml(p.metodoPago || "-");
                const refTexto = p.referencia ? ` · <span class="text-muted">${escaparHtml(p.referencia)}</span>` : "";
                const hrefRecibo = `/Caja/Recibo?folio=${encodeURIComponent(p.folio || "")}`;
                return `<tr>` +
                    `<td class="ps-3"><a href="${hrefRecibo}" class="fw-semibold text-decoration-none sf-monospace">${folioSeguro}</a></td>` +
                    `<td class="text-dark">${formatearFecha(p.fechaPago)}</td>` +
                    `<td>${badgeTipoPagoHtml(p.tipoPago)}</td>` +
                    `<td class="text-dark">${metodoSeguro}${refTexto}</td>` +
                    `<td class="text-end text-dark">${formatearMonedaMXN(p.montoCapital)}</td>` +
                    `<td class="text-end text-muted">${formatearMonedaMXN(p.montoInteres)}</td>` +
                    `<td class="text-end fw-bold text-dark">${formatearMonedaMXN(p.montoTotal)}</td>` +
                    `<td class="text-end pe-3 fw-medium text-success">${formatearMonedaMXN(p.saldoPosterior)}</td>` +
                    `</tr>`;
            }).join("");
        }

        async function cargarPorId() {
            const acceso = verificarAcceso();
            if (!acceso.autorizado) return;
            rolActual = acceso.session?.rol || "";

            if (!identificadorParam && !loteClaveParam) {
                mostrarNoEncontrado();
                return;
            }

            if (!contratosApi && !ventasApi) return;

            ocultarErrorDetalle();

            try {
                let dto = null;
                if (identificadorParam) {
                    dto = contratosApi
                        ? await contratosApi.obtenerDetalleContrato(identificadorParam)
                        : await ventasApi.obtenerVentaPorId(ventaIdParam);
                } else if (loteClaveParam && contratosApi && typeof contratosApi.obtenerCartera === "function") {
                    const cartera = await contratosApi.obtenerCartera({
                        buscar: loteClaveParam,
                        tab: "Todos",
                        page: 1,
                        pageSize: 20
                    });
                    const items = Array.isArray(cartera?.items) ? cartera.items : [];
                    const loteUpper = loteClaveParam.toUpperCase();
                    const coincidencias = items.filter(function (i) {
                        return String(i.loteClave || "").trim().toUpperCase() === loteUpper;
                    });
                    const match = coincidencias.find(function (i) {
                        return String(i.estado || "").toLowerCase() !== "cancelado";
                    }) || coincidencias[0];

                    if (!match) {
                        mostrarNoEncontrado();
                        return;
                    }
                    dto = await contratosApi.obtenerDetalleContrato(match.folioContrato || match.ventaId);
                } else {
                    mostrarNoEncontrado();
                    return;
                }

                poblarDesdeApi(dto);
            } catch (err) {
                if (err?.status === 404) {
                    mostrarNoEncontrado();
                } else {
                    mostrarErrorDetalle(err?.message || "No fue posible consultar el expediente del contrato.");
                    if (tbodyAmortizacion && doc.getElementById("filaCargandoAmortizacion")) {
                        tbodyAmortizacion.innerHTML = "";
                    }
                    if (tbodyHistorialPagos && doc.getElementById("filaCargandoHistorialPagos")) {
                        tbodyHistorialPagos.innerHTML = "";
                    }
                    if (sfUi && typeof sfUi.showToast === "function") {
                        sfUi.showToast({
                            type: "error",
                            title: "Error al cargar contrato",
                            message: "No fue posible consultar el expediente del contrato."
                        });
                    }
                }
            }
        }

        if (modalEl) {
            modalEl.addEventListener("hidden.bs.modal", function () {
                ocultarAlertaModal();
                if (motivoInput) motivoInput.value = "";
            });
        }

        if (btnReintentarDetalle) {
            btnReintentarDetalle.addEventListener("click", function () {
                cargarPorId();
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
                    await cargarPorId();
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
