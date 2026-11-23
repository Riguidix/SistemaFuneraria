# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-094 |
| **Título del Ticket** | Suite QA HTTP CP-09 (Vencimiento sin Mora $0.00 con FakeTimeProvider), Dashboard y Reportes (Cierre v0.9.0 — CODE FREEZE) |
| **Caso de Prueba Relacionado** | CP-09 (Milestone v0.9.0) |
| **Fecha de Ejecución** | 23/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-094-qa-http-cp09-vencimientos-mora-cero` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la integración del evaluador temporal en los flujos de consulta de Caja y Resumen (Fase RED):
1. **CP-09**: Creación de un contrato financiado al corriente, avance controlado del tiempo en 45 días mediante `FakeTimeProvider` y consulta en ventanilla de caja vía `GET /api/v1/cobranza/contrato/{folio}`.
2. Comprobar que en la consulta inicial, al no dispararse la evaluación temporal dinámica sobre las mensualidades exigibles vencidas, el contador `CuotasVencidas` permanece en 0 (falla esperada).
3. Verificar control de acceso RBAC a endpoints de administración de usuarios (403 Forbidden para cajeros).

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~VencimientosYReportesTests"
```

---

### 3. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.97]     SF-094 / CP-09: Cuota atrasada evaluada con FakeTimeProvider debe marcarse Vencida con $0.00 de mora [FAIL]
  Con error SF-094 / CP-09: Cuota atrasada evaluada con FakeTimeProvider debe marcarse Vencida con $0.00 de mora [149 ms]
  Mensaje de error:
   Expected cajaVencidaDto.CuotasVencidas to be greater than or equal to 1, but found 0 (difference of -1).
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Integration.Api.VencimientosYReportesTests.CP09_CuotaAtrasada_ConFakeTimeProvider_DebeMarcarVencidaYSinRecargoMoratorio() in tests/SistemaFuneraria.Tests/Integration/Api/VencimientosYReportesTests.cs:line 108

Con error! - Con error: 1, Superado: 1, Omitido: 0, Total: 2, Duración: 152 ms - SistemaFuneraria.Tests.dll (net8.0)
```
