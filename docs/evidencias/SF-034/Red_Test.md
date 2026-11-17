# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-034 |
| **Título del Ticket** | Pantalla Figma /Clientes/Index (Captura 3): Buscador, botón Exportar directorio y tabla con avatares y badges |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 19:15 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-034-clientes-index |

---

### 1. Objetivo de la Prueba
Demostrar que la vista `/Clientes/Index` carecía de los elementos interactivos del Directorio de Clientes requeridos por Figma (*Captura 3*):
1. Ausencia del campo de búsqueda reactiva por nombre, teléfono o CURP (`#inputBuscarClientes`).
2. Ausencia del botón secundario institucional `Exportar directorio` (`#btnExportarDirectorio`).
3. Ausencia de la tabla paginada con avatares circulares de iniciales (`LC`, `RM`, `EV`, `JS`), contratos activos, saldo formateado en moneda mexicana, fecha de último pago y badges de estado (`Al corriente`, `Pago vencido`, `En revisión`).

---

### 2. Datos de Entrada Utilizados
* **Ruta evaluada:** `/Clientes`
* **Elementos DOM requeridos:**
  * `#inputBuscarClientes` con placeholder `"Buscar por nombre, teléfono o CURP"`
  * `#btnExportarDirectorio` con icono `fa-file-arrow-up`
  * `.filter-pills` con contadores dinámicos por estado (Todos, Al corriente, Pago vencido, En revisión)
  * `#tablaClientes` con columnas `CLIENTE`, `CONTACTO`, `CONTRATOS`, `SALDO ACTUAL`, `ÚLTIMO PAGO`, `ESTADO` y acción `Abrir >`.

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

[FAIL] SistemaFuneraria.Tests.Unit.Web.ClientesPageTests.Clientes_DebeRenderizarTablaBuscadorYBadges
Error Message:
 Expected element '#inputBuscarClientes' to exist in DOM, but found null.
 Expected element '#tablaClientes' with avatars 'LC, RM, EV, JS' to exist, but found 0 matching rows.
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at SistemaFuneraria.Tests.Unit.Web.ClientesPageTests.Clientes_DebeRenderizarTablaBuscadorYBadges()

Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 42 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló de forma determinista en fase **ROJO**:
* La vista `/Clientes/Index.cshtml` solo contenía el encabezado estático sin la tabla de datos, el buscador reactivo ni los badges de estado.
* Queda validada la necesidad técnica de implementar el marcado y la lógica reactiva en el frontend.
