/**
 * LoginPage.js — Control de autenticación real contra SistemaFuneraria.Api (SF-028).
 */
(function (root, factory) {
    if (typeof module === "object" && module.exports) {
        module.exports = factory();
    } else {
        const api = factory();
        root.LoginPage = api;
        if (typeof document !== "undefined") {
            document.addEventListener("DOMContentLoaded", function () {
                api.init();
            });
        }
    }
})(typeof globalThis !== "undefined" ? globalThis : this, function () {
    "use strict";

    const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    let intentosFallidos = 0;
    let tiempoRestanteSegundos = 15 * 60;
    let intervaloContador = null;
    let mensajeBloqueoActual = "Cuenta bloqueada temporalmente. Intenta nuevamente en 15 minutos.";

    function init() {
        if (typeof window !== "undefined" && window.authApi) {
            const session = window.authApi.getSession();
            const token = window.authApi.getToken();
            if (session && token && !window.authApi.isTokenExpired(token)) {
                const returnUrl = new URLSearchParams(window.location.search).get("returnUrl");
                window.location.assign(window.authApi.resolvePostLoginUrl(session, returnUrl));
                return;
            }
        }

        inicializarFormularioLogin();
        inicializarTogglePassword();
        inicializarAccesosDemo();
    }

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
            const rememberCheck = document.getElementById("rememberMeCheck");
            const submitBtn = document.getElementById("btnSubmitLogin");

            const correo = emailInput ? emailInput.value.trim() : "";
            const password = passwordInput ? passwordInput.value : "";
            const rememberMe = Boolean(rememberCheck?.checked);

            if (!correo || !password) {
                mostrarAlerta("danger", "El correo electrónico y la contraseña son obligatorios.");
                return;
            }

            if (!EMAIL_REGEX.test(correo)) {
                mostrarAlerta("danger", "El formato del correo electrónico no es válido.");
                return;
            }

            const originalBtnHtml = submitBtn ? submitBtn.innerHTML : "";
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Accediendo...';
            }

            try {
                const response = await window.authApi.login(correo, password, { rememberMe });
                const returnUrl = new URLSearchParams(window.location.search).get("returnUrl");
                window.location.assign(window.authApi.resolvePostLoginUrl(response, returnUrl));
            } catch (err) {
                manejarErrorLogin(err);
            } finally {
                if (submitBtn) {
                    submitBtn.innerHTML = originalBtnHtml;
                    if (intentosFallidos < 5) {
                        submitBtn.disabled = false;
                    }
                }
            }
        });
    }

    function manejarErrorLogin(err) {
        const status = err?.status ?? 0;
        const payload = err?.payload ?? null;

        if (status === 423) {
            const minutos = payload?.minutosBloqueoRestantes ?? 15;
            const textoBloqueo = payload?.advertenciaSeguridad
                || `Cuenta bloqueada temporalmente. Intenta nuevamente en ${minutos} ${minutos === 1 ? "minuto" : "minutos"}.`;
            intentosFallidos = 5;
            bloquearFormulario(minutos * 60, textoBloqueo);
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
            btnTogglePassword.setAttribute("aria-pressed", esPassword ? "true" : "false");
            btnTogglePassword.setAttribute("aria-label", esPassword ? "Ocultar contraseña" : "Mostrar contraseña");
            const icono = btnTogglePassword.querySelector("i");
            if (icono) {
                icono.className = esPassword ? "fa-solid fa-eye-slash" : "fa-solid fa-eye";
            }
        });
    }

    function renderAlertBox(className, iconClass, mensaje) {
        const alertContainer = document.getElementById("loginAlertContainer");
        if (!alertContainer) {
            return;
        }

        alertContainer.innerHTML = "";
        const box = document.createElement("div");
        box.className = className;
        box.setAttribute("role", "alert");

        const icon = document.createElement("i");
        icon.className = iconClass;
        icon.setAttribute("aria-hidden", "true");

        const text = document.createElement("div");
        text.textContent = mensaje;

        box.append(icon, text);
        alertContainer.appendChild(box);
    }

    function mostrarAlerta(tipo, mensaje) {
        renderAlertBox(
            `alert alert-${tipo} d-flex align-items-center gap-2 mb-3 rounded-3`,
            "fa-solid fa-circle-xmark",
            mensaje);
    }

    function mostrarAlertaAdvertencia(mensaje) {
        renderAlertBox(
            "alert sf-alert-gold d-flex align-items-center gap-2 mb-3 rounded-3",
            "fa-solid fa-triangle-exclamation",
            mensaje);
    }

    function setFormularioDeshabilitado(deshabilitado) {
        const emailInput = document.getElementById("Input_Email");
        const passwordInput = document.getElementById("Input_Password");
        const submitBtn = document.getElementById("btnSubmitLogin");
        const demoButtons = document.querySelectorAll(".btn-demo-role");

        if (emailInput) emailInput.disabled = deshabilitado;
        if (passwordInput) passwordInput.disabled = deshabilitado;
        if (submitBtn) submitBtn.disabled = deshabilitado;
        demoButtons.forEach((btn) => { btn.disabled = deshabilitado; });
    }

    function bloquearFormulario(segundosBloqueo, mensajeBloqueo) {
        tiempoRestanteSegundos = segundosBloqueo;
        const minutosEstimados = Math.max(1, Math.ceil(segundosBloqueo / 60));
        mensajeBloqueoActual = mensajeBloqueo
            || `Cuenta bloqueada temporalmente. Intenta nuevamente en ${minutosEstimados} ${minutosEstimados === 1 ? "minuto" : "minutos"}.`;
        setFormularioDeshabilitado(true);
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

        let countdownEl = document.getElementById("lockoutCountdown");
        if (countdownEl) {
            countdownEl.textContent = tiempoFormateado;
            return;
        }

        alertContainer.innerHTML = "";
        const box = document.createElement("div");
        box.id = "lockoutAlert";
        box.className = "alert sf-alert-rose d-flex flex-column gap-1 mb-3 rounded-3";
        box.setAttribute("role", "alert");

        const header = document.createElement("div");
        header.className = "d-flex align-items-center gap-2";

        const icon = document.createElement("i");
        icon.className = "fa-solid fa-lock";
        icon.setAttribute("aria-hidden", "true");

        const strong = document.createElement("strong");
        strong.textContent = mensajeBloqueoActual;
        header.append(icon, strong);

        const sub = document.createElement("div");
        sub.className = "small mt-1 text-muted";
        sub.append(document.createTextNode("Tiempo restante de desbloqueo: "));

        countdownEl = document.createElement("span");
        countdownEl.id = "lockoutCountdown";
        countdownEl.className = "fw-bold sf-lockout-countdown";
        countdownEl.textContent = tiempoFormateado;
        sub.appendChild(countdownEl);

        box.append(header, sub);
        alertContainer.appendChild(box);
    }

    function desbloquearFormulario() {
        intentosFallidos = 0;
        setFormularioDeshabilitado(false);
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

    return {
        init,
        bloquearFormulario,
        desbloquearFormulario,
        getIntentosFallidos: () => intentosFallidos
    };
});
