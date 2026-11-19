# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-044 |
| **Título del Ticket** | Pantalla Figma /Lotes/Index (Captura 2): 3 KPI Cards, Buscador, Selectores de Sección/Estado y Tabla Paginada |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 20:15 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-044-catalogo-lotes-index |

---

### 1. Objetivo de la Prueba
Demostrar que la aplicación web no contaba con la vista del Catálogo de Lotes según las especificaciones de Figma (*Captura 2*):
1. Inexistencia de la ruta y PageModel `/Catalogo` (`Pages/Catalogo/Index.cshtml`).
2. Ausencia de las 3 KPI Cards operativas (`Disponibles: 64 de 178`, `Precio desde: $120,000`, `Sección con más espacio: Jardín Norte`).
3. Ausencia de los filtros combinados (`#inputBuscarLotes`, `#selectSeccion`, `#selectEstado`) y la tabla `#tablaLotes` con acciones contextuales (`Cotizar`, `Ver detalle`, `Ver contrato`).

---

### 2. Datos de Entrada Utilizados
* **Ruta evaluada:** `/Catalogo`
* **Elementos DOM requeridos:**
  * Kicker `"INVENTARIO"`, título `"Catálogo de lotes"` y botón disparador `"+ Cotizar lote"`.
  * KPI 1: Disponibles 64 / de 178 lotes.
  * KPI 2: Precio desde $120,000 / pago de contado.
  * KPI 3: Sección con más espacio / Jardín Norte (24 lotes libres).
  * Selectores `#selectSeccion` (`Jardín Norte`, `Jardín Central`, `Robles`, `Las Magnolias`) y `#selectEstado` (`Disponible`, `Reservado`, `Vendido`).
  * Tabla `#tablaLotes` con claves de lote (`JN-A-042`, `JC-C-118`, `RO-B-027`, `LM-A-014`).

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~CatalogoPageTests" --logger "console;verbosity=normal"
```

---

### 4. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

[FAIL] SistemaFuneraria.Tests.Unit.Web.CatalogoPageTests.Catalogo_DebeRenderizarKpiCardsYTablaLotes
Error Message:
 Expected route '/Catalogo' to return HTTP 200 OK, but found HTTP 404 NotFound.
 Missing PageModel: SistemaFuneraria.Web.Pages.Catalogo.IndexModel.
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at SistemaFuneraria.Tests.Unit.Web.CatalogoPageTests.Catalogo_DebeRenderizarKpiCardsYTablaLotes()

Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 37 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló deterministamente en fase **ROJO**:
* La vista `/Catalogo/Index.cshtml` no existía en el proyecto `SistemaFuneraria.Web`.
* Se confirma la necesidad técnica de maquetar la vista completa del catálogo de lotes con KPIs y tabla de inventario memorial.
