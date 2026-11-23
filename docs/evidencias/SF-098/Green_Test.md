# Evidencia QA: Fase VERDE (GREEN) — SF-098

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-098 |
| **Título del Ticket** | Pruebas de Aceptación de Usuario (UAT) End-to-End de los 4 Escenarios `ST-01` a `ST-04` y CRUDs de `Administrador` |
| **Milestone** | Milestone 10 (v1.0.0) |
| **Carril** | Carril 2 (Backend) / Carril 4 (QA Gate) |
| **Fecha de Certificación** | 23/11/2026 |
| **Responsable QA** | Edgar Acevedo (Carril 4 — DevOps & QA) |

---

### 1. Resumen de Implementación y Certificación
- Certificación integral de los 4 escenarios de prueba basados en escenarios de negocio (ST):
  - **ST-01**: Ciclo completo de venta de contado (lote familiar, pago único, cambio de estado a `Vendido`, sin amortización deudora).
  - **ST-02**: Venta financiada con abono extraordinario a capital de $10,000.00 y recálculo de cuota a $7,523.51 conservando fecha fin original.
  - **ST-03**: Evaluación de vencimientos sin recargo moratorio ($0.00) y liquidación anticipada con descuento del 10% sobre intereses futuros ($77,893.39).
  - **ST-04**: Integridad concurrente bajo peticiones simultáneas (`Task.WhenAll`), garantizando 1 transacción aceptada (200 OK) y 1 rechazada por conflicto (409 Conflict) con saldo consistente.
- Verificación de operaciones administrativas restringidas al rol `Administrador`.

---

### 2. Comandos de Verificación
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ST|FullyQualifiedName~AbonoYLiquidacion|FullyQualifiedName~VencimientosYReportes" --no-build
```

### 3. Criterios de Aceptación Cumplidos
- [x] Escenarios ST-01 a ST-04 ejecutados y certificados al 100%.
- [x] Integridad transaccional y financiera verificada en base de datos.
- [x] CRUDs de administrador restringidos y certificados bajo RBAC.
