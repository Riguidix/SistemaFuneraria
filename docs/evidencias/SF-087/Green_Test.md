# Evidencia QA: Fase VERDE (GREEN) — SF-087

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-087 |
| **Título del Ticket** | Endpoints `POST /api/v1/cobranza/abono-capital`, `POST /api/v1/cobranza/liquidacion` y Conexión de las 3 Tarjetas en `/Caja/Index` |
| **Milestone** | Milestone 9 (v0.9.0) |
| **Carril** | Carril 2 (Backend) / Carril 3 (Web UI) |
| **Fecha de Certificación** | 23/11/2026 |
| **Responsable QA** | Edgar Acevedo (Carril 4 — DevOps & QA) |

---

### 1. Resumen de Implementación y Certificación
- Exposición de endpoints REST `POST /api/v1/cobranza/abono-capital` y `POST /api/v1/cobranza/liquidacion` en `CobranzaController.cs`.
- Procesamiento transaccional ACID con bloqueo pesimista `SELECT FOR UPDATE` para evitar condiciones de carrera.
- Conexión de las 3 tarjetas operativas en `/Caja/Index`:
  1. Pago ordinario de mensualidad.
  2. Abono extraordinario a capital (con recálculo de cuota periódica manteniendo plazo).
  3. Liquidación anticipada de contrato (con descuento comercial sobre intereses no devengados).
- Generación y retorno del DTO de recibo formal de cobranza.

---

### 2. Comandos de Verificación
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~AbonoYLiquidacion" --no-build
```

### 3. Criterios de Aceptación Cumplidos
- [x] Endpoints probados mediante suite de integración con código HTTP 200 OK y recibo persistido.
- [x] Conexión reactiva en vista de caja para abono a capital y liquidación.
- [x] Manejo de claves de idempotencia `X-Idempotency-Key` en operaciones de cobro.
