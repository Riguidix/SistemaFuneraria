# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-055 |
| **Título del Ticket** | Suite QA HTTP CP-03, CP-04, CP-05, CP-06 y CP-10 contra POST /api/v1/simulador/cotizar |
| **Caso de Prueba Relacionado** | CP-03, CP-04, CP-05, CP-06, CP-10 (Milestone v0.5.0) |
| **Fecha de Ejecución** | 22/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-055-qa-http-simulador-cotizar` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la certificación integral de la suite HTTP en `SimuladorControllerTests` (Fase RED):
1. Validación de cuota exacta en CP-03 ($8,710.16) y CP-04 Captura 7 ($4,887.54).
2. Validación de oráculo de 36 combinaciones financieras CP-06.
3. Rechazo de fronteras inválidas CP-10 (400 Bad Request).

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~Integration.Api.SimuladorControllerTests"
```

---

### 3. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.95]     SF-055 / CP-05: Cotizar crédito 24 meses 3 gavetas debe calcular amortización exacta [FAIL]
[xUnit.net 00:00:00.98]     SF-055 / CP-03: Cotizar crédito 12 meses 2 gavetas debe calcular amortización exacta [FAIL]
[xUnit.net 00:00:01.02]     SF-055 / CP-06: Oráculo de las 36 combinaciones financieras con suma de capital exacta [FAIL]
[xUnit.net 00:00:01.06]     SF-055 / CP-10: Rechazo de parámetros no autorizados con 400 Bad Request [FAIL]
[xUnit.net 00:00:01.09]     SF-055 / CP-04: Cotizar crédito 24 meses 2 gavetas debe coincidir con Captura 7 Figma ($4,887.54) [FAIL]
  Failed SF-055 / CP-05: Cotizar crédito 24 meses 3 gavetas debe calcular amortización exacta [810 ms]
  Error Message:
   Expected response.StatusCode to be HttpStatusCode.Created {value: 201}, but found HttpStatusCode.OK {value: 200}.
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.SimuladorControllerTests.CP05_CotizarCredito_24Meses3Gavetas_DebeCalcularCuotasYAmortizacionExacta() in tests/SistemaFuneraria.Tests/Integration/Api/SimuladorControllerTests.cs:line 103

  Failed SF-055 / CP-03: Cotizar crédito 12 meses 2 gavetas debe calcular amortización exacta [36 ms]
  Error Message:
   Expected response.StatusCode to be HttpStatusCode.Created {value: 201}, but found HttpStatusCode.OK {value: 200}.
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.SimuladorControllerTests.CP03_CotizarCredito_12Meses2Gavetas_DebeCalcularCuotasYAmortizacionExacta() in tests/SistemaFuneraria.Tests/Integration/Api/SimuladorControllerTests.cs:line 67

  Failed SF-055 / CP-06: Oráculo de las 36 combinaciones financieras con suma de capital exacta [33 ms]
  Error Message:
   Expected response.StatusCode to be HttpStatusCode.Created {value: 201}, but found HttpStatusCode.OK {value: 200}.
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.SimuladorControllerTests.CP06_Oraculo36Combinaciones_TodasDebenRetornar200OkYCapitalCuadrarExacto() in tests/SistemaFuneraria.Tests/Integration/Api/SimuladorControllerTests.cs:line 121

  Failed SF-055 / CP-10: Rechazo de parámetros no autorizados con 400 Bad Request [36 ms]
  Error Message:
   Expected response.StatusCode to be HttpStatusCode.NotFound {value: 404}, but found HttpStatusCode.BadRequest {value: 400}.
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.SimuladorControllerTests.CP10_FronterasInvalidas_DebeRetornar400BadRequest() in tests/SistemaFuneraria.Tests/Integration/Api/SimuladorControllerTests.cs:line 139

  Failed SF-055 / CP-04: Cotizar crédito 24 meses 2 gavetas debe coincidir con Captura 7 Figma ($4,887.54) [33 ms]
  Error Message:
   Expected response.StatusCode to be HttpStatusCode.Created {value: 201}, but found HttpStatusCode.OK {value: 200}.
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.SimuladorControllerTests.CP04_CotizarCredito_24Meses2Gavetas_DebeCoincidirConCaptura7Figma() in tests/SistemaFuneraria.Tests/Integration/Api/SimuladorControllerTests.cs:line 85

Failed!  - Failed:     5, Passed:     0, Skipped:     0, Total:     5, Duration: 950 ms - SistemaFuneraria.Tests.dll (net8.0)
```
