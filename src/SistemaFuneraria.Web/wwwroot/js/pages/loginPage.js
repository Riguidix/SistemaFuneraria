/**
 * LoginPage.js — Control de autenticación real contra SistemaFuneraria.Api (SF-028).
 */
(function () {
    "use strict";

    let intentosFallidos = 0;
    let tiempoRestanteSegundos = 15 * 60;
    let intervaloContador = null;

    document.addEventListener("DOMContentLoaded", function () {
        inicializarFormularioLogin();
        inicializarTogglePassword();
        inicializarAccesosDemo();
    });

    function inicializarFormularioLogin() {
        const loginForm = document.getElementById("loginForm");
        if (!loginForm) {
            return;
        }

        loginForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            if (intentosFallidos >= 5) {
                return;
            }

            const emailInput = document.getElementById("Input_Email");
            const passwordInput = document.getElementById("Input_Password");
            const submitBtn = document.getElementById("btnSubmitLogin");

            const correo = emailInput ? emailInput.value.trim() : "";
            const password = passwordInput ? passwordInput.value : "";

            if (!correo || !password) {
                mostrarAlerta("danger", "El correo electrónico y la contraseña son obligatorios.");
                return;
            }

            if (submitBtn) {
                submitBtn.disabled = true;
            }

            try {
                await window.authApi.login(correo, password);
                window.location.assign("/");
            } catch (err) {
                manejarErrorLogin(err);
            } finally {
                if (submitBtn && intentosFallidos < 5) {
                    submitBtn.disabled = false;
                }
            }
        });
    }

    function manejarErrorLogin(err) {
        const status = err?.status ?? 0;
        const payload = err?.payload ?? null;

        if (status === 423) {
            const minutos = payload?.minutosBloqueoRestantes ?? 15;
            intentosFallidos = 5;
            bloquearFormulario(minutos * 60);
            return;
        }

        if (status === 401) {
            intentosFallidos = payload?.intentosFallidos ?? (intentosFallidos + 1);
            if (payload?.advertenciaSeguridad) {
                mostrarAlertaAdvertencia(payload.advertenciaSeguridad);
            } else {
                mostrarAlerta("danger", `Credenciales incorrectas. Intento ${intentosFallidos} de 5.`);
            }
            return;
        }

        mostrarAlerta("danger", err?.message || "No se pudo conectar con el servidor de autenticación.");
    }

    function inicializarTogglePassword() {
        const btnTogglePassword = document.getElementById("btnTogglePassword");
        const inputPassword = document.getElementById("Input_Password");
        if (!btnTogglePassword || !inputPassword) {
            return;
        }

        btnTogglePassword.addEventListener("click", function () {
            const esPassword = inputPassword.type === "password";
            inputPassword.type = esPassword ? "text" : "password";
            const icono = btnTogglePassword.querySelector("i");
            if (icono) {
                icono.className = esPassword ? "fa-solid fa-eye-slash" : "fa-solid fa-eye";
            }
        });
    }

    function mostrarAlerta(tipo, mensaje) {
        const alertContainer = document.getElementById("loginAlertContainer");
        if (!alertContainer) {
            return;
        }

        alertContainer.innerHTML = "";
        const box = document.createElement("div");
        box.className = `alert alert-${tipo} d-flex align-items-center gap-2 mb-3 rounded-3`;
        box.setAttribute("role", "alert");

        const icon = document.createElement("i");
        icon.className = "fa-solid fa-circle-xmark";

        const text = document.createElement("div");
        text.textContent = mensaje;

        box.append(icon, text);
        alertContainer.appendChild(box);
    }

    function mostrarAlertaAdvertencia(mensaje) {
        const alertContainer = document.getElementById("loginAlertContainer");
        if (!alertContainer) {
            return;
        }

        alertContainer.innerHTML = "";
        const box = document.createElement("div");
        box.className = "alert alert-serenia-gold d-flex align-items-center gap-2 mb-3 rounded-3";
        box.setAttribute("role", "alert");
        box.style.backgroundColor = "#fbf6ec";
        box.style.border = "1px solid #c59d52";
        box.style.color = "#7a5814";

        const icon = document.createElement("i");
        icon.className = "fa-solid fa-triangle-exclamation";

        const text = document.createElement("div");
        text.textContent = mensaje;

        box.append(icon, text);
        alertContainer.appendChild(box);
    }

    function bloquearFormulario(segundosBloqueo) {
        tiempoRestanteSegundos = segundosBloqueo;

        const emailInput = document.getElementById("Input_Email");
        const passwordInput = document.getElementById("Input_Password");
        const submitBtn = document.getElementById("btnSubmitLogin");
        const demoButtons = document.querySelectorAll(".btn-demo-role");

        if (emailInput) { emailInput.disabled = true; }
        if (passwordInput) { passwordInput.disabled = true; }
        if (submitBtn) { submitBtn.disabled = true; }
        demoButtons.forEach((btn) => { btn.disabled = true; });

        actualizarMensajeBloqueo();

        if (intervaloContador) {
            clearInterval(intervaloContador);
        }
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
        const alertContainer = document.getElementById("loginAlertContainer");
        if (!alertContainer) {
            return;
        }

        const minutos = Math.floor(tiempoRestanteSegundos / 60);
        const segundos = tiempoRestanteSegundos % 60;
        const tiempoFormateado = `${String(minutos).padStart(2, "0")}:${String(segundos).padStart(2, "0")}`;

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
        const emailInput = document.getElementById("Input_Email");
        const passwordInput = document.getElementById("Input_Password");
        const submitBtn = document.getElementById("btnSubmitLogin");
        const demoButtons = document.querySelectorAll(".btn-demo-role");

        if (emailInput) { emailInput.disabled = false; }
        if (passwordInput) { passwordInput.disabled = false; }
        if (submitBtn) { submitBtn.disabled = false; }
        demoButtons.forEach((btn) => { btn.disabled = false; });

        mostrarAlerta("success", "Bloqueo temporal finalizado. Puedes intentar iniciar sesión nuevamente.");
    }

    function inicializarAccesosDemo() {
        const rolesDemo = {
            admin: {
                email: "andrea.morales@sistemafunerario.mx",
                pass: "Sistema Funerario2026!"
            },
            vendedor: {
                email: "vendedor@sistemafunerario.mx",
                pass: "Sistema Funerario2026!"
            },
            cajero: {
                email: "cajero@sistemafunerario.mx",
                pass: "Sistema Funerario2026!"
            }
        };

        document.querySelectorAll(".btn-demo-role").forEach(function (button) {
            button.addEventListener("click", function () {
                const role = this.getAttribute("data-role");
                const creds = rolesDemo[role];
                if (!creds) {
                    return;
                }

                const emailInput = document.getElementById("Input_Email");
                const passwordInput = document.getElementById("Input_Password");

                if (emailInput) {
                    emailInput.value = creds.email;
                    emailInput.dispatchEvent(new Event("input", { bubbles: true }));
                }
                if (passwordInput) {
                    passwordInput.value = creds.pass;
                    passwordInput.dispatchEvent(new Event("input", { bubbles: true }));
                }

                document.querySelectorAll(".btn-demo-role").forEach((b) => b.classList.remove("active"));
                this.classList.add("active");
            });
        });
    }

    window.LoginPage = {
        bloquearFormulario,
        desbloquearFormulario,
        getIntentosFallidos: () => intentosFallidos
    };
})();
