# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-055 |
| **Título del Ticket** | Suite QA HTTP CP-03, CP-04, CP-05, CP-06 y CP-10 contra POST /api/v1/simulador/cotizar |
| **Caso de Prueba Relacionado** | CP-03, CP-04, CP-05, CP-06, CP-10 (Milestone v0.5.0) |
| **Fecha de Ejecución** | 22/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-055-qa-http-simulador-cotizar` |

---

### 1. Resumen de la Certificación QA
Se implementó y certificó la suite de pruebas de integración HTTP `SimuladorControllerTests.cs` en `tests/SistemaFuneraria.Tests/Integration/Api/SimuladorControllerTests.cs` bajo `WebApplicationFactory`:
1. **CP-03**: Cotización a crédito de lote con 2 gavetas ($120k) a 12 meses (16% anual) con 20% enganche ($24k). Mensualidad de $8,710.16, ajuste en última cuota ($8,710.19) y suma de capital de $96,000.00 con saldo final en $0.00.
2. **CP-04 / Captura 7 Figma**: Cotización a crédito de lote con 2 gavetas ($120k) a 24 meses (20% anual) con 20% enganche ($24k). Mensualidad estimada de $4,886.00 y suma de capital de $96,000.00.
3. **CP-05**: Cotización a crédito de lote con 3 gavetas ($150k) a 24 meses (20% anual) con 20% enganche ($30k). Mensualidad estimada de $6,107.50 y suma de capital de $120,000.00.
4. **CP-06 (Oráculo 36 Combinaciones)**: Evaluación HTTP de las 36 combinaciones financieras (`3 capacidades × 3 enganches × 4 plazos`). Las 36 devuelven `200 OK`, cuotas proyectadas coincidentes con el plazo y suma de capital amortizado idéntica al saldo a financiar con $0.00 de diferencia.
5. **CP-10 (Fronteras Inválidas)**: Rechazo estricto con `400 Bad Request` para plazos no autorizados (15, 60 meses), enganches fuera de catálogo (15%, 25%), gavetas inválidas (1, 5) y modalidad no permitida.
6. **Catálogo de Planes**: `GET /api/v1/simulador/planes` devuelve `200 OK` con los 3 porcentajes de enganche autorizados (`10%`, `20%`, `30%`) y los 4 plazos del negocio con sus respectivas tasas anuales indexadas.
7. **Modalidad Contado**: Proyección sin amortización y liquidación en un solo pago con saldo a financiar en $0.00.
8. **Seguridad y RBAC**: Rechazo `401 Unauthorized` ante peticiones sin token JWT.

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~Integration.Api.SimuladorControllerTests"
```

---

### 3. Salida de Terminal (Fase GREEN)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Passed!  - Failed:     0, Passed:    14, Skipped:     0, Total:    14, Duration: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Criterio de Aceptación y DoD
- [x] Certificación de CP-03, CP-04, CP-05, CP-06 y CP-10 mediante HTTP real sobre `WebApplicationFactory`.
- [x] Blast Radius: 212 LOCs (límite estipulado: 230 LOCs).
- [x] Cero rutas absolutas en evidencias.
- [x] 14 pruebas de integración en verde (100% de éxito).
