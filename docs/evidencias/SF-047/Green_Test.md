# Evidencia TDD: Fase VERDE (GREEN)

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
Certificar la fase VERDE para el Gate de Integración HTTP del Milestone `v0.4.0` (Catálogo de Lotes y CP-01):
1. Verificar que el caso de prueba `CP-01` se cumple vía HTTP: filtrado combinado por sección (`Jardín Norte`), estado (`Disponible`) y gavetas (`2`), retornando únicamente lotes conformes con precio de lista oficial de $120,000.00.
2. Comprobar que el endpoint `/api/v1/lotes` calcula y retorna correctamente los 3 KPIs de resumen (`TotalDisponibles`, `PrecioDesde` y `SeccionMasEspacio`).
3. Validar el caso borde: búsqueda sin coincidencias devuelve HTTP `200 OK` con colección vacía de items (sin errores 500 ni 404).
4. Validar el control de acceso basado en roles (RBAC): `POST /api/v1/lotes` responde `403 Forbidden` a usuarios con rol `Vendedor` y `201 Created` para `Administrador`.

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter LotesControllerTests
```

---

### 3. Salida de Terminal (Pase Exitoso)
```text
Determinando los proyectos que se van a restaurar...
Todos los proyectos están actualizados para la restauración.
SistemaFuneraria.Core -> src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
SistemaFuneraria.Application -> src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
SistemaFuneraria.Infrastructure -> src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
SistemaFuneraria.Api -> src/SistemaFuneraria.Api/bin/Debug/net8.0/SistemaFuneraria.Api.dll
SistemaFuneraria.Web -> src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
SistemaFuneraria.Tests -> tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll
Serie de pruebas para tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.2 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:     4, Omitido:     0, Total:     4, Duración: 106 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Certificación del Gate Milestone v0.4.0
* **Total ejecutadas:** 4 | **Superadas:** 4 | **Fallidas:** 0 (100% de éxito).
* **Cumplimiento BDD / CP-01:**
  * `CP01_FiltroCombinadoLotesDisponibles_DebeRetornarLotesConCriteriosExactos`: **Superada** (`200 OK`, lotes disponibles en Jardín Norte de 2 gavetas a $120,000.00).
  * `ResumenKpis_DebeCalcularDisponiblesPrecioDesdeYSeccionConMasEspacio`: **Superada** (`TotalDisponibles > 0`, `PrecioDesde = $120,000.00`, `SeccionMasEspacio = "Jardín Norte"`).
  * `BusquedaSinCoincidencias_DebeRetornar200OkConItemsVacio`: **Superada** (`200 OK` con items vacío).
  * `SeguridadRbac_CrearLote_VendedorDebeRecibir403YAdminDebeCrear`: **Superada** (`403 Forbidden` a Vendedor y `201 Created` a Administrador).
* **Registro de ejecución:** Actualizado formalmente en `docs/05-registro-ejecucion.md`.
