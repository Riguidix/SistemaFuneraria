/**
 * paso2Lote.js - Lógica interactiva del Paso 2 del Wizard (SF-053)
 * Selección de lote y sincronización reactiva en vivo con el panel derecho RESUMEN DE COTIZACIÓN.
 */
(function () {
    'use strict';

    const DRAFT_KEY = 'sf_nueva_venta_draft';

    function initPaso2() {
        const step1Card = document.getElementById('step1Card');
        const step2Card = document.getElementById('step2Card');
        const btnAnterior = document.getElementById('btnAnteriorPaso2');
        const btnContinuar2 = document.getElementById('btnContinuarPaso2');
        const tarjetas = document.querySelectorAll('.sf-lote-card');

        // Elementos del Stepper
        const step1Indicator = document.getElementById('stepIndicator1');
        const step2Indicator = document.getElementById('stepIndicator2');

        // Elementos del Panel Lateral "RESUMEN DE COTIZACIÓN"
        const resumenTitulo = document.getElementById('resumenLoteTitulo');
        const resumenClave = document.getElementById('resumenLoteClave');
        const resumenCapacidad = document.getElementById('resumenLoteCapacidad');
        const resumenPrecio = document.getElementById('resumenLotePrecio');
        const resumenTotal = document.getElementById('resumenTotalEstimado');
        const slotCells = document.querySelectorAll('.sf-slot-cell');

        if (!step2Card) return;

        // 1. Escuchar transición desde Paso 1 hacia Paso 2
        window.addEventListener('sf:paso1-completado', function () {
            mostrarPaso2();
        });

        // 2. Navegación hacia atrás (Regresar al Paso 1)
        if (btnAnterior) {
            btnAnterior.addEventListener('click', function () {
                mostrarPaso1();
            });
        }

        // 3. Selección reactiva de tarjetas de lote
        tarjetas.forEach(function (card) {
            card.addEventListener('click', function () {
                seleccionarTarjeta(card);
            });
        });

        // 4. Continuar hacia Paso 3
        if (btnContinuar2) {
            btnContinuar2.addEventListener('click', function () {
                const tarjetaActiva = document.querySelector('.sf-lote-card[data-activa="true"]') || tarjetas[0];
                if (tarjetaActiva) {
                    guardarLoteEnBorrador(tarjetaActiva, 3);
                }
                window.dispatchEvent(new CustomEvent('sf:paso2-completado'));
            });
        }

        function seleccionarTarjeta(card) {
            tarjetas.forEach(function (c) {
                c.removeAttribute('data-activa');
                c.style.border = '1px solid #E5E7EB';
                c.style.backgroundColor = '#FFFFFF';
            });

            card.setAttribute('data-activa', 'true');
            card.style.border = '2px solid #2D4224';
            card.style.backgroundColor = '#FAFBF9';

            const clave = card.getAttribute('data-lote-clave') || 'JN-A-042';
            const tipo = card.getAttribute('data-lote-tipo') || 'Lote Básico';
            const capacidad = card.getAttribute('data-lote-capacidad') || '2';
            const precioNum = parseFloat(card.getAttribute('data-lote-precio') || '120000');
            const slot = card.getAttribute('data-slot') || '42';

            const precioFormateado = '$' + precioNum.toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

            // Sincronizar Panel Lateral Derecho
            if (resumenTitulo) resumenTitulo.textContent = tipo;
            if (resumenClave) resumenClave.textContent = clave;
            if (resumenCapacidad) resumenCapacidad.textContent = capacidad + ' gavetas';
            if (resumenPrecio) resumenPrecio.textContent = precioFormateado;
            if (resumenTotal) resumenTotal.textContent = precioFormateado;

            // Sincronizar Casilla en Cuadrícula 2x4
            slotCells.forEach(function (cell) {
                const cellSlot = cell.getAttribute('data-slot');
                if (cellSlot === slot) {
                    cell.style.backgroundColor = '#2D4224';
                    cell.style.color = '#FFFFFF';
                    cell.style.fontWeight = 'bold';
                    cell.classList.add('shadow-sm');
                    cell.classList.remove('text-secondary', 'bg-white', 'border');
                } else {
                    cell.style.backgroundColor = '#FFFFFF';
                    cell.style.color = '#4B5563';
                    cell.style.fontWeight = '500';
                    cell.classList.remove('shadow-sm');
                    cell.classList.add('text-secondary', 'bg-white', 'border');
                }
            });

            guardarLoteEnBorrador(card, 2);
        }

        function mostrarPaso1() {
            if (step1Card) step1Card.classList.remove('d-none');
            if (step2Card) step2Card.classList.add('d-none');

            // Actualizar Stepper: Paso 1 Activo, Paso 2 Inactivo
            if (step1Indicator && step2Indicator) {
                const circle1 = step1Indicator.querySelector('div');
                const circle2 = step2Indicator.querySelector('div');
                if (circle1) {
                    circle1.style.backgroundColor = '#2D4224';
                    circle1.style.borderColor = '#2D4224';
                    circle1.classList.remove('text-muted', 'bg-white', 'border');
                    circle1.classList.add('text-white', 'fw-bold');
                }
                if (circle2) {
                    circle2.style.backgroundColor = '#FFFFFF';
                    circle2.style.borderColor = '#D1D5DB';
                    circle2.classList.remove('text-white', 'fw-bold');
                    circle2.classList.add('text-muted', 'fw-semibold', 'bg-white', 'border');
                }
            }
        }

        function mostrarPaso2() {
            if (step1Card) step1Card.classList.add('d-none');
            if (step2Card) step2Card.classList.remove('d-none');

            // Actualizar Stepper: Paso 2 Activo
            if (step1Indicator && step2Indicator) {
                const circle1 = step1Indicator.querySelector('div');
                const circle2 = step2Indicator.querySelector('div');
                if (circle1) {
                    circle1.style.backgroundColor = '#FFFFFF';
                    circle1.style.borderColor = '#2D4224';
                    circle1.classList.remove('text-white');
                    circle1.classList.add('text-success', 'border', 'border-2');
                    circle1.innerHTML = '<i class="fa-solid fa-check" aria-hidden="true"></i>';
                }
                if (circle2) {
                    circle2.style.backgroundColor = '#2D4224';
                    circle2.style.borderColor = '#2D4224';
                    circle2.classList.remove('text-muted', 'bg-white', 'border');
                    circle2.classList.add('text-white', 'fw-bold', 'shadow-sm');
                }
            }
        }

        function guardarLoteEnBorrador(card, pasoNum) {
            try {
                const borrador = JSON.parse(sessionStorage.getItem(DRAFT_KEY) || '{}');
                borrador.lote = {
                    clave: card.getAttribute('data-lote-clave'),
                    tipo: card.getAttribute('data-lote-tipo'),
                    capacidad: parseInt(card.getAttribute('data-lote-capacidad'), 10),
                    precio: parseFloat(card.getAttribute('data-lote-precio')),
                    slot: card.getAttribute('data-slot')
                };
                borrador.paso = pasoNum;
                sessionStorage.setItem(DRAFT_KEY, JSON.stringify(borrador));
                window.dispatchEvent(new CustomEvent('sf:draft-updated', { detail: borrador }));
            } catch (err) {
                console.warn('Error al persistir lote en sessionStorage:', err);
            }
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPaso2);
    } else {
        initPaso2();
    }
})();
