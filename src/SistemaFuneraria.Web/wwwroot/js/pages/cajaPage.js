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

    const ROLES_PERMITIDOS = ["Administrador", "Cajero"];

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

    function esContratoCerrado(contrato) {
        if (!contrato) return false;
        const est = String(contrato.estadoVisual || "").trim().toLowerCase();
        return est === "liquidado" || est === "cancelado" || Number(contrato.saldoInsoluto ?? 0) <= 0;
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const cobranzaApi = deps.cobranzaApi ?? win?.cobranzaApi;

        if (!doc) return null;

        const hasExplicitStorage = deps.storage !== undefined;
        const storageOpts = apiClient
            ? (hasExplicitStorage ? { client: apiClient, storage: deps.storage } : { client: apiClient })
            : {};

        const cajaPrincipal = doc.getElementById("cajaContenidoPrincipal");
        const cajaRestringido = doc.getElementById("cajaAccesoRestringido");
        const btnVolver403 = doc.getElementById("btnVolver403Caja");

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
                if (cajaPrincipal) cajaPrincipal.classList.add("d-none");
                if (cajaRestringido) cajaRestringido.classList.remove("d-none");
                return { autorizado: false, redirigido: false, session };
            }

            return { autorizado: true, redirigido: false, session };
        }

        const accesoInicial = verificarAcceso();
        if (!accesoInicial.autorizado) {
            return { autorizado: false, redirigido: accesoInicial.redirigido, session: accesoInicial.session };
        }

        const inputBuscador = doc.getElementById("inputBuscadorContrato");
        const btnLimpiar = doc.getElementById("btnLimpiarBuscador");
        const btnBuscar = doc.getElementById("btnBuscarContratoCaja");
        const estadoSinContrato = doc.getElementById("estadoSinContratoCaja");
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
        const asteriscoRef = doc.getElementById("asteriscoReferenciaCaja");
        const ayudaRef = doc.getElementById("ayudaReferenciaCaja");
        const btnAplicar = doc.getElementById("btnAplicarPago");
        const iconoBtn = doc.getElementById("iconoBtnAplicarPago");
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
        const sidebarBadge = doc.getElementById("sidebarCajaBadge");

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

        function sincronizarReferencia() {
            const metodo = mapearMetodoPago(selectMetodo ? selectMetodo.value : "TransferenciaBancaria");
            const esEfectivo = metodo === "Efectivo";
            if (asteriscoRef) {
                asteriscoRef.classList.toggle("d-none", esEfectivo);
            }
            if (inputReferencia) {
                inputReferencia.placeholder = esEfectivo ? "Opcional en efectivo" : "Folio o últimos 4 dígitos";
            }
            if (ayudaRef) {
                ayudaRef.textContent = esEfectivo
                    ? "Opcional para pagos en efectivo."
                    : "Obligatoria para transferencia o tarjeta (mín. 4 caracteres).";
            }
        }

        function actualizarImportesPorTipo() {
            const montoMens = Number(contratoActual?.montoMensualidad ?? 0);
            const montoLiq = Number(contratoActual?.montoLiquidacionEstimado ?? 0);
            const saldoInsoluto = Number(contratoActual?.saldoInsoluto ?? 0);

            if (cardMensualidad) {
                const act = tipoActual === "mensualidad";
                cardMensualidad.classList.toggle("active", act);
                cardMensualidad.setAttribute("aria-pressed", act ? "true" : "false");
            }
            if (cardAbono) {
                const act = tipoActual === "abono";
                cardAbono.classList.toggle("active", act);
                cardAbono.setAttribute("aria-pressed", act ? "true" : "false");
            }
            if (cardLiquidacion) {
                const act = tipoActual === "liquidacion";
                cardLiquidacion.classList.toggle("active", act);
                cardLiquidacion.setAttribute("aria-pressed", act ? "true" : "false");
            }

            if (iconoBtn) {
                iconoBtn.innerHTML = '<i class="fa-solid fa-circle-check" aria-hidden="true"></i>';
            }

            if (!contratoActual) {
                if (inputImporte) {
                    inputImporte.value = "$0.00";
                    inputImporte.readOnly = true;
                }
                if (btnAplicar) btnAplicar.disabled = true;
                if (textoBtn) textoBtn.textContent = "Selecciona un contrato para cobrar";
                return;
            }

            if (esContratoCerrado(contratoActual)) {
                const esCancelado = String(contratoActual.estadoVisual || "").trim().toLowerCase() === "cancelado";
                if (inputImporte) {
                    inputImporte.value = "$0.00";
                    inputImporte.readOnly = true;
                }
                if (btnAplicar) btnAplicar.disabled = true;
                if (textoBtn) {
                    textoBtn.textContent = esCancelado
                        ? "Contrato cancelado"
                        : "Contrato liquidado / sin saldo pendiente";
                }
                return;
            }

            if (btnAplicar) btnAplicar.disabled = false;
            if (!inputImporte || !textoBtn) return;

            if (tipoActual === "mensualidad") {
                const formatted = formatearMonedaMXN(montoMens);
                inputImporte.value = formatted;
                inputImporte.readOnly = true;
                textoBtn.textContent = `Aplicar pago por ${formatted}`;
            } else if (tipoActual === "abono") {
                const sugerido = saldoInsoluto > 0 ? Math.min(10000, saldoInsoluto) : 0;
                const formatted = formatearMonedaMXN(sugerido);
                inputImporte.value = formatted;
                inputImporte.readOnly = false;
                textoBtn.textContent = `Aplicar abono por ${formatted}`;
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

            if (estadoSinContrato) estadoSinContrato.classList.add("d-none");
            if (cardCliente) cardCliente.classList.remove("d-none");
            if (avatarCliente) avatarCliente.textContent = dto.inicialesCliente || "SF";
            if (nombreCliente) nombreCliente.textContent = dto.clienteNombre || "-";
            if (detalleContrato) detalleContrato.textContent = `${dto.folioContrato} · ${dto.loteUbicacion || dto.loteClave}`;

            if (badgeEstadoCliente) {
                const estLower = String(dto.estadoVisual || "").trim().toLowerCase();
                const esAlerta = Number(dto.cuotasVencidas ?? 0) > 0 || estLower === "cancelado";
                badgeEstadoCliente.textContent = dto.estadoVisual || "Al corriente";
                if (badgeEstadoCliente.style) {
                    badgeEstadoCliente.style.backgroundColor = esAlerta ? "#FEE8E8" : "#E8F5E9";
                    badgeEstadoCliente.style.color = esAlerta ? "#B91C1C" : "#1B5E20";
                    badgeEstadoCliente.style.border = esAlerta ? "1px solid #FCA5A5" : "1px solid #A7F3D0";
                }
            }

            actualizarImportesPorTipo();
        }

        function limpiarContrato() {
            if (inputBuscador) inputBuscador.value = "";
            contratoActual = null;
            clavePendiente = null;
            if (cardCliente) cardCliente.classList.add("d-none");
            if (estadoSinContrato) estadoSinContrato.classList.remove("d-none");
            if (inputReferencia) inputReferencia.value = "";
            limpiarAlertas();
            actualizarImportesPorTipo();
            if (inputBuscador && typeof inputBuscador.focus === "function") {
                inputBuscador.focus();
            }
        }

        function pintarResumenCaja(resumen) {
            if (!resumen) return;
            if (fechaCorteDia) fechaCorteDia.textContent = formatearFechaLarga(resumen.fecha);
            if (montoSaldoInicial) montoSaldoInicial.textContent = formatearMonedaMXN(resumen.saldoInicial);
            if (contadorPagos) contadorPagos.textContent = String(resumen.pagosRecibidosHoy ?? 0);
            if (montoIngresos) montoIngresos.textContent = formatearMonedaMXN(resumen.ingresosDelDia ?? 0);
            if (montoTotalCaja) montoTotalCaja.textContent = formatearMonedaMXN(resumen.totalEnCaja ?? 0);
            const n = Number(resumen.pagosVencidosCount ?? 0);
            if (tituloPagosAtencion) {
                tituloPagosAtencion.textContent = `${n} pago${n === 1 ? "" : "s"} requieren atención`;
            }
            if (sidebarBadge) {
                if (n > 0) {
                    sidebarBadge.textContent = String(n);
                    sidebarBadge.classList.remove("d-none");
                } else {
                    sidebarBadge.textContent = "";
                    sidebarBadge.classList.add("d-none");
                }
            }
        }

        async function cargarResumenCaja() {
            const acc = verificarAcceso();
            if (!cobranzaApi || !acc.autorizado) return;
            try {
                const resumen = await cobranzaApi.obtenerResumenCaja();
                pintarResumenCaja(resumen);
            } catch {
                // Silencioso si solo falla resumen en background
            }
        }

        async function buscarContrato(folio) {
            const q = String(folio || "").trim();
            const acc = verificarAcceso();
            if (!q || !cobranzaApi || !acc.autorizado) return;
            ocultarError();
            try {
                const dto = await cobranzaApi.obtenerContratoParaCaja(q);
                pintarContrato(dto);
            } catch (err) {
                contratoActual = null;
                clavePendiente = null;
                if (cardCliente) cardCliente.classList.add("d-none");
                if (estadoSinContrato) estadoSinContrato.classList.remove("d-none");
                actualizarImportesPorTipo();
                mostrarError(err?.message || `No se localizó el contrato ${q}.`);
            }
        }

        function registrarTarjetaMovimiento(card, tipo) {
            if (!card) return;
            const seleccionar = function () {
                tipoActual = tipo;
                actualizarImportesPorTipo();
            };
            card.addEventListener("click", seleccionar);
            card.addEventListener("keydown", function (e) {
                if (e.key === "Enter" || e.key === " ") {
                    if (typeof e.preventDefault === "function") e.preventDefault();
                    seleccionar();
                }
            });
        }

        registrarTarjetaMovimiento(cardMensualidad, "mensualidad");
        registrarTarjetaMovimiento(cardAbono, "abono");
        registrarTarjetaMovimiento(cardLiquidacion, "liquidacion");

        if (selectMetodo) {
            selectMetodo.addEventListener("change", sincronizarReferencia);
        }

        if (btnLimpiar) {
            btnLimpiar.addEventListener("click", limpiarContrato);
        }

        let debounce = null;
        if (btnBuscar && inputBuscador) {
            btnBuscar.addEventListener("click", function () {
                clearTimeout(debounce);
                buscarContrato(inputBuscador.value);
            });
        }

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
            inputBuscador.addEventListener("keydown", function (e) {
                if (e.key === "Enter") {
                    if (typeof e.preventDefault === "function") e.preventDefault();
                    clearTimeout(debounce);
                    buscarContrato(inputBuscador.value);
                }
            });
        }

        if (inputImporte) {
            inputImporte.addEventListener("input", function () {
                if (tipoActual === "abono" && textoBtn && contratoActual && !esContratoCerrado(contratoActual)) {
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
                if (!folio || !contratoActual) {
                    mostrarError("Localiza un contrato válido antes de aplicar el pago.");
                    return;
                }

                if (esContratoCerrado(contratoActual)) {
                    mostrarError("Este contrato ya no admite cobros porque está liquidado o cancelado.");
                    actualizarImportesPorTipo();
                    return;
                }

                const acc = verificarAcceso();
                if (!cobranzaApi || !acc.autorizado) return;

                const importeNum = Number(String(inputImporte?.value || "0").replace(/[^0-9.]/g, ""));
                if (!Number.isFinite(importeNum) || importeNum <= 0) {
                    mostrarError("El importe debe ser estrictamente mayor a cero.");
                    return;
                }

                const saldoInsoluto = Number(contratoActual.saldoInsoluto ?? 0);
                if (tipoActual === "abono" && saldoInsoluto > 0 && importeNum > saldoInsoluto) {
                    mostrarError(`El abono no puede exceder el saldo insoluto (${formatearMonedaMXN(saldoInsoluto)}). Usa Liquidación para cerrar el contrato.`);
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

                if (btnAplicar) btnAplicar.disabled = true;
                if (iconoBtn) iconoBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
                if (textoBtn) textoBtn.textContent = "Aplicando pago...";

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
                    actualizarImportesPorTipo();
                }
            });
        }

        sincronizarReferencia();
        actualizarImportesPorTipo();
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
            pintarResumenCaja,
            limpiarContrato,
            actualizarImportesPorTipo
        };
    }

    return {
        formatearMonedaMXN,
        mapearMetodoPago,
        esContratoCerrado,
        init
    };
});
