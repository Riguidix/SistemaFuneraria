/**
 * Sistema Funeraria (Gestión Memorial)
 * Kit Estandarizado de los 5 Estados UI (Ticket SF-027)
 * Namespace Global: window.SfUi
 */
(function (window, document) {
    'use strict';

    const SfUi = {};

    // Almacén para restaurar contenido previo al mostrar skeletons
    const _skeletonBackups = new WeakMap();

    /**
     * Resuelve un selector o elemento DOM
     */
    function getElement(target) {
        if (!target) return null;
        if (typeof target === 'string') return document.querySelector(target);
        if (target instanceof HTMLElement) return target;
        return null;
    }

    /**
     * 1. ESTADO 1: Loading (Skeletons Cálidos Shimmer)
     * @param {string|HTMLElement} targetElement - Contenedor donde renderizar
     * @param {'table'|'card'|'input'} type - Tipo de skeleton visual
     * @param {number} count - Cantidad de filas o elementos
     */
    SfUi.showSkeleton = function (targetElement, type = 'card', count = 3) {
        const el = getElement(targetElement);
        if (!el) return;

        if (!_skeletonBackups.has(el)) {
            _skeletonBackups.set(el, el.innerHTML);
        }

        let skeletonHtml = '';

        if (type === 'table') {
            let rowsHtml = '';
            for (let i = 0; i < count; i++) {
                rowsHtml += `
                    <tr class="sf-skeleton-row">
                        <td class="ps-4"><div class="sf-skeleton sf-skeleton-text" style="width: 80px;"></div></td>
                        <td><div class="sf-skeleton sf-skeleton-text" style="width: 140px;"></div></td>
                        <td><div class="sf-skeleton sf-skeleton-text" style="width: 100px;"></div></td>
                        <td class="text-end"><div class="sf-skeleton sf-skeleton-text ms-auto" style="width: 85px;"></div></td>
                        <td><div class="sf-skeleton sf-skeleton-badge" style="width: 90px;"></div></td>
                        <td class="text-end pe-4"><div class="sf-skeleton sf-skeleton-button ms-auto" style="width: 70px;"></div></td>
                    </tr>
                `;
            }
            skeletonHtml = `
                <div class="sf-skeleton-wrapper" role="status" aria-busy="true" aria-label="Cargando datos...">
                    <table class="table align-middle mb-0">
                        <tbody>${rowsHtml}</tbody>
                    </table>
                </div>
            `;
        } else if (type === 'card') {
            let cardsHtml = '';
            for (let i = 0; i < count; i++) {
                cardsHtml += `
                    <div class="sf-skeleton-card p-3 rounded-4 mb-3 d-flex align-items-center gap-3">
                        <div class="sf-skeleton sf-skeleton-avatar"></div>
                        <div class="flex-grow-1">
                            <div class="sf-skeleton sf-skeleton-text mb-2" style="width: 60%; height: 16px;"></div>
                            <div class="sf-skeleton sf-skeleton-text" style="width: 40%; height: 12px;"></div>
                        </div>
                    </div>
                `;
            }
            skeletonHtml = `<div class="sf-skeleton-wrapper" role="status" aria-busy="true" aria-label="Cargando...">${cardsHtml}</div>`;
        } else if (type === 'input') {
            let inputsHtml = '';
            for (let i = 0; i < count; i++) {
                inputsHtml += `
                    <div class="mb-3">
                        <div class="sf-skeleton sf-skeleton-text mb-1" style="width: 120px; height: 14px;"></div>
                        <div class="sf-skeleton sf-skeleton-input rounded-3"></div>
                    </div>
                `;
            }
            skeletonHtml = `<div class="sf-skeleton-wrapper" role="status" aria-busy="true" aria-label="Cargando formulario...">${inputsHtml}</div>`;
        }

        el.innerHTML = skeletonHtml;
    };

    /**
     * Retira el skeleton y restaura el contenedor original (o lo deja vacío)
     */
    SfUi.hideSkeleton = function (targetElement, restoreOriginal = true) {
        const el = getElement(targetElement);
        if (!el) return;

        if (restoreOriginal && _skeletonBackups.has(el)) {
            el.innerHTML = _skeletonBackups.get(el);
            _skeletonBackups.delete(el);
        } else {
            const skeletonWrapper = el.querySelector('.sf-skeleton-wrapper');
            if (skeletonWrapper) {
                skeletonWrapper.remove();
            }
            _skeletonBackups.delete(el);
        }
    };

    /**
     * 2. ESTADO 2: Empty State (Mensaje Empático con CTA)
     */
    SfUi.showEmpty = function (targetElement, options = {}) {
        const el = getElement(targetElement);
        if (!el) return;

        const title = options.title || 'No se encontraron registros';
        const message = options.message || 'No hay elementos para mostrar en este momento.';
        const icon = options.icon || 'fa-solid fa-folder-open';
        const ctaText = options.ctaText || null;
        const ctaAction = options.ctaAction || null;

        let ctaButtonHtml = '';
        if (ctaText) {
            ctaButtonHtml = `
                <button type="button" class="btn px-4 py-2 mt-3 rounded-3 fw-semibold shadow-sm sf-empty-cta text-white" style="background-color: var(--sf-primary-olive, #2D4224); border: none;">
                    ${ctaText}
                </button>
            `;
        }

        const emptyHtml = `
            <div class="sf-empty-state text-center py-5 px-3 rounded-4 bg-white border shadow-sm">
                <div class="sf-empty-icon mb-3">
                    <i class="${icon} text-muted fs-1 opacity-50"></i>
                </div>
                <h4 class="fw-bold mb-2" style="color: var(--ink, #17211f); font-size: 1.25rem;">${title}</h4>
                <p class="text-muted mx-auto mb-0" style="max-width: 480px; font-size: 0.95rem;">${message}</p>
                ${ctaButtonHtml}
            </div>
        `;

        el.innerHTML = emptyHtml;

        if (ctaText && typeof ctaAction === 'function') {
            const btn = el.querySelector('.sf-empty-cta');
            if (btn) btn.addEventListener('click', ctaAction);
        }
    };

    /**
     * 3. ESTADO 3: Error Banner (ProblemDetails Compatible)
     */
    SfUi.showError = function (targetElement, options = {}) {
        const el = getElement(targetElement);
        if (!el) return;

        const title = options.title || 'Ha ocurrido un error';
        let detailHtml = '';
        const onRetry = options.onRetry || null;
        const icon = options.icon || 'fa-solid fa-circle-exclamation';

        if (options.detail) {
            if (typeof options.detail === 'object') {
                const pd = options.detail;
                let msgs = [];
                if (pd.detail) msgs.push(pd.detail);
                if (pd.errors && typeof pd.errors === 'object') {
                    for (const key in pd.errors) {
                        const errList = Array.isArray(pd.errors[key]) ? pd.errors[key] : [pd.errors[key]];
                        errList.forEach(m => msgs.push(m));
                    }
                }
                detailHtml = msgs.length > 0 ? msgs.join('<br>') : (pd.title || 'Error en la petición.');
            } else {
                detailHtml = String(options.detail);
            }
        }

        let retryHtml = '';
        if (typeof onRetry === 'function') {
            retryHtml = `
                <button type="button" class="btn btn-sm btn-outline-danger ms-auto rounded-3 px-3 fw-medium sf-error-retry">
                    Reintentar
                </button>
            `;
        }

        const bannerHtml = `
            <div class="sf-error-banner p-3 p-md-4 rounded-3 d-flex align-items-center mb-3 shadow-sm" role="alert" aria-live="polite" style="background-color: #FDF2F2; border-left: 4px solid var(--rose, #a84f4b); border-top: 1px solid #FEE2E2; border-right: 1px solid #FEE2E2; border-bottom: 1px solid #FEE2E2;">
                <div class="d-flex align-items-start gap-3 flex-grow-1">
                    <i class="${icon} text-danger fs-5 mt-1"></i>
                    <div>
                        <div class="fw-bold text-danger mb-1" style="font-size: 0.95rem;">${title}</div>
                        ${detailHtml ? `<div class="small" style="color: #4B5563 !important;">${detailHtml}</div>` : ''}
                    </div>
                </div>
                ${retryHtml}
            </div>
        `;

        el.innerHTML = bannerHtml;

        if (typeof onRetry === 'function') {
            const btn = el.querySelector('.sf-error-retry');
            if (btn) btn.addEventListener('click', onRetry);
        }
    };

    /**
     * 4. ESTADO 4: Toast Flotante (Notificación Accesible Temporizada)
     */
    SfUi.showToast = function (options = {}) {
        const type = options.type || 'success';
        const title = options.title || (type === 'success' ? 'Éxito' : type === 'error' ? 'Error' : 'Aviso');
        const message = options.message || '';
        const durationMs = options.durationMs !== undefined ? options.durationMs : 4000;

        let container = document.getElementById('sf-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'sf-toast-container';
            container.className = 'position-fixed top-0 end-0 p-3';
            container.style.zIndex = '1080';
            document.body.appendChild(container);
        }

        let iconClass = 'fa-solid fa-circle-check text-success';
        let borderColor = 'border-success-subtle';
        if (type === 'error') {
            iconClass = 'fa-solid fa-circle-xmark text-danger';
            borderColor = 'border-danger-subtle';
        } else if (type === 'warning') {
            iconClass = 'fa-solid fa-triangle-exclamation text-warning';
            borderColor = 'border-warning-subtle';
        } else if (type === 'info') {
            iconClass = 'fa-solid fa-circle-info text-primary';
            borderColor = 'border-primary-subtle';
        }

        const toastEl = document.createElement('div');
        toastEl.className = `toast show align-items-center bg-white border ${borderColor} rounded-3 shadow-lg mb-2`;
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.setAttribute('aria-atomic', 'true');
        toastEl.style.minWidth = '320px';

        toastEl.innerHTML = `
            <div class="d-flex align-items-center p-3">
                <i class="${iconClass} fs-5 me-3 flex-shrink-0"></i>
                <div class="flex-grow-1">
                    <div class="fw-bold text-dark small mb-0">${title}</div>
                    ${message ? `<div class="text-muted small mt-1">${message}</div>` : ''}
                </div>
                <button type="button" class="btn-close ms-2 me-1" aria-label="Cerrar"></button>
            </div>
        `;

        const closeBtn = toastEl.querySelector('.btn-close');
        const dismiss = () => {
            toastEl.classList.remove('show');
            setTimeout(() => toastEl.remove(), 250);
        };
        closeBtn.addEventListener('click', dismiss);

        container.appendChild(toastEl);

        if (durationMs > 0) {
            setTimeout(dismiss, durationMs);
        }
    };

    /**
     * 5. ESTADO 5: Modal Redondeado de Confirmación (Captura 4 de Figma)
     * Retorna una Promise<boolean>
     */
    SfUi.confirm = function (options = {}) {
        return new Promise((resolve) => {
            const title = options.title || '¿Estás seguro?';
            const message = options.message || 'Esta acción no se puede deshacer.';
            const confirmText = options.confirmText || 'Confirmar';
            const cancelText = options.cancelText || 'Cancelar';
            const icon = options.icon || 'fa-triangle-exclamation';
            const isDestructive = options.isDestructive || false;

            const modalEl = document.getElementById('sf-confirm-modal');
            if (!modalEl || typeof bootstrap === 'undefined' || !bootstrap.Modal) {
                const res = window.confirm(`${title}\n\n${message}`);
                resolve(res);
                return;
            }

            const modalTitle = modalEl.querySelector('#sf-confirm-title');
            const modalMessage = modalEl.querySelector('#sf-confirm-message');
            const modalIcon = modalEl.querySelector('#sf-confirm-icon');
            const btnConfirm = modalEl.querySelector('#sf-confirm-btn');
            const btnCancel = modalEl.querySelector('#sf-cancel-btn');

            if (modalTitle) modalTitle.textContent = title;
            if (modalMessage) modalMessage.textContent = message;
            if (modalIcon) {
                modalIcon.className = `fa-solid ${icon} fs-1 ` + (isDestructive ? 'text-danger' : 'text-warning');
            }
            if (btnConfirm) {
                btnConfirm.textContent = confirmText;
                btnConfirm.className = isDestructive 
                    ? 'btn btn-danger rounded-3 px-4 py-2 fw-semibold shadow-sm' 
                    : 'btn rounded-3 px-4 py-2 fw-semibold text-white shadow-sm';
                if (!isDestructive) {
                    btnConfirm.style.backgroundColor = 'var(--sf-primary-olive, #2D4224)';
                } else {
                    btnConfirm.style.backgroundColor = '';
                }
            }
            if (btnCancel) btnCancel.textContent = cancelText;

            let bsModal = bootstrap.Modal.getInstance(modalEl);
            if (!bsModal) {
                bsModal = new bootstrap.Modal(modalEl, { backdrop: 'static', keyboard: true });
            }

            let answered = false;

            const onConfirmClick = () => {
                answered = true;
                cleanListeners();
                bsModal.hide();
                if (typeof options.onConfirm === 'function') options.onConfirm();
                resolve(true);
            };

            const onCancelClick = () => {
                answered = true;
                cleanListeners();
                bsModal.hide();
                if (typeof options.onCancel === 'function') options.onCancel();
                resolve(false);
            };

            const onHidden = () => {
                cleanListeners();
                if (!answered) {
                    if (typeof options.onCancel === 'function') options.onCancel();
                    resolve(false);
                }
            };

            function cleanListeners() {
                btnConfirm.removeEventListener('click', onConfirmClick);
                btnCancel.removeEventListener('click', onCancelClick);
                modalEl.removeEventListener('hidden.bs.modal', onHidden);
            }

            btnConfirm.addEventListener('click', onConfirmClick);
            btnCancel.addEventListener('click', onCancelClick);
            modalEl.addEventListener('hidden.bs.modal', onHidden, { once: true });

            bsModal.show();
        });
    };

    window.SfUi = SfUi;

})(window, document);
