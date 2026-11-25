/**
 * paso3Financiamiento.js - Lógica interactiva del Paso 3 del Wizard (SF-054).
 * Conecta el cotizador en vivo con simuladorApi.js (GET /planes y POST /cotizar) y wizardState.js.
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.paso3Financiamiento = api;
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

    function formatearNumeroSinSimbolo(monto) {
        const num = Number(monto);
        const seguro = Number.isFinite(num) ? num : 0;
        return seguro.toLocaleString("es-MX", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function formatearFechaPrimerPago(fechaIso) {
        if (!fechaIso || typeof fechaIso !== "string") {
            return "Por definir";
        }
        const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(fechaIso.trim());
        const fecha = match
            ? new Date(Date.UTC(Number(match[1]), Number(match[2]) - 1, Number(match[3])))
            : new Date(fechaIso);

        if (Number.isNaN(fecha.getTime())) {
            return fechaIso;
        }

        return new Intl.DateTimeFormat("es-MX", {
            day: "2-digit",
            month: "short",
            year: "numeric",
            timeZone: "UTC"
        }).format(fecha);
    }

    function construirPayloadCotizacion(draft, modalidad, pctEnganche, plazoMeses) {
        const lote = draft?.lote || {};
        const esContado = String(modalidad || "").trim().toLowerCase() === "contado";

        return {
            loteId: lote.id || null,
            gavetas: Number(lote.gavetas ?? lote.capacidad ?? 2),
            modalidad: esContado ? "Contado" : "Credito",
            porcentajeEnganche: esContado ? 0 : Number(pctEnganche || 20),
            plazoMeses: esContado ? 0 : Number(plazoMeses || 24)
        };
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const simuladorApi = deps.simuladorApi ?? win?.simuladorApi;
        const wizardState = deps.wizardState ?? win?.wizardState;

        if (!doc) return null;

        const step2Card = doc.getElementById("step2Card");
        const step3Card = doc.getElementById("step3Card");

        const btnCredito = doc.getElementById("btnModalidadCredito");
        const btnContado = doc.getElementById("btnModalidadContado");
        const contenedorCredito = doc.getElementById("contenedorOpcionesCredito");

        const selectEnganche = doc.getElementById("selectEnganche");
        const selectPlazo = doc.getElementById("selectPlazo");

        const lblTitulo = doc.getElementById("lblBannerTitulo");
        const lblSubtitulo = doc.getElementById("lblBannerSubtitulo");
        const lblMonto = doc.getElementById("lblBannerMonto");
        const contenedorBtnAmortizacion = doc.getElementById("contenedorBtnAmortizacion");
        const btnVerTablaAmortizacion = doc.getElementById("btnVerTablaAmortizacion");
        const tbodyAmortizacion = doc.getElementById("tbodyTablaAmortizacionPaso3");
        const subtituloAmortizacion = doc.getElementById("subtituloTablaAmortizacion");

        const alertaError = doc.getElementById("paso3AlertaError");
        const mensajeError = doc.getElementById("paso3MensajeError");
        const btnReintentar = doc.getElementById("btnReintentarCotizacion");

        const btnAnterior = doc.getElementById("btnAnteriorPaso3");
        const btnContinuar = doc.getElementById("btnContinuarPaso3");

        const step2Indicator = doc.getElementById("stepIndicator2");
        const step3Indicator = doc.getElementById("stepIndicator3");
        const step4Indicator = doc.getElementById("stepIndicator4");

        if (!step3Card) return null;

        let modalidadActual = "Credito";
        let precioLoteActual = 120000.0;
        let secuenciaCotizacion = 0;
        let cotizacionActual = null;
        let planesCargados = false;

        function ocultarError() {
            if (alertaError) alertaError.classList.add("d-none");
        }

        function mostrarError(msg) {
            if (mensajeError) mensajeError.textContent = msg;
            if (alertaError) alertaError.classList.remove("d-none");
        }

        function actualizarOpcionesEnganche(precio, porcentajes = null) {
            if (!selectEnganche) return;
            const lista = Array.isArray(porcentajes) && porcentajes.length > 0 ? porcentajes : [10, 20, 30];
            const valorActual = selectEnganche.value || "20";

            selectEnganche.innerHTML = "";
            for (const pct of lista) {
                const opt = doc.createElement("option");
                opt.value = String(pct);
                opt.textContent = `${pct}% · ${formatearMonedaMXN(precio * (Number(pct) / 100))}`;
                if (String(pct) === valorActual) {
                    opt.selected = true;
                }
                selectEnganche.appendChild(opt);
            }
        }

        function poblarPlazos(plazos) {
            if (!selectPlazo || !Array.isArray(plazos) || plazos.length === 0) return;
            const valorActual = selectPlazo.value || "24";

            selectPlazo.innerHTML = "";
            for (const item of plazos) {
                const opt = doc.createElement("option");
                opt.value = String(item.meses);
                opt.textContent = item.descripcion || `${item.meses} meses · ${item.tasaAnualPorcentaje}% anual`;
                if (String(item.meses) === valorActual) {
                    opt.selected = true;
                }
                selectPlazo.appendChild(opt);
            }
        }

        async function cargarPlanesSiAplica() {
            if (planesCargados || !simuladorApi) return;
            try {
                const planes = await simuladorApi.obtenerPlanes();
                if (planes) {
                    planesCargados = true;
                    actualizarOpcionesEnganche(precioLoteActual, planes.porcentajesEnganche);
                    poblarPlazos(planes.plazos);
                }
            } catch {
                // Mantiene los planes oficiales precargados en el select si aún no cargan
            }
        }

        function renderizarTablaAmortizacion(res) {
            if (!tbodyAmortizacion) return;
            const filas = Array.isArray(res?.tablaAmortizacion) ? res.tablaAmortizacion : [];
            if (filas.length === 0) {
                tbodyAmortizacion.innerHTML = '<tr><td colspan="7" class="text-center py-4 text-muted">Sin tabla de amortización para modalidad de contado.</td></tr>';
                if (btnVerTablaAmortizacion) btnVerTablaAmortizacion.disabled = true;
                return;
            }

            if (subtituloAmortizacion) {
                subtituloAmortizacion.textContent = `${res.plazoMeses} mensualidades · Tasa ${res.tasaAnualPorcentaje}% anual · Capital financiado: ${formatearMonedaMXN(res.saldoAFinanciar)} · Total a pagar: ${formatearMonedaMXN(res.totalAPagar)}`;
            }

            tbodyAmortizacion.innerHTML = filas.map(function (f) {
                return `<tr>` +
                    `<td class="ps-4 fw-semibold">${Number(f.numeroCuota || 0)}</td>` +
                    `<td class="text-muted">${formatearFechaPrimerPago(f.fechaVencimiento)}</td>` +
                    `<td class="text-end sf-money">${formatearMonedaMXN(f.saldoInicial)}</td>` +
                    `<td class="text-end sf-money text-muted">${formatearMonedaMXN(f.interes)}</td>` +
                    `<td class="text-end sf-money">${formatearMonedaMXN(f.capital)}</td>` +
                    `<td class="text-end sf-money fw-bold text-dark">${formatearMonedaMXN(f.cuotaTotal)}</td>` +
                    `<td class="text-end pe-4 sf-money fw-semibold">${formatearMonedaMXN(f.saldoFinal)}</td>` +
                    `</tr>`;
            }).join("");

            if (btnVerTablaAmortizacion) btnVerTablaAmortizacion.disabled = false;
        }

        function actualizarEstilosToggle(esContado) {
            if (btnCredito && btnContado) {
                btnCredito.setAttribute("aria-pressed", esContado ? "false" : "true");
                btnContado.setAttribute("aria-pressed", esContado ? "true" : "false");
                if (esContado) {
                    btnContado.style.backgroundColor = "#2D4224";
                    btnContado.style.borderColor = "#2D4224";
                    btnContado.className = "btn px-4 py-2 rounded-3 fw-medium text-white shadow-sm transition-all";

                    btnCredito.style.backgroundColor = "#FFFFFF";
                    btnCredito.style.borderColor = "#E5E7EB";
                    btnCredito.className = "btn px-4 py-2 rounded-3 fw-medium text-dark bg-white transition-all";
                } else {
                    btnCredito.style.backgroundColor = "#2D4224";
                    btnCredito.style.borderColor = "#2D4224";
                    btnCredito.className = "btn px-4 py-2 rounded-3 fw-medium text-white shadow-sm transition-all";

                    btnContado.style.backgroundColor = "#FFFFFF";
                    btnContado.style.borderColor = "#E5E7EB";
                    btnContado.className = "btn px-4 py-2 rounded-3 fw-medium text-dark bg-white transition-all";
                }
            }
            if (contenedorCredito) {
                contenedorCredito.classList.toggle("d-none", esContado);
            }
            if (contenedorBtnAmortizacion) {
                contenedorBtnAmortizacion.classList.toggle("d-none", esContado);
            }
        }

        async function cotizarEnServidor() {
            if (!simuladorApi) return;
            ocultarError();
            cotizacionActual = null;
            if (btnContinuar) btnContinuar.disabled = true;
            if (btnVerTablaAmortizacion) btnVerTablaAmortizacion.disabled = true;

            const draft = wizardState ? wizardState.getDraft() : {};
            if (draft?.lote?.precio) {
                precioLoteActual = Number(draft.lote.precio);
                actualizarOpcionesEnganche(precioLoteActual);
            }

            const esContado = modalidadActual === "Contado";
            if (lblTitulo) {
                lblTitulo.textContent = esContado ? "Pago único de contado" : "Mensualidad estimada";
            }
            if (lblSubtitulo) {
                lblSubtitulo.textContent = esContado
                    ? "Sin intereses · Liquidación inmediata"
                    : "Calculando plan oficial...";
            }
            if (lblMonto) {
                lblMonto.textContent = "Calculando...";
            }

            const pctEnganche = Number(selectEnganche ? selectEnganche.value : "20");
            const plazoMeses = Number(selectPlazo ? selectPlazo.value : "24");
            const payload = construirPayloadCotizacion(draft, modalidadActual, pctEnganche, plazoMeses);

            const reqId = ++secuenciaCotizacion;
            try {
                const res = await simuladorApi.cotizar(payload);
                if (reqId !== secuenciaCotizacion) return;

                cotizacionActual = res;
                if (esContado) {
                    if (lblTitulo) lblTitulo.textContent = "Pago único de contado";
                    if (lblSubtitulo) lblSubtitulo.textContent = "Sin intereses · Liquidación inmediata";
                    if (lblMonto) lblMonto.textContent = formatearMonedaMXN(res.precioLista);
                } else {
                    if (lblTitulo) lblTitulo.textContent = "Mensualidad estimada";
                    if (lblSubtitulo) lblSubtitulo.textContent = `Primer pago: ${formatearFechaPrimerPago(res.fechaPrimerPago)}`;
                    if (lblMonto) lblMonto.textContent = formatearMonedaMXN(res.mensualidadEstimada);
                }
                renderizarTablaAmortizacion(res);

                if (wizardState) {
                    wizardState.updateDraft({
                        financiamiento: {
                            modalidad: res.modalidad,
                            enganchePorcentaje: Number(res.porcentajeEnganche ?? 0),
                            montoEnganche: Number(res.montoEnganche ?? 0),
                            saldoAFinanciar: Number(res.saldoAFinanciar ?? 0),
                            plazoMeses: Number(res.plazoMeses ?? 0),
                            tasaAnualPorcentaje: Number(res.tasaAnualPorcentaje ?? 0),
                            mensualidadEstimada: esContado
                                ? formatearNumeroSinSimbolo(res.precioLista)
                                : formatearNumeroSinSimbolo(res.mensualidadEstimada),
                            mensualidadEstimadaNumero: Number(res.mensualidadEstimada ?? 0),
                            fechaPrimerPago: res.fechaPrimerPago || null,
                            totalAPagar: Number(res.totalAPagar ?? res.precioLista ?? precioLoteActual)
                        },
                        paso: 3
                    });
                }

                if (btnContinuar) btnContinuar.disabled = false;
            } catch (err) {
                if (reqId !== secuenciaCotizacion) return;
                cotizacionActual = null;
                if (btnContinuar) btnContinuar.disabled = true;
                if (lblMonto) lblMonto.textContent = "$0.00";
                mostrarError(err?.message || "No fue posible obtener la cotización del simulador.");
            }
        }

        function activarCredito() {
            modalidadActual = "Credito";
            actualizarEstilosToggle(false);
            cotizarEnServidor();
        }

        function activarContado() {
            modalidadActual = "Contado";
            actualizarEstilosToggle(true);
            cotizarEnServidor();
        }

        if (win) {
            win.addEventListener("sf:paso2-completado", async function () {
                const draft = wizardState ? wizardState.getDraft() : {};
                if (draft?.lote?.precio) {
                    precioLoteActual = Number(draft.lote.precio);
                }
                await cargarPlanesSiAplica();
                actualizarOpcionesEnganche(precioLoteActual);
                mostrarPaso3();
            });
        }

        if (btnAnterior) {
            btnAnterior.addEventListener("click", mostrarPaso2);
        }

        if (btnCredito) {
            btnCredito.addEventListener("click", activarCredito);
        }
        if (btnContado) {
            btnContado.addEventListener("click", activarContado);
        }

        if (selectEnganche) {
            selectEnganche.addEventListener("change", cotizarEnServidor);
        }
        if (selectPlazo) {
            selectPlazo.addEventListener("change", cotizarEnServidor);
        }
        if (btnReintentar) {
            btnReintentar.addEventListener("click", cotizarEnServidor);
        }

        if (btnContinuar) {
            btnContinuar.addEventListener("click", function () {
                if (!cotizacionActual) return;
                if (wizardState) {
                    const draftActual = wizardState.getDraft();
                    wizardState.updateDraft({ ...draftActual, paso: 4 });
                }
                avanzarPaso4();
                if (win) {
                    win.dispatchEvent(new CustomEvent("sf:paso3-completado", {
                        detail: { cotizacion: cotizacionActual }
                    }));
                }
            });
        }

        function mostrarPaso3() {
            if (step2Card) step2Card.classList.add("d-none");
            if (step3Card) step3Card.classList.remove("d-none");

            if (step2Indicator && step3Indicator) {
                const circle2 = step2Indicator.querySelector("div");
                const circle3 = step3Indicator.querySelector("div");
                if (circle2) {
                    circle2.style.backgroundColor = "#FFFFFF";
                    circle2.style.borderColor = "#2D4224";
                    circle2.classList.remove("text-white");
                    circle2.classList.add("text-success", "border", "border-2");
                    circle2.innerHTML = '<i class="fa-solid fa-check" aria-hidden="true"></i>';
                }
                if (circle3) {
                    circle3.style.backgroundColor = "#2D4224";
                    circle3.style.borderColor = "#2D4224";
                    circle3.classList.remove("text-muted", "bg-white", "border");
                    circle3.classList.add("text-white", "fw-bold", "shadow-sm");
                }
            }

            cotizarEnServidor();
        }

        function mostrarPaso2() {
            if (step3Card) step3Card.classList.add("d-none");
            if (step2Card) step2Card.classList.remove("d-none");

            if (step2Indicator && step3Indicator) {
                const circle2 = step2Indicator.querySelector("div");
                const circle3 = step3Indicator.querySelector("div");
                if (circle2) {
                    circle2.style.backgroundColor = "#2D4224";
                    circle2.style.borderColor = "#2D4224";
                    circle2.classList.remove("text-success", "bg-white", "border");
                    circle2.classList.add("text-white", "fw-bold");
                    circle2.textContent = "2";
                }
                if (circle3) {
                    circle3.style.backgroundColor = "#FFFFFF";
                    circle3.style.borderColor = "#D1D5DB";
                    circle3.classList.remove("text-white", "fw-bold");
                    circle3.classList.add("text-muted", "fw-semibold", "bg-white", "border");
                }
            }
        }

        function avanzarPaso4() {
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

        return {
            cotizar: cotizarEnServidor
        };
    }

    return {
        construirPayloadCotizacion,
        formatearFechaPrimerPago,
        init
    };
});
