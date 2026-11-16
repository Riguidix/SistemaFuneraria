# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-023 |
| **Caso de Prueba Relacionado** | RNF-03 / ST-05 (Bloqueo Temporal al 5.º Intento & Jitter Anti-Enumeración) |
| **Fecha y Hora de Ejecución** | 15/11/2026 14:42 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-023-auth-login-lockout-jitter` |

---

### 1. Objetivo de la Prueba
Verificar que la máquina de estados de autenticación en `AuthController` cumpla todos los criterios de aceptación y DoD:
1. Retorno de `200 OK` con token y perfil completo (`NombreCompleto`, `Iniciales`, `Rol`, `Sucursal`) ante credenciales correctas.
2. Incremento de `AccessFailedCount` y retorno de `401 Unauthorized` ante intentos erróneos.
3. Advertencia de seguridad preventiva obligatoria al 3.er intento consecutivo.
4. Bloqueo temporal de 15 minutos (`LockoutEnd = UtcNow + 15m`) y código HTTP `423 Locked` al 5.º intento consecutivo (Escenario BDD Gherkin).
5. Respuesta `423 Locked` con cálculo de minutos restantes ante cuentas previamente bloqueadas.
6. Respuesta `401 Unauthorized` idéntica para usuarios inexistentes (mitigación anti-enumeración).
7. Endpoint `/olvide-contrasena` con respuesta genérica `200 OK` y retardo pseudoaleatorio (jitter anti-timing attack de 200ms a 600ms).
8. Endpoint `/me` con recuperación de perfil del usuario en sesión.

### 2. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~IniciarSesionUsuarioTests"
```

### 3. Salida Real de Terminal (Fase GREEN)
```text
  Determining projects to restore...
  All projects are up-to-date for restore.
  SistemaFuneraria.Core -> src\SistemaFuneraria.Core\bin\Debug\net8.0\SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> src\SistemaFuneraria.Application\bin\Debug\net8.0\SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> src\SistemaFuneraria.Infrastructure\bin\Debug\net8.0\SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> src\SistemaFuneraria.Web\bin\Debug\net8.0\SistemaFuneraria.Web.dll
  SistemaFuneraria.Api -> src\SistemaFuneraria.Api\bin\Debug\net8.0\SistemaFuneraria.Api.dll
  SistemaFuneraria.Tests -> tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     8, Skipped:     0, Total:     8, Duration: 773 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Verificación de Regresión (Suite No-Integración Completa)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   312, Skipped:     0, Total:   312, Duration: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Conclusión de Aceptación
Todos los criterios de aceptación de SF-023 y las políticas RNF-03 de seguridad quedaron plenamente satisfechos con 0 advertencias de compilación (`TreatWarningsAsErrors: true`).
