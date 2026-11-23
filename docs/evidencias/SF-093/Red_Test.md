# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-093 |
| **Título del Ticket** | Suite QA HTTP CP-07 (Abono a Capital $7,523.51) y CP-08 (Liquidación Anticipada $77,893.39) |
| **Caso de Prueba Relacionado** | CP-07, CP-08 (Milestone v0.9.0) |
| **Fecha de Ejecución** | 23/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-093-qa-http-cp07-cp08-abono-liquidacion` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la implementación de los endpoints de cobranza avanzada en `CobranzaController` y `CobranzaService` (Fase RED):
1. **CP-07**: Verificación de abono a capital de $10,000.00 sobre contrato a 12 meses (tras 3 cuotas ordinarias pagadas) y recálculo exacto de cuotas restantes (4..12) a **$7,523.51**.
2. **CP-08**: Verificación de liquidación anticipada por **$77,893.39** con 10% de descuento por pronto pago sobre intereses no devengados, marcando el contrato en estado liquidado y saldo $0.00.
3. **Validaciones**: Verificación de rechazo con `400 Bad Request` ante parámetros inválidos en abono a capital.

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~AbonoYLiquidacionControllerTests"
```

---

### 3. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:01.07]     SF-093 / CP-08: Liquidación anticipada $77,893.39 liquida el contrato con 10% descuento [FAIL]
[xUnit.net 00:00:01.15]     SF-093 / CP-07: Abono a capital $10,000 recalcula cuotas 4..12 en $7,523.51 [FAIL]
[xUnit.net 00:00:01.18]     SF-093: Abono a capital con parámetros inválidos o sin idempotencia debe retornar 400 Bad Request [FAIL]
  Con error SF-093 / CP-08: Liquidación anticipada $77,893.39 liquida el contrato con 10% descuento [933 ms]
  Mensaje de error:
   Expected resLiq.StatusCode to be HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.AbonoYLiquidacionControllerTests.CP08_LiquidacionAnticipada_ConDescuento10Porciento_DebeCerrarContrato() in tests/SistemaFuneraria.Tests/Integration/Api/AbonoYLiquidacionControllerTests.cs:line 178

  Con error SF-093 / CP-07: Abono a capital $10,000 recalcula cuotas 4..12 en $7,523.51 [84 ms]
  Mensaje de error:
   Expected resAbono.StatusCode to be HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.AbonoYLiquidacionControllerTests.CP07_AbonoCapital_10k_DebeReducirSaldoYRecalcularCuotasA752351() in tests/SistemaFuneraria.Tests/Integration/Api/AbonoYLiquidacionControllerTests.cs:line 134

  Con error SF-093: Abono a capital con parámetros inválidos o sin idempotencia debe retornar 400 Bad Request [32 ms]
  Mensaje de error:
   Expected res.StatusCode to be HttpStatusCode.BadRequest {value: 400}, but found HttpStatusCode.NotFound {value: 404}.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.AbonoYLiquidacionControllerTests.AbonoCapital_Validaciones_DebeRetornar400BadRequest() in tests/SistemaFuneraria.Tests/Integration/Api/AbonoYLiquidacionControllerTests.cs:line 198

Con error! - Con error: 3, Superado: 0, Omitido: 0, Total: 3, Duración: 117 ms - SistemaFuneraria.Tests.dll (net8.0)
```
