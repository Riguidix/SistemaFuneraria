# Evidencia TDD / Auditoría QA Gate: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-056 |
| **Título del Ticket** | Auditoría QA Visual y Funcional del Wizard Pasos 1–3 (*Capturas 5, 6 y 7*) |
| **Caso de Prueba Relacionado** | RNF-01, RNF-06, Gate v0.5.0 (Cierre de Milestone) |
| **Fecha de Ejecución** | 22/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-056-qa-gate-wizard-pasos-1-3` |

---

### 1. Resumen de la Auditoría QA Gate v0.5.0
Se auditó y certificó la paridad visual y funcional del Wizard "Nueva Venta" (`/Ventas/Nueva`) cubriendo los Pasos 1 a 3 y el panel lateral reactivo contra las especificaciones de diseño Figma:

1. **Paso 1 (Captura 5 Figma)**:
   - Kicker `PROCESO COMERCIAL`, título `Nueva venta` y badge reactivo `✓ Borrador guardado`.
   - Stepper horizontal de 4 pasos con el Paso 1 activo en verde institucional `#2D4224`.
   - Panel lateral fijo `RESUMEN DE COTIZACIÓN`: cuadrícula de gavetas con gaveta `42` activa, tipo `Lote Básico`, clave `JN-A-042`, capacidad `2 gavetas` y precio `$120,000.00 MXN`.
2. **Paso 2 (Captura 6 Figma)**:
   - Selección de lote en inventario, tarjeta de confirmación del lote `JN-A-042` y sincronización con el panel lateral.
3. **Paso 3 (Captura 7 Figma)**:
   - Toggle interactivo `Crédito` / `Contado`.
   - Selects de enganche (`20% · $24,000.00`) y plazo (`24 meses · 20% anual`).
   - Banner institucional `#2D4224` con cálculo exacto de mensualidad estimada de `$4,887.54`.
4. **Flujo de Regresión BDD**:
   - Navegación bidireccional Paso 1 $\leftrightarrow$ Paso 2 $\leftrightarrow$ Paso 3 con botones `Continuar →` y `Anterior`.
   - Preservación íntegra del estado en `sessionStorage` (`sf_nueva_venta_draft`) y disparo del evento `sf:draft-updated`.
   - Cero errores en consola del navegador.

---

### 2. Matriz de Paridad con Figma (Cero Disparidad)

| Componente | Especificación Figma | Resultado en Código / UI | Estado |
|---|---|---|---|
| **Badge de Estado** | `✓ Borrador guardado` en verde suave | Implementado en `wizardState.js` y `Nueva.cshtml` | `Aprobado` |
| **Panel Lateral** | Cuadrícula con gaveta `42` destacada | Renderizado reactivo en `_Paso1Cliente.cshtml` | `Aprobado` |
| **Lote Seleccionado** | `JN-A-042` · 2 gavetas · $120k | Modelado en `NuevaModel.cs` y `paso2Lote.js` | `Aprobado` |
| **Mensualidad Estimada**| Banner verde `#2D4224` con `$4,887.54` | Calculado en `FinanciamientoCalculadora.cs` | `Aprobado` |
| **Modalidad Contado** | Pago único de contado `$120,000.00` | Toggle en `paso3Financiamiento.js` | `Aprobado` |
| **Regresión de Pasos** | Botón `Anterior` sin pérdida de datos | Validado en `wizardState.js` y suite xUnit | `Aprobado` |

---

### 3. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~WizardPasosAuditoriaTests"
```

---

### 4. Salida de Terminal (Fase GREEN)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4, Duration: 13 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Criterio de Aceptación y DoD
- [x] Certificación E2E de los Pasos 1, 2 y 3 del Wizard contra Capturas 5, 6 y 7 de Figma.
- [x] Regresión hacia adelante y atrás validada sin pérdida de datos.
- [x] Suite `WizardPasosAuditoriaTests.cs` en verde (85 LOCs $\le$ 120 LOCs).
- [x] Cero rutas absolutas en evidencias.
- [x] Cierre formal del Milestone v0.5.0.
