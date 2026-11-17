# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-032 |
| **Caso de Prueba Relacionado** | RF-02 / RNF-03 (Controlador REST ClientesController) |
| **Fecha y Hora de Ejecución** | 17/11/2026 10:44 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-032-api-clientes-crud-controller` |

---

### 1. Objetivo de la Prueba
Verificar la falla controlada inicial (`NotImplementedException: SF-032: Fase RED.`) en los endpoints REST de `ClientesController`:
1. `GET /api/v1/clientes`: consulta y búsqueda multicriterio con y sin filtros.
2. `GET /api/v1/clientes/{id}`: consulta por identificador y retorno 404 ante inexistencia.
3. `POST /api/v1/clientes`: registro de nuevo cliente con retorno 201 Created y ProblemDetails 400 ante invariantes inválidas.
4. `PUT /api/v1/clientes/{id}`: actualización con retorno 200 OK y ProblemDetails 404 ante inexistencia.
5. `DELETE /api/v1/clientes/{id}`: eliminación física de clientes sin contratos con retorno 200 OK / 204 NoContent, y 404 ante inexistencia.
6. `GET /api/v1/clientes/exportar`: generación de archivo `directorio-clientes.csv` en codificación UTF-8.

---

### 2. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClientesControllerTests"
```

---

### 3. Salida Real de Terminal (Fase RED)
```text
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.48]     SF-032: Eliminar cliente inexistente debe retornar 404 Not Found [FAIL]
[xUnit.net 00:00:00.97]     SF-032: ObtenerPorId con cliente existente debe retornar 200 OK [FAIL]
[xUnit.net 00:00:00.98]     SF-032: Registrar con datos válidos debe retornar 201 Created [FAIL]
[xUnit.net 00:00:01.01]     SF-032: ExportarDirectorio debe retornar archivo CSV UTF-8 con cabecera y datos [FAIL]
[xUnit.net 00:00:01.03]     SF-032 (BDD): Eliminar cliente sin contratos por Administrador debe retornar 200 OK o 204 NoContent [FAIL]
[xUnit.net 00:00:01.03]     SF-032: Buscar con filtro debe retornar 200 OK con clientes coincidentes [FAIL]
[xUnit.net 00:00:01.03]     SF-032 (BDD): Actualizar con datos válidos por Administrador debe retornar 200 OK [FAIL]
[xUnit.net 00:00:01.03]     SF-032: Registrar con datos inválidos debe retornar 400 Bad Request [FAIL]
  Failed SF-032: Eliminar cliente inexistente debe retornar 404 Not Found [111 ms]
  Error Message:
   System.NotImplementedException : SF-032: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.ClientesController.Eliminar(Guid id, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\ClientesController.cs:line 46
  Failed SF-032: ObtenerPorId con cliente existente debe retornar 200 OK [474 ms]
  Error Message:
   System.NotImplementedException : SF-032: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.ClientesController.ObtenerPorId(Guid id, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\ClientesController.cs:line 27
  Failed SF-032: Registrar con datos válidos debe retornar 201 Created [2 ms]
  Error Message:
   System.NotImplementedException : SF-032: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.ClientesController.Registrar(RegistrarClienteRequest request, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\ClientesController.cs:line 33
  Failed SF-032: ExportarDirectorio debe retornar archivo CSV UTF-8 con cabecera y datos [34 ms]
  Error Message:
   System.NotImplementedException : SF-032: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.ClientesController.ExportarDirectorio(CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\ClientesController.cs:line 52
  Failed SF-032 (BDD): Eliminar cliente sin contratos por Administrador debe retornar 200 OK o 204 NoContent [16 ms]
  Error Message:
   System.NotImplementedException : SF-032: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.ClientesController.Eliminar(Guid id, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\ClientesController.cs:line 46
  Failed SF-032: Buscar con filtro debe retornar 200 OK con clientes coincidentes [3 ms]
  Error Message:
   System.NotImplementedException : SF-032: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.ClientesController.Buscar(String filtro, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\ClientesController.cs:line 21
  Failed SF-032 (BDD): Actualizar con datos válidos por Administrador debe retornar 200 OK [1 ms]
  Error Message:
   System.NotImplementedException : SF-032: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.ClientesController.Actualizar(Guid id, ActualizarClienteRequest request, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\ClientesController.cs:line 39
  Failed SF-032: Registrar con datos inválidos debe retornar 400 Bad Request [< 1 ms]
  Error Message:
   System.NotImplementedException : SF-032: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.ClientesController.Registrar(RegistrarClienteRequest request, CancellationToken ct) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\ClientesController.cs:line 33
[xUnit.net 00:00:01.07]     SF-032: Buscar sin filtro debe retornar 200 OK con todos los clientes [FAIL]
[xUnit.net 00:00:01.08]     SF-032: Actualizar cliente inexistente debe retornar 404 Not Found [FAIL]
[xUnit.net 00:00:01.08]     SF-032: ObtenerPorId con cliente inexistente debe retornar 404 Not Found [FAIL]

Failed!  - Failed:    11, Passed:     2, Skipped:     0, Total:    13, Duration: 588 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Conclusión
Se certifica la fase RED: las pruebas de autorización RBAC (`Administrador,Vendedor` y `Administrador` exclusivo en `Eliminar`) fueron reconocidas y validadas, y los 11 métodos de acción fallaron con `NotImplementedException`.
