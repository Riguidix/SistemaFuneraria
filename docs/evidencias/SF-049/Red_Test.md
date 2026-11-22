# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-049 |
| **Caso de Prueba Relacionado** | CP-10 / RF-01 (SimuladorController: POST /api/v1/simulador/cotizar & GET /api/v1/simulador/planes) |
| **Fecha y Hora de Ejecución** | 21/11/2026 23:21 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-049-api-simulador-controller` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la implementación de los endpoints en `SimuladorController` (`NotImplementedException: SF-049: Fase RED.`):
1. `GET /api/v1/simulador/planes`: catálogo de enganches (10%, 20%, 30%) y plazos autorizados (12, 18, 24, 48 meses con tasas indexadas).
2. `POST /api/v1/simulador/cotizar` en modalidad `Contado`: liquidación total inmediata sin tabla de amortización.
3. `POST /api/v1/simulador/cotizar` en modalidad `Credito`: cálculo de enganche, saldo insoluto, mensualidad ordinaria ($4,887.54 para $120k / 20% / 24 meses) y tabla proyectada.
4. Consulta con `LoteId` existente: resolución de capacidad/gavetas del lote en inventario.
5. `LoteId` inexistente: rechazo con `404 Not Found`.
6. Escenario BDD (CP-10): Rechazo de plazos/enganches no autorizados (`PlazoMeses = 15`, `PorcentajeEnganche = 15`) retornando `400 Bad Request` con `ProblemDetails`.

---

### 2. Comando de Terminal Ejecutado
```powershell
$env:PATH = "%DOTNET_HOME%;$env:PATH"; $env:DOTNET_ROOT = "%DOTNET_HOME%"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~SimuladorControllerTests" -v normal
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
[xUnit.net 00:00:00.62]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.63]   Starting:    SistemaFuneraria.Tests
  Failed SF-049: Cotizar con LoteId existente debe consultar gavetas del lote y retornar 200 OK [1 s]
  Error Message:
   System.NotImplementedException : SF-049: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.SimuladorController.Cotizar(SimularCotizacionRequest request, CancellationToken ct) in <repo>/src\SistemaFuneraria.Api\Controllers\V1\SimuladorController.cs:line 40
   at SistemaFuneraria.Tests.Unit.Api.SimuladorControllerTests.Cotizar_ConLoteIdExistente_DebeResolverGavetasYRetornar200Ok() in <repo>/tests\SistemaFuneraria.Tests\Unit\Api\SimuladorControllerTests.cs:line 200
  Passed SF-049: SimuladorController debe tener Route api/v1/simulador, ApiController y Authorize [12 ms]
  Failed SF-049: ObtenerPlanes debe retornar 200 OK con enganches y plazos autorizados de Figma Captura 7 [2 ms]
  Error Message:
   System.NotImplementedException : SF-049: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Api.Controllers.V1.SimuladorController.ObtenerPlanes() in <repo>/src\SistemaFuneraria.Api\Controllers\V1\SimuladorController.cs:line 28
   at SistemaFuneraria.Tests.Unit.Api.SimuladorControllerTests.ObtenerPlanes_DebeRetornar200OkConPlanesAutorizados() in <repo>/tests\SistemaFuneraria.Tests\Unit\Api\SimuladorControllerTests.cs:line 109
  Failed SF-049 (BDD / CP-10): Rechazo de plazo inválido de 15 meses por API debe retornar 400 Bad Request [1 ms]
  Error Message:
   System.NotImplementedException : SF-049: Fase RED.
  Failed SF-049: Cotizar modalidad Crédito Captura 7 ($120k, 20%, 24 meses) debe retornar 200 OK con cuota exacta $4,887.54 [1 ms]
  Error Message:
   System.NotImplementedException : SF-049: Fase RED.
  Failed SF-049: Cotizar con LoteId inexistente debe retornar 404 Not Found [1 ms]
  Error Message:
   System.NotImplementedException : SF-049: Fase RED.
  Failed SF-049: Cotizar modalidad Contado debe retornar 200 OK con liquidación sin tabla [1 ms]
  Error Message:
   System.NotImplementedException : SF-049: Fase RED.

Test Run Failed.
Total tests: 7
     Passed: 1
     Failed: 6
 Total time: 2.5772 Seconds
```
