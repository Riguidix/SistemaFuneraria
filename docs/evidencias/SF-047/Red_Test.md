# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-047 |
| **Título del Ticket** | Suite QA HTTP CP-01 (Filtros Catálogo de Lotes, Paginación, KPIs y Seguridad Admin) |
| **Caso de Prueba Relacionado** | CP-01 (RF-01, RN-01), Gate v0.4.0 |
| **Fecha de Ejecución** | 18/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-047-qa-http-cp01-catalogo-lotes` |

---

### 1. Objetivo de la Prueba
Certificar la fase ROJO para el Gate de Integración HTTP del Milestone `v0.4.0` (Catálogo de Lotes y CP-01):
1. Validar que la suite HTTP `LotesControllerTests` falla ante la ausencia del controlador `LotesController` (`/api/v1/lotes`).
2. Comprobar que las peticiones devuelven `404 Not Found` en lugar de `200 OK` (consulta de catálogo y filtros combinados) o `403 Forbidden` (bloqueo RBAC a rol Vendedor).

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter LotesControllerTests
```

---

### 3. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.76]     SF-047 (BDD): RBAC en creación de lotes: Vendedor 403 Forbidden y Administrador 201 Created [FAIL]
[xUnit.net 00:00:00.79]     SF-047 (BDD): Búsqueda sin coincidencias debe retornar 200 OK con lista vacía [FAIL]
[xUnit.net 00:00:00.82]     SF-047 / CP-01 (BDD): Filtro combinado de lotes disponibles por sección y gavetas [FAIL]
[xUnit.net 00:00:00.86]     SF-047 (BDD): Consulta del catálogo debe retornar los 3 KPIs de resumen calculados [FAIL]
  Con error SF-047 (BDD): RBAC en creación de lotes: Vendedor 403 Forbidden y Administrador 201 Created [648 ms]
  Mensaje de error:
   Expected resVendedor.StatusCode to be HttpStatusCode.Forbidden {value: 403}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.LotesControllerTests.SeguridadRbac_CrearLote_VendedorDebeRecibir403YAdminDebeCrear() in tests/SistemaFuneraria.Tests/Integration/Api/LotesControllerTests.cs:line 128

  Con error SF-047 (BDD): Búsqueda sin coincidencias debe retornar 200 OK con lista vacía [32 ms]
  Mensaje de error:
   Expected res.StatusCode to be HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.LotesControllerTests.BusquedaSinCoincidencias_DebeRetornar200OkConItemsVacio() in tests/SistemaFuneraria.Tests/Integration/Api/LotesControllerTests.cs:line 102

  Con error SF-047 / CP-01 (BDD): Filtro combinado de lotes disponibles por sección y gavetas [32 ms]
  Mensaje de error:
   Expected res.StatusCode to be HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.LotesControllerTests.CP01_FiltroCombinadoLotesDisponibles_DebeRetornarLotesConCriteriosExactos() in tests/SistemaFuneraria.Tests/Integration/Api/LotesControllerTests.cs:line 59

  Con error SF-047 (BDD): Consulta del catálogo debe retornar los 3 KPIs de resumen calculados [32 ms]
  Mensaje de error:
   Expected res.StatusCode to be HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.LotesControllerTests.ResumenKpis_DebeCalcularDisponiblesPrecioDesdeYSeccionConMasEspacio() in tests/SistemaFuneraria.Tests/Integration/Api/LotesControllerTests.cs:line 84

Con error! - Con error:     4, Superado:     0, Omitido:     0, Total:     4, Duración: 96 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Diagnóstico del Fallo (TDD RED)
* **Total ejecutadas:** 4 | **Fallidas:** 4 (100% fase roja confirmada).
* **Causa raíz:** Las rutas `/api/v1/lotes` no están expuestas en `SistemaFuneraria.Api` porque el controlador `LotesController` y la infraestructura del catálogo aún no se han implementado.
* **Siguiente paso:** Implementar el seeder inicial de lotes, el servicio de catálogo y `LotesController.cs` para pasar la suite a estado VERDE.
