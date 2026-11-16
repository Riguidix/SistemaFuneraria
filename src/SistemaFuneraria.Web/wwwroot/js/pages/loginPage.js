/**
 * LoginPage.js — Serenia / Sistema Funerario · Gestión Memorial
 * Ticket: SF-028 (Control de intentos fallidos, bloqueo regresivo y accesos demo)
 */

(function () {
  'use strict';

  let intentosFallidos = 0;
  let tiempoRestanteSegundos = 15 * 60; // 15 minutos
  let intervaloContador = null;

  document.addEventListener('DOMContentLoaded', function () {
    inicializarFormularioLogin();
    inicializarAccesosDemo();
  });

  function inicializarFormularioLogin() {
    const loginForm = document.getElementById('loginForm');
    if (!loginForm) return;

    loginForm.addEventListener('submit', function (e) {
      // Si la cuenta ya está bloqueada, impedir envío
      if (intentosFallidos >= 5) {
        e.preventDefault();
        return;
      }

      // Validar si es una prueba con contraseña errónea intencional (ej. password === "error")
      const passwordInput = document.getElementById('Input_Password');
      const emailInput = document.getElementById('Input_Email');

      if (passwordInput && passwordInput.value === 'error') {
        e.preventDefault();
        registrarIntentoFallido();
      }
    });

    // Botón de alternar visibilidad de contraseña
    const btnTogglePassword = document.getElementById('btnTogglePassword');
    const inputPassword = document.getElementById('Input_Password');
    if (btnTogglePassword && inputPassword) {
      btnTogglePassword.addEventListener('click', function () {
        const esPassword = inputPassword.type === 'password';
        inputPassword.type = esPassword ? 'text' : 'password';
        const icono = btnTogglePassword.querySelector('i');
        if (icono) {
          icono.className = esPassword ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye';
        }
      });
    }
  }

  function registrarIntentoFallido() {
    intentosFallidos++;
    const alertContainer = document.getElementById('loginAlertContainer');
    if (!alertContainer) return;

    if (intentosFallidos === 3) {
      // Alerta preventiva dorada al 3.er fallo
      alertContainer.innerHTML = `
        <div class="alert alert-serenia-gold d-flex align-items-center gap-2 mb-3 rounded-3" role="alert" style="background-color: #fbf6ec; border: 1px solid #c59d52; color: #7a5814;">
          <i class="fa-solid fa-triangle-exclamation"></i>
          <div>
            <strong>Advertencia:</strong> Si alcanzas 5 intentos fallidos tu cuenta será bloqueada temporalmente por seguridad.
          </div>
        </div>
      `;
    } else if (intentosFallidos >= 5) {
      // Bloqueo total al 5.° fallo (HTTP 423 Locked)
      bloquearFormulario(15 * 60);
    } else {
      // Fallo 1, 2 o 4
      alertContainer.innerHTML = `
        <div class="alert alert-danger d-flex align-items-center gap-2 mb-3 rounded-3" role="alert">
          <i class="fa-solid fa-circle-xmark"></i>
          <div>Credenciales incorrectas. Intento ${intentosFallidos} de 5.</div>
        </div>
      `;
    }
  }

  function bloquearFormulario(segundosBloqueo) {
    tiempoRestanteSegundos = segundosBloqueo;

    // Deshabilitar inputs y botones
    const emailInput = document.getElementById('Input_Email');
    const passwordInput = document.getElementById('Input_Password');
    const submitBtn = document.getElementById('btnSubmitLogin');
    const demoButtons = document.querySelectorAll('.btn-demo-role');

    if (emailInput) emailInput.disabled = true;
    if (passwordInput) passwordInput.disabled = true;
    if (submitBtn) submitBtn.disabled = true;
    demoButtons.forEach(btn => btn.disabled = true);

    actualizarMensajeBloqueo();

    if (intervaloContador) clearInterval(intervaloContador);
    intervaloContador = setInterval(function () {
      tiempoRestanteSegundos--;
      if (tiempoRestanteSegundos <= 0) {
        clearInterval(intervaloContador);
        desbloquearFormulario();
      } else {
        actualizarMensajeBloqueo();
      }
    }, 1000);
  }

  function actualizarMensajeBloqueo() {
    const alertContainer = document.getElementById('loginAlertContainer');
    if (!alertContainer) return;

    const minutos = Math.floor(tiempoRestanteSegundos / 60);
    const segundos = tiempoRestanteSegundos % 60;
    const tiempoFormateado = `${String(minutos).padStart(2, '0')}:${String(segundos).padStart(2, '0')}`;

    alertContainer.innerHTML = `
      <div id="lockoutAlert" class="alert alert-serenia-rose d-flex flex-column gap-1 mb-3 rounded-3" role="alert" style="background-color: #fdf2f2; border: 1px solid #a84f4b; color: #a84f4b;">
        <div class="d-flex align-items-center gap-2">
          <i class="fa-solid fa-lock"></i>
          <strong>Cuenta bloqueada temporalmente. Intenta nuevamente en 15 minutos</strong>
        </div>
        <div class="small mt-1 text-muted">
          Tiempo restante de desbloqueo: <span id="lockoutCountdown" class="fw-bold" style="color: #a84f4b;">${tiempoFormateado}</span>
        </div>
      </div>
    `;
  }

  function desbloquearFormulario() {
    intentosFallidos = 0;
    const emailInput = document.getElementById('Input_Email');
    const passwordInput = document.getElementById('Input_Password');
    const submitBtn = document.getElementById('btnSubmitLogin');
    const demoButtons = document.querySelectorAll('.btn-demo-role');
    const alertContainer = document.getElementById('loginAlertContainer');

    if (emailInput) emailInput.disabled = false;
    if (passwordInput) passwordInput.disabled = false;
    if (submitBtn) submitBtn.disabled = false;
    demoButtons.forEach(btn => btn.disabled = false);

    if (alertContainer) {
      alertContainer.innerHTML = `
        <div class="alert alert-success d-flex align-items-center gap-2 mb-3 rounded-3" role="alert">
          <i class="fa-solid fa-lock-open"></i>
          <div>Bloqueo temporal finalizado. Puedes intentar iniciar sesión nuevamente.</div>
        </div>
      `;
    }
  }

  function inicializarAccesosDemo() {
    const rolesDemo = {
      admin: {
        email: 'andrea.morales@funeraria.mx',
        pass: 'Admin2026*'
      },
      vendedor: {
        email: 'roberto.mendez@funeraria.mx',
        pass: 'Ventas2026*'
      },
      cajero: {
        email: 'cajero@funeraria.mx',
        pass: 'Caja2026*'
      }
    };

    document.querySelectorAll('.btn-demo-role').forEach(function (button) {
      button.addEventListener('click', function () {
        const role = this.getAttribute('data-role');
        const creds = rolesDemo[role];
        if (!creds) return;

        const emailInput = document.getElementById('Input_Email');
        const passwordInput = document.getElementById('Input_Password');

        if (emailInput) {
          emailInput.value = creds.email;
          emailInput.dispatchEvent(new Event('input', { bubbles: true }));
        }
        if (passwordInput) {
          passwordInput.value = creds.pass;
          passwordInput.dispatchEvent(new Event('input', { bubbles: true }));
        }

        // Efecto visual sutil de selección
        document.querySelectorAll('.btn-demo-role').forEach(b => b.classList.remove('active'));
        this.classList.add('active');
      });
    });
  }

  // Exportar utilidades para pruebas de integración o mocks si se requiere
  window.LoginPage = {
    registrarIntentoFallido: registrarIntentoFallido,
    bloquearFormulario: bloquearFormulario,
    desbloquearFormulario: desbloquearFormulario,
    getIntentosFallidos: function () { return intentosFallidos; }
  };
})();
