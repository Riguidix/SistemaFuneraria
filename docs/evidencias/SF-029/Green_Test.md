# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-029 |
| **Caso de Prueba Relacionado** | RNF-03 / ST-05 (CRUD de Usuarios y Roles para Administrador) |
| **Fecha y Hora de Ejecución** | 15/11/2026 14:56 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-029-api-crud-usuarios-admin` |

---

### 1. Objetivo de la Prueba
Demostrar el pase exitoso de toda la suite de pruebas unitarias que validan el controlador `UsuariosAdminController` y el módulo cliente JavaScript `adminApi.js`:
1. `GET /api/v1/admin/usuarios` retorna `200 OK` con todos los usuarios del sistema, sus roles (`Administrador`, `Vendedor`, `Cajero`) y el indicador de cuenta bloqueada.
2. `POST /api/v1/admin/usuarios` da de alta nuevos usuarios en `UserManager`, asigna rol y claims de nombre/sucursal, retornando `201 Created`.
3. Rechazo estricto con `400 Bad Request` ante roles no autorizados o datos incompletos.
4. `PUT /api/v1/admin/usuarios/{id}` actualiza nombre, sucursal, rol y limpia el bloqueo temporal si se solicita (`Desbloquear: true`).
5. `DELETE /api/v1/admin/usuarios/{id}` rechaza con `400 Bad Request` si la administradora activa intenta eliminar su propia cuenta en sesión (prevención de auto-bloqueo).
6. `DELETE /api/v1/admin/usuarios/{id}` elimina a cualquier otro usuario operativo retornando `204 NoContent`.

### 2. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~UsuariosAdminControllerTests"
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

Passed!  - Failed:     0, Passed:     9, Skipped:     0, Total:     9, Duration: 517 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Verificación de Regresión (Suite No-Integración Completa)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   321, Skipped:     0, Total:   321, Duration: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Conclusión de Aceptación
Los 9 casos de prueba pasaron exitosamente. La solución compila con 0 advertencias (`TreatWarningsAsErrors: true`), respetando Clean Architecture y las políticas de seguridad RNF-03 de gobernanza administrativa.
