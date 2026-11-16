# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-029 |
| **Caso de Prueba Relacionado** | RNF-03 / ST-05 (CRUD de Usuarios y Roles para Administrador) |
| **Fecha y Hora de Ejecución** | 15/11/2026 14:53 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-029-api-crud-usuarios-admin` |

---

### 1. Objetivo de la Prueba
Demostrar la falla inicial de las pruebas unitarias que verifican los endpoints de administración en `UsuariosAdminController` (`GET`, `POST`, `PUT /{id}`, `DELETE /{id}` en `/api/v1/admin/usuarios`):
1. Listado de usuarios con roles (`Administrador`, `Vendedor`, `Cajero`) y estado de bloqueo.
2. Alta de nuevos usuarios operativos con validación de roles permitidos.
3. Actualización de datos, cambio de rol y desbloqueo de cuenta.
4. Prevención estricta de auto-eliminación de la cuenta administradora en sesión (`400 Bad Request`).
5. Eliminación de otros usuarios operativos con respuesta `204 NoContent`.

### 2. Datos de Entrada Utilizados
* Listado con usuarios `Administrador` y `Vendedor` (uno bloqueado con `LockoutEnd`).
* Petición de alta: `CrearUsuarioRequest("Luis Torres", "cajero@sistemafunerario.mx", "Password123!", "Cajero", "Jardines del Valle")`.
* Peticiones con roles inválidos (`"SuperAdmin"`, `"Invitado"`, `""`).
* Petición de actualización: `ActualizarUsuarioRequest("Carlos Vega Actualizado", "Cajero", "Sucursal Norte", Desbloquear: true)`.
* Petición de auto-eliminación con ID de administradora en sesión y eliminación de tercero.

### 3. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~UsuariosAdminControllerTests"
```

### 4. Salida Real de Terminal (Fase RED)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:01.48]     SF-029: Listar usuarios debe retornar 200 OK con usuarios, roles y estado de bloqueo [FAIL]
[xUnit.net 00:00:01.54]     SF-029: Crear usuario con rol inválido debe retornar 400 Bad Request(rolInvalido: "SuperAdmin") [FAIL]
[xUnit.net 00:00:01.58]     SF-029: Crear usuario con rol inválido debe retornar 400 Bad Request(rolInvalido: "Invitado") [FAIL]
[xUnit.net 00:00:01.63]     SF-029: Crear usuario con rol inválido debe retornar 400 Bad Request(rolInvalido: "") [FAIL]
[xUnit.net 00:00:01.71]     SF-029: Eliminar otro usuario operativo debe removerlo de Identity y retornar 204 NoContent [FAIL]
[xUnit.net 00:00:01.75]     SF-029 (BDD): Crear usuario con datos válidos debe retornar 201 Created y persistir [FAIL]
[xUnit.net 00:00:01.84]     SF-029: Actualizar usuario debe modificar rol, datos y desbloquear cuenta si se solicita [FAIL]
[xUnit.net 00:00:01.88]     SF-029: Administradora no puede eliminarse a sí misma en sesión (400 Bad Request) [FAIL]
[xUnit.net 00:00:01.92]     SF-029: Eliminar usuario no existente debe retornar 404 Not Found [FAIL]
  Failed SF-029: Listar usuarios debe retornar 200 OK con usuarios, roles y estado de bloqueo [878 ms]
  Error Message:
   System.NotImplementedException : SF-029: Fase RED.
  Failed SF-029 (BDD): Crear usuario con datos válidos debe retornar 201 Created y persistir [43 ms]
  Error Message:
   System.NotImplementedException : SF-029: Fase RED.

Failed!  - Failed:     9, Passed:     0, Skipped:     0, Total:     9, Duration: 445 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Diagnóstico Técnico
`UsuariosAdminController` está en estado inicial (stub arrojando `NotImplementedException`). Se procede a implementar la lógica completa en la fase GREEN, integrando `UserManager<IdentityUser>`, `RoleManager<IdentityRole>`, claims de perfil y el cliente JavaScript `adminApi.js`.
