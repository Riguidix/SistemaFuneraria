# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-094 |
| **Título del Ticket** | Suite QA HTTP CP-09 (Vencimiento sin Mora $0.00 con FakeTimeProvider), Dashboard y Reportes (Cierre v0.9.0 — CODE FREEZE) |
| **Caso de Prueba Relacionado** | CP-09 (Milestone v0.9.0 — CODE FREEZE) |
| **Fecha de Ejecución** | 23/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-094-qa-http-cp09-vencimientos-mora-cero` |

---

### 1. Resumen de la Certificación QA
Se implementó y completó la suite de pruebas de integración HTTP `VencimientosYReportesTests.cs` en `tests/SistemaFuneraria.Tests/Integration/Api/VencimientosYReportesTests.cs` bajo `WebApplicationFactory` con inyección controlada de `TimeProvider`:

1. **CP-09 (Vencimiento sin mora con FakeTimeProvider)**:
   - Creación de un contrato financiado a 12 meses con 20% de enganche.
   - Verificación de estado inicial `Al corriente` y `CuotasVencidas == 0`.
   - Avance del reloj en 45 días mediante `FakeTimeProvider`.
   - Consulta del contrato en ventanilla de caja vía `GET /api/v1/cobranza/contrato/{folio}`: la cuota transiciona a `Vencida`, el financiamiento a `EnMora`, el estado visual refleja `1 vencido`, y el monto mensual exigible se mantiene estrictamente idéntico a la cuota original (**$0.00 de interés moratorio o recargo**, conforme a RF-14 y RN-10).
   - Verificación de alerta en el corte del día vía `GET /api/v1/cobranza/resumen-caja` con `PagosVencidosCount >= 1`.
   - Cobro exitoso de la mensualidad vencida sin cargos adicionales emitiendo recibo oficial por el importe exacto.

2. **Administración de Usuarios y Seguridad RBAC**:
   - `GET /api/v1/admin/usuarios`: acceso autorizado (`200 OK`) para perfil Administrador y rechazo estricto (`403 Forbidden`) para perfil Cajero.

3. **Cartera y Contratos Activos**:
   - Consulta de cartera de contratos vía `GET /api/v1/contratos` retornando `200 OK` con estados comerciales actualizados.

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~VencimientosYReportesTests"
```

---

### 3. Salida de Terminal (Fase GREEN)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:     3, Omitido:     0, Total:     3, Duración: 217 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Criterio de Aceptación y DoD
- [x] Certificación HTTP real de CP-09 con abstracción `TimeProvider`.
- [x] Cobro exacto sin penalización ni interés moratorio ($0.00 mora).
- [x] Suite de pruebas HTTP pasando al 100% (3/3).
- [x] Cero rutas absolutas en evidencias y cero comentarios de IA en código fuente.
- [x] Declaración de Code Freeze funcional rumbo a Milestone v1.0.0.
