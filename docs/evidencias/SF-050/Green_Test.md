# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-050 |
| **Caso de Prueba Relacionado** | RF-01 / RNF-01 (Wrapper JS simuladorApi.js y Persistencia de Borrador en sessionStorage wizardState.js) |
| **Fecha y Hora de Ejecución** | 21/11/2026 23:56 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-050-web-simulador-api-wizard-state` |

---

### 1. Resumen de la Implementación
Se implementaron exitosamente los módulos del carril web para cotización financiera y persistencia de estado del Wizard (SF-050) con un blast radius acotado ($\le 180$ LOCs):
1. **Wrapper JS `simuladorApi.js` (`src/SistemaFuneraria.Web/wwwroot/js/api/simuladorApi.js`)**:
   - `obtenerPlanes(opts)`: consulta `/api/v1/simulador/planes` delegando en `apiClient.js`.
   - `cotizar(payload, opts)`: ejecuta `/api/v1/simulador/cotizar` con manejo de errores y soporte UMD (`window.simuladorApi` y `module.exports`).
2. **Gestor de Estado `wizardState.js` (`src/SistemaFuneraria.Web/wwwroot/js/ventas/wizardState.js`)**:
   - `STORAGE_KEY: "sf_nueva_venta_draft"`.
   - `getDraft()`: lee y parsea el borrador de `sessionStorage`.
   - `updateDraft(partial)`: aplica cambios incrementales, actualiza el indicador `"Borrador guardado"` y emite el evento personalizado `sf:draft-updated`.
   - `clearDraft()`: elimina inmediatamente la clave de `sessionStorage` y notifica a los suscriptores (`sf:draft-cleared`).
   - `syncDraftBadge(hasDraft)`: sincroniza el badge superior `#draftBadge`.
   - `syncSummaryPanel(draft)`: actualiza en tiempo real el panel lateral `"RESUMEN DE COTIZACIÓN"` (`#resumenLoteTitulo`, `#resumenLoteClave`, `#resumenLoteCapacidad`, `#resumenLotePrecio`, `#resumenTotalEstimado`).
3. **Integración en `Pages/Ventas/Nueva.cshtml`**:
   - Inclusión de `simuladorApi.js` y `wizardState.js` en la sección `@section Scripts`.

---

### 2. Comando de Terminal Ejecutado
```powershell
$env:PATH = "%DOTNET_HOME%;$env:PATH"; $env:DOTNET_ROOT = "%DOTNET_HOME%"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~SimuladorApiAndWizardStateTests" -v normal
```

---

### 3. Salida Real de Terminal (Fase GREEN)
```text
Test run for <repo>/tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.07]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.16]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.16]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.20]   Finished:    SistemaFuneraria.Tests
  Passed SF-050: simuladorApi.js debe existir físicamente en wwwroot/js/api/ [5 ms]
  Passed SF-050: wizardState.js debe sincronizar en vivo el panel derecho RESUMEN DE COTIZACIÓN [1 ms]
  Passed SF-050: Pages/Ventas/Nueva.cshtml debe incluir simuladorApi.js y wizardState.js [< 1 ms]
  Passed SF-050: wizardState.js debe existir físicamente en wwwroot/js/ventas/ [< 1 ms]
  Passed SF-050: simuladorApi.js debe exponer obtenerPlanes y cotizar delegando en apiClient [< 1 ms]
  Passed SF-050: wizardState.js debe usar la clave sf_nueva_venta_draft y exponer getDraft, updateDraft y clearDraft [< 1 ms]
  Passed SF-050 (BDD): wizardState.js debe sincronizar el pill 'Borrador guardado' (draftBadge) [< 1 ms]

Test Run Successful.
Total tests: 7
     Passed: 7
 Total time: 0.5738 Seconds
```

---

### 4. Prueba de Regresión Global
```text
Passed!  - Failed:     0, Passed:   561, Skipped:     0, Total:   561, Duration: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```
100% de la suite de pruebas unitarias exitosa sin regresiones en ninguna capa arquitectónica (561 pruebas pasadas).
