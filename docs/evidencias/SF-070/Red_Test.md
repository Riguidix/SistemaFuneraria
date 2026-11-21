# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-070 |
| **Título del Ticket** | Pantalla Figma /Contratos/Index (Captura 9: 4 KPI Cards, Buscador, 4 Tabs Todos/Activos/Vencidos/Liquidados y Tabla de Cartera) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 15/11/2026 00:20 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-070-contratos-cartera-index |

---

### 1. Objetivo de la Prueba
Demostrar que la aplicación web carecía de la vista de Cartera de Contratos requerida por la *Captura 9* de Figma:
1. Inexistencia de la ruta y PageModel `/Contratos` (`Pages/Contratos/Index.cshtml` y `.cs`).
2. Ausencia de las 4 KPI Cards de cartera:
   * **Activos:** `128` (Total vigentes en cartera).
   * **Al corriente:** `120` (Sin atraso de cuotas) con badge `93.7%`.
   * **Con atraso:** `8` (Mensualidades vencidas) con badge `Requieren atención`.
   * **Liquidados este año:** `32` (Cierre total de amortización) con badge `Meta 2025`.
3. Ausencia del buscador reactivo y el control segmentado de 4 pestañas (`Todos`, `Activos`, `Vencidos`, `Liquidados`).
4. Ausencia de la tabla de cartera con acción directa **`Cobrar`** en filas vencidas (ej. *Elena Vargas*, `CTR-2024-0118`) y botón **`Detalle`** en contratos al corriente.

---

### 2. Datos de Entrada Utilizados
* **Ruta evaluada:** `/Contratos`
* **Elementos DOM requeridos:**
  * Kicker `"CARTERA"`, título `"Contratos"` y botón primario `"+ Nueva venta"`.
  * 4 KPI Cards con métricas institucionales de cartera vigentes.
  * Input `#inputBuscarContratos` (`placeholder="Buscar contrato o cliente..."`).
  * Tabs de filtrado con clases `.tab-cartera` (`Todos`, `Activos`, `Vencidos`, `Liquidados`).
  * Tabla `#tablaContratos` con columnas `CONTRATO`, `CLIENTE`, `LOTE`, `SALDO`, `PRÓXIMO PAGO`, `ESTADO`, `ACCIÓN`.

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ContratosPageTests" --logger "console;verbosity=normal"
```

---

### 4. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

[FAIL] SistemaFuneraria.Tests.Unit.Web.ContratosPageTests.Contratos_OnGet_DebeInicializarKpisYListaContratosFigma
Error Message:
 Expected route '/Contratos' to return HTTP 200 OK, but found HTTP 404 NotFound.
 Missing PageModel: SistemaFuneraria.Web.Pages.Contratos.IndexModel.
 Missing 4 KPI Cards: Activos (128), Al corriente (120), Con atraso (8), Liquidados (32).
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at SistemaFuneraria.Tests.Unit.Web.ContratosPageTests.Contratos_OnGet_DebeInicializarKpisYListaContratosFigma()

Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 38 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló deterministamente en fase **ROJO**:
* No existía la página `Contratos/Index.cshtml` ni su modelo en el ensamble Web.
* Se valida la necesidad técnica de maquetar el módulo de seguimiento de cartera con KPIs y tabla de contratos según la *Captura 9*.
