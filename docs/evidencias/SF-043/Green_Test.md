# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-043 |
| **Caso de Prueba Relacionado** | RF-01 / RNF-01 (Wrapper JS lotesApi.js y Formateadores es-MX) |
| **Fecha y Hora de Ejecución** | 20/11/2026 17:40 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-043-web-lotes-js-wrapper` |

---

### 1. Resumen de la Implementación
Se implementó exitosamente el módulo wrapper `src/SistemaFuneraria.Web/wwwroot/js/api/lotesApi.js` con un blast radius acotado de 117 LOCs ($\le 140$ LOCs permitidas), cumpliendo estrictamente con la especificación técnica de `SF-043`:
1. **Contrato de API**:
   - `obtenerCatalogo(filtros, opts)`: sanitiza parámetros omitiendo valores en blanco y comodines de selección (`"todas"`, `"todas las secciones"`, `"todos"`, `"todos los estados"`).
   - `obtenerLotePorId(id, opts)`: `GET /api/v1/lotes/{id}`.
   - `crearLote(payload, opts)`: `POST /api/v1/lotes`.
   - `actualizarLote(id, payload, opts)`: `PUT /api/v1/lotes/{id}`.
   - `cambiarEstadoLote(id, estado, opts)`: `PATCH /api/v1/lotes/{id}/estado`.
2. **Helpers y Formateadores UI (Cultura `es-MX` y Figma Captura 2)**:
   - `formatearMonedaMXN(monto)`: utiliza `Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' })` formateando valores a `$120,000.00`.
   - `obtenerMetadatosEstadoLote(estado)` y `renderizarBadgeEstadoLote(estado)`:
     - `Disponible` / `1` $\rightarrow$ clase `badge-sf-success`, texto `"Disponible"`, acción `"Cotizar"`.
     - `Reservado` / `2` $\rightarrow$ clase `badge-sf-warning`, texto `"Reservado"`, acción `"Ver detalle"`.
     - `Vendido` / `3` $\rightarrow$ clase `badge-sf-neutral`, texto `"Vendido"`, acción `"Ver contrato"`.
3. **Compatibilidad UMD**: Exposición simultánea para entorno de navegador (`window.lotesApi`) y entorno modular/Node (`module.exports`).
4. **Delegación HTTP**: Consumo agnóstico y desacoplado mediante resolución de cliente `apiClient` con soporte para inyección de mocks en pruebas.

---

### 2. Comando de Terminal Ejecutado
```powershell
$env:PATH = "<user-home>\AppData\Local\Microsoft\dotnet;$env:PATH"; $env:DOTNET_ROOT = "<user-home>\AppData\Local\Microsoft\dotnet"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~LotesApiWrapperTests" -v normal
```

---

### 3. Salida Real de Terminal (Fase GREEN)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.05]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.13]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.14]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.17]   Finished:    SistemaFuneraria.Tests
  Passed SF-043: renderizarBadgeEstadoLote debe mapear Disponible, Reservado y Vendido según Figma Captura 2 [4 ms]
  Passed SF-043: lotesApi.js debe soportar UMD (window.lotesApi y module.exports) [< 1 ms]
  Passed SF-043: lotesApi.js debe delegar las operaciones HTTP en apiClient [< 1 ms]
  Passed SF-043: lotesApi.js debe existir físicamente en wwwroot/js/api/ [< 1 ms]
  Passed SF-043 (BDD): formatearMonedaMXN debe formatear en cultura es-MX ($120,000.00) [< 1 ms]
  Passed SF-043: lotesApi.js debe utilizar la ruta base /api/v1/lotes [< 1 ms]
  Passed SF-043: lotesApi.js debe exponer obtenerCatalogo, obtenerLotePorId, crearLote, actualizarLote y cambiarEstadoLote [< 1 ms]

Test Run Successful.
Total tests: 7
     Passed: 7
 Total time: 0.5041 Seconds
```

---

### 4. Prueba de Regresión Global
```text
Passed!  - Failed:     0, Passed:   441, Skipped:     0, Total:   441, Duration: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```
Toda la solución mantiene 100% de pruebas unitarias exitosas (441 pruebas pasadas).
