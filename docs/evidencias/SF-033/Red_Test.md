# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-033 |
| **Caso de Prueba Relacionado** | RF-02 / RNF-01 (Módulo Wrapper wwwroot/js/api/clientesApi.js) |
| **Fecha y Hora de Ejecución** | 17/11/2026 13:03 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-033-clientes-js-wrapper` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la implementación de `src/SistemaFuneraria.Web/wwwroot/js/api/clientesApi.js`, verificando que la suite unitaria detecta la ausencia del archivo y de los métodos del contrato:
1. Existencia física en `wwwroot/js/api/clientesApi.js`.
2. Métodos del contrato: `listar`, `obtenerPorId`, `registrar`, `actualizar`, `eliminar` y `exportarDirectorio`.
3. Consumo de los endpoints `/api/v1/clientes` y `/api/v1/clientes/exportar`.
4. Soporte UMD (`window.clientesApi` y `module.exports`).
5. Delegación en `apiClient` (`resolveClient`).
6. Escenario BDD: Sanitización automática de espacios en teléfono y mayúsculas en CURP mediante `sanitizarPayload`.
7. Descarga configurada para el archivo `directorio-clientes.csv`.

---

### 2. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClientesApiWrapperTests"
```

---

### 3. Salida Real de Terminal (Fase RED)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:01.07]     SF-033: clientesApi.js debe soportar UMD (window.clientesApi y module.exports) [FAIL]
[xUnit.net 00:00:01.07]     SF-033: clientesApi.js debe delegar las operaciones HTTP en apiClient [FAIL]
[xUnit.net 00:00:01.07]     SF-033 (BDD): registrar y actualizar deben sanitizar teléfono (sin espacios) y CURP (mayúsculas) [FAIL]
[xUnit.net 00:00:01.07]     SF-033: clientesApi.js debe existir físicamente en wwwroot/js/api/ [FAIL]
[xUnit.net 00:00:01.07]     SF-033: exportarDirectorio debe configurar descarga del archivo directorio-clientes.csv [FAIL]
[xUnit.net 00:00:01.07]     SF-033: clientesApi.js debe exponer listar, obtenerPorId, registrar, actualizar, eliminar y exportarDirectorio [FAIL]
[xUnit.net 00:00:01.07]     SF-033: clientesApi.js debe utilizar la ruta base /api/v1/clientes y /exportar [FAIL]
  Failed SF-033: clientesApi.js debe soportar UMD (window.clientesApi y module.exports) [395 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\clientesApi.js', but found False.
  Failed SF-033: clientesApi.js debe delegar las operaciones HTTP en apiClient [1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\clientesApi.js', but found False.
  Failed SF-033: exportarDirectorio debe configurar descarga del archivo directorio-clientes.csv [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\clientesApi.js', but found False.
  Failed SF-033: clientesApi.js debe exponer listar, obtenerPorId, registrar, actualizar, eliminar y exportarDirectorio [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\clientesApi.js', but found False.
  Failed SF-033: clientesApi.js debe utilizar la ruta base /api/v1/clientes y /exportar [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo 'src\SistemaFuneraria.Web\wwwroot\js\api\clientesApi.js', but found False.

Failed!  - Failed:     7, Passed:     0, Skipped:     0, Total:     7, Duration: 5 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Conclusión
Fase RED verificada y certificada: las 7 pruebas unitarias fallan debido a la ausencia del archivo JavaScript y su lógica de sanitización/comunicación.
