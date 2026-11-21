# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-038 |
| **Título del Ticket** | QA Gate v0.3.0: Suite HTTP para CRUD Completo de /api/v1/clientes (Permisos Admin y BVA) + Tag v0.3.0 |
| **Caso de Prueba Relacionado** | RF-02 (CRUD Clientes), RNF-03 (Seguridad y RBAC), Gate v0.3.0 |
| **Fecha de Ejecución** | 17/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-038-qa-gate-v030-clientes-crud-http` |

---

### 1. Objetivo de la Prueba
Certificar la fase ROJO para el Gate de Integración HTTP del Milestone `v0.3.0`:
1. Validar que la suite HTTP `ClientesCrudHttpTests` falla ante la inexistencia o falta de implementación del controlador `ClientesController` (`/api/v1/clientes`).
2. Comprobar que las peticiones devuelven `404 Not Found` en lugar de `200 OK` / `400 BadRequest` / `403 Forbidden`.

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter ClientesCrudHttpTests
```

---

### 3. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.75]     SF-038 (BDD): Ciclo completo CRUD de clientes y exportación CSV [FAIL]
[xUnit.net 00:00:00.79]     SF-038 (BDD): Validación BVA de teléfono no conforme y CURP duplicada [FAIL]
[xUnit.net 00:00:00.85]     SF-038 (BDD): RBAC en DELETE /api/v1/clientes: Vendedor 403 Forbidden y Administrador permitido [FAIL]
  Con error SF-038 (BDD): Ciclo completo CRUD de clientes y exportación CSV [642 ms]
  Mensaje de error:
   Expected postRes.StatusCode to be HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.ClientesCrudHttpTests.CicloCompleto_CrudClientes_DebeCrearBuscarActualizarYExportar() in tests/SistemaFuneraria.Tests/Integration/Api/ClientesCrudHttpTests.cs:line 68

  Con error SF-038 (BDD): Validación BVA de teléfono no conforme y CURP duplicada [36 ms]
  Mensaje de error:
   Expected resCorto.StatusCode to be HttpStatusCode.BadRequest {value: 400}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.ClientesCrudHttpTests.ValidacionBva_TelefonoYCurp_DebeRetornar400BadRequest() in tests/SistemaFuneraria.Tests/Integration/Api/ClientesCrudHttpTests.cs:line 113

  Con error SF-038 (BDD): RBAC en DELETE /api/v1/clientes: Vendedor 403 Forbidden y Administrador permitido [64 ms]
  Mensaje de error:
   Expected postRes.StatusCode to be HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.ClientesCrudHttpTests.SeguridadRbac_EliminarCliente_VendedorDebeRecibir403YAdminDebeEliminar() in tests/SistemaFuneraria.Tests/Integration/Api/ClientesCrudHttpTests.cs:line 148

Con error! - Con error:     3, Superado:     0, Omitido:     0, Total:     3, Duración: 100 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Diagnóstico del Fallo (TDD RED)
* **Total ejecutadas:** 3 | **Fallidas:** 3 (100% fase roja confirmada).
* **Causa raíz:** Las rutas `/api/v1/clientes` no están expuestas en `SistemaFuneraria.Api` porque el controlador `ClientesController` aún no se ha integrado.
* **Siguiente paso:** Implementar el controlador y la lógica de soporte en la API para pasar la suite a estado VERDE.
