/**
 * cajaPage.js - Conector real de la ventanilla /Caja/Index (SF-079, SF-080, SF-081).
 * Conecta buscador por folio/UUID, tarjeta de cliente, corte del día y cobro idempotente.
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.cajaPage = api;
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

    function formatearFechaLarga(fechaIso) {
        if (!fechaIso) return "-";
        const d = new Date(String(fechaIso) + "T12:00:00Z");
        if (Number.isNaN(d.getTime())) return String(fechaIso);
        return new Intl.DateTimeFormat("es-MX", {
            day: "numeric",
            month: "long",
            year: "numeric",
            timeZone: "UTC"
        }).format(d);
    }

    function mapearMetodoPago(valor) {
        const norm = String(valor || "").trim().toLowerCase();
        if (norm.includes("efectivo")) return "Efectivo";
        if (norm.includes("tarjeta")) return "Tarjeta";
        return "TransferenciaBancaria";
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const cobranzaApi = deps.cobranzaApi ?? win?.cobranzaApi;

        if (!doc) return null;

        const inputBuscador = doc.getElementById("inputBuscadorContrato");
        const btnLimpiar = doc.getElementById("btnLimpiarBuscador");
        const cardCliente = doc.getElementById("cardClienteLocalizado");
        const avatarCliente = doc.getElementById("avatarCliente");
        const nombreCliente = doc.getElementById("nombreCliente");
        const detalleContrato = doc.getElementById("detalleContrato");
        const badgeEstadoCliente = doc.getElementById("badgeEstadoCliente");

        const cardMensualidad = doc.getElementById("cardOpcionMensualidad");
        const cardAbono = doc.getElementById("cardOpcionAbono");
        const cardLiquidacion = doc.getElementById("cardOpcionLiquidacion");

        const formPago = doc.getElementById("formAplicarPago");
        const inputImporte = doc.getElementById("inputImporteRecibir");
        const selectMetodo = doc.getElementById("selectMetodoPago");
        const inputReferencia = doc.getElementById("inputReferencia");
        const btnAplicar = doc.getElementById("btnAplicarPago");
        const textoBtn = doc.getElementById("textoBtnAplicarPago");

        const alertaExito = doc.getElementById("alertaPagoExitoso");
        const textoExito = doc.getElementById("textoPagoExitoso");
        const subtextoExito = doc.getElementById("subtextoPagoExitoso");
        const lnkVerRecibo = doc.getElementById("lnkVerReciboPago");
        const alertaError = doc.getElementById("alertaErrorCaja");

        const fechaCorteDia = doc.getElementById("fechaCorteDia");
        const montoSaldoInicial = doc.getElementById("montoSaldoInicial");
        const contadorPagos = doc.getElementById("contadorPagosRecibidos");
        const montoIngresos = doc.getElementById("montoIngresosDia");
        const montoTotalCaja = doc.getElementById("montoTotalCaja");
        const tituloPagosAtencion = doc.getElementById("tituloPagosAtencion");

        let tipoActual = "mensualidad";
        let contratoActual = null;
        let clavePendiente = null;

        function mostrarError(msg) {
            if (!alertaError) return;
            alertaError.textContent = String(msg || "Error en operación de caja.").replace(/^\[(CONFLICTO|NO_ENCONTRADO)\]\s*/i, "");
            alertaError.classList.remove("d-none");
        }

        function ocultarError() {
            if (!alertaError) return;
            alertaError.classList.add("d-none");
            alertaError.textContent = "";
        }

        function limpiarAlertas() {
            ocultarError();
            if (alertaExito) alertaExito.classList.add("d-none");
        }

        function verificarSesion() {
            if (authApi && apiClient) {
                const storage = deps.storage ?? (typeof sessionStorage !== "undefined" ? sessionStorage : null);
                const token = storage ? storage.getItem(apiClient.TOKEN_STORAGE_KEY) : null;
                const session = authApi.getSession({ client: apiClient, storage });
                if (!token || !session) {
                    win?.location?.assign?.("/Cuenta/Login");
                    return false;
                }
            }
            return true;
        }

        function actualizarImportesPorTipo() {
            const montoMens = Number(contratoActual?.montoMensualidad ?? 0);
            const montoLiq = Number(contratoActual?.montoLiquidacionEstimado ?? 0);

            if (cardMensualidad) cardMensualidad.classList.toggle("active", tipoActual === "mensualidad");
            if (cardAbono) cardAbono.classList.toggle("active", tipoActual === "abono");
            if (cardLiquidacion) cardLiquidacion.classList.toggle("active", tipoActual === "liquidacion");

            if (!inputImporte || !textoBtn) return;

            if (tipoActual === "mensualidad") {
                const formatted = formatearMonedaMXN(montoMens);
                inputImporte.value = formatted;
                inputImporte.readOnly = true;
                textoBtn.textContent = `Aplicar pago por ${formatted}`;
            } else if (tipoActual === "abono") {
                inputImporte.value = "$10,000.00";
                inputImporte.readOnly = false;
                textoBtn.textContent = "Aplicar abono por $10,000.00";
            } else {
                const formatted = formatearMonedaMXN(montoLiq);
                inputImporte.value = formatted;
                inputImporte.readOnly = true;
                textoBtn.textContent = `Aplicar liquidación por ${formatted}`;
            }
        }

        function pintarContrato(dto) {
            contratoActual = dto;
            clavePendiente = cobranzaApi?.generarClaveIdempotencia?.() || null;

            if (cardCliente) cardCliente.classList.remove("d-none");
            if (avatarCliente) avatarCliente.textContent = dto.inicialesCliente || "SF";
            if (nombreCliente) nombreCliente.textContent = dto.clienteNombre || "-";
            if (detalleContrato) detalleContrato.textContent = `${dto.folioContrato} · ${dto.loteUbicacion || dto.loteClave}`;

            if (badgeEstadoCliente) {
                const tieneVencidas = Number(dto.cuotasVencidas ?? 0) > 0;
                badgeEstadoCliente.textContent = dto.estadoVisual || "Al corriente";
                badgeEstadoCliente.style.backgroundColor = tieneVencidas ? "#FEE8E8" : "#E8F5E9";
                badgeEstadoCliente.style.color = tieneVencidas ? "#B91C1C" : "#1B5E20";
                badgeEstadoCliente.style.border = tieneVencidas ? "1px solid #FCA5A5" : "1px solid #A7F3D0";
            }

            actualizarImportesPorTipo();
        }

        function pintarResumenCaja(resumen) {
            if (!resumen) return;
            if (fechaCorteDia) fechaCorteDia.textContent = formatearFechaLarga(resumen.fecha);
            if (montoSaldoInicial) montoSaldoInicial.textContent = formatearMonedaMXN(resumen.saldoInicial);
            if (contadorPagos) contadorPagos.textContent = String(resumen.pagosRecibidosHoy ?? 0);
            if (montoIngresos) montoIngresos.textContent = formatearMonedaMXN(resumen.ingresosDelDia ?? 0);
            if (montoTotalCaja) montoTotalCaja.textContent = formatearMonedaMXN(resumen.totalEnCaja ?? 0);
            if (tituloPagosAtencion) {
                const n = Number(resumen.pagosVencidosCount ?? 0);
                tituloPagosAtencion.textContent = `${n} pago${n === 1 ? "" : "s"} requieren atención`;
            }
        }

        async function cargarResumenCaja() {
            if (!cobranzaApi || !verificarSesion()) return;
            try {
                const resumen = await cobranzaApi.obtenerResumenCaja();
                pintarResumenCaja(resumen);
            } catch {
                // Silencioso si solo falla resumen en background
            }
        }

        async function buscarContrato(folio) {
            const q = String(folio || "").trim();
            if (!q || !cobranzaApi || !verificarSesion()) return;
            ocultarError();
            try {
                const dto = await cobranzaApi.obtenerContratoParaCaja(q);
                pintarContrato(dto);
            } catch (err) {
                mostrarError(err?.message || `No se localizó el contrato ${q}.`);
            }
        }

        if (cardMensualidad) {
            cardMensualidad.addEventListener("click", function () {
                tipoActual = "mensualidad";
                actualizarImportesPorTipo();
            });
        }
        if (cardAbono) {
            cardAbono.addEventListener("click", function () {
                tipoActual = "abono";
                actualizarImportesPorTipo();
            });
        }
        if (cardLiquidacion) {
            cardLiquidacion.addEventListener("click", function () {
                tipoActual = "liquidacion";
                actualizarImportesPorTipo();
            });
        }

        if (btnLimpiar && inputBuscador) {
            btnLimpiar.addEventListener("click", function () {
                inputBuscador.value = "";
                inputBuscador.focus();
            });
        }

        let debounce = null;
        if (inputBuscador) {
            inputBuscador.addEventListener("input", function () {
                const val = inputBuscador.value.trim();
                clearTimeout(debounce);
                if (val.length >= 8) {
                    debounce = setTimeout(function () {
                        buscarContrato(val);
                    }, 260);
                }
            });
        }

        if (inputImporte) {
            inputImporte.addEventListener("input", function () {
                if (tipoActual === "abono" && textoBtn) {
                    const num = Number(String(inputImporte.value || "0").replace(/[^0-9.]/g, ""));
                    textoBtn.textContent = `Aplicar abono por ${formatearMonedaMXN(num)}`;
                }
            });
        }

        if (formPago) {
            formPago.addEventListener("submit", async function (e) {
                e.preventDefault();
                limpiarAlertas();

                const folio = contratoActual?.folioContrato || inputBuscador?.value?.trim() || "";
                if (!folio) {
                    mostrarError("Localiza un contrato válido antes de aplicar el pago.");
                    return;
                }

                if (!cobranzaApi || !verificarSesion()) return;

                const importeNum = Number(String(inputImporte?.value || "0").replace(/[^0-9.]/g, ""));
                if (!Number.isFinite(importeNum) || importeNum <= 0) {
                    mostrarError("El importe debe ser estrictamente mayor a cero.");
                    return;
                }

                const metodo = mapearMetodoPago(selectMetodo ? selectMetodo.value : "TransferenciaBancaria");
                const referencia = inputReferencia ? inputReferencia.value.trim() : "";

                if ((metodo === "TransferenciaBancaria" || metodo === "Tarjeta") && referencia.length < 4) {
                    mostrarError("La referencia (folio o últimos 4 dígitos) es obligatoria para transferencias y tarjetas.");
                    return;
                }

                const clave = clavePendiente || cobranzaApi.generarClaveIdempotencia();
                clavePendiente = clave;

                const htmlOrig = btnAplicar ? btnAplicar.innerHTML : "";
                if (btnAplicar) {
                    btnAplicar.disabled = true;
                    btnAplicar.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Aplicando pago...';
                }

                try {
                    let recibo;
                    if (tipoActual === "abono") {
                        recibo = await cobranzaApi.aplicarAbonoCapital({
                            folioContrato: folio,
                            montoAbono: importeNum,
                            metodoPago: metodo,
                            referencia: referencia || null,
                            claveIdempotencia: clave
                        });
                    } else if (tipoActual === "liquidacion") {
                        recibo = await cobranzaApi.aplicarLiquidacion({
                            folioContrato: folio,
                            importeRecibido: importeNum,
                            metodoPago: metodo,
                            referencia: referencia || null,
                            claveIdempotencia: clave
                        });
                    } else {
                        recibo = await cobranzaApi.aplicarPagoMensualidad({
                            folioContrato: folio,
                            importeRecibido: importeNum,
                            metodoPago: metodo,
                            referencia: referencia || null,
                            numeroCuota: contratoActual?.proximaCuotaNumero ?? null,
                            claveIdempotencia: clave
                        });
                    }

                    clavePendiente = cobranzaApi.generarClaveIdempotencia();

                    if (textoExito) textoExito.textContent = `¡Pago de ${formatearMonedaMXN(recibo.montoTotal)} aplicado con éxito!`;
                    if (subtextoExito) subtextoExito.textContent = `Recibo ${recibo.folioPago} (${recibo.concepto}) emitido correctamente.`;
                    if (lnkVerRecibo) {
                        lnkVerRecibo.setAttribute("href", `/Caja/Recibo?folio=${encodeURIComponent(recibo.folioPago)}`);
                        lnkVerRecibo.classList.remove("d-none");
                    }
                    if (alertaExito) alertaExito.classList.remove("d-none");

                    await Promise.all([cargarResumenCaja(), buscarContrato(folio)]);
                } catch (err) {
                    mostrarError(err?.message || "No fue posible aplicar el pago.");
                    clavePendiente = cobranzaApi.generarClaveIdempotencia();
                } finally {
                    if (btnAplicar) {
                        btnAplicar.disabled = false;
                        btnAplicar.innerHTML = htmlOrig;
                    }
                }
            });
        }

        cargarResumenCaja();
        const params = new URLSearchParams(win?.location?.search || "");
        const contratoParam = (params.get("contrato") || inputBuscador?.value || "").trim();
        if (contratoParam) {
            if (inputBuscador) inputBuscador.value = contratoParam;
            buscarContrato(contratoParam);
        }

        return {
            cargarResumenCaja,
            buscarContrato,
            pintarContrato,
            pintarResumenCaja
        };
    }

    return {
        formatearMonedaMXN,
        mapearMetodoPago,
        init
    };
});
