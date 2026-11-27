/**
 * firmaPad.js - Componente reutilizable de firma digital sobre HTML5 Canvas (mouse, touch y stylus).
 * Sin dependencias externas, compatible con navegador (window.SfFirmaPad) y Node (module.exports).
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        root.SfFirmaPad = factory();
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    function create(canvas, options = {}) {
        if (!canvas || typeof canvas.getContext !== "function") {
            return null;
        }

        const strokeColor = options.strokeColor || "#1E221E";
        const lineWidth = Number(options.lineWidth) > 0 ? Number(options.lineWidth) : 2.2;
        const onChange = typeof options.onChange === "function" ? options.onChange : null;

        let drawing = false;
        let hasStrokes = false;
        let lastPoint = null;
        let ctx = canvas.getContext("2d");

        function configureContext() {
            if (!ctx) return;
            ctx.strokeStyle = strokeColor;
            ctx.lineWidth = lineWidth;
            ctx.lineCap = "round";
            ctx.lineJoin = "round";
        }

        function resize(preserveDrawing = true) {
            if (!ctx) return;
            const rect = typeof canvas.getBoundingClientRect === "function"
                ? canvas.getBoundingClientRect()
                : { width: canvas.width || 480, height: canvas.height || 150 };
            const cssWidth = Math.max(Math.round(rect.width), 240);
            const cssHeight = Math.max(Math.round(rect.height), 120);
            const dpr = (typeof window !== "undefined" && Number(window.devicePixelRatio) > 0)
                ? Math.min(window.devicePixelRatio, 2)
                : 1;

            const prevDataUrl = preserveDrawing && hasStrokes ? canvas.toDataURL("image/png") : null;

            canvas.width = Math.round(cssWidth * dpr);
            canvas.height = Math.round(cssHeight * dpr);
            if (typeof ctx.setTransform === "function") {
                ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
            }
            configureContext();

            if (prevDataUrl && typeof Image !== "undefined") {
                const img = new Image();
                img.onload = function () {
                    ctx.drawImage(img, 0, 0, cssWidth, cssHeight);
                };
                img.src = prevDataUrl;
            }
        }

        function getPoint(ev) {
            const rect = canvas.getBoundingClientRect();
            return {
                x: ev.clientX - rect.left,
                y: ev.clientY - rect.top
            };
        }

        function onPointerDown(ev) {
            if (ev.button !== undefined && ev.button !== 0 && ev.pointerType === "mouse") {
                return;
            }
            if (typeof ev.preventDefault === "function") ev.preventDefault();
            if (typeof canvas.setPointerCapture === "function" && ev.pointerId !== undefined) {
                try { canvas.setPointerCapture(ev.pointerId); } catch { /* ignore */ }
            }
            drawing = true;
            lastPoint = getPoint(ev);
            configureContext();
            ctx.beginPath();
            ctx.moveTo(lastPoint.x, lastPoint.y);
            ctx.lineTo(lastPoint.x + 0.01, lastPoint.y + 0.01);
            ctx.stroke();

            if (!hasStrokes) {
                hasStrokes = true;
                if (onChange) onChange(true);
            }
        }

        function onPointerMove(ev) {
            if (!drawing || !lastPoint) return;
            if (typeof ev.preventDefault === "function") ev.preventDefault();
            const point = getPoint(ev);
            ctx.beginPath();
            ctx.moveTo(lastPoint.x, lastPoint.y);
            ctx.lineTo(point.x, point.y);
            ctx.stroke();
            lastPoint = point;
        }

        function onPointerEnd(ev) {
            if (!drawing) return;
            drawing = false;
            lastPoint = null;
            if (typeof canvas.releasePointerCapture === "function" && ev?.pointerId !== undefined) {
                try { canvas.releasePointerCapture(ev.pointerId); } catch { /* ignore */ }
            }
        }

        function clear() {
            drawing = false;
            lastPoint = null;
            if (ctx) {
                ctx.save();
                if (typeof ctx.setTransform === "function") {
                    ctx.setTransform(1, 0, 0, 1, 0, 0);
                }
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                ctx.restore();
                configureContext();
            }
            const changed = hasStrokes;
            hasStrokes = false;
            if (onChange && changed) onChange(false);
        }

        function isEmpty() {
            return !hasStrokes;
        }

        function toDataURL() {
            if (!hasStrokes) return "";
            return canvas.toDataURL("image/png");
        }

        function destroy() {
            canvas.removeEventListener("pointerdown", onPointerDown);
            canvas.removeEventListener("pointermove", onPointerMove);
            canvas.removeEventListener("pointerup", onPointerEnd);
            canvas.removeEventListener("pointercancel", onPointerEnd);
            canvas.removeEventListener("pointerleave", onPointerEnd);
        }

        resize(false);
        canvas.addEventListener("pointerdown", onPointerDown);
        canvas.addEventListener("pointermove", onPointerMove);
        canvas.addEventListener("pointerup", onPointerEnd);
        canvas.addEventListener("pointercancel", onPointerEnd);
        canvas.addEventListener("pointerleave", onPointerEnd);

        return {
            resize,
            clear,
            isEmpty,
            toDataURL,
            destroy
        };
    }

    return {
        create
    };
});
