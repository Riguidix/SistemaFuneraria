/**
 * paso4Confirmacion.js - Lógica interactiva del Paso 4 del Wizard (SF-062)
 * Resumen 2x2, validación con checkbox y generación de contrato.
 */
(function () {
    'use strict';

    const DRAFT_KEY = 'sf_nueva_venta_draft';

    function initPaso4() {
        const step3Card = document.getElementById('step3Card');
        const step4Card = document.getElementById('step4Card');

        const chkConfirmar = document.getElementById('chkConfirmarDatos');
        const btnGenerar = document.getElementById('btnGenerarContrato');
        const btnAnterior = document.getElementById('btnAnteriorPaso4');
        const alertaExito = document.getElementById('alertaExitoContrato');

        // Tarjetas Resumen 2x2
        const lblClienteNombre = document.getElementById('resumenClienteNombre');
        const lblClienteEmail = document.getElementById('resumenClienteEmail');
        const lblLoteClave = document.getElementById('resumenLoteClaveYJardin');
        const lblLoteCapacidad = document.getElementById('resumenLoteCapacidad');
        const lblPrecioTotal = document.getElementById('resumenPrecioTotal');
        const lblPrecioEnganche = document.getElementById('resumenPrecioEnganche');
        const lblPlanDetalle = document.getElementById('resumenPlanDetalle');
        const lblPlanTasa = document.getElementById('resumenPlanTasa');

        // Stepper
        const step3Indicator = document.getElementById('stepIndicator3');
        const step4Indicator = document.getElementById('stepIndicator4');

        if (!step4Card) return;

        // 1. Escuchar transición desde Paso 3
        window.addEventListener('sf:paso3-completado', function () {
            cargarDatosBorrador();
            mostrarPaso4();
        });

        // 2. Control de Desbloqueo mediante Checkbox
        if (chkConfirmar && btnGenerar) {
            chkConfirmar.addEventListener('change', function () {
                btnGenerar.disabled = !chkConfirmar.checked;
            });
        }

        // 3. Botón Anterior (Regresar a Paso 3)
        if (btnAnterior) {
            btnAnterior.addEventListener('click', function () {
                mostrarPaso3();
            });
        }

        // 4. Botón Generar Contrato
        if (btnGenerar) {
            btnGenerar.addEventListener('click', function () {
                if (!chkConfirmar.checked) return;

                btnGenerar.disabled = true;
                btnGenerar.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Generando contrato...';

                setTimeout(function () {
                    btnGenerar.innerHTML = '<i class="fa-solid fa-check me-2" aria-hidden="true"></i> Contrato generado';
                    btnGenerar.classList.remove('btn-dark');
                    btnGenerar.style.backgroundColor = '#1e2e18';

                    if (alertaExito) {
                        alertaExito.classList.remove('d-none');
                    }

                    // Limpiar borrador de sesión conforme a DoD
                    try {
                        sessionStorage.removeItem(DRAFT_KEY);
                    } catch (e) {
                        console.warn('Error al limpiar sessionStorage:', e);
                    }

                    const draftBadge = document.getElementById('draftBadge');
                    if (draftBadge) {
                        draftBadge.innerHTML = '<i class="fa-solid fa-check-double me-1 text-success"></i> Contrato generado';
                    }

                    window.dispatchEvent(new CustomEvent('sf:contrato-generado', {
                        detail: { folio: 'CTR-2026-0190' }
                    }));
                }, 800);
            });
        }

        function cargarDatosBorrador() {
            try {
                const raw = sessionStorage.getItem(DRAFT_KEY);
                const draft = raw ? JSON.parse(raw) : {};

                // 1. Cliente
                if (draft.cliente) {
                    if (lblClienteNombre && draft.cliente.nombre) {
                        lblClienteNombre.textContent = draft.cliente.nombre + ' ' + (draft.cliente.apellidos || '');
                    }
                    if (lblClienteEmail && draft.cliente.email) {
                        lblClienteEmail.textContent = draft.cliente.email;
                    }
                }

                // 2. Lote
                if (draft.lote) {
                    const clave = draft.lote.clave || 'JN-A-042';
                    const jardin = draft.lote.clave === 'JC-C-118' ? 'Jardín Central' :
                                   draft.lote.clave === 'RO-E-081' ? 'Robles' : 'Jardín Norte';
                    if (lblLoteClave) lblLoteClave.textContent = clave + ' · ' + jardin;
                    if (lblLoteCapacidad) lblLoteCapacidad.textContent = (draft.lote.capacidad || 2) + ' gavetas';
                }

                // 3. Precio y 4. Financiamiento
                const precio = draft.lote && draft.lote.precio ? parseFloat(draft.lote.precio) : 120000.00;
                const precioStr = '$' + precio.toLocaleString('es-MX', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                if (lblPrecioTotal) lblPrecioTotal.textContent = precioStr;

                if (draft.financiamiento && draft.financiamiento.modalidad === 'Contado') {
                    if (lblPrecioEnganche) lblPrecioEnganche.textContent = 'Pago único de contado';
                    if (lblPlanDetalle) lblPlanDetalle.textContent = 'Liquidación inmediata · 0% interés';
                    if (lblPlanTasa) lblPlanTasa.textContent = 'Sin intereses · Pago único de ' + precioStr;
                } else {
                    const pctEnganche = draft.financiamiento && draft.financiamiento.enganchePorcentaje ? draft.financiamiento.enganchePorcentaje : 20;
                    const montoEng = precio * (pctEnganche / 100.0);
                    const plazo = draft.financiamiento && draft.financiamiento.plazoMeses ? draft.financiamiento.plazoMeses : 24;
                    const mensualidad = draft.financiamiento && draft.financiamiento.mensualidadEstimada ? draft.financiamiento.mensualidadEstimada : '4,887.54';

                    let tasa = '20%';
                    if (plazo === 12) tasa = '16%';
                    else if (plazo === 18) tasa = '18%';
                    else if (plazo === 48) tasa = '24%';

                    if (lblPrecioEnganche) lblPrecioEnganche.textContent = 'Enganche de $' + montoEng.toLocaleString('es-MX', { minimumFractionDigits: 2 });
                    if (lblPlanDetalle) lblPlanDetalle.textContent = plazo + ' pagos de $' + mensualidad;
                    if (lblPlanTasa) lblPlanTasa.textContent = 'Tasa anual fija del ' + tasa;
                }
            } catch (err) {
                console.warn('Error al leer borrador para el Paso 4:', err);
            }
        }

        function mostrarPaso4() {
            if (step3Card) step3Card.classList.add('d-none');
            if (step4Card) step4Card.classList.remove('d-none');

            // Resetear checkbox y botón
            if (chkConfirmar) chkConfirmar.checked = false;
            if (btnGenerar) btnGenerar.disabled = true;

            // Actualizar Stepper: Paso 4 Activo
            if (step3Indicator && step4Indicator) {
                const circle3 = step3Indicator.querySelector('div');
                const circle4 = step4Indicator.querySelector('div');
                if (circle3) {
                    circle3.style.backgroundColor = '#FFFFFF';
                    circle3.style.borderColor = '#2D4224';
                    circle3.classList.remove('text-white');
                    circle3.classList.add('text-success', 'border', 'border-2');
                    circle3.innerHTML = '<i class="fa-solid fa-check" aria-hidden="true"></i>';
                }
                if (circle4) {
                    circle4.style.backgroundColor = '#2D4224';
                    circle4.style.borderColor = '#2D4224';
                    circle4.classList.remove('text-muted', 'bg-white', 'border');
                    circle4.classList.add('text-white', 'fw-bold', 'shadow-sm');
                }
            }
        }

        function mostrarPaso3() {
            if (step4Card) step4Card.classList.add('d-none');
            if (step3Card) step3Card.classList.remove('d-none');

            // Actualizar Stepper: Paso 3 Activo
            if (step3Indicator && step4Indicator) {
                const circle3 = step3Indicator.querySelector('div');
                const circle4 = step4Indicator.querySelector('div');
                if (circle3) {
                    circle3.style.backgroundColor = '#2D4224';
                    circle3.style.borderColor = '#2D4224';
                    circle3.classList.remove('text-success', 'bg-white', 'border');
                    circle3.classList.add('text-white', 'fw-bold');
                    circle3.innerHTML = '3';
                }
                if (circle4) {
                    circle4.style.backgroundColor = '#FFFFFF';
                    circle4.style.borderColor = '#D1D5DB';
                    circle4.classList.remove('text-white', 'fw-bold');
                    circle4.classList.add('text-muted', 'fw-semibold', 'bg-white', 'border');
                }
            }
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPaso4);
    } else {
        initPaso4();
    }
})();
