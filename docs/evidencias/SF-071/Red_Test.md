# Evidencia TDD / Frontend UI: Fase ROJA (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-071 |
| **Título del Ticket** | Expediente del Contrato (`Pages/Contratos/Detalle.cshtml`), tabla de amortización con 24 mensualidades y panel de gestión para Administrador |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) / SF-027 |
| **Fecha y Hora de Ejecución** | 18/11/2026 21:22 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaContratosDetalle |
| **Resultado** | FALLO ESPERADO (Fase RED - DetalleModel y vista Detalle.cshtml no implementados aún) |

---

### 1. Justificación de la Fase RED
Se diseñó la suite `ContratosDetallePageTests` para validar los requerimientos de negocio y visualización del expediente:
1. `Detalle_OnGet_ConFolioPorDefecto_DebeCargarContratoYAmortizacion24Cuotas`: Carga del contrato base `CTR-2025-0189`, titular `Laura Castañeda`, lote `JC-C-118`, plan de $150,000 con enganche de $30,000 a 24 meses y generación del calendario de 24 cuotas con saldo final en $0.00.
2. `Detalle_OnGet_ConFolioExistente_DebePersonalizarTitularYLote`: Resolución dinámica de contratos existentes como `CTR-2024-0118` (`Elena Vargas`, `RO-B-027`).

La prueba falló la compilación debido a que `DetalleModel` no ha sido implementado aún.
