/**
 * wizardState.js - Gestor de persistencia reactiva en sessionStorage para el Wizard de Nueva Venta (SF-050).
 * Maneja el borrador con la clave 'sf_nueva_venta_draft', el pill 'Borrador guardado' y el panel lateral.
 */
const DRAFT_STORAGE_KEY = "sf_nueva_venta_draft";

function resolveSessionStorage() {
    if (typeof window !== "undefined" && window.sessionStorage) {
        return window.sessionStorage;
    }
    return null;
}

const wizardState = {
    STORAGE_KEY: DRAFT_STORAGE_KEY,

    /**
     * Consulta el borrador actual guardado en sessionStorage.
     * @returns {Object}
     */
    getDraft: function () {
        const storage = resolveSessionStorage();
        if (!storage) return {};
        try {
            const raw = storage.getItem(DRAFT_STORAGE_KEY);
            return raw ? JSON.parse(raw) : {};
        } catch (e) {
            console.warn("wizardState: Error al leer sessionStorage", e);
            return {};
        }
    },

    /**
     * Actualiza incrementalmente el borrador, sincroniza la interfaz y notifica el evento 'sf:draft-updated'.
     * @param {Object} partial
     * @returns {Object}
     */
    updateDraft: function (partial) {
        const storage = resolveSessionStorage();
        const current = this.getDraft();
        const updated = { ...current, ...(partial || {}) };

        if (storage) {
            try {
                storage.setItem(DRAFT_STORAGE_KEY, JSON.stringify(updated));
            } catch (e) {
                console.warn("wizardState: Error al escribir en sessionStorage", e);
            }
        }

        this.syncDraftBadge(true);
        this.syncSummaryPanel(updated);

        if (typeof window !== "undefined") {
            window.dispatchEvent(new CustomEvent("sf:draft-updated", { detail: updated }));
        }

        return updated;
    },

    /**
     * Limpia inmediatamente el borrador en sessionStorage al finalizar el contrato o cerrar sesión.
     */
    clearDraft: function () {
        const storage = resolveSessionStorage();
        if (storage) {
            try {
                storage.removeItem(DRAFT_STORAGE_KEY);
            } catch (e) {
                console.warn("wizardState: Error al limpiar sessionStorage", e);
            }
        }

        this.syncDraftBadge(false);

        if (typeof window !== "undefined") {
            window.dispatchEvent(new CustomEvent("sf:draft-cleared"));
            window.dispatchEvent(new CustomEvent("sf:draft-updated", { detail: {} }));
        }
    },

    /**
     * Sincroniza la visibilidad del pill 'Borrador guardado' (#draftBadge).
     * @param {boolean} hasDraft
     */
    syncDraftBadge: function (hasDraft) {
        if (typeof document === "undefined") return;
        const badge = document.getElementById("draftBadge");
        if (badge) {
            if (hasDraft) {
                badge.classList.remove("d-none");
                badge.style.display = "";
            } else {
                badge.classList.add("d-none");
            }
        }
    },

    /**
     * Sincroniza el panel lateral 'RESUMEN DE COTIZACIÓN' con los datos del borrador.
     * @param {Object} [draft]
     */
    syncSummaryPanel: function (draft) {
        if (typeof document === "undefined") return;
        const d = draft || this.getDraft();
        const lote = d.lote || {};
        const plan = d.financiamiento || {};

        const elTitulo = document.getElementById("resumenLoteTitulo");
        if (elTitulo && (lote.seccion || lote.tipo)) {
            elTitulo.textContent = lote.tipo || ("Lote " + (lote.seccion || ""));
        }

        const elClave = document.getElementById("resumenLoteClave");
        if (elClave && lote.clave) {
            elClave.textContent = lote.clave;
        }

        const elCapacidad = document.getElementById("resumenLoteCapacidad");
        if (elCapacidad && (lote.gavetas || lote.capacidad)) {
            elCapacidad.textContent = (lote.gavetas || lote.capacidad) + " gavetas";
        }

        const elPrecio = document.getElementById("resumenLotePrecio");
        if (elPrecio && lote.precio) {
            elPrecio.textContent = "$" + Number(lote.precio).toLocaleString("es-MX", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        }

        const elTotal = document.getElementById("resumenTotalEstimado");
        if (elTotal) {
            const total = plan.totalAPagar || lote.precio || 120000;
            elTotal.textContent = "$" + Number(total).toLocaleString("es-MX", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        }
    },

    /**
     * Inicializa el estado al cargar la página restaurando borrador y escuchando eventos.
     */
    init: function () {
        if (typeof window === "undefined") return;
        const draft = this.getDraft();
        const hasData = Object.keys(draft).length > 0;
        this.syncDraftBadge(hasData);
        if (hasData) {
            this.syncSummaryPanel(draft);
        }

        window.addEventListener("sf:draft-updated", (e) => {
            this.syncSummaryPanel(e.detail);
        });
    }
};

if (typeof window !== "undefined") {
    window.wizardState = wizardState;
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", () => wizardState.init());
    } else {
        wizardState.init();
    }
}

if (typeof module !== "undefined" && module.exports) {
    module.exports = wizardState;
}
