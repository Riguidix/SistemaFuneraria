# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-023 |
| **Caso de Prueba Relacionado** | RNF-03 / ST-05 (Bloqueo Temporal al 5.º Intento & Jitter Anti-Enumeración) |
| **Fecha y Hora de Ejecución** | 15/11/2026 14:39 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-023-auth-login-lockout-jitter` |

---

### 1. Objetivo de la Prueba
Demostrar la falla inicial de las pruebas unitarias que validan la máquina de estados de autenticación en `AuthController`, el incremento de intentos fallidos, la advertencia preventiva al 3.er intento, el bloqueo temporal de 15 minutos (HTTP 423 Locked) al 5.º intento consecutivo y el jitter anti-enumeración en `/olvide-contrasena`.

### 2. Datos de Entrada Utilizados
* Peticiones `LoginRequest("andrea.morales@sistemafunerario.mx", "Password123!")` para éxito.
* Peticiones `LoginRequest("andrea.morales@sistemafunerario.mx", "PasswordIncorrecta")` con 1, 3 y 5 intentos consecutivos.
* Petición a cuenta con `LockoutEnd = UtcNow + 10m`.
* Petición `OlvideContrasenaRequest("cualquiera@sistemafunerario.mx")`.
* Contexto autenticado con claims para `GET /api/v1/auth/me`.

### 3. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~IniciarSesionUsuarioTests"
```

### 4. Salida Real de Terminal (Falla Esperada)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:03.59]     SF-023 (BDD): Quinto intento fallido consecutivo debe bloquear la cuenta por 15 minutos y retornar 423 Locked [FAIL]
[xUnit.net 00:00:03.66]     SF-023: Me debe retornar perfil del usuario autenticado en sesión [FAIL]
[xUnit.net 00:00:03.71]     SF-023: Login con contraseña incorrecta en 1er intento debe retornar 401 Unauthorized [FAIL]
  Failed SF-023 (BDD): Quinto intento fallido consecutivo debe bloquear la cuenta por 15 minutos y retornar 423 Locked [1 s]
  Error Message:
   System.NotImplementedException : SF-023: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.AuthController.Login(LoginRequest request) in src\SistemaFuneraria.Api\Controllers\V1\AuthController.cs:line 20
   at SistemaFuneraria.Tests.Unit.Api.IniciarSesionUsuarioTests.Login_ConQuintoIntentoFallido_DebeBloquearCuentaYRetornar423Locked() in tests\SistemaFuneraria.Tests\Unit\Api\IniciarSesionUsuarioTests.cs:line 188
  Failed SF-023: Login con 3 intentos fallidos debe retornar 401 Unauthorized y advertencia preventiva [42 ms]
  Error Message:
   System.NotImplementedException : SF-023: Fase RED.
  Failed SF-023: Usuario bloqueado que intenta login debe recibir 423 Locked con minutos restantes [43 ms]
  Error Message:
   System.NotImplementedException : SF-023: Fase RED.
  Failed SF-023: OlvideContrasena debe responder 200 OK con jitter anti-enumeración entre 200ms y 600ms [1 ms]
  Error Message:
   System.NotImplementedException : SF-023: Fase RED.
  Failed SF-023: Login con credenciales válidas debe retornar 200 OK con token y perfil [42 ms]
  Error Message:
   System.NotImplementedException : SF-023: Fase RED.

Failed!  - Failed:     8, Passed:     0, Skipped:     0, Total:     8, Duration: 250 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Diagnóstico Técnico
Los endpoints de `AuthController` no tienen implementada la lógica de verificación de contraseñas de Identity, gestión de `AccessFailedCount`, cálculo de `LockoutEnd`, retardo pseudoaleatorio (jitter) ni consulta de perfil. Se procede a la fase GREEN implementando la lógica en `src/SistemaFuneraria.Api/Controllers/V1/AuthController.cs`.
