# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-036 |
| **Título del Ticket** | Drawer de Expediente de Cliente al Pulsar Abrir en /Clientes + Botones CRUD (Editar / Eliminar) para Administrador |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 21:25 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-036-figma-drawer-expediente-cliente-admin |

---

### 1. Objetivo de la Prueba
Demostrar la ausencia de la acción reactiva al hacer clic en **`Abrir`** en la tabla de clientes de `/Clientes/Index` (*Captura 3*):
1. Inexistencia del enlace o disparador en la columna de acciones para abrir el expediente detallado del cliente.
2. Ausencia de la precarga de datos del titular (`CT-2025-089`, `Laura Castañeda`, contratos activos y saldo pendiente) al interactuar con la fila.

---

### 2. Datos de Entrada Utilizados
* **Ruta evaluada:** `/Clientes`
* **Elementos DOM requeridos:**
  * Enlaces `.btn-abrir-cobro` con atributos `data-folio`, `data-cliente` y `data-cuota`.
  * Integración con componente modal/drawer de expediente y cobro de cuotas.

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClientesPageTests" --logger "console;verbosity=normal"
```

---

### 4. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

[FAIL] SistemaFuneraria.Tests.Unit.Web.ClientesPageTests.Clientes_DebeContenerAccionAbrirExpedienteEnFilas
Error Message:
 Expected action link '.btn-abrir-cobro' to be present on each customer row, but found no listeners attached.
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at SistemaFuneraria.Tests.Unit.Web.ClientesPageTests.Clientes_DebeContenerAccionAbrirExpedienteEnFilas()

Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 34 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló deterministamente en fase **ROJO**:
* Los enlaces de acción `Abrir >` en la tabla de clientes no contaban con los atributos de datos ni los escuchadores de eventos para interactuar con el expediente del cliente.
