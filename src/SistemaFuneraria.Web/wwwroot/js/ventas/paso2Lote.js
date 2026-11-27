/**
 * paso2Lote.js - Lógica interactiva del Paso 2 del Wizard (SF-053).
 * Carga lotes disponibles reales desde lotesApi.js y sincroniza con wizardState.js y el panel derecho.
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.paso2Lote = api;
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

    function obtenerTipoLotePorGavetas(gavetas) {
        const g = Number(gavetas);
        if (g === 4) return "Lote Memorial";
        if (g === 3) return "Lote Familiar";
        return "Lote Básico";
    }

    function extraerSlotDeClave(clave) {
        if (!clave || typeof clave !== "string") return "";
        const match = /(\d+)$/.exec(clave.trim());
        if (!match) return "";
        const num = parseInt(match[1], 10);
        return Number.isFinite(num) ? String(num) : "";
    }

    function mapearLoteParaBorrador(lote) {
        if (!lote) return null;
        const gavetas = Number(lote.gavetas ?? lote.capacidad ?? 2);
        const precio = Number(lote.precioContado ?? lote.precio ?? 0);
        return {
            id: lote.id || "",
            clave: lote.clave || "",
            seccion: lote.seccion || "",
            andadorFila: lote.andadorFila || "",
            gavetas,
            capacidad: gavetas,
            precio,
            tipo: obtenerTipoLotePorGavetas(gavetas),
            slot: extraerSlotDeClave(lote.clave)
        };
    }

    function formatearMonedaSinDecimales(monto) {
        const n = Number(monto) || 0;
        return "$" + n.toLocaleString("es-MX", { minimumFractionDigits: 0, maximumFractionDigits: 0 });
    }

    function formatearMonedaMXN(monto) {
        const n = Number(monto) || 0;
        return "$" + n.toLocaleString("es-MX", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const lotesApi = deps.lotesApi ?? win?.lotesApi;
        const mapaLotes = deps.mapaLotes ?? win?.mapaLotes;
        const wizardState = deps.wizardState ?? win?.wizardState;

        if (!doc) return null;

        const step1Card = doc.getElementById("step1Card");
        const step2Card = doc.getElementById("step2Card");
        const contenedor = doc.getElementById("contenedorTarjetasLote");
        const contenedorMapa = doc.getElementById("contenedorMapaPaso2");
        const btnVistaLista = doc.getElementById("btnVistaListaPaso2");
        const btnVistaMapa = doc.getElementById("btnVistaMapaPaso2");
        const alertaError = doc.getElementById("paso2AlertaError");
        const mensajeError = doc.getElementById("paso2MensajeError");
        const btnReintentar = doc.getElementById("btnReintentarLotesPaso2");
        const btnAnterior = doc.getElementById("btnAnteriorPaso2");
        const btnContinuar2 = doc.getElementById("btnContinuarPaso2");

        const step1Indicator = doc.getElementById("stepIndicator1");
        const step2Indicator = doc.getElementById("stepIndicator2");

        const resumenTitulo = doc.getElementById("resumenLoteTitulo");
        const resumenClave = doc.getElementById("resumenLoteClave");
        const resumenCapacidad = doc.getElementById("resumenLoteCapacidad");
        const resumenPrecio = doc.getElementById("resumenLotePrecio");
        const resumenTotal = doc.getElementById("resumenTotalEstimado");
        const slotCells = doc.querySelectorAll(".sf-slot-cell");

        if (!step2Card || !contenedor) return null;

        let lotesDisponibles = [];
        let loteSeleccionado = null;
        let cargaIniciada = false;
        let mapaPaso2 = null;
        let vistaActual = "lista";

        function sincronizarBotonContinuar() {
            if (btnContinuar2) {
                btnContinuar2.disabled = !loteSeleccionado;
            }
        }

        function ocultarError() {
            if (alertaError) alertaError.classList.add("d-none");
        }

        function mostrarError(msg) {
            if (mensajeError) mensajeError.textContent = msg;
            if (alertaError) alertaError.classList.remove("d-none");
        }

        function sincronizarCuadriculaSlots(loteDraft) {
            const numericCells = Array.from(slotCells).filter(function (cell) {
                const s = cell.getAttribute("data-slot") || "";
                return !/^[A-Z]$/i.test(s);
            });

            if (numericCells.length > 0 && lotesDisponibles.length > 0) {
                const slotsReales = [];
                for (const l of lotesDisponibles) {
                    const sl = extraerSlotDeClave(l.clave);
                    if (sl && !slotsReales.includes(sl)) {
                        slotsReales.push(sl);
                    }
                    if (slotsReales.length >= numericCells.length) break;
                }
                if (loteDraft?.slot && !slotsReales.includes(loteDraft.slot) && slotsReales.length > 0) {
                    slotsReales[0] = loteDraft.slot;
                }
                numericCells.forEach(function (cell, idx) {
                    if (slotsReales[idx]) {
                        cell.setAttribute("data-slot", slotsReales[idx]);
                        cell.textContent = slotsReales[idx];
                    }
                });
            }

            slotCells.forEach(function (cell) {
                const cellSlot = cell.getAttribute("data-slot");
                if (cellSlot && loteDraft?.slot && cellSlot === loteDraft.slot) {
                    cell.style.backgroundColor = "#2D4224";
                    cell.style.color = "#FFFFFF";
                    cell.style.fontWeight = "bold";
                    cell.classList.add("shadow-sm");
                    cell.classList.remove("text-secondary", "bg-white", "border");
                } else {
                    cell.style.backgroundColor = "#FFFFFF";
                    cell.style.color = "#4B5563";
                    cell.style.fontWeight = "500";
                    cell.classList.remove("shadow-sm");
                    cell.classList.add("text-secondary", "bg-white", "border");
                }
            });
        }

        function sincronizarPanelDerecho(loteDraft) {
            if (!loteDraft) return;
            const precioStr = formatearMonedaMXN(loteDraft.precio);
            if (resumenTitulo) resumenTitulo.textContent = loteDraft.tipo;
            if (resumenClave) resumenClave.textContent = loteDraft.clave;
            if (resumenCapacidad) resumenCapacidad.textContent = `${loteDraft.gavetas} gavetas`;
            if (resumenPrecio) resumenPrecio.textContent = precioStr;
            if (resumenTotal) resumenTotal.textContent = precioStr;

            sincronizarCuadriculaSlots(loteDraft);
        }

        function marcarTarjetaActiva(clave) {
            const tarjetas = contenedor.querySelectorAll(".sf-lote-card");
            tarjetas.forEach(function (card) {
                const esActiva = card.getAttribute("data-lote-clave") === clave;
                if (esActiva) {
                    card.setAttribute("data-activa", "true");
                    card.style.border = "2px solid #2D4224";
                    card.style.backgroundColor = "#FAFBF9";
                } else {
                    card.removeAttribute("data-activa");
                    card.style.border = "1px solid #E5E7EB";
                    card.style.backgroundColor = "#FFFFFF";
                }
            });
        }

        function previsualizarLote(loteObj) {
            const loteDraft = mapearLoteParaBorrador(loteObj);
            if (!loteDraft) return;

            loteSeleccionado = loteDraft;
            marcarTarjetaActiva(loteDraft.clave);
            if (mapaPaso2 && vistaActual === "mapa") {
                mapaPaso2.setSeleccion(loteDraft.clave);
            }
            sincronizarPanelDerecho(loteDraft);
            sincronizarBotonContinuar();
            return loteDraft;
        }

        function seleccionarLote(loteObj, pasoActual = 2) {
            const loteDraft = previsualizarLote(loteObj);
            if (!loteDraft) return;

            if (wizardState) {
                wizardState.updateDraft({ lote: loteDraft, paso: pasoActual });
            }
        }

        function renderizarTarjetas(items) {
            contenedor.innerHTML = "";

            if (!Array.isArray(items) || items.length === 0) {
                const vacio = doc.createElement("div");
                vacio.className = "p-4 rounded-3 border bg-light text-center text-muted small";
                vacio.textContent = "No hay lotes disponibles en este momento.";
                contenedor.appendChild(vacio);
                loteSeleccionado = null;
                sincronizarBotonContinuar();
                return;
            }

            for (const item of items) {
                const mapped = mapearLoteParaBorrador(item);
                const card = doc.createElement("div");
                card.className = "sf-lote-card p-3 rounded-3 d-flex align-items-center justify-content-between transition-all";
                card.setAttribute("role", "button");
                card.setAttribute("tabindex", "0");
                card.setAttribute("data-lote-id", mapped.id);
                card.setAttribute("data-lote-clave", mapped.clave);
                card.setAttribute("data-lote-tipo", mapped.tipo);
                card.setAttribute("data-lote-capacidad", String(mapped.gavetas));
                card.setAttribute("data-lote-precio", String(mapped.precio));
                card.setAttribute("data-slot", mapped.slot);
                card.style.cursor = "pointer";
                card.style.border = "1px solid #E5E7EB";
                card.style.backgroundColor = "#FFFFFF";
                card.style.transition = "all 0.2s ease";

                const spanClave = doc.createElement("span");
                spanClave.className = "fw-bold text-dark me-4 sf-monospace";
                spanClave.style.minWidth = "90px";
                spanClave.style.fontSize = "1.05rem";
                spanClave.textContent = mapped.clave;

                const divInfo = doc.createElement("div");
                divInfo.className = "flex-grow-1";
                const divSeccion = doc.createElement("div");
                divSeccion.className = "fw-bold text-dark";
                divSeccion.textContent = mapped.seccion;
                const smallMeta = doc.createElement("small");
                smallMeta.className = "text-muted";
                smallMeta.textContent = `${mapped.gavetas} gavetas · ${mapped.andadorFila}`;
                divInfo.append(divSeccion, smallMeta);

                const spanPrecio = doc.createElement("span");
                spanPrecio.className = "h5 fw-bold text-dark mb-0 sf-money";
                spanPrecio.textContent = formatearMonedaSinDecimales(mapped.precio);

                card.append(spanClave, divInfo, spanPrecio);

                card.addEventListener("click", function () {
                    seleccionarLote(item, 2);
                });
                card.addEventListener("keydown", function (e) {
                    if (e.key === "Enter" || e.key === " ") {
                        e.preventDefault();
                        seleccionarLote(item, 2);
                    }
                });

                contenedor.appendChild(card);
            }
        }

        function obtenerLoteClaveQuery() {
            if (!win?.location?.search) return "";
            const params = new URLSearchParams(win.location.search);
            return (params.get("lote") || params.get("loteClave") || "").trim().toUpperCase();
        }

        async function cargarLotesDisponibles() {
            if (!lotesApi) return;
            cargaIniciada = true;
            ocultarError();

            try {
                const resumen = await lotesApi.obtenerCatalogo({ estado: "Disponible", pageSize: 50 });
                const items = Array.isArray(resumen?.items) ? resumen.items : [];
                lotesDisponibles = items;
                renderizarTarjetas(items);

                const claveQuery = obtenerLoteClaveQuery();
                const draft = wizardState ? wizardState.getDraft() : {};
                const claveDraft = draft?.lote?.clave || "";

                let elegido = null;
                if (claveQuery) {
                    elegido = items.find((l) => (l.clave || "").toUpperCase() === claveQuery);
                }
                if (!elegido && claveDraft) {
                    elegido = items.find((l) => l.clave === claveDraft);
                }
                if (!elegido && items.length > 0) {
                    elegido = items[0];
                }

                if (elegido) {
                    if (claveQuery || claveDraft) {
                        seleccionarLote(elegido, draft?.paso || 2);
                    } else {
                        previsualizarLote(elegido);
                    }
                } else {
                    loteSeleccionado = null;
                    sincronizarBotonContinuar();
                }
            } catch (err) {
                loteSeleccionado = null;
                sincronizarBotonContinuar();
                contenedor.innerHTML = "";
                mostrarError(err?.message || "No fue posible cargar el inventario de lotes disponibles.");
            }
        }

        async function cargarMapaPaso2() {
            if (!contenedorMapa || !mapaLotes || typeof mapaLotes.crearMapa !== "function") return;
            if (!mapaPaso2) {
                mapaPaso2 = mapaLotes.crearMapa(contenedorMapa, {
                    modo: "seleccion",
                    claveSeleccionada: loteSeleccionado?.clave || "",
                    onSelectLote: function (lote) {
                        seleccionarLote(lote, 2);
                    }
                });
            }
            try {
                const resTodos = await lotesApi.obtenerCatalogo({ page: 1, pageSize: 100 });
                mapaPaso2.setLotes(resTodos?.items || lotesDisponibles, {
                    claveSeleccionada: loteSeleccionado?.clave || ""
                });
            } catch {
                mapaPaso2.setLotes(lotesDisponibles, {
                    claveSeleccionada: loteSeleccionado?.clave || ""
                });
            }
        }

        function cambiarVistaPaso2(nuevaVista) {
            vistaActual = nuevaVista === "mapa" ? "mapa" : "lista";
            const esMapa = vistaActual === "mapa";
            if (contenedor) {
                contenedor.classList.toggle("d-none", esMapa);
                contenedor.classList.toggle("d-flex", !esMapa);
            }
            if (contenedorMapa) contenedorMapa.classList.toggle("d-none", !esMapa);
            if (btnVistaLista) {
                btnVistaLista.classList.toggle("active", !esMapa);
                btnVistaLista.setAttribute("aria-pressed", !esMapa ? "true" : "false");
            }
            if (btnVistaMapa) {
                btnVistaMapa.classList.toggle("active", esMapa);
                btnVistaMapa.setAttribute("aria-pressed", esMapa ? "true" : "false");
            }
            if (esMapa) {
                cargarMapaPaso2();
            }
        }

        if (btnVistaLista) {
            btnVistaLista.addEventListener("click", function () {
                cambiarVistaPaso2("lista");
            });
        }
        if (btnVistaMapa) {
            btnVistaMapa.addEventListener("click", function () {
                cambiarVistaPaso2("mapa");
            });
        }

        // Permitir selección desde la cuadrícula derecha 2x4 para los lotes numéricos cargados
        slotCells.forEach(function (cell) {
            cell.addEventListener("click", function () {
                const slot = cell.getAttribute("data-slot");
                if (!slot || !/^\d+$/.test(slot)) return;
                const match = lotesDisponibles.find((l) => extraerSlotDeClave(l.clave) === slot);
                if (match) {
                    seleccionarLote(match, 2);
                }
            });
        });

        if (win) {
            win.addEventListener("sf:paso1-completado", function () {
                mostrarPaso2();
                if (!cargaIniciada || lotesDisponibles.length === 0) {
                    cargarLotesDisponibles();
                }
            });
        }

        if (btnReintentar) {
            btnReintentar.addEventListener("click", cargarLotesDisponibles);
        }

        if (btnAnterior) {
            btnAnterior.addEventListener("click", mostrarPaso1);
        }

        if (btnContinuar2) {
            btnContinuar2.addEventListener("click", function () {
                if (!loteSeleccionado) return;
                if (wizardState) {
                    wizardState.updateDraft({ lote: loteSeleccionado, paso: 3 });
                }
                if (win) {
                    win.dispatchEvent(new CustomEvent("sf:paso2-completado", {
                        detail: { lote: loteSeleccionado }
                    }));
                }
            });
        }

        function mostrarPaso1() {
            if (step1Card) step1Card.classList.remove("d-none");
            if (step2Card) step2Card.classList.add("d-none");

            if (step1Indicator && step2Indicator) {
                const circle1 = step1Indicator.querySelector("div");
                const circle2 = step2Indicator.querySelector("div");
                if (circle1) {
                    circle1.style.backgroundColor = "#2D4224";
                    circle1.style.borderColor = "#2D4224";
                    circle1.classList.remove("text-muted", "bg-white", "border", "text-success");
                    circle1.classList.add("text-white", "fw-bold");
                    circle1.textContent = "1";
                }
                if (circle2) {
                    circle2.style.backgroundColor = "#FFFFFF";
                    circle2.style.borderColor = "#D1D5DB";
                    circle2.classList.remove("text-white", "fw-bold");
                    circle2.classList.add("text-muted", "fw-semibold", "bg-white", "border");
                }
            }
        }

        function mostrarPaso2() {
            if (step1Card) step1Card.classList.add("d-none");
            if (step2Card) step2Card.classList.remove("d-none");

            if (step1Indicator && step2Indicator) {
                const circle1 = step1Indicator.querySelector("div");
                const circle2 = step2Indicator.querySelector("div");
                if (circle1) {
                    circle1.style.backgroundColor = "#FFFFFF";
                    circle1.style.borderColor = "#2D4224";
                    circle1.classList.remove("text-white");
                    circle1.classList.add("text-success", "border", "border-2");
                    circle1.innerHTML = '<i class="fa-solid fa-check" aria-hidden="true"></i>';
                }
                if (circle2) {
                    circle2.style.backgroundColor = "#2D4224";
                    circle2.style.borderColor = "#2D4224";
                    circle2.classList.remove("text-muted", "bg-white", "border");
                    circle2.classList.add("text-white", "fw-bold", "shadow-sm");
                }
            }
        }

        // Precargar lotes si el usuario ya está autenticado (soporta sessionStorage y localStorage "Recordarme")
        const token = deps.storage?.getItem?.("sf_token")
            ?? (typeof win?.authApi?.getToken === "function" ? win.authApi.getToken() : null)
            ?? (typeof sessionStorage !== "undefined" ? sessionStorage.getItem("sf_token") : null)
            ?? (typeof localStorage !== "undefined" ? localStorage.getItem("sf_token") : null);
        if (token && lotesApi) {
            cargarLotesDisponibles();
        }

        return {
            recargar: cargarLotesDisponibles
        };
    }

    return {
        obtenerTipoLotePorGavetas,
        extraerSlotDeClave,
        mapearLoteParaBorrador,
        init
    };
});
