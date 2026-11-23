# Evidencia TDD / Auditoría QA Gate: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-056 |
| **Título del Ticket** | Auditoría QA Visual y Funcional del Wizard Pasos 1–3 (*Capturas 5, 6 y 7*) |
| **Caso de Prueba Relacionado** | RNF-01, RNF-06, Gate v0.5.0 (Cierre de Milestone) |
| **Fecha de Ejecución** | 22/11/2026 |
| **Tester Responsable** | Edgar Acevedo / Corrección QA Tech Lead |
| **Rama Git** | `feature/SF-056-qa-gate-wizard-pasos-1-3` |

---

### 1. Resumen de la Auditoría QA Gate v0.5.0
Se reemplazó la auditoría previa por verificaciones reales sobre el DOM Razor de Pasos 1–3, el motor financiero oficial `SimuladorService` y la ejecución en Node de `wizardState.js` + `simuladorApi.js`:

1. **Integridad DOM y Cuadrícula 2x4 (`AuditoriaFigma_ShellYPasos1a3_EstructuraDomYCuadricula_DebeCumplirEspecificacion`):**
   - Valida `NuevaModel` (`PROCESO COMERCIAL`, `Nueva venta`).
   - Verifica en `Nueva.cshtml` y `_Paso1Cliente.cshtml` / `_Paso2Lote.cshtml` / `_Paso3Financiamiento.cshtml` la presencia real de `stepIndicator1..4`, `resumenLoteTitulo`, `resumenLoteClave`, `resumenLoteCapacidad`, `resumenLotePrecio`, `resumenTotalEstimado`, `draftBadge`, las 8 casillas `data-slot` (`A`, `B`, `42`, `43`, `C`, `44`, `45`, `46`) y controles interactivos de los 3 pasos.
2. **Paridad con Motor Financiero Real (`AuditoriaFinanciera_SimuladorReal_DebeCoincidirEnCreditoYContadoSinDisparidad`):**
   - Ejecuta `SimuladorService.Cotizar` validando `$4,886.00` y `Sum(Capital) == $96,000.00` en Crédito (2 gavetas, 20%, 24 meses) y `$120,000.00` sin intereses en Contado.
3. **Regresión BDD en Node (`AuditoriaBDD_RegresionPasos_WizardStateYSimuladorApi_DebeConservarDatosYConsumirContratoEnNode`):**
   - Ejecuta `node` cargando directamente `wwwroot/js/ventas/wizardState.js` y `wwwroot/js/api/simuladorApi.js` con mocks de `sessionStorage`, DOM y `apiClient`.
   - Verifica obtención de planes (`/api/v1/simulador/planes`), cotización (`/api/v1/simulador/cotizar`), flujo Paso 1 → Paso 2 → Paso 3, regresión `Anterior` (Paso 3 → Paso 2) y retorno sin pérdida de datos (`cliente`, `lote.clave === "JN-A-042"`, `plazoMeses === 24`), actualización de `resumenLoteClave`, `draftBadge` y evento `sf:draft-updated`, más limpieza con `clearDraft()`.

---

### 2. Comando Ejecutado
```bash
dotnet build SistemaFuneraria.sln --configuration Release --verbosity minimal
dotnet test SistemaFuneraria.sln --configuration Release --no-build --filter "FullyQualifiedName~WizardPasosAuditoriaTests" --verbosity minimal
```

---

### 3. Salida de Terminal (Fase GREEN)
```text
Compilación correcta. 0 Advertencia(s), 0 Errores
Correctas! - Con error:     0, Superado:     3, Omitido:     0, Total:     3
```

---

### 4. Criterio de Aceptación y DoD
- [x] Auditoría real de DOM (Pasos 1–3 + cuadrícula 2x4), motor `SimuladorService` y regresión `wizardState.js` + `simuladorApi.js` en Node.
- [x] Cero rutas absolutas en evidencias.
- [x] 3/3 pruebas de `WizardPasosAuditoriaTests` en verde.
