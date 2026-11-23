/**
 * paso1Cliente.js - Lógica interactiva del Paso 1 del Wizard (SF-052)
 * Búsqueda reactiva de cliente existente y captura inline del titular con persistencia en sessionStorage.
 */
(function () {
    'use strict';

    const DRAFT_KEY = 'sf_nueva_venta_draft';

    const clientesDemo = [
        {
            id: 1,
            nombre: 'Mariana',
            apellidos: 'Torres Aguirre',
            curp: 'TOAM850423MDFRRG01',
            telefono: '5512345678',
            email: 'mariana.torres@email.mx'
        },
        {
            id: 2,
            nombre: 'Carlos',
            apellidos: 'Ramírez Soto',
            curp: 'RASC780112HDFNN02',
            telefono: '5598765432',
            email: 'carlos.ramirez@email.mx'
        },
        {
            id: 3,
            nombre: 'Beatriz',
            apellidos: 'Mendoza Luna',
            curp: 'MELB900518MDFRR05',
            telefono: '5544332211',
            email: 'beatriz.mendoza@email.mx'
        }
    ];

    function initPaso1() {
        const inputBuscar = document.getElementById('inputBuscarCliente');
        const btnLimpiar = document.getElementById('btnLimpiarBusqueda');
        const dropdownResultados = document.getElementById('resultadosBusquedaCliente');

        const txtNombre = document.getElementById('txtClienteNombre');
        const txtApellidos = document.getElementById('txtClienteApellidos');
        const txtCurp = document.getElementById('txtClienteCurp');
        const txtTelefono = document.getElementById('txtClienteTelefono');
        const txtEmail = document.getElementById('txtClienteEmail');

        const btnContinuar = document.getElementById('btnContinuarPaso1');
        const alertaValidacion = document.getElementById('paso1AlertaValidacion');

        if (!btnContinuar) return;

        restaurarBorrador();

        // 1. Buscador reactivo
        if (inputBuscar && dropdownResultados) {
            inputBuscar.addEventListener('input', function () {
                const query = inputBuscar.value.trim().toLowerCase();
                if (query.length < 2) {
                    dropdownResultados.classList.add('d-none');
                    dropdownResultados.innerHTML = '';
                    return;
                }

                const coincidencias = clientesDemo.filter(function (c) {
                    return c.nombre.toLowerCase().includes(query) ||
                        c.apellidos.toLowerCase().includes(query) ||
                        c.curp.toLowerCase().includes(query) ||
                        c.telefono.includes(query);
                });

                if (coincidencias.length === 0) {
                    dropdownResultados.innerHTML = '<div class="list-group-item text-muted small py-2">Sin clientes encontrados</div>';
                    dropdownResultados.classList.remove('d-none');
                    return;
                }

                dropdownResultados.innerHTML = coincidencias.map(function (c) {
                    return '<button type="button" class="list-group-item list-group-item-action py-2 d-flex justify-content-between align-items-center" data-cliente-id="' + c.id + '">' +
                        '<div>' +
                            '<div class="fw-semibold small text-dark">' + c.nombre + ' ' + c.apellidos + '</div>' +
                            '<div class="text-muted" style="font-size: 0.75rem;">CURP: ' + c.curp + ' · Tel: ' + c.telefono + '</div>' +
                        '</div>' +
                        '<span class="badge bg-light text-secondary border">Seleccionar</span>' +
                    '</button>';
                }).join('');
                dropdownResultados.classList.remove('d-none');
            });

            dropdownResultados.addEventListener('click', function (e) {
                const btnItem = e.target.closest('[data-cliente-id]');
                if (!btnItem) return;

                const clienteId = parseInt(btnItem.getAttribute('data-cliente-id'), 10);
                const cliente = clientesDemo.find(function (c) { return c.id === clienteId; });
                if (cliente) {
                    txtNombre.value = cliente.nombre;
                    txtApellidos.value = cliente.apellidos;
                    txtCurp.value = cliente.curp;
                    txtTelefono.value = cliente.telefono;
                    txtEmail.value = cliente.email;
                    inputBuscar.value = cliente.nombre + ' ' + cliente.apellidos;
                }

                dropdownResultados.classList.add('d-none');
                ocultarError();
            });

            document.addEventListener('click', function (e) {
                if (!inputBuscar.contains(e.target) && !dropdownResultados.contains(e.target)) {
                    dropdownResultados.classList.add('d-none');
                }
            });
        }

        // 2. Limpiar búsqueda
        if (btnLimpiar) {
            btnLimpiar.addEventListener('click', function () {
                if (inputBuscar) inputBuscar.value = '';
                if (dropdownResultados) {
                    dropdownResultados.classList.add('d-none');
                    dropdownResultados.innerHTML = '';
                }
                txtNombre.value = '';
                txtApellidos.value = '';
                txtCurp.value = '';
                txtTelefono.value = '';
                txtEmail.value = '';
                ocultarError();
            });
        }

        // 3. Validación y avance
        btnContinuar.addEventListener('click', function () {
            ocultarError();

            const nombre = txtNombre ? txtNombre.value.trim() : '';
            const apellidos = txtApellidos ? txtApellidos.value.trim() : '';
            const curp = txtCurp ? txtCurp.value.trim().toUpperCase() : '';
            const telefono = txtTelefono ? txtTelefono.value.trim().replace(/\D/g, '') : '';
            const email = txtEmail ? txtEmail.value.trim() : '';

            if (!nombre || !apellidos) {
                mostrarError('Por favor completa el Nombre(s) y Apellidos del titular.');
                return;
            }

            if (telefono && telefono.length !== 10) {
                mostrarError('El teléfono debe tener exactamente 10 dígitos numéricos.');
                return;
            }

            if (email && (!email.includes('@') || !email.includes('.'))) {
                mostrarError('El correo electrónico no tiene un formato válido.');
                return;
            }

            guardarBorrador({
                nombre: nombre,
                apellidos: apellidos,
                curp: curp,
                telefono: telefono,
                email: email
            });

            window.dispatchEvent(new CustomEvent('sf:paso1-completado', {
                detail: { cliente: { nombre, apellidos, curp, telefono, email } }
            }));

            const draftBadge = document.getElementById('draftBadge');
            if (draftBadge) {
                draftBadge.classList.remove('d-none');
            }
        });

        function mostrarError(mensaje) {
            if (alertaValidacion) {
                alertaValidacion.textContent = mensaje;
                alertaValidacion.classList.remove('d-none');
            }
        }

        function ocultarError() {
            if (alertaValidacion) {
                alertaValidacion.classList.add('d-none');
                alertaValidacion.textContent = '';
            }
        }

        function guardarBorrador(cliente) {
            try {
                const borrador = JSON.parse(sessionStorage.getItem(DRAFT_KEY) || '{}');
                borrador.cliente = cliente;
                borrador.paso = 2;
                sessionStorage.setItem(DRAFT_KEY, JSON.stringify(borrador));
                window.dispatchEvent(new CustomEvent('sf:draft-updated', { detail: borrador }));
            } catch (err) {
                console.warn('No se pudo acceder a sessionStorage:', err);
            }
        }

        function restaurarBorrador() {
            try {
                const raw = sessionStorage.getItem(DRAFT_KEY);
                if (!raw) return;
                const borrador = JSON.parse(raw);
                if (borrador.cliente) {
                    if (txtNombre && borrador.cliente.nombre) txtNombre.value = borrador.cliente.nombre;
                    if (txtApellidos && borrador.cliente.apellidos) txtApellidos.value = borrador.cliente.apellidos;
                    if (txtCurp && borrador.cliente.curp) txtCurp.value = borrador.cliente.curp;
                    if (txtTelefono && borrador.cliente.telefono) txtTelefono.value = borrador.cliente.telefono;
                    if (txtEmail && borrador.cliente.email) txtEmail.value = borrador.cliente.email;
                }
            } catch (err) {
                console.warn('Error al leer sessionStorage:', err);
            }
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initPaso1);
    } else {
        initPaso1();
    }
})();
