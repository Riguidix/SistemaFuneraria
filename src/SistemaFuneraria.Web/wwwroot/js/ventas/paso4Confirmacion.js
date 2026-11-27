/**
 * paso4Confirmacion.js - Lógica interactiva del Paso 4 del Wizard (SF-062).
 * Conecta la generación real de contratos de Contado con ventasApi.js y maneja conflictos 409.
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.paso4Confirmacion = api;
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

    function formatearMonedaMXN(monto) {
        const num = Number(monto);
        const seguro = Number.isFinite(num) ? num : 0;
        return "$" + seguro.toLocaleString("es-MX", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function limpiarPrefijoError(mensaje) {
        return String(mensaje ?? "").replace(/^\[(CONFLICTO|NO_ENCONTRADO)\]\s*/i, "");
    }

    function construirPayloadVentaContado(draft, metodoPago = "Efectivo", referenciaPago = "", firmaClienteBase64 = "") {
        const clienteId = draft?.cliente?.id || "";
        const loteId = draft?.lote?.id || "";
        const metodo = String(metodoPago || "Efectivo").trim() || "Efectivo";
        const refLimpia = String(referenciaPago ?? "").trim();
        const firmaLimpia = String(firmaClienteBase64 ?? "").trim();

        const payload = {
            clienteId,
            loteId,
            metodoPago: metodo,
            referenciaPago: refLimpia ? refLimpia : null
        };
        if (firmaLimpia) {
            payload.firmaClienteBase64 = firmaLimpia;
        }
        return payload;
    }

    function construirPayloadVentaFinanciada(draft, firmaClienteBase64 = "") {
        const fin = draft?.financiamiento || {};
        const firmaLimpia = String(firmaClienteBase64 ?? "").trim();
        const payload = {
            clienteId: draft?.cliente?.id || "",
            loteId: draft?.lote?.id || "",
            porcentajeEnganche: Number(fin.enganchePorcentaje ?? 20),
            plazoMeses: Number(fin.plazoMeses ?? 24)
        };
        if (firmaLimpia) {
            payload.firmaClienteBase64 = firmaLimpia;
        }
        return payload;
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const ventasApi = deps.ventasApi ?? win?.ventasApi;
        const wizardState = deps.wizardState ?? win?.wizardState;
        const authApi = deps.authApi ?? win?.authApi;
        const firmaPadLib = deps.SfFirmaPad ?? win?.SfFirmaPad;
        const autoRedirectDelayMs = deps.autoRedirectDelayMs ?? 700;

        if (!doc) return null;

        const step2Card = doc.getElementById("step2Card");
        const step3Card = doc.getElementById("step3Card");
        const step4Card = doc.getElementById("step4Card");

        if (!step4Card) return null;

        const chkConfirmar = doc.getElementById("chkConfirmarDatos");
        const btnGenerar = doc.getElementById("btnGenerarContrato");
        const btnAnterior = doc.getElementById("btnAnteriorPaso4");
        const alertaExito = doc.getElementById("alertaExitoContrato");
        const lblFolioGenerado = doc.getElementById("lblFolioGenerado");
        const lnkVerContrato = doc.getElementById("lnkVerContratoGenerado");

        const alertaError = doc.getElementById("alertaErrorContrato");
        const lblError = doc.getElementById("lblErrorContrato");
        const btnElegirOtroLote = doc.getElementById("btnElegirOtroLote");

        const contenedorPagoContado = doc.getElementById("contenedorPagoContado");
        const selectMetodoPago = doc.getElementById("selectMetodoPagoContado");
        const txtReferenciaPago = doc.getElementById("txtReferenciaPagoContado");

        const lblClienteNombre = doc.getElementById("resumenClienteNombre");
        const lblClienteEmail = doc.getElementById("resumenClienteEmail");
        const lblLoteClave = doc.getElementById("resumenLoteClaveYJardin");
        const lblLoteCapacidad = doc.getElementById("resumenLoteCapacidad");
        const lblPrecioTotal = doc.getElementById("resumenPrecioTotal");
        const lblPrecioEnganche = doc.getElementById("resumenPrecioEnganche");
        const lblPlanDetalle = doc.getElementById("resumenPlanDetalle");
        const lblPlanTasa = doc.getElementById("resumenPlanTasa");

        const step2Indicator = doc.getElementById("stepIndicator2");
        const step3Indicator = doc.getElementById("stepIndicator3");
        const step4Indicator = doc.getElementById("stepIndicator4");

        const canvasFirma = doc.getElementById("canvasFirmaCliente");
        const btnLimpiarFirma = doc.getElementById("btnLimpiarFirmaCliente");
        const badgeEstadoFirma = doc.getElementById("badgeEstadoFirma");
        const placeholderFirma = doc.getElementById("firmaClientePlaceholder");
        const lblFirmaClienteNombre = doc.getElementById("firmaClienteNombrePreview");
        const lblFirmaVendedor = doc.getElementById("firmaVendedorPreview");

        const htmlBtnOriginal = btnGenerar ? btnGenerar.innerHTML : "";
        let firmaPad = null;

        function actualizarEstadoFirmaUI(tieneFirma) {
            if (placeholderFirma) {
                placeholderFirma.classList.toggle("d-none", Boolean(tieneFirma));
            }
            if (badgeEstadoFirma) {
                if (tieneFirma) {
                    badgeEstadoFirma.textContent = "Firmado";
                    badgeEstadoFirma.className = "badge rounded-pill bg-success-subtle text-success border border-success-subtle px-2 py-1";
                } else {
                    badgeEstadoFirma.textContent = "Pendiente";
                    badgeEstadoFirma.className = "badge rounded-pill bg-warning-subtle text-warning-emphasis border border-warning-subtle px-2 py-1";
                }
            }
        }

        function actualizarEstadoBotonGenerar() {
            if (!btnGenerar) return;
            const checkOk = Boolean(chkConfirmar?.checked);
            const firmaOk = !firmaPad || !firmaPad.isEmpty();
            btnGenerar.disabled = !(checkOk && firmaOk);
        }

        if (canvasFirma && firmaPadLib && typeof firmaPadLib.create === "function") {
            firmaPad = firmaPadLib.create(canvasFirma, {
                onChange: function (tieneFirma) {
                    actualizarEstadoFirmaUI(tieneFirma);
                    actualizarEstadoBotonGenerar();
                    ocultarError();
                }
            });
            actualizarEstadoFirmaUI(false);
        }

        if (btnLimpiarFirma) {
            btnLimpiarFirma.addEventListener("click", function () {
                if (firmaPad) firmaPad.clear();
                actualizarEstadoFirmaUI(false);
                actualizarEstadoBotonGenerar();
            });
        }

        function ocultarError() {
            if (alertaError) alertaError.classList.add("d-none");
            if (btnElegirOtroLote) btnElegirOtroLote.classList.add("d-none");
        }

        function mostrarError(mensaje, esConflictoLote = false) {
            if (lblError) lblError.textContent = limpiarPrefijoError(mensaje);
            if (alertaError) alertaError.classList.remove("d-none");
            if (btnElegirOtroLote) btnElegirOtroLote.classList.toggle("d-none", !esConflictoLote);
        }

        function obtenerBorradorSeguro() {
            if (wizardState && typeof wizardState.getDraft === "function") {
                return wizardState.getDraft() || {};
            }
            try {
                const raw = typeof sessionStorage !== "undefined"
                    ? sessionStorage.getItem("sf_nueva_venta_draft")
                    : null;
                return raw ? JSON.parse(raw) : {};
            } catch {
                return {};
            }
        }

        function cargarDatosBorrador() {
            ocultarError();
            if (alertaExito) alertaExito.classList.add("d-none");

            const draft = obtenerBorradorSeguro();
            const cliente = draft.cliente || {};
            const lote = draft.lote || {};
            const fin = draft.financiamiento || {};

            if (lblClienteNombre) {
                const nombre = `${cliente.nombre || ""} ${cliente.apellidos || ""}`.trim();
                lblClienteNombre.textContent = nombre || "-";
                if (lblFirmaClienteNombre) {
                    lblFirmaClienteNombre.textContent = nombre || "-";
                }
            }
            if (lblFirmaVendedor) {
                const session = authApi && typeof authApi.getSession === "function"
                    ? authApi.getSession()
                    : null;
                lblFirmaVendedor.textContent = session?.nombreCompleto || session?.nombre || session?.email || "Sesión autenticada";
            }
            if (lblClienteEmail) {
                lblClienteEmail.textContent = cliente.correo || cliente.email || "-";
            }

            if (lblLoteClave) {
                const seccion = lote.seccion || lote.tipo || "";
                lblLoteClave.textContent = seccion ? `${lote.clave || "-"} · ${seccion}` : (lote.clave || "-");
            }
            if (lblLoteCapacidad) {
                lblLoteCapacidad.textContent = `${lote.gavetas || lote.capacidad || 0} gavetas`;
            }

            const precio = Number(lote.precio || 0);
            const precioStr = formatearMonedaMXN(precio);
            if (lblPrecioTotal) lblPrecioTotal.textContent = precioStr;

            const esContado = String(fin.modalidad || "").toLowerCase() === "contado";
            if (contenedorPagoContado) {
                contenedorPagoContado.classList.toggle("d-none", !esContado);
            }

            if (esContado) {
                if (lblPrecioEnganche) lblPrecioEnganche.textContent = "Pago único de contado";
                if (lblPlanDetalle) lblPlanDetalle.textContent = "Liquidación inmediata · 0% interés";
                if (lblPlanTasa) lblPlanTasa.textContent = `Sin intereses · Pago único de ${precioStr}`;
            } else {
                const pctEnganche = Number(fin.enganchePorcentaje ?? 20);
                const montoEng = Number.isFinite(Number(fin.montoEnganche)) && Number(fin.montoEnganche) > 0
                    ? Number(fin.montoEnganche)
                    : precio * (pctEnganche / 100.0);
                const plazo = Number(fin.plazoMeses ?? 24);
                const mensualidadNum = Number(fin.mensualidadEstimadaNumero ?? 0);
                const mensualidadStr = mensualidadNum > 0
                    ? formatearMonedaMXN(mensualidadNum)
                    : `$${fin.mensualidadEstimada || "0.00"}`;
                const tasa = Number(fin.tasaAnualPorcentaje ?? (plazo === 12 ? 16 : plazo === 18 ? 18 : plazo === 48 ? 24 : 20));

                if (lblPrecioEnganche) lblPrecioEnganche.textContent = `Enganche de ${formatearMonedaMXN(montoEng)} (${pctEnganche}%)`;
                if (lblPlanDetalle) lblPlanDetalle.textContent = `${plazo} pagos de ${mensualidadStr}`;
                if (lblPlanTasa) lblPlanTasa.textContent = `Tasa anual fija del ${tasa}%`;
            }
        }

        function mostrarPaso4() {
            if (step3Card) step3Card.classList.add("d-none");
            if (step4Card) step4Card.classList.remove("d-none");

            if (chkConfirmar) chkConfirmar.checked = false;
            if (firmaPad) {
                firmaPad.clear();
                firmaPad.resize(false);
            }
            actualizarEstadoFirmaUI(false);
            if (btnGenerar) {
                btnGenerar.disabled = true;
                btnGenerar.innerHTML = htmlBtnOriginal || '<i class="fa-solid fa-check me-2" aria-hidden="true"></i> Generar contrato';
            }

            if (step3Indicator && step4Indicator) {
                const circle3 = step3Indicator.querySelector("div");
                const circle4 = step4Indicator.querySelector("div");
                if (circle3) {
                    circle3.style.backgroundColor = "#FFFFFF";
                    circle3.style.borderColor = "#2D4224";
                    circle3.classList.remove("text-white");
                    circle3.classList.add("text-success", "border", "border-2");
                    circle3.innerHTML = '<i class="fa-solid fa-check" aria-hidden="true"></i>';
                }
                if (circle4) {
                    circle4.style.backgroundColor = "#2D4224";
                    circle4.style.borderColor = "#2D4224";
                    circle4.classList.remove("text-muted", "bg-white", "border");
                    circle4.classList.add("text-white", "fw-bold", "shadow-sm");
                }
            }
        }

        function mostrarPaso3() {
            if (step4Card) step4Card.classList.add("d-none");
            if (step3Card) step3Card.classList.remove("d-none");

            if (step3Indicator && step4Indicator) {
                const circle3 = step3Indicator.querySelector("div");
                const circle4 = step4Indicator.querySelector("div");
                if (circle3) {
                    circle3.style.backgroundColor = "#2D4224";
                    circle3.style.borderColor = "#2D4224";
                    circle3.classList.remove("text-success", "bg-white", "border");
                    circle3.classList.add("text-white", "fw-bold");
                    circle3.textContent = "3";
                }
                if (circle4) {
                    circle4.style.backgroundColor = "#FFFFFF";
                    circle4.style.borderColor = "#D1D5DB";
                    circle4.classList.remove("text-white", "fw-bold");
                    circle4.classList.add("text-muted", "fw-semibold", "bg-white", "border");
                }
            }
        }

        function regresarAPaso2PorConflicto() {
            ocultarError();
            if (chkConfirmar) chkConfirmar.checked = false;
            if (btnGenerar) btnGenerar.disabled = true;

            if (step4Card) step4Card.classList.add("d-none");
            if (step3Card) step3Card.classList.add("d-none");
            if (step2Card) step2Card.classList.remove("d-none");

            if (step2Indicator && step3Indicator && step4Indicator) {
                const c2 = step2Indicator.querySelector("div");
                const c3 = step3Indicator.querySelector("div");
                const c4 = step4Indicator.querySelector("div");
                if (c2) {
                    c2.style.backgroundColor = "#2D4224";
                    c2.style.borderColor = "#2D4224";
                    c2.classList.remove("text-success", "bg-white", "border");
                    c2.classList.add("text-white", "fw-bold");
                    c2.textContent = "2";
                }
                for (const [el, num] of [[c3, "3"], [c4, "4"]]) {
                    if (el) {
                        el.style.backgroundColor = "#FFFFFF";
                        el.style.borderColor = "#D1D5DB";
                        el.classList.remove("text-white", "fw-bold", "text-success");
                        el.classList.add("text-muted", "fw-semibold", "bg-white", "border");
                        el.textContent = num;
                    }
                }
            }

            if (win?.paso2Lote?.recargar) {
                win.paso2Lote.recargar();
            }
        }

        if (win) {
            win.addEventListener("sf:paso3-completado", function () {
                cargarDatosBorrador();
                mostrarPaso4();
            });
        }

        if (chkConfirmar && btnGenerar) {
            chkConfirmar.addEventListener("change", function () {
                actualizarEstadoBotonGenerar();
            });
        }

        if (btnAnterior) {
            btnAnterior.addEventListener("click", mostrarPaso3);
        }

        if (btnElegirOtroLote) {
            btnElegirOtroLote.addEventListener("click", regresarAPaso2PorConflicto);
        }

        function sincronizarReferenciaContado() {
            if (!selectMetodoPago || !txtReferenciaPago) return;
            const metodo = String(selectMetodoPago.value || "Efectivo").trim();
            const requiereRef = metodo === "TransferenciaBancaria" || metodo === "Tarjeta";
            txtReferenciaPago.required = requiereRef;
            txtReferenciaPago.placeholder = requiereRef
                ? "Obligatoria: folio de transferencia o autorización"
                : "Opcional para pagos en efectivo";
        }

        if (selectMetodoPago) {
            selectMetodoPago.addEventListener("change", function () {
                sincronizarReferenciaContado();
                ocultarError();
            });
            sincronizarReferenciaContado();
        }

        if (btnGenerar) {
            btnGenerar.addEventListener("click", async function () {
                if (!chkConfirmar?.checked) return;
                if (firmaPad && firmaPad.isEmpty()) {
                    mostrarError("Captura la firma digital del cliente antes de generar el contrato.");
                    return;
                }
                ocultarError();

                const draft = obtenerBorradorSeguro();
                const modalidad = String(draft?.financiamiento?.modalidad || "Credito");

                if (!draft?.cliente?.id || !draft?.lote?.id) {
                    mostrarError("Faltan identificadores reales del cliente o del lote en el borrador. Regresa e inténtalo de nuevo.");
                    return;
                }

                if (!ventasApi) {
                    mostrarError("No se encontró el cliente de API de ventas (ventasApi.js).");
                    return;
                }

                const esContado = modalidad.toLowerCase() === "contado";
                const metodoSeleccionado = selectMetodoPago ? selectMetodoPago.value : "Efectivo";
                const referenciaTexto = txtReferenciaPago ? txtReferenciaPago.value.trim() : "";

                if (esContado && (metodoSeleccionado === "TransferenciaBancaria" || metodoSeleccionado === "Tarjeta") && !referenciaTexto) {
                    mostrarError("La referencia de pago es obligatoria para pagos con Transferencia bancaria o Tarjeta.");
                    if (txtReferenciaPago) txtReferenciaPago.focus();
                    return;
                }

                const firmaClienteBase64 = firmaPad ? firmaPad.toDataURL() : "";
                const payload = esContado
                    ? construirPayloadVentaContado(draft, metodoSeleccionado, referenciaTexto, firmaClienteBase64)
                    : construirPayloadVentaFinanciada(draft, firmaClienteBase64);

                btnGenerar.disabled = true;
                btnGenerar.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Generando contrato...';

                try {
                    const res = esContado
                        ? await ventasApi.registrarVentaContado(payload)
                        : await ventasApi.registrarVentaFinanciada(payload);

                    btnGenerar.innerHTML = '<i class="fa-solid fa-check me-2" aria-hidden="true"></i> Contrato generado';
                    btnGenerar.style.backgroundColor = "#1e2e18";

                    const destinoDetalle = `/Contratos/Detalle?id=${encodeURIComponent(res.ventaId)}`;
                    if (lblFolioGenerado) {
                        lblFolioGenerado.textContent = esContado
                            ? `Folio asignado: ${res.folioContrato} · Comprobante: ${res.folioPago || "-"}`
                            : `Folio asignado: ${res.folioContrato} · ${res.plazoMeses || res.totalMensualidades || "-"} mensualidades generadas`;
                    }
                    if (lnkVerContrato) {
                        lnkVerContrato.setAttribute("href", destinoDetalle);
                    }
                    if (alertaExito) {
                        alertaExito.classList.remove("d-none");
                    }

                    if (wizardState && typeof wizardState.clearDraft === "function") {
                        wizardState.clearDraft();
                    } else if (typeof sessionStorage !== "undefined") {
                        sessionStorage.removeItem("sf_nueva_venta_draft");
                    }

                    const draftBadge = doc.getElementById("draftBadge");
                    if (draftBadge) {
                        draftBadge.classList.remove("d-none");
                        draftBadge.textContent = `✓ Contrato ${res.folioContrato}`;
                    }

                    if (win) {
                        win.dispatchEvent(new CustomEvent("sf:contrato-generado", { detail: res }));
                        if (autoRedirectDelayMs >= 0 && typeof win.location?.assign === "function") {
                            setTimeout(function () {
                                win.location.assign(destinoDetalle);
                            }, autoRedirectDelayMs);
                        }
                    }
                } catch (err) {
                    const esConflicto = err?.status === 409;
                    mostrarError(
                        err?.message || "No fue posible generar el contrato.",
                        esConflicto
                    );
                    btnGenerar.disabled = false;
                    btnGenerar.innerHTML = htmlBtnOriginal || '<i class="fa-solid fa-check me-2" aria-hidden="true"></i> Generar contrato';
                }
            });
        }

        return {
            cargarDatosBorrador,
            regresarAPaso2PorConflicto
        };
    }

    return {
        construirPayloadVentaContado,
        construirPayloadVentaFinanciada,
        init
    };
});
