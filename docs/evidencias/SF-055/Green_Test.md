# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-055 |
| **Título del Ticket** | Suite QA HTTP CP-03, CP-04, CP-05, CP-06 y CP-10 contra POST /api/v1/simulador/cotizar |
| **Caso de Prueba Relacionado** | CP-03, CP-04, CP-05, CP-06, CP-10 (Milestone v0.5.0) |
| **Fecha de Ejecución** | 22/11/2026 |
| **Tester Responsable** | Edgar Acevedo / Corrección QA Tech Lead |
| **Rama Git** | `feature/SF-055-qa-http-simulador-cotizar` |

---

### 1. Resumen de la Certificación QA
Se implementó y completó la suite de pruebas de integración HTTP `SimuladorControllerTests.cs` en `tests/SistemaFuneraria.Tests/Integration/Api/SimuladorControllerTests.cs` bajo `WebApplicationFactory`:
1. **CP-03**: Cotización a crédito de lote con 2 gavetas ($120,000) a 12 meses (16% anual) con 20% enganche ($24,000). Mensualidad de `$8,710.16`, ajuste en última cuota (`$8,710.19`) y suma de capital de `$96,000.00` con saldo final en `$0.00`.
2. **CP-04**: Cotización a crédito de lote con 2 gavetas ($120,000) a 24 meses (20% anual) con 20% enganche ($24,000). Mensualidad exacta del motor de `$4,886.00` y suma de capital de `$96,000.00`.
3. **CP-05**: Cotización a crédito de lote con 3 gavetas ($150,000) a 24 meses (20% anual) con 20% enganche ($30,000). Mensualidad de `$6,107.50` y suma de capital de `$120,000.00`.
4. **CP-06 (Oráculo 36 Combinaciones)**: Evaluación HTTP de las 36 combinaciones financieras (`3 capacidades × 3 enganches × 4 plazos`). Todas retornan `200 OK` con `Sum(Capital) == SaldoAFinanciar` y saldo final en `$0.00`.
5. **CP-10 (Fronteras Inválidas)**: Rechazo con `400 Bad Request` incluyendo plazo `0`, `15`, `60`, enganches `15%`, `25%`, `100%` en Crédito, gavetas `1`, `5` y modalidad inválida.
6. **Resolución por `LoteId` real y `404 Not Found`**:
   - `Cotizar_ConLoteIdExistente_DebeResolverGavetasYRetornar200Ok`: obtiene el `id` de `JN-A-042` vía `GET /api/v1/lotes?buscar=JN-A-042` y cotiza con `Gavetas = null`.
   - `Cotizar_ConLoteIdInexistente_DebeRetornar404NotFound`: verifica `404` cuando el UUID no existe.
7. **Catálogo de Planes, Contado y Seguridad**: `GET /api/v1/simulador/planes`, modalidad `Contado` y rechazo `401 Unauthorized` sin token JWT.

---

### 2. Comando Ejecutado
```bash
dotnet build SistemaFuneraria.sln --configuration Release --verbosity minimal
dotnet test SistemaFuneraria.sln --configuration Release --no-build --filter "FullyQualifiedName~Integration.Api.SimuladorControllerTests" --verbosity minimal
```

---

### 3. Salida de Terminal (Fase GREEN)
```text
Compilación correcta. 0 Advertencia(s), 0 Errores
Correctas! - Con error:     0, Superado:    18, Omitido:     0, Total:    18
```

---

### 4. Criterio de Aceptación y DoD
- [x] Certificación HTTP real de CP-03, CP-04, CP-05, CP-06 y CP-10 (incluyendo plazo `0`, enganche `100%` y resolución `LoteId` / `404`).
- [x] Cero rutas absolutas en evidencias.
- [x] 18/18 pruebas de integración en verde.
