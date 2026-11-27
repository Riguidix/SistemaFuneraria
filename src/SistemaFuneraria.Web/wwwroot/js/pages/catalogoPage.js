/**
 * catalogoPage.js — Controlador real de /Catalogo conectado a /api/v1/lotes (paginación, filtros, detalle y CRUD Admin).
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.catalogoPage = api;
        if (typeof document !== "undefined") {
            document.addEventListener("DOMContentLoaded", function () {
                api.init();
            });
        }
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    const PRECIOS_RN01 = Object.freeze({
        2: 120000,
        3: 150000,
        4: 180000
    });

    const PAGE_SIZE = 10;

    function escapeHtml(text) {
        return String(text ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function obtenerPrecioPorGavetas(gavetas) {
        return PRECIOS_RN01[Number(gavetas)] ?? 120000;
    }

    function construirUrlContrato(lote) {
        if (lote?.folioContratoActivo) {
            return `/Contratos/Detalle?folio=${encodeURIComponent(lote.folioContratoActivo)}`;
        }
        return `/Contratos/Detalle?loteClave=${encodeURIComponent(lote?.clave || "")}`;
    }

    function init(deps = {}) {
        const doc = deps.document ?? (typeof document !== "undefined" ? document : null);
        const win = deps.window ?? (typeof window !== "undefined" ? window : null);
        const authApi = deps.authApi ?? win?.authApi;
        const apiClient = deps.apiClient ?? win?.apiClient;
        const lotesApi = deps.lotesApi ?? win?.lotesApi;
        const mapaLotes = deps.mapaLotes ?? win?.mapaLotes;
        const sfUi = deps.SfUi ?? win?.SfUi;

        if (!doc || !lotesApi) return null;

        let previewReqId = 0;

        const state = {
            page: 1,
            totalPages: 1,
            totalFiltrados: 0,
            items: [],
            loteSeleccionado: null,
            orden: { columna: null, ascendente: true },
            esAdmin: false,
            vista: "tabla"
        };

        let debounceTimer = null;

        const inputBuscar = doc.getElementById("inputBuscarLotes");
        const btnClear = doc.getElementById("btnClearSearchLotes");
        const selectSeccion = doc.getElementById("selectSeccion");
        const selectEstado = doc.getElementById("selectEstado");
        const btnReset = doc.getElementById("btnResetLotes");
        const tbody = doc.getElementById("tbodyLotes");
        const filaCargando = doc.getElementById("filaCargandoLotes");
        const filaVacia = doc.getElementById("filaSinResultadosLotes");
        const paginacionEl = doc.getElementById("paginacionLotes");
        const lotesDesde = doc.getElementById("lotesDesde");
        const lotesHasta = doc.getElementById("lotesHasta");
        const lotesTotal = doc.getElementById("lotesTotal");
        const sortHeaders = doc.querySelectorAll(".th-sortable");

        function formatearMoneda(monto) {
            return lotesApi.formatearMonedaMXN(Number(monto || 0));
        }

        function ocultarError() {
            const banner = doc.getElementById("catalogoErrorBanner");
            if (banner) {
                banner.classList.add("d-none");
                banner.classList.remove("d-flex");
            }
        }

        function mostrarError(mensaje) {
            const banner = doc.getElementById("catalogoErrorBanner");
            const textEl = doc.getElementById("catalogoErrorText");
            if (textEl) {
                textEl.textContent = mensaje || "No se pudo cargar el catálogo de lotes.";
            }
            if (banner) {
                banner.classList.remove("d-none");
                banner.classList.add("d-flex");
            }
        }

        function actualizarKpis(data) {
            const dispEl = doc.getElementById("kpiDisponiblesValor");
            const totalTextoEl = doc.getElementById("kpiTotalLotesTexto");
            const precioEl = doc.getElementById("kpiPrecioDesde");
            const seccionEl = doc.getElementById("kpiSeccionMasEspacio");
            const seccionLibresEl = doc.getElementById("kpiSeccionLibresTexto");

            if (dispEl) dispEl.textContent = String(data.totalDisponibles ?? 0);
            if (totalTextoEl) totalTextoEl.textContent = `de ${data.totalLotes ?? 0} lotes`;
            if (precioEl) {
                const precio = Number(data.precioDesde ?? 0);
                precioEl.textContent = precio > 0
                    ? "$" + Math.round(precio).toLocaleString("es-MX")
                    : "$0";
            }
            if (seccionEl) seccionEl.textContent = data.seccionMasEspacio || "N/A";
            if (seccionLibresEl) {
                const libres = Number(data.lotesLibresSeccionMasEspacio ?? 0);
                seccionLibresEl.textContent = `${libres} ${libres === 1 ? "lote libre" : "lotes libres"}`;
            }
        }

        function renderAccionesFila(lote) {
            const estadoNorm = String(lote.estado || "").trim().toLowerCase();
            const claveEnc = encodeURIComponent(lote.clave || "");
            const claveEsc = escapeHtml(lote.clave || "");
            const btnDetalleSecundario = `<button type="button" class="btn btn-link btn-sm text-secondary p-0 ms-3 text-decoration-none btn-ver-detalle-lote" data-clave="${claveEsc}" title="Ver detalle de lote ${claveEsc}">Ver detalle</button>`;

            if (estadoNorm === "disponible") {
                return `<a href="/Ventas/Nueva?lote=${claveEnc}" class="funeraria-action-link" title="Cotizar lote ${claveEsc}"><span>Cotizar</span></a>${btnDetalleSecundario}`;
            }

            if (estadoNorm === "vendido") {
                const urlContrato = construirUrlContrato(lote);
                return `<a href="${urlContrato}" class="funeraria-action-link" title="Ver contrato de lote ${claveEsc}"><span>Ver contrato</span></a>${btnDetalleSecundario}`;
            }

            return `<button type="button" class="btn btn-link btn-sm funeraria-action-link p-0 text-decoration-none btn-ver-detalle-lote" data-clave="${claveEsc}" title="Ver detalle de lote ${claveEsc}"><span>Ver detalle</span></button>`;
        }

        function renderFilas(items) {
            if (!tbody) return;

            tbody.querySelectorAll("tr.fila-lote-dinamica").forEach(function (tr) {
                tr.remove();
            });

            if (filaCargando) filaCargando.classList.add("d-none");

            if (!items || items.length === 0) {
                if (filaVacia) filaVacia.classList.remove("d-none");
                return;
            }

            if (filaVacia) filaVacia.classList.add("d-none");

            const fragment = doc.createDocumentFragment();
            items.forEach(function (lote) {
                const tr = doc.createElement("tr");
                tr.className = "fila-lote-dinamica";
                tr.setAttribute("data-id", lote.id || "");
                tr.setAttribute("data-clave", lote.clave || "");
                tr.setAttribute("data-ubicacion", lote.seccion || "");
                tr.setAttribute("data-andador-fila", lote.andadorFila || "");
                tr.setAttribute("data-capacidad", String(lote.gavetas ?? 0));
                tr.setAttribute("data-precio", String(lote.precioContado ?? 0));
                tr.setAttribute("data-estado", lote.estado || "");

                const badgeHtml = lotesApi.renderizarBadgeEstadoLote(lote.estado);
                tr.innerHTML =
                    `<td class="ps-4 fw-medium text-dark">${escapeHtml(lote.clave)}</td>` +
                    `<td><div class="fw-bold text-dark">${escapeHtml(lote.seccion)}</div><div class="text-muted small">${escapeHtml(lote.andadorFila)}</div></td>` +
                    `<td>${escapeHtml(String(lote.gavetas))} gavetas</td>` +
                    `<td class="fw-bold formato-moneda text-tabular sf-money">${escapeHtml(formatearMoneda(lote.precioContado))}</td>` +
                    `<td>${badgeHtml}</td>` +
                    `<td class="text-end pe-4 text-nowrap">${renderAccionesFila(lote)}</td>`;

                fragment.appendChild(tr);
            });

            tbody.insertBefore(fragment, filaVacia || null);
        }

        function renderPaginacion() {
            const count = state.items.length;
            const total = state.totalFiltrados;
            const desde = count > 0 ? ((state.page - 1) * PAGE_SIZE) + 1 : 0;
            const hasta = count > 0 ? desde + count - 1 : 0;

            if (lotesDesde) lotesDesde.textContent = String(desde);
            if (lotesHasta) lotesHasta.textContent = String(hasta);
            if (lotesTotal) lotesTotal.textContent = String(total);

            if (!paginacionEl) return;

            const prevDisabled = state.page <= 1;
            const nextDisabled = state.page >= state.totalPages;
            const parts = [];

            parts.push(
                `<li><button type="button" class="page-btn ${prevDisabled ? "disabled" : ""}" data-page="${state.page - 1}" ${prevDisabled ? "disabled" : ""} aria-label="Página anterior">Anterior</button></li>`
            );

            for (let p = 1; p <= state.totalPages; p++) {
                const isActive = p === state.page;
                parts.push(
                    `<li><button type="button" class="page-btn ${isActive ? "active" : ""}" data-page="${p}" ${isActive ? 'aria-current="page"' : ""}>${p}</button></li>`
                );
            }

            parts.push(
                `<li><button type="button" class="page-btn ${nextDisabled ? "disabled" : ""}" data-page="${state.page + 1}" ${nextDisabled ? "disabled" : ""} aria-label="Página siguiente">Siguiente</button></li>`
            );

            paginacionEl.innerHTML = parts.join("");
        }

        function aplicarOrdenEnPagina() {
            if (!state.orden.columna || state.items.length === 0) return;

            const col = state.orden.columna;
            const asc = state.orden.ascendente;
            const copia = [...state.items].sort(function (a, b) {
                if (col === "precio") {
                    return asc ? (a.precioContado - b.precioContado) : (b.precioContado - a.precioContado);
                }
                if (col === "capacidad") {
                    return asc ? (a.gavetas - b.gavetas) : (b.gavetas - a.gavetas);
                }
                const valA = col === "ubicacion" ? `${a.seccion} ${a.andadorFila}` : String(a.clave || "");
                const valB = col === "ubicacion" ? `${b.seccion} ${b.andadorFila}` : String(b.clave || "");
                return asc
                    ? valA.localeCompare(valB, "es", { numeric: true })
                    : valB.localeCompare(valA, "es", { numeric: true });
            });

            renderFilas(copia);
        }

        async function cargarCatalogo(pagina = 1) {
            if (authApi && typeof authApi.requireAuth === "function") {
                const session = authApi.requireAuth({ client: apiClient, storage: deps.storage });
                if (!session) return;
                state.esAdmin = session.rol === authApi.ROL_ADMINISTRADOR;
            }

            state.page = Math.max(1, pagina);
            ocultarError();

            if (filaCargando && state.items.length === 0) {
                filaCargando.classList.remove("d-none");
            }

            const buscar = inputBuscar ? inputBuscar.value.trim() : "";
            const seccion = selectSeccion ? selectSeccion.value : "todas";
            const estado = selectEstado ? selectEstado.value : "todos";

            if (btnClear) {
                btnClear.style.display = buscar.length > 0 ? "block" : "none";
            }

            try {
                const data = await lotesApi.obtenerCatalogo({
                    buscar,
                    seccion,
                    estado,
                    page: state.page,
                    pageSize: PAGE_SIZE
                });

                state.items = Array.isArray(data.items) ? data.items : [];
                state.page = Number(data.paginaActual || 1);
                state.totalPages = Math.max(1, Number(data.totalPaginas || 1));
                state.totalFiltrados = Number(data.totalFiltrados ?? state.items.length);

                actualizarKpis(data);
                if (state.orden.columna) {
                    aplicarOrdenEnPagina();
                } else {
                    renderFilas(state.items);
                }
                renderPaginacion();

                if (state.vista === "mapa") {
                    await cargarMapaCatalogo();
                }
            } catch (err) {
                if (filaCargando) filaCargando.classList.add("d-none");
                mostrarError(err?.message || "No se pudo cargar el catálogo de lotes.");
            }
        }

        const btnVistaTabla = doc.getElementById("btnVistaTablaCatalogo");
        const btnVistaMapa = doc.getElementById("btnVistaMapaCatalogo");
        const vistaTablaEl = doc.getElementById("catalogoVistaTabla");
        const vistaMapaEl = doc.getElementById("catalogoVistaMapa");
        const contenedorMapa = doc.getElementById("contenedorMapaCatalogo");
        let mapaCatalogo = null;

        async function cargarMapaCatalogo() {
            if (!contenedorMapa || !mapaLotes || typeof mapaLotes.crearMapa !== "function") return;
            if (!mapaCatalogo) {
                mapaCatalogo = mapaLotes.crearMapa(contenedorMapa, {
                    modo: "catalogo",
                    onSelectLote: function (lote) {
                        if (lote?.clave) abrirModalDetalleLote(lote.clave);
                    }
                });
            }
            const buscar = inputBuscar ? inputBuscar.value.trim() : "";
            const estado = selectEstado ? selectEstado.value : "todos";
            const seccion = selectSeccion ? selectSeccion.value : "todas";
            try {
                const resMapa = await lotesApi.obtenerCatalogo({
                    buscar,
                    estado,
                    page: 1,
                    pageSize: 100
                });
                mapaCatalogo.setLotes(resMapa?.items || [], { seccion });
            } catch {
                mapaCatalogo.setLotes(state.items, { seccion });
            }
        }

        function cambiarVistaCatalogo(nuevaVista) {
            state.vista = nuevaVista === "mapa" ? "mapa" : "tabla";
            const esMapa = state.vista === "mapa";
            if (vistaTablaEl) vistaTablaEl.classList.toggle("d-none", esMapa);
            if (vistaMapaEl) vistaMapaEl.classList.toggle("d-none", !esMapa);
            if (btnVistaTabla) {
                btnVistaTabla.classList.toggle("active", !esMapa);
                btnVistaTabla.setAttribute("aria-pressed", !esMapa ? "true" : "false");
            }
            if (btnVistaMapa) {
                btnVistaMapa.classList.toggle("active", esMapa);
                btnVistaMapa.setAttribute("aria-pressed", esMapa ? "true" : "false");
            }
            if (esMapa) {
                cargarMapaCatalogo();
            }
        }

        if (btnVistaTabla) {
            btnVistaTabla.addEventListener("click", function () {
                cambiarVistaCatalogo("tabla");
            });
        }
        if (btnVistaMapa) {
            btnVistaMapa.addEventListener("click", function () {
                cambiarVistaCatalogo("mapa");
            });
        }

        function renderTrazabilidadLote(lote) {
            const lista = doc.getElementById("historialEstadoLote");
            if (!lista) return;

            const estadoNorm = String(lote.estado || "").trim().toLowerCase();
            const items = [];

            if (estadoNorm === "vendido") {
                const folioTexto = lote.folioContratoActivo
                    ? `Vinculado al contrato ${escapeHtml(lote.folioContratoActivo)}`
                    : "Unidad comercializada y asignada a titular";
                items.push({
                    titulo: "Lote Vendido",
                    detalle: folioTexto,
                    etiqueta: "Estado vigente"
                });
            } else if (estadoNorm === "reservado") {
                items.push({
                    titulo: "Lote Reservado",
                    detalle: "Apartado temporalmente en espera de formalización",
                    etiqueta: "Estado vigente"
                });
            } else if (estadoNorm === "mantenimiento") {
                items.push({
                    titulo: "En Mantenimiento",
                    detalle: "Unidad temporalmente fuera de comercialización",
                    etiqueta: "Estado vigente"
                });
            } else {
                items.push({
                    titulo: "Disponible para comercialización",
                    detalle: "Unidad libre para venta de contado o financiamiento",
                    etiqueta: "Estado vigente"
                });
            }

            items.push({
                titulo: "Registro en Catálogo Físico",
                detalle: `${escapeHtml(lote.seccion)} · ${escapeHtml(lote.andadorFila)} (${escapeHtml(String(lote.gavetas))} gavetas)`,
                etiqueta: "Inventario"
            });

            lista.innerHTML = items.map(function (it, idx) {
                const border = idx < items.length - 1 ? "border-bottom" : "";
                return `<li class="py-2 ${border} d-flex justify-content-between align-items-center gap-2">` +
                    `<div><span class="fw-semibold text-dark">${escapeHtml(it.titulo)}</span>` +
                    `<span class="text-muted d-block small">${it.detalle}</span></div>` +
                    `<span class="text-muted text-nowrap">${escapeHtml(it.etiqueta)}</span>` +
                    `</li>`;
            }).join("");
        }

        function poblarModalDetalle(lote) {
            state.loteSeleccionado = lote;
            const estadoNorm = String(lote.estado || "").trim().toLowerCase();

            const setTxt = function (id, val) {
                const el = doc.getElementById(id);
                if (el) el.textContent = val;
            };

            setTxt("detalleLoteClave", lote.clave || "--");
            setTxt("detalleLoteUbicacion", `${lote.seccion || ""} · ${lote.andadorFila || ""}`);
            setTxt("detalleLoteCapacidad", `${lote.gavetas ?? 0} gavetas`);
            setTxt("detalleLotePrecio", formatearMoneda(lote.precioContado));

            const estadoEl = doc.getElementById("detalleLoteEstado");
            if (estadoEl) {
                estadoEl.innerHTML = lotesApi.renderizarBadgeEstadoLote(lote.estado);
            }

            renderTrazabilidadLote(lote);

            const alertaEstado = doc.getElementById("alertaCambioEstadoLote");
            if (alertaEstado) {
                alertaEstado.classList.add("d-none");
                alertaEstado.textContent = "";
            }

            const selectEstadoModal = doc.getElementById("selectCambioEstadoLote");
            if (selectEstadoModal && lote.estado) {
                selectEstadoModal.value = lote.estado;
            }

            const btnCotizar = doc.getElementById("btnCotizarDesdeDetalle");
            if (btnCotizar) {
                if (estadoNorm === "disponible") {
                    btnCotizar.classList.remove("d-none");
                    btnCotizar.href = `/Ventas/Nueva?lote=${encodeURIComponent(lote.clave || "")}`;
                } else {
                    btnCotizar.classList.add("d-none");
                }
            }

            const btnVerContrato = doc.getElementById("btnVerContratoDesdeDetalle");
            if (btnVerContrato) {
                if (estadoNorm === "vendido" || lote.folioContratoActivo) {
                    btnVerContrato.classList.remove("d-none");
                    btnVerContrato.href = construirUrlContrato(lote);
                } else {
                    btnVerContrato.classList.add("d-none");
                }
            }

            const btnEditar = doc.getElementById("btnEditarDesdeDetalle");
            if (btnEditar) {
                if (state.esAdmin && estadoNorm !== "vendido") {
                    btnEditar.classList.remove("d-none");
                } else {
                    btnEditar.classList.add("d-none");
                }
            }
        }

        async function abrirModalDetalleLote(clave) {
            const elModal = doc.getElementById("modalDetalleLote");
            if (!elModal) return;

            const cached = state.items.find(function (l) {
                return String(l.clave || "").toLowerCase() === String(clave || "").toLowerCase();
            });
            if (cached) {
                poblarModalDetalle(cached);
            }

            if (typeof bootstrap !== "undefined" && bootstrap.Modal) {
                bootstrap.Modal.getOrCreateInstance(elModal).show();
            }

            try {
                const detalle = await lotesApi.obtenerLotePorClave(clave);
                if (detalle) {
                    poblarModalDetalle(detalle);
                }
            } catch {
                // Si ya se mostró el dato en caché, se mantiene visible
            }
        }

        async function cargarVistaPreviaUbicacion(seccion) {
            const claveInput = doc.getElementById("loteFormClave");
            const andadorInput = doc.getElementById("loteFormAndador");
            const alertaForm = doc.getElementById("alertaFormLote");
            const reqId = ++previewReqId;

            if (claveInput) claveInput.value = "Calculando...";
            if (andadorInput) andadorInput.value = "Calculando...";

            if (typeof lotesApi.obtenerSiguienteUbicacion !== "function") {
                if (claveInput) claveInput.value = "";
                if (andadorInput) andadorInput.value = "";
                return;
            }

            try {
                const preview = await lotesApi.obtenerSiguienteUbicacion(seccion);
                if (reqId !== previewReqId) return;
                if (claveInput) claveInput.value = preview?.clave || "";
                if (andadorInput) andadorInput.value = preview?.andadorFila || "";
            } catch (err) {
                if (reqId !== previewReqId) return;
                if (claveInput) claveInput.value = "";
                if (andadorInput) andadorInput.value = "";
                mostrarAlertaModal(alertaForm, "warning", err?.message || "No se pudo calcular la vista previa de ubicación.");
            }
        }

        function abrirModalFormulario(loteEditar = null) {
            const elModal = doc.getElementById("modalFormularioLote");
            if (!elModal) return;

            const titleEl = doc.getElementById("modalFormularioLoteLabel");
            const idInput = doc.getElementById("loteFormId");
            const claveInput = doc.getElementById("loteFormClave");
            const seccionInput = doc.getElementById("loteFormSeccion");
            const andadorInput = doc.getElementById("loteFormAndador");
            const hintUbicacion = doc.getElementById("loteFormUbicacionHint");
            const gavetasInput = doc.getElementById("loteFormGavetas");
            const precioInput = doc.getElementById("loteFormPrecio");
            const grupoEstado = doc.getElementById("grupoLoteFormEstado");
            const estadoInput = doc.getElementById("loteFormEstado");
            const alertaForm = doc.getElementById("alertaFormLote");

            if (alertaForm) {
                alertaForm.classList.add("d-none");
                alertaForm.textContent = "";
            }

            if (loteEditar) {
                previewReqId++;
                if (titleEl) titleEl.textContent = `Editar lote ${loteEditar.clave}`;
                if (idInput) idInput.value = loteEditar.id || "";
                if (claveInput) {
                    claveInput.value = loteEditar.clave || "";
                    claveInput.readOnly = true;
                }
                if (seccionInput) {
                    seccionInput.value = loteEditar.seccion || "Jardín Norte";
                    seccionInput.disabled = true;
                }
                if (andadorInput) {
                    andadorInput.value = loteEditar.andadorFila || "";
                    andadorInput.readOnly = true;
                }
                if (hintUbicacion) {
                    hintUbicacion.textContent = "La ubicación física de un lote existente es inmutable.";
                }
                if (gavetasInput) gavetasInput.value = String(loteEditar.gavetas || 2);
                if (precioInput) precioInput.value = formatearMoneda(obtenerPrecioPorGavetas(loteEditar.gavetas || 2));
                if (grupoEstado) grupoEstado.classList.remove("d-none");
                if (estadoInput) estadoInput.value = loteEditar.estado || "Disponible";
            } else {
                if (titleEl) titleEl.textContent = "Nuevo lote";
                if (idInput) idInput.value = "";
                if (claveInput) claveInput.readOnly = true;
                if (andadorInput) andadorInput.readOnly = true;
                if (seccionInput) {
                    seccionInput.disabled = false;
                    seccionInput.value = "Jardín Norte";
                }
                if (hintUbicacion) {
                    hintUbicacion.textContent = "Ubicación secuencial asignada automáticamente al guardar.";
                }
                if (gavetasInput) gavetasInput.value = "2";
                if (precioInput) precioInput.value = formatearMoneda(obtenerPrecioPorGavetas(2));
                if (grupoEstado) grupoEstado.classList.add("d-none");
                if (estadoInput) estadoInput.value = "Disponible";
                cargarVistaPreviaUbicacion(seccionInput ? seccionInput.value : "Jardín Norte");
            }

            if (typeof bootstrap !== "undefined" && bootstrap.Modal) {
                bootstrap.Modal.getOrCreateInstance(elModal).show();
            }
        }

        function mostrarAlertaModal(el, tipo, mensaje) {
            if (!el) return;
            el.className = `alert alert-${tipo} mt-2 mb-0 py-2 px-3 small rounded-3`;
            el.textContent = mensaje;
        }

        // Eventos de filtros y búsqueda
        if (inputBuscar) {
            inputBuscar.addEventListener("input", function () {
                if (debounceTimer) clearTimeout(debounceTimer);
                debounceTimer = setTimeout(function () {
                    cargarCatalogo(1);
                }, 250);
            });
        }

        if (btnClear) {
            btnClear.addEventListener("click", function () {
                if (inputBuscar) {
                    inputBuscar.value = "";
                    inputBuscar.focus();
                }
                cargarCatalogo(1);
            });
        }

        if (selectSeccion) {
            selectSeccion.addEventListener("change", function () {
                cargarCatalogo(1);
            });
        }

        if (selectEstado) {
            selectEstado.addEventListener("change", function () {
                cargarCatalogo(1);
            });
        }

        if (btnReset) {
            btnReset.addEventListener("click", function () {
                if (inputBuscar) inputBuscar.value = "";
                if (selectSeccion) selectSeccion.value = "todas";
                if (selectEstado) selectEstado.value = "todos";
                cargarCatalogo(1);
            });
        }

        const btnReintentar = doc.getElementById("btnReintentarCatalogo");
        if (btnReintentar) {
            btnReintentar.addEventListener("click", function () {
                cargarCatalogo(state.page);
            });
        }

        // Delegación para botones "Ver detalle" y paginación
        if (tbody) {
            tbody.addEventListener("click", function (e) {
                const btn = e.target.closest(".btn-ver-detalle-lote");
                if (btn) {
                    const clave = btn.getAttribute("data-clave");
                    if (clave) abrirModalDetalleLote(clave);
                }
            });
        }

        if (paginacionEl) {
            paginacionEl.addEventListener("click", function (e) {
                const btn = e.target.closest("button[data-page]");
                if (!btn || btn.disabled) return;
                const targetPage = Number(btn.getAttribute("data-page"));
                if (Number.isInteger(targetPage) && targetPage >= 1 && targetPage <= state.totalPages) {
                    cargarCatalogo(targetPage);
                }
            });
        }

        // Ordenamiento accesible por clic y teclado (Enter / Space)
        function toggleOrden(th) {
            const col = th.getAttribute("data-sort-col");
            if (!col) return;

            if (state.orden.columna === col) {
                state.orden.ascendente = !state.orden.ascendente;
            } else {
                state.orden.columna = col;
                state.orden.ascendente = true;
            }

            sortHeaders.forEach(function (header) {
                header.setAttribute("aria-sort", "none");
                const icon = header.querySelector(".sort-icon");
                if (icon) icon.className = "fa-solid fa-sort sort-icon";
            });

            th.setAttribute("aria-sort", state.orden.ascendente ? "ascending" : "descending");
            const currentIcon = th.querySelector(".sort-icon");
            if (currentIcon) {
                currentIcon.className = state.orden.ascendente
                    ? "fa-solid fa-sort-up sort-icon text-dark"
                    : "fa-solid fa-sort-down sort-icon text-dark";
            }

            aplicarOrdenEnPagina();
        }

        sortHeaders.forEach(function (th) {
            th.addEventListener("click", function () {
                toggleOrden(th);
            });
            th.addEventListener("keydown", function (e) {
                if (e.key === "Enter" || e.key === " ") {
                    e.preventDefault();
                    toggleOrden(th);
                }
            });
        });

        // Cambio rápido de estado (Administrador) en _DetalleLoteModal
        const btnGuardarEstado = doc.getElementById("btnGuardarCambioEstadoLote");
        if (btnGuardarEstado) {
            btnGuardarEstado.addEventListener("click", async function () {
                if (!state.loteSeleccionado || !state.loteSeleccionado.id) return;
                const selectEstadoModal = doc.getElementById("selectCambioEstadoLote");
                const alertaEstado = doc.getElementById("alertaCambioEstadoLote");
                const nuevoEstado = selectEstadoModal ? selectEstadoModal.value : "";

                if (!nuevoEstado) return;

                btnGuardarEstado.disabled = true;
                try {
                    await lotesApi.cambiarEstadoLote(state.loteSeleccionado.id, nuevoEstado);
                    state.loteSeleccionado = { ...state.loteSeleccionado, estado: nuevoEstado };
                    poblarModalDetalle(state.loteSeleccionado);
                    mostrarAlertaModal(alertaEstado, "success", `Estado actualizado a ${nuevoEstado}.`);
                    await cargarCatalogo(state.page);
                } catch (err) {
                    mostrarAlertaModal(alertaEstado, "danger", err?.message || "No se pudo actualizar el estado del lote.");
                } finally {
                    btnGuardarEstado.disabled = false;
                }
            });
        }

        // Botones de Alta / Edición Admin
        const btnNuevoLote = doc.getElementById("btnNuevoLoteAdmin");
        if (btnNuevoLote) {
            btnNuevoLote.addEventListener("click", function () {
                abrirModalFormulario(null);
            });
        }

        const btnEditarDesdeDetalle = doc.getElementById("btnEditarDesdeDetalle");
        if (btnEditarDesdeDetalle) {
            btnEditarDesdeDetalle.addEventListener("click", function () {
                if (!state.loteSeleccionado) return;
                const elDetalle = doc.getElementById("modalDetalleLote");
                if (elDetalle && typeof bootstrap !== "undefined" && bootstrap.Modal) {
                    bootstrap.Modal.getOrCreateInstance(elDetalle).hide();
                }
                abrirModalFormulario(state.loteSeleccionado);
            });
        }

        const seccionFormSelect = doc.getElementById("loteFormSeccion");
        if (seccionFormSelect) {
            seccionFormSelect.addEventListener("change", function () {
                const idActual = (doc.getElementById("loteFormId")?.value || "").trim();
                if (!idActual) {
                    cargarVistaPreviaUbicacion(seccionFormSelect.value);
                }
            });
        }

        const gavetasSelect = doc.getElementById("loteFormGavetas");
        if (gavetasSelect) {
            gavetasSelect.addEventListener("change", function () {
                const precioInput = doc.getElementById("loteFormPrecio");
                if (precioInput) {
                    precioInput.value = formatearMoneda(obtenerPrecioPorGavetas(gavetasSelect.value));
                }
            });
        }

        const formLoteAdmin = doc.getElementById("formLoteAdmin");
        if (formLoteAdmin) {
            formLoteAdmin.addEventListener("submit", async function (e) {
                e.preventDefault();
                const id = (doc.getElementById("loteFormId")?.value || "").trim();
                const clave = (doc.getElementById("loteFormClave")?.value || "").trim().toUpperCase();
                const seccion = (doc.getElementById("loteFormSeccion")?.value || "").trim();
                const andadorFila = (doc.getElementById("loteFormAndador")?.value || "").trim();
                const gavetas = Number(doc.getElementById("loteFormGavetas")?.value || 2);
                const precioContado = obtenerPrecioPorGavetas(gavetas);
                const estado = (doc.getElementById("loteFormEstado")?.value || "Disponible").trim();
                const alertaForm = doc.getElementById("alertaFormLote");
                const btnGuardar = doc.getElementById("btnGuardarLote");

                if (!seccion) {
                    mostrarAlertaModal(alertaForm, "danger", "Selecciona una sección válida.");
                    return;
                }

                const originalText = btnGuardar ? btnGuardar.textContent : "";
                if (btnGuardar) {
                    btnGuardar.disabled = true;
                    btnGuardar.textContent = "Guardando...";
                }

                try {
                    if (id) {
                        await lotesApi.actualizarLote(id, { seccion, andadorFila, gavetas, precioContado, estado });
                        if (sfUi && typeof sfUi.showToast === "function") {
                            sfUi.showToast({
                                type: "success",
                                title: "Lote actualizado",
                                message: `Se actualizaron los datos del lote ${clave}.`
                            });
                        }
                    } else {
                        const creado = await lotesApi.crearLote({ seccion, gavetas, precioContado });
                        if (sfUi && typeof sfUi.showToast === "function") {
                            const claveFinal = creado?.clave || clave || "Nuevo lote";
                            const ubicacionFinal = creado?.andadorFila || andadorFila;
                            sfUi.showToast({
                                type: "success",
                                title: "Lote registrado",
                                message: ubicacionFinal
                                    ? `Lote ${claveFinal} creado en ${seccion} (${ubicacionFinal}).`
                                    : `Lote ${claveFinal} creado correctamente.`
                            });
                        }
                    }

                    const elModal = doc.getElementById("modalFormularioLote");
                    if (elModal && typeof bootstrap !== "undefined" && bootstrap.Modal) {
                        bootstrap.Modal.getOrCreateInstance(elModal).hide();
                    }
                    await cargarCatalogo(id ? state.page : 1);
                } catch (err) {
                    mostrarAlertaModal(alertaForm, "danger", err?.message || "No se pudo guardar el lote.");
                } finally {
                    if (btnGuardar) {
                        btnGuardar.disabled = false;
                        btnGuardar.textContent = originalText;
                    }
                }
            });
        }

        if (win) {
            win.abrirModalDetalleLote = abrirModalDetalleLote;
        }

        cargarCatalogo(1);
        return { cargarCatalogo, abrirModalDetalleLote, abrirModalFormulario, cargarVistaPreviaUbicacion };
    }

    return { PRECIOS_RN01, obtenerPrecioPorGavetas, construirUrlContrato, init };
});
