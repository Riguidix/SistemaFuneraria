# Evidencia TDD: Fase VERDE (GREEN)

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
Certificar la fase VERDE para el Gate de Integración HTTP del Milestone `v0.3.0`:
1. Verificar que el ciclo CRUD completo de `/api/v1/clientes` funciona de extremo a extremo: alta de cliente (*Laura Castañeda*), búsqueda reactiva por filtro, actualización de teléfono y exportación del directorio en formato CSV UTF-8.
2. Comprobar que las validaciones de frontera BVA rechazan teléfonos menores a 10 dígitos (9 dígitos) o mayores (11 dígitos) y colisiones de CURP con `400 Bad Request`.
3. Validar el control de acceso basado en roles (RBAC): `DELETE /api/v1/clientes/{id}` es denegado con `403 Forbidden` para rol `Vendedor` y permitido para `Administrador`, verificando que el cliente eliminado retorne `404 Not Found` en consultas subsecuentes.

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter ClientesCrudHttpTests
```

---

### 3. Salida de Terminal (Pase Exitoso)
```text
Determinando los proyectos que se van a restaurar...
Todos los proyectos están actualizados para la restauración.
SistemaFuneraria.Core -> src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
SistemaFuneraria.Application -> src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
SistemaFuneraria.Infrastructure -> src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
SistemaFuneraria.Web -> src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
SistemaFuneraria.Api -> src/SistemaFuneraria.Api/bin/Debug/net8.0/SistemaFuneraria.Api.dll
SistemaFuneraria.Tests -> tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll
Serie de pruebas para tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.2 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:     3, Omitido:     0, Total:     3, Duración: 106 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Certificación del Gate Milestone v0.3.0
* **Total ejecutadas:** 3 | **Superadas:** 3 | **Fallidas:** 0 (100% de éxito).
* **Cumplimiento BDD:**
  * `CicloCompleto_CrudClientes_DebeCrearBuscarActualizarYExportar`: **Superada** (`200 OK`, persistencia, búsqueda y exportación CSV).
  * `ValidacionBva_TelefonoYCurp_DebeRetornar400BadRequest`: **Superada** (rechazo `400` ante teléfonos de 9 y 11 dígitos y duplicidad de CURP).
  * `SeguridadRbac_EliminarCliente_VendedorDebeRecibir403YAdminDebeEliminar`: **Superada** (`403 Forbidden` para Vendedor, `200 OK` para Administrador y `404` post-eliminación).
* **Registro de ejecución:** Actualizado formalmente en `docs/05-registro-ejecucion.md`.
