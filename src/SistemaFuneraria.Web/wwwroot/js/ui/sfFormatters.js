/**
 * Utilidades compartidas de formato, sanitización HTML y control de entrada para SistemaFuneraria.Web.
 * Compatible con navegador (window.sfFormatters) y Node.js (module.exports).
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        root.sfFormatters = factory();
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    const monedaExactaFormatter = new Intl.NumberFormat("es-MX", {
        style: "currency",
        currency: "MXN",
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    const monedaSinDecimalesFormatter = new Intl.NumberFormat("es-MX", {
        style: "currency",
        currency: "MXN",
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    });

    const fechaCortaFormatter = new Intl.DateTimeFormat("es-MX", {
        day: "2-digit",
        month: "short",
        year: "numeric",
        timeZone: "UTC"
    });

    function toFiniteNumber(monto) {
        const numero = Number(monto);
        return Number.isFinite(numero) ? numero : 0;
    }

    function formatearMonedaMXN(monto) {
        return monedaExactaFormatter.format(toFiniteNumber(monto));
    }

    function formatearMonedaSinDecimales(monto) {
        return monedaSinDecimalesFormatter.format(toFiniteNumber(monto));
    }

    function formatearFechaCorta(fechaIso, fallback = "Sin pagos") {
        if (!fechaIso) {
            return fallback;
        }
        const fecha = fechaIso instanceof Date ? fechaIso : new Date(fechaIso);
        if (Number.isNaN(fecha.getTime())) {
            return fallback;
        }
        return fechaCortaFormatter.format(fecha);
    }

    function escapeHtml(text) {
        if (text === null || text === undefined) {
            return "";
        }
        return String(text)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function obtenerIniciales(nombreCompleto, fallback = "--") {
        if (!nombreCompleto || typeof nombreCompleto !== "string") {
            return fallback;
        }
        const partes = nombreCompleto.trim().split(/\s+/).filter(Boolean);
        if (partes.length >= 2) {
            return `${partes[0][0]}${partes[1][0]}`.toUpperCase();
        }
        if (partes.length === 1) {
            return partes[0].slice(0, 2).toUpperCase();
        }
        return fallback;
    }

    function debounce(fn, delayMs = 220) {
        let timer = null;
        return function (...args) {
            if (timer) {
                clearTimeout(timer);
            }
            timer = setTimeout(() => fn.apply(this, args), delayMs);
        };
    }

    return {
        formatearMonedaMXN,
        formatearMonedaSinDecimales,
        formatearFechaCorta,
        escapeHtml,
        obtenerIniciales,
        debounce
    };
});
