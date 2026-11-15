# Evidencia TDD - Fase RED: SF-028
## Pantalla Figma /Cuenta/Login y /OlvideContrasena: Alerta al 3.er Fallo, Contador de Bloqueo al 5.º y Redirección

- **Fecha:** 2026-11-14
- **Ticket:** SF-028
- **Milestone:** v0.2.0
- **Carril:** Carril 3 — Frontend & Figma UI (Juan Romero)
- **Estado:** RED 🔴 Confirmado

### 1. Definición de la Prueba Unitaria
Se estableció la suite de pruebas en `tests/SistemaFuneraria.Tests/Unit/Web/LoginPageTests.cs`:
* `Login_OnGet_DebeInicializarEstadoLimpio`: Verifica que el modelo inicie con 0 intentos fallidos, sin bloqueo y minutos restantes en 0.
* `Login_OnPost_ConModeloInvalido_DebeRetornarPaginaConErrores`: Verifica el manejo de errores de validación de correo/contraseña requeridos.
* `OlvideContrasena_OnPost_DebeMostrarMensajeUniformeSeguro`: Verifica que la recuperación de contraseña retorne siempre el mensaje uniforme contra enumeración:
  *"Si el correo ingresado se encuentra registrado, recibirás un enlace de restablecimiento en breve"*.

### 2. Evidencia de Ejecución RED
Al compilar y ejecutar las pruebas antes de la integración del ensamblado Web con los nuevos PageModels:
```text
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\LoginPageTests.cs(4,34): error CS0234: El tipo o el nombre del espacio de nombres 'Cuenta' no existe en el espacio de nombres 'SistemaFuneraria.Web.Pages' (¿falta alguna referencia de ensamblado?) [C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
Compilación fallida.
```
