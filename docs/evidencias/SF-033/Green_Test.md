# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-033 |
| **Caso de Prueba Relacionado** | RF-02 / RNF-01 (Módulo Wrapper wwwroot/js/api/clientesApi.js) |
| **Fecha y Hora de Ejecución** | 17/11/2026 13:04 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-033-clientes-js-wrapper` |

---

### 1. Objetivo de la Prueba
Demostrar el pase al 100% de las pruebas unitarias y de contrato para `src/SistemaFuneraria.Web/wwwroot/js/api/clientesApi.js`:
1. Exposición completa de los métodos del contrato: `listar`, `obtenerPorId`, `registrar`, `actualizar`, `eliminar` y `exportarDirectorio`.
2. Sanitización ergonómica en `registrar` y `actualizar` (eliminación de espacios en teléfono y conversión de CURP a mayúsculas automáticas).
3. Consumo estricto de `/api/v1/clientes` y `/api/v1/clientes/exportar`.
4. Compatibilidad UMD (`window.clientesApi` y `module.exports`).
5. Descarga de `directorio-clientes.csv`.
6. Certificación de cero regresiones en toda la solución (408 pruebas unitarias aprobadas).

---

### 2. Comando de Terminal Ejecutado (Pruebas del Ticket SF-033)
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClientesApiWrapperTests" -v normal
```

---

### 3. Salida Real de Terminal (Fase GREEN - ClientesApiWrapperTests)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.07]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.15]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.16]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.19]   Finished:    SistemaFuneraria.Tests
  Passed SF-033: clientesApi.js debe soportar UMD (window.clientesApi y module.exports) [4 ms]
  Passed SF-033: clientesApi.js debe delegar las operaciones HTTP en apiClient [< 1 ms]
  Passed SF-033 (BDD): registrar y actualizar deben sanitizar teléfono (sin espacios) y CURP (mayúsculas) [< 1 ms]
  Passed SF-033: clientesApi.js debe existir físicamente en wwwroot/js/api/ [< 1 ms]
  Passed SF-033: exportarDirectorio debe configurar descarga del archivo directorio-clientes.csv [< 1 ms]
  Passed SF-033: clientesApi.js debe exponer listar, obtenerPorId, registrar, actualizar, eliminar y exportarDirectorio [< 1 ms]
  Passed SF-033: clientesApi.js debe utilizar la ruta base /api/v1/clientes y /exportar [< 1 ms]

Test Run Successful.
Total tests: 7
     Passed: 7
 Total time: 0.5683 Seconds

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### 4. Salida Real de Verificación Global (Regresión Cero)
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName!~Integration"
```
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   408, Skipped:     0, Total:   408, Duration: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Conclusión y Dictamen
Ciclo TDD completado exitosamente:
- Las 7 pruebas de contrato, arquitectura y BDD pasan en verde.
- 408 pruebas globales ejecutadas sin ningún error ni advertencia (`TreatWarningsAsErrors`).
- Se cumple estrictamente con el Blast Radius ($\le 160$ LOCs).
