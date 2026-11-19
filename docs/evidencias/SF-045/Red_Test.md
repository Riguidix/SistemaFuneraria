# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-045 |
| **Título del Ticket** | Modal Admin de Lotes (Crear / Editar Lote, Precio y Estado para Rol Administrador) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 20:35 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-045-modal-admin-lotes |

---

### 1. Objetivo de la Prueba
Demostrar la ausencia de la funcionalidad de gestión y edición de lotes para el rol `Administrador` en la vista de Catálogo de Lotes:
1. Inexistencia del botón de administración `+ Cotizar lote` / `+ Nuevo lote` accesible desde el catálogo.
2. Inexistencia del modal o interfaz para consultar precios de lista y condiciones de gavetas por sección.

---

### 2. Datos de Entrada Utilizados
* **Ruta evaluada:** `/Catalogo`
* **Elementos DOM requeridos:**
  * Controles de visualización y cotización de lotes para el perfil institucional (`Andrea Morales · Administradora`).
  * Desglose de precios de lista ($120,000, $150,000, $180,000) según capacidad de gavetas (2, 3, 4 gavetas).

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

[FAIL] SistemaFuneraria.Tests.Unit.Web.CatalogoPageTests.Catalogo_DebeProveerAccionCotizarYPreciosAdministrador
Error Message:
 Expected modal or trigger for lot cotization to be configured for Administrador, but found element not linked.
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at SistemaFuneraria.Tests.Unit.Web.CatalogoPageTests.Catalogo_DebeProveerAccionCotizarYPreciosAdministrador()

Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 35 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló deterministamente en fase **ROJO**:
* El catálogo no presentaba la interacción completa de cotización y edición vinculada con la barra de navegación del rol Administrador.
