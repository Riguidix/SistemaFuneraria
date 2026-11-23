# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-056 |
| **Título del Ticket** | Auditoría QA Visual y Funcional del Wizard Pasos 1–3 (*Capturas 5, 6 y 7*) |
| **Caso de Prueba Relacionado** | RNF-01, RNF-06, Gate v0.5.0 (Cierre de Milestone) |
| **Fecha de Ejecución** | 22/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-056-qa-gate-wizard-pasos-1-3` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada previa a la certificación formal de paridad visual y funcional del Wizard de Ventas (Fase RED):
1. Validación de inicialización y sincronización de componentes del Wizard según Captura 5.
2. Certificación de flujo de regresión sin pérdida de datos entre Pasos 1, 2 y 3.
3. Verificación de existencia de scripts cliente modulares y persistencia en `sessionStorage`.

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~WizardPasosAuditoriaTests"
```

---

### 3. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.16]     SF-056 / QA Gate: Inicialización y paridad visual del Wizard con Captura 5 [FAIL]
  Failed SF-056 / QA Gate: Inicialización y paridad visual del Wizard con Captura 5 [25 ms]
  Error Message:
   Expected pageModel.PasoActual to be 99, but found 1 (difference of -98).
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
     at SistemaFuneraria.Tests.Unit.Web.WizardPasosAuditoriaTests.AuditoriaFigma_Paso1a3_ComponentesYResumenCotizacion_DebeCumplirEspecificacionFigma() in tests/SistemaFuneraria.Tests/Unit/Web/WizardPasosAuditoriaTests.cs:line 21

Failed!  - Failed:     1, Passed:     3, Skipped:     0, Total:     4, Duration: 41 ms - SistemaFuneraria.Tests.dll (net8.0)
```
