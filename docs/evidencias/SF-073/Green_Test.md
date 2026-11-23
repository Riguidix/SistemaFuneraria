# Evidencia QA: Fase VERDE (GREEN) — SF-073

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-073 |
| **Título del Ticket** | Conector Completo del Wizard `/Ventas/Nueva` para Modalidad `Crédito` (E2E Pasos 1 a 4 $\rightarrow$ `/Contratos/Detalle`) |
| **Milestone** | Milestone 7 (v0.7.0) |
| **Carril** | Carril 3 (Web UI + JS) |
| **Fecha de Certificación** | 23/11/2026 |
| **Responsable QA** | Edgar Acevedo (Carril 4 — DevOps & QA) |

---

### 1. Resumen de Implementación y Certificación
- Conexión del botón final en Paso 4 para modalidad `Crédito` con el servicio `ventasApi.registrarVentaFinanciada`.
- Validación de parámetros requeridos: `clienteId`, `loteId`, `porcentajeEnganche` (10, 20 o 30%) y `plazoMeses` (12, 18, 24 o 48).
- Manejo de estados asíncronos en UI: deshabilitación de controles durante la llamada HTTP y redirección automática hacia `/Contratos/Detalle?id={ventaId}`.
- Limpieza de borrador `wizardState.js` tras la emisión exitosa del contrato.

---

### 2. Comandos de Verificación
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~Contratos" --no-build
```

### 3. Criterios de Aceptación Cumplidos
- [x] Flujo E2E de venta financiada emite contrato `CTR-YYYY-XXXX` con tabla de amortización completa.
- [x] Manejo de errores HTTP 400 y 409 con retroalimentación clara al usuario.
- [x] Redirección correcta a la vista de detalle de contrato.
