/**
 * mapaLotes.js — Componente reutilizable de Plano / Mapa visual de lotes por Sección, Andador y Fila.
 * Usado en /Catalogo (modo "catalogo" -> abre detalle) y en /Ventas/Nueva Paso 2 (modo "seleccion" -> elige lote).
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        root.mapaLotes = factory();
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    const SECCIONES_OFICIALES = [
        "Jardín Norte",
        "Jardín Central",
        "Robles",
        "Las Magnolias"
    ];

    function escapeHtml(text) {
        return String(text ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function formatearMoneda(monto) {
        const n = Number(monto || 0);
        return "$" + n.toLocaleString("es-MX", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function normalizarEstado(estado) {
        const norm = String(estado || "").trim().toLowerCase();
        if (norm === "disponible" || norm === "1") return "disponible";
        if (norm === "reservado" || norm === "2") return "reservado";
        if (norm === "vendido" || norm === "3") return "vendido";
        if (norm === "mantenimiento" || norm === "4") return "mantenimiento";
        return "vendido";
    }

    function etiquetaEstado(estadoNorm) {
        switch (estadoNorm) {
            case "disponible": return "Disponible";
            case "reservado": return "Reservado";
            case "vendido": return "Vendido";
            case "mantenimiento": return "Mantenimiento";
            default: return "No disponible";
        }
    }

    /**
     * Extrae Sección, Andador, Fila (1..N), Columna (1..10) y número corto a partir de un lote.
     * @param {{ clave?: string, seccion?: string, andadorFila?: string }} lote
     */
    function extraerCoordenadas(lote) {
        const clave = String(lote?.clave || "").trim().toUpperCase();
        const andadorFila = String(lote?.andadorFila || "").trim();
        const seccion = String(lote?.seccion || "").trim() || "Jardín Norte";

        let andador = "A";
        let consecutivo = 1;

        const matchClave = /^[A-Z]{2,3}-([A-Z])-(\d{3})$/.exec(clave);
        if (matchClave) {
            andador = matchClave[1];
            consecutivo = parseInt(matchClave[2], 10) || 1;
        } else {
            const matchAndador = /Andador\s+([A-Z])/i.exec(andadorFila);
            if (matchAndador) andador = matchAndador[1].toUpperCase();
            const matchNum = /(\d+)$/.exec(clave);
            if (matchNum) consecutivo = parseInt(matchNum[1], 10) || 1;
        }

        let fila = Math.floor(((Math.max(1, consecutivo) - 1) % 100) / 10) + 1;
        const matchFila = /Fila\s+(\d+)/i.exec(andadorFila);
        if (matchFila) {
            const parsedFila = parseInt(matchFila[1], 10);
            if (Number.isFinite(parsedFila) && parsedFila >= 1) {
                fila = parsedFila;
            }
        }

        const columna = ((Math.max(1, consecutivo) - 1) % 10) + 1;
        const numeroCorto = String(consecutivo).padStart(3, "0");

        return {
            seccion,
            andador,
            fila,
            columna,
            consecutivo,
            numeroCorto
        };
    }

    /**
     * Construye el índice por Sección -> Andador -> Fila/Columna para renderizar la cuadrícula.
     * @param {Array<Object>} lotes
     */
    function agruparLotes(lotes) {
        const items = Array.isArray(lotes) ? lotes : [];
        const porSeccion = new Map();

        SECCIONES_OFICIALES.forEach(function (sec) {
            porSeccion.set(sec, []);
        });

        items.forEach(function (lote) {
            const coords = extraerCoordenadas(lote);
            const claveSec = SECCIONES_OFICIALES.find(
                (s) => s.toLowerCase() === coords.seccion.toLowerCase()
            ) || coords.seccion;

            if (!porSeccion.has(claveSec)) {
                porSeccion.set(claveSec, []);
            }
            porSeccion.get(claveSec).push({
                lote,
                coords: { ...coords, seccion: claveSec },
                estadoNorm: normalizarEstado(lote.estado)
            });
        });

        return porSeccion;
    }

    /**
     * Crea un controlador interactivo de Mapa de Lotes montado en un elemento contenedor.
     * @param {HTMLElement} container
     * @param {{ modo?: "catalogo"|"seleccion", seccionInicial?: string, claveSeleccionada?: string, onSelectLote?: (lote: Object) => void, onSeccionChange?: (seccion: string) => void }} [opts]
     */
    function crearMapa(container, opts = {}) {
        if (!container) return null;

        const modo = opts.modo === "seleccion" ? "seleccion" : "catalogo";
        const state = {
            lotes: [],
            seccionActiva: opts.seccionInicial || SECCIONES_OFICIALES[0],
            andadorActivo: "A",
            claveSeleccionada: opts.claveSeleccionada || ""
        };

        function resolverAndadores(listaSeccion) {
            const set = new Set();
            listaSeccion.forEach(function (entry) {
                set.add(entry.coords.andador);
            });
            if (set.size === 0) set.add("A");
            return Array.from(set).sort();
        }

        function render() {
            const agrupados = agruparLotes(state.lotes);
            const secciones = Array.from(agrupados.keys());

            if (!agrupados.has(state.seccionActiva)) {
                state.seccionActiva = secciones[0] || SECCIONES_OFICIALES[0];
            }

            const listaSeccion = agrupados.get(state.seccionActiva) || [];
            const andadores = resolverAndadores(listaSeccion);
            if (!andadores.includes(state.andadorActivo)) {
                state.andadorActivo = andadores[0];
            }

            const listaAndador = listaSeccion.filter((e) => e.coords.andador === state.andadorActivo);
            let maxFila = 10;
            listaAndador.forEach(function (e) {
                if (e.coords.fila > maxFila) maxFila = e.coords.fila;
            });

            // Ubicar cada lote en (fila, col) evitando sobrescrituras si dos lotes heredados comparten columna
            const grid = new Map();
            listaAndador.forEach(function (entry) {
                const fila = entry.coords.fila;
                let col = entry.coords.columna;
                if (grid.has(`${fila}-${col}`)) {
                    for (let c = 1; c <= 10; c++) {
                        if (!grid.has(`${fila}-${c}`)) {
                            col = c;
                            break;
                        }
                    }
                }
                grid.set(`${fila}-${col}`, entry);
            });

            const tabsHtml = secciones.map(function (sec) {
                const count = (agrupados.get(sec) || []).length;
                const active = sec === state.seccionActiva;
                return `<button type="button" class="btn btn-sm rounded-pill px-3 sf-map-sec-btn ${active ? "active" : ""}" data-map-seccion="${escapeHtml(sec)}" aria-pressed="${active ? "true" : "false"}">` +
                    `${escapeHtml(sec)} <span class="badge rounded-pill ms-1 ${active ? "bg-white text-dark" : "bg-light text-muted border"}">${count}</span>` +
                    `</button>`;
            }).join("");

            const andadoresHtml = andadores.map(function (andador) {
                const countAnd = listaSeccion.filter((e) => e.coords.andador === andador).length;
                const active = andador === state.andadorActivo;
                return `<button type="button" class="btn btn-sm rounded-3 px-3 sf-map-andador-btn ${active ? "active" : ""}" data-map-andador="${escapeHtml(andador)}" aria-pressed="${active ? "true" : "false"}">` +
                    `Andador ${escapeHtml(andador)} <span class="small opacity-75">(${countAnd})</span>` +
                    `</button>`;
            }).join("");

            let headerCols = `<div class="sf-plot-axis-corner" aria-hidden="true">Fila</div>`;
            for (let c = 1; c <= 10; c++) {
                headerCols += `<div class="sf-plot-axis-col" aria-hidden="true">${c}</div>`;
            }

            let rowsHtml = "";
            for (let f = 1; f <= maxFila; f++) {
                rowsHtml += `<div class="sf-plot-axis-row" aria-hidden="true">F${f}</div>`;
                for (let c = 1; c <= 10; c++) {
                    const entry = grid.get(`${f}-${c}`);
                    if (!entry) {
                        rowsHtml += `<div class="sf-plot-cell sf-plot-empty" aria-hidden="true"></div>`;
                        continue;
                    }

                    const lote = entry.lote;
                    const est = entry.estadoNorm;
                    const estLabel = etiquetaEstado(est);
                    const claveEsc = escapeHtml(lote.clave || "");
                    const isSelected = Boolean(state.claveSeleccionada && state.claveSeleccionada.toUpperCase() === String(lote.clave || "").toUpperCase());
                    const disabledInSelect = modo === "seleccion" && est !== "disponible";
                    const tooltip = `${lote.clave} · ${lote.gavetas} gavetas · ${formatearMoneda(lote.precioContado)} · ${estLabel}`;

                    rowsHtml +=
                        `<button type="button" ` +
                        `class="sf-plot-cell sf-plot-${est}${isSelected ? " sf-plot-selected" : ""}" ` +
                        `data-map-clave="${claveEsc}" ` +
                        `title="${escapeHtml(tooltip)}" ` +
                        `aria-label="${escapeHtml(tooltip)}" ` +
                        `${modo === "seleccion" ? `aria-pressed="${isSelected ? "true" : "false"}" ` : ""}` +
                        `${disabledInSelect ? 'disabled aria-disabled="true"' : ""}>` +
                        `<span class="sf-plot-num">${escapeHtml(entry.coords.numeroCorto)}</span>` +
                        `<span class="sf-plot-gav">${escapeHtml(String(lote.gavetas || 2))}g</span>` +
                        `</button>`;
                }
            }

            const leyendaSeleccion = modo === "seleccion"
                ? `<span class="sf-map-legend-item"><span class="sf-map-swatch sf-plot-selected"></span>Seleccionado</span>`
                : "";

            container.innerHTML =
                `<div class="sf-map-toolbar">` +
                `  <div class="d-flex flex-wrap align-items-center gap-2" role="group" aria-label="Secciones del plano">${tabsHtml}</div>` +
                `  <div class="d-flex flex-wrap align-items-center gap-2 mt-2" role="group" aria-label="Andadores de la sección">${andadoresHtml}</div>` +
                `</div>` +
                `<div class="sf-map-legend">` +
                `  <span class="sf-map-legend-item"><span class="sf-map-swatch sf-plot-disponible"></span>Disponible</span>` +
                `  <span class="sf-map-legend-item"><span class="sf-map-swatch sf-plot-reservado"></span>Reservado</span>` +
                `  <span class="sf-map-legend-item"><span class="sf-map-swatch sf-plot-vendido"></span>Vendido</span>` +
                `  <span class="sf-map-legend-item"><span class="sf-map-swatch sf-plot-mantenimiento"></span>Mantenimiento</span>` +
                `  <span class="sf-map-legend-item"><span class="sf-map-swatch sf-plot-empty"></span>Sin asignar</span>` +
                `  ${leyendaSeleccion}` +
                `</div>` +
                `<div class="sf-plot-scroll">` +
                `  <div class="sf-plot-grid" role="grid" aria-label="Plano de lotes ${escapeHtml(state.seccionActiva)} Andador ${escapeHtml(state.andadorActivo)}">` +
                `    ${headerCols}` +
                `    ${rowsHtml}` +
                `  </div>` +
                `</div>` +
                `<div class="sf-map-footer small text-muted">` +
                `  <span><i class="fa-regular fa-compass me-1" aria-hidden="true"></i><strong>${escapeHtml(state.seccionActiva)}</strong> · Andador ${escapeHtml(state.andadorActivo)} (${listaAndador.length} ${listaAndador.length === 1 ? "lote" : "lotes"})</span>` +
                `  <span>${modo === "seleccion" ? "Haz clic en un lote disponible para seleccionarlo" : "Haz clic en cualquier lote para ver detalle"}</span>` +
                `</div>`;
        }

        container.addEventListener("click", function (e) {
            const target = e.target;
            if (!target || typeof target.closest !== "function") return;

            const btnSec = target.closest(".sf-map-sec-btn");
            if (btnSec) {
                const nuevaSec = btnSec.getAttribute("data-map-seccion");
                if (nuevaSec && nuevaSec !== state.seccionActiva) {
                    state.seccionActiva = nuevaSec;
                    state.andadorActivo = "A";
                    render();
                    if (typeof opts.onSeccionChange === "function") {
                        opts.onSeccionChange(nuevaSec);
                    }
                }
                return;
            }

            const btnAnd = target.closest(".sf-map-andador-btn");
            if (btnAnd) {
                const nuevoAnd = btnAnd.getAttribute("data-map-andador");
                if (nuevoAnd && nuevoAnd !== state.andadorActivo) {
                    state.andadorActivo = nuevoAnd;
                    render();
                }
                return;
            }

            const btnCell = target.closest("button.sf-plot-cell");
            if (btnCell && !btnCell.disabled) {
                const clave = btnCell.getAttribute("data-map-clave");
                const match = state.lotes.find(
                    (l) => String(l.clave || "").toUpperCase() === String(clave || "").toUpperCase()
                );
                if (match) {
                    if (modo === "seleccion") {
                        state.claveSeleccionada = match.clave || "";
                        render();
                    }
                    if (typeof opts.onSelectLote === "function") {
                        opts.onSelectLote(match);
                    }
                }
            }
        });

        return {
            setLotes(lotes, opciones = {}) {
                state.lotes = Array.isArray(lotes) ? lotes : [];
                if (opciones.seccion && opciones.seccion !== "todas") {
                    state.seccionActiva = opciones.seccion;
                } else if (opciones.claveSeleccionada) {
                    const sel = state.lotes.find(
                        (l) => String(l.clave || "").toUpperCase() === String(opciones.claveSeleccionada).toUpperCase()
                    );
                    if (sel) {
                        const c = extraerCoordenadas(sel);
                        state.seccionActiva = c.seccion;
                        state.andadorActivo = c.andador;
                    }
                }
                if (opciones.claveSeleccionada !== undefined) {
                    state.claveSeleccionada = opciones.claveSeleccionada || "";
                }
                render();
            },
            setSeleccion(clave) {
                state.claveSeleccionada = clave || "";
                if (clave) {
                    const sel = state.lotes.find(
                        (l) => String(l.clave || "").toUpperCase() === String(clave).toUpperCase()
                    );
                    if (sel) {
                        const c = extraerCoordenadas(sel);
                        state.seccionActiva = c.seccion;
                        state.andadorActivo = c.andador;
                    }
                }
                render();
            },
            setSeccion(seccion) {
                if (seccion && seccion !== "todas") {
                    state.seccionActiva = seccion;
                    state.andadorActivo = "A";
                    render();
                }
            },
            getState() {
                return { ...state };
            }
        };
    }

    return {
        SECCIONES_OFICIALES,
        extraerCoordenadas,
        agruparLotes,
        crearMapa
    };
});
