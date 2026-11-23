# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-043 |
| **Caso de Prueba Relacionado** | RF-01 / RNF-01 (Wrapper JS lotesApi.js y Formateadores es-MX) |
| **Fecha y Hora de Ejecución** | 20/11/2026 17:37 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-043-web-lotes-js-wrapper` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la implementación de `src/SistemaFuneraria.Web/wwwroot/js/api/lotesApi.js`, verificando que la suite unitaria detecta la ausencia del archivo y los contratos:
1. Existencia física en `wwwroot/js/api/lotesApi.js`.
2. Métodos del contrato: `obtenerCatalogo`, `obtenerLotePorId`, `crearLote`, `actualizarLote` y `cambiarEstadoLote`.
3. Consumo de los endpoints `/api/v1/lotes` y `/estado`.
4. Soporte UMD (`window.lotesApi` y `module.exports`).
5. Delegación en `apiClient` (`resolveClient` con verbos `get`, `post`, `put`, `patch`).
6. Escenario BDD: Formateo de moneda consistente en cultura `es-MX` con `Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' })`.
7. Renderizado exacto de badges de estado del mockup (Figma Captura 2: `Disponible` con `badge-sf-success` y acción `"Cotizar"`, `Reservado` con `badge-sf-warning` y acción `"Ver detalle"`, `Vendido` con `badge-sf-neutral` y acción `"Ver contrato"`).

---

### 2. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~LotesApiWrapperTests"
```

---

### 3. Salida Real de Terminal (Fase RED)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.93]     SF-043: renderizarBadgeEstadoLote debe mapear Disponible, Reservado y Vendido según Figma Captura 2 [FAIL]
[xUnit.net 00:00:00.94]     SF-043: lotesApi.js debe soportar UMD (window.lotesApi y module.exports) [FAIL]
[xUnit.net 00:00:00.94]     SF-043: lotesApi.js debe delegar las operaciones HTTP en apiClient [FAIL]
[xUnit.net 00:00:00.94]     SF-043: lotesApi.js debe existir físicamente en wwwroot/js/api/ [FAIL]
[xUnit.net 00:00:00.94]     SF-043 (BDD): formatearMonedaMXN debe formatear en cultura es-MX ($120,000.00) [FAIL]
[xUnit.net 00:00:00.94]     SF-043: lotesApi.js debe utilizar la ruta base /api/v1/lotes [FAIL]
[xUnit.net 00:00:00.94]     SF-043: lotesApi.js debe exponer obtenerCatalogo, obtenerLotePorId, crearLote, actualizarLote y cambiarEstadoLote [FAIL]
  Failed SF-043: renderizarBadgeEstadoLote debe mapear Disponible, Reservado y Vendido según Figma Captura 2 [282 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\lotesApi.js', but found False.
  Failed SF-043: lotesApi.js debe soportar UMD (window.lotesApi y module.exports) [1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\lotesApi.js', but found False.
  Failed SF-043: lotesApi.js debe delegar las operaciones HTTP en apiClient [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\lotesApi.js', but found False.
  Failed SF-043: lotesApi.js debe existir físicamente en wwwroot/js/api/ [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\lotesApi.js', but found False.
  Failed SF-043 (BDD): formatearMonedaMXN debe formatear en cultura es-MX ($120,000.00) [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\lotesApi.js', but found False.
  Failed SF-043: lotesApi.js debe utilizar la ruta base /api/v1/lotes [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\lotesApi.js', but found False.
  Failed SF-043: lotesApi.js debe exponer obtenerCatalogo, obtenerLotePorId, crearLote, actualizarLote y cambiarEstadoLote [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\lotesApi.js', but found False.

Failed!  - Failed:     7, Passed:     0, Skipped:     0, Total:     7, Duration: 4 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Conclusión
Fase RED verificada y certificada: las 7 pruebas unitarias fallan debido a la ausencia del módulo `lotesApi.js`.
