# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-042 |
| **Caso de Prueba Relacionado** | CP-01 / RF-01 / RNF-03 (LotesController REST y RBAC Admin) |
| **Fecha y Hora de Ejecución** | 20/11/2026 17:32 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-042-api-lotes-controller` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la implementación funcional en `LotesController.cs` (`NotImplementedException: SF-042: Fase RED.`):
1. `GET /api/v1/lotes`: catálogo paginado con KPIs de cabecera.
2. `GET /api/v1/lotes/{id}`: consulta por identificador y retorno 404 ante inexistencia.
3. `POST /api/v1/lotes`: creación de lote con `201 Created` y `CreatedAtAction`, y `409 Conflict` ante clave duplicada (restringido a `Administrador`).
4. `PUT /api/v1/lotes/{id}`: actualización administrativa con retorno `200 OK` (restringido a `Administrador`).
5. `PATCH /api/v1/lotes/{id}/estado`: actualización del estado operativo (restringido a `Administrador`).
6. Verificación de metadatos de seguridad: `[Authorize]` en controlador y `[Authorize(Roles = "Administrador")]` en `POST`, `PUT` y `PATCH`.

---

### 2. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~LotesControllerTests"
```

---

### 3. Salida Real de Terminal (Fase RED)
```text
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:03.18]     SF-042: Crear lote con clave duplicada debe retornar 409 Conflict [FAIL]
[xUnit.net 00:00:03.19]     SF-042: Crear lote válido por Administrador debe retornar 201 Created [FAIL]
[xUnit.net 00:00:03.35]     SF-042: ObtenerPorId existente debe retornar 200 OK con DTO [FAIL]
  Failed SF-042: Crear lote con clave duplicada debe retornar 409 Conflict [1 s]
  Error Message:
   System.NotImplementedException : SF-042: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.LotesController.Crear(CrearLoteCommand command, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\LotesController.cs:line 56
  Failed SF-042: Crear lote válido por Administrador debe retornar 201 Created [5 ms]
  Error Message:
   System.NotImplementedException : SF-042: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.LotesController.Crear(CrearLoteCommand command, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\LotesController.cs:line 56
[xUnit.net 00:00:03.43]     SF-042: CambiarEstado operativo por Administrador debe retornar 200 OK [FAIL]
[xUnit.net 00:00:03.45]     SF-042: ObtenerCatalogo sin filtros debe retornar 200 OK con KPIs e items [FAIL]
[xUnit.net 00:00:03.45]     SF-042 (BDD): Actualizar lote existente por Administrador debe retornar 200 OK [FAIL]
[xUnit.net 00:00:03.46]     SF-042: ObtenerPorId inexistente debe retornar 404 Not Found [FAIL]
  Failed SF-042: ObtenerPorId existente debe retornar 200 OK con DTO [143 ms]
  Error Message:
   System.NotImplementedException : SF-042: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.LotesController.ObtenerPorId(Guid id, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\LotesController.cs:line 43
  Failed SF-042: CambiarEstado operativo por Administrador debe retornar 200 OK [77 ms]
  Error Message:
   System.NotImplementedException : SF-042: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.LotesController.CambiarEstado(Guid id, CambiarEstadoLoteRequest request, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\LotesController.cs:line 84
  Failed SF-042: ObtenerCatalogo sin filtros debe retornar 200 OK con KPIs e items [14 ms]
  Error Message:
   System.NotImplementedException : SF-042: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.LotesController.ObtenerCatalogo(String buscar, String seccion, String estado, Nullable`1 gavetas, Int32 page, Int32 pageSize, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\LotesController.cs:line 32
  Failed SF-042 (BDD): Actualizar lote existente por Administrador debe retornar 200 OK [4 ms]
  Error Message:
   System.NotImplementedException : SF-042: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.LotesController.Actualizar(Guid id, ActualizarLoteCommand command, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\LotesController.cs:line 70
  Failed SF-042: ObtenerPorId inexistente debe retornar 404 Not Found [1 ms]
  Error Message:
   System.NotImplementedException : SF-042: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.LotesController.ObtenerPorId(Guid id, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\LotesController.cs:line 43

Failed!  - Failed:     7, Passed:     4, Skipped:     0, Total:    11, Duration: 275 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Conclusión
Fase RED comprobada: 4 pruebas de configuración de atributos y permisos RBAC pasaron, mientras que los 7 endpoints de acción fallaron con `NotImplementedException`.
