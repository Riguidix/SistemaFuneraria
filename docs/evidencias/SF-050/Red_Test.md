# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-050 |
| **Caso de Prueba Relacionado** | RF-01 / RNF-01 (Wrapper JS simuladorApi.js y Persistencia de Borrador en sessionStorage wizardState.js) |
| **Fecha y Hora de Ejecución** | 21/11/2026 23:54 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-050-web-simulador-api-wizard-state` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la implementación de los módulos JavaScript del carril web para cotización y persistencia:
1. Ausencia del archivo wrapper `src/SistemaFuneraria.Web/wwwroot/js/api/simuladorApi.js`.
2. Ausencia de métodos `obtenerPlanes` y `cotizar` delegando sobre `apiClient`.
3. Ausencia del módulo gestor `src/SistemaFuneraria.Web/wwwroot/js/ventas/wizardState.js`.
4. Ausencia del contrato con métodos `getDraft`, `updateDraft`, `clearDraft` y clave `sf_nueva_venta_draft`.
5. Ausencia de la emisión del evento personalizado `sf:draft-updated`.
6. Ausencia de sincronización reactiva del pill `"Borrador guardado"` (`#draftBadge`) y del panel lateral `"RESUMEN DE COTIZACIÓN"`.
7. Ausencia de inclusión de ambos scripts en `Pages/Ventas/Nueva.cshtml`.

---

### 2. Comando de Terminal Ejecutado
```powershell
$env:PATH = "%DOTNET_HOME%;$env:PATH"; $env:DOTNET_ROOT = "%DOTNET_HOME%"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~SimuladorApiAndWizardStateTests" -v normal
```

---

### 3. Salida Real de Terminal (Fase RED)
```text
Test run for <repo>/tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.41]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.61]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.62]   Starting:    SistemaFuneraria.Tests
  Failed SF-050: wizardState.js debe sincronizar en vivo el panel derecho RESUMEN DE COTIZACIÓN [260 ms]
  Error Message:
   Expected File.Exists(path) to be true, but found False.
  Failed SF-050: Pages/Ventas/Nueva.cshtml debe incluir simuladorApi.js y wizardState.js [15 ms]
  Error Message:
   Expected File.ReadAllText(path) to contain "simuladorApi.js".
  Failed SF-050: simuladorApi.js debe existir físicamente en wwwroot/js/api/ [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because el archivo simuladorApi.js debe existir en wwwroot/js/api/, but found False.
  Failed SF-050: wizardState.js debe existir físicamente en wwwroot/js/ventas/ [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because el archivo wizardState.js debe existir en wwwroot/js/ventas/, but found False.
  Failed SF-050: simuladorApi.js debe exponer obtenerPlanes y cotizar delegando en apiClient [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true, but found False.
  Failed SF-050: wizardState.js debe usar la clave sf_nueva_venta_draft y exponer getDraft, updateDraft y clearDraft [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true, but found False.
  Failed SF-050 (BDD): wizardState.js debe sincronizar el pill 'Borrador guardado' (draftBadge) [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true, but found False.

Test Run Failed.
Total tests: 7
     Failed: 7
 Total time: 1.2838 Seconds
```
