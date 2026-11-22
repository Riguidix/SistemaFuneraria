# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-046 |
| **Caso de Prueba Relacionado** | RF-01 / RNF-01 (Endpoint y Modal Ver detalle de Lote e Historial de Estado) |
| **Fecha y Hora de Ejecución** | 21/11/2026 23:08 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-046-api-web-detalle-lote-modal` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la implementación de los componentes requeridos por `SF-046`:
1. Falla controlada en `LotesController.ObtenerPorClave` (`NotImplementedException: SF-046: Fase RED.`).
2. Ausencia del modal `src/SistemaFuneraria.Web/Pages/Shared/_DetalleLoteModal.cshtml`.
3. Ausencia del método `obtenerLotePorClave` en `lotesApi.js`.
4. Ausencia del enlace hacia `/Contratos/Detalle?loteClave=LM-A-014` para lotes en estado `Vendido`.
5. Ausencia del soporte del parámetro `loteClave` en `DetalleModel` (`Pages/Contratos/Detalle.cshtml.cs`).
6. Ausencia de la conexión entre el botón `"Ver detalle"` de la tabla y la apertura del modal con la información técnica de lote (`RO-B-027`: `"Robles · Andador B · Fila 2"`, `"4 gavetas"`, `"$180,000.00"`).

---

### 2. Comando de Terminal Ejecutado
```powershell
$env:PATH = "C:\Users\Billy\AppData\Local\Microsoft\dotnet;$env:PATH"; $env:DOTNET_ROOT = "C:\Users\Billy\AppData\Local\Microsoft\dotnet"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~DetalleLoteModalTests|FullyQualifiedName~ObtenerPorClave" -v normal
```

---

### 3. Salida Real de Terminal (Fase RED)
```text
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:03.94]     SF-046: ObtenerPorClave existente debe retornar 200 OK con DTO técnico [FAIL]
[xUnit.net 00:00:03.94]       System.NotImplementedException : SF-046: Fase RED.
[xUnit.net 00:00:03.94]       Stack Trace:
[xUnit.net 00:00:03.94]         C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Api\Controllers\V1\LotesController.cs(57,0): at SistemaFuneraria.Api.Controllers.V1.LotesController.ObtenerPorClave(String clave, CancellationToken ct)
[xUnit.net 00:00:03.94]         C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Api\LotesControllerTests.cs(176,0): at SistemaFuneraria.Tests.Unit.Api.LotesControllerTests.ObtenerPorClave_LoteExistente_DebeRetornar200Ok()
[xUnit.net 00:00:03.94]         --- End of stack trace from previous location ---
[xUnit.net 00:00:03.95]   Finished:    SistemaFuneraria.Tests
  Failed SF-046: ObtenerPorClave existente debe retornar 200 OK con DTO técnico [973 ms]
  Error Message:
   System.NotImplementedException : SF-046: Fase RED.
  Failed SF-046: ObtenerPorClave inexistente debe retornar 404 Not Found [749 ms]
  Error Message:
   System.NotImplementedException : SF-046: Fase RED.
  Failed SF-046: El componente _DetalleLoteModal.cshtml debe existir en Pages/Shared/ [280 ms]
  Error Message:
   Expected File.Exists(path) to be true because el modal _DetalleLoteModal.cshtml debe existir en Pages/Shared, but found False.
  Failed SF-046 (BDD): Modal debe contener campos técnicos requeridos (Clave, Ubicación, Gavetas, Precio, Estado, Historial) [1 ms]
  Failed SF-046: Pages/Catalogo/Index.cshtml debe incluir el partial _DetalleLoteModal y conectar botón Ver detalle [2 ms]
  Failed SF-046: Enlace 'Ver contrato' para lote Vendido (LM-A-014) debe apuntar a /Contratos/Detalle?loteClave=LM-A-014 [1 ms]
  Failed SF-046: DetalleModel debe soportar parámetro de búsqueda por loteClave [1 ms]
  Failed SF-046: lotesApi.js debe exponer método obtenerLotePorClave [1 ms]

Test Run Failed.
Total tests: 8
     Failed: 8
 Total time: 5.2149 Seconds
```
