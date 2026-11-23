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

    function construirPayloadVentaContado(draft, metodoPago = "Efectivo", referenciaPago = "") {
        const clienteId = draft?.cliente?.id || "";
        const loteId = draft?.lote?.id || "";
        const metodo = String(metodoPago || "Efectivo").trim() || "Efectivo";
        const refLimpia = String(referenciaPago ?? "").trim();

        return {
            clienteId,
            loteId,
            metodoPago: metodo,
            referenciaPago: refLimpia ? refLimpia : null
        };
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const ventasApi = deps.ventasApi ?? win?.ventasApi;
        const wizardState = deps.wizardState ?? win?.wizardState;
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

        const htmlBtnOriginal = btnGenerar ? btnGenerar.innerHTML : "";

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
                btnGenerar.disabled = !chkConfirmar.checked;
            });
        }

        if (btnAnterior) {
            btnAnterior.addEventListener("click", mostrarPaso3);
        }

        if (btnElegirOtroLote) {
            btnElegirOtroLote.addEventListener("click", regresarAPaso2PorConflicto);
        }

        if (btnGenerar) {
            btnGenerar.addEventListener("click", async function () {
                if (!chkConfirmar?.checked) return;
                ocultarError();

                const draft = obtenerBorradorSeguro();
                const modalidad = String(draft?.financiamiento?.modalidad || "Credito");

                if (!draft?.cliente?.id || !draft?.lote?.id) {
                    mostrarError("Faltan identificadores reales del cliente o del lote en el borrador. Regresa e inténtalo de nuevo.");
                    return;
                }

                if (modalidad.toLowerCase() !== "contado") {
                    mostrarError("El registro de ventas a crédito estará disponible en el Milestone v0.7.0 (SF-067). Para completar una venta hoy, elige modalidad Contado en el Paso 3.");
                    return;
                }

                if (!ventasApi) {
                    mostrarError("No se encontró el cliente de API de ventas (ventasApi.js).");
                    return;
                }

                const payload = construirPayloadVentaContado(
                    draft,
                    selectMetodoPago ? selectMetodoPago.value : "Efectivo",
                    txtReferenciaPago ? txtReferenciaPago.value : ""
                );

                btnGenerar.disabled = true;
                btnGenerar.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Generando contrato...';

                try {
                    const res = await ventasApi.registrarVentaContado(payload);

                    btnGenerar.innerHTML = '<i class="fa-solid fa-check me-2" aria-hidden="true"></i> Contrato generado';
                    btnGenerar.style.backgroundColor = "#1e2e18";

                    const destinoDetalle = `/Contratos/Detalle?id=${encodeURIComponent(res.ventaId)}`;
                    if (lblFolioGenerado) {
                        lblFolioGenerado.textContent = `Folio asignado: ${res.folioContrato} · Comprobante: ${res.folioPago || "-"}`;
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
                        err?.message || "No fue posible generar el contrato de contado.",
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
        init
    };
});
