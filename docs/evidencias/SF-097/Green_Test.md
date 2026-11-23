# Evidencia QA / DevOps: Certificación Integral Pipeline CI/CD

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-097 |
| **Título del Ticket** | Ejecución Integral del Pipeline CI/CD (Build + Unit + Integration + Coverage >= 90% Core + Validador de Evidencias) |
| **Milestone Relacionado** | Milestone 10 (v1.0.0 — Hardening, Cobertura y Go-Live) |
| **Fecha de Ejecución** | 23/11/2026 |
| **Tester / DevOps Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-097-ejecucion-integral-pipeline-cicd-cobertura` |

---

### 1. Resumen de Certificación del Gatekeeper CI/CD
Se ejecutó la validación integral del pipeline de calidad y recolección automatizada de métricas de cobertura de código sobre la solución completa:

1. **Compilación Limpia (Build Quality Gate)**:
   - Configuración: `Release`
   - Warnings de compilación: **0 Advertencias**
   - Errores de compilación: **0 Errores**

2. **Auditoría de Omisión de Pruebas (Zero Skips)**:
   - Verificación estricta de atributos `[Fact(Skip = ...)]` y `[Theory(Skip = ...)]`: **0 pruebas omitidas** en toda la suite de pruebas unitarias e integración.

3. **Métricas de Cobertura de Código (XPlat Code Coverage)**:
   - Motor Financiero de Dominio (`SistemaFuneraria.Core`):
     - **Líneas cubiertas**: **96.22%** (Criterio DoD: $\ge 90\%$) $\rightarrow$ **SUPERADO**
     - **Ramas cubiertas**: **85.91%**
   - Capa de Aplicación (`SistemaFuneraria.Application`):
     - **Líneas cubiertas**: **88.39%** (Criterio DoD: $\ge 85\%$) $\rightarrow$ **SUPERADO**
     - **Ramas cubiertas**: **72.15%**
   - Capa de Controladores REST (`SistemaFuneraria.Api`):
     - **Líneas cubiertas**: **90.14%**
     - **Ramas cubiertas**: **63.17%**

4. **Validador de Evidencias (Rutas Relativas)**:
   - Auditoría regex de rutas de máquina host sobre el directorio `docs/evidencias/`: **0 rutas absolutas detectadas** (cumplimiento 100% estricto de portabilidad multi-entorno).

---

### 2. Comandos Ejecutados
```bash
# 1. Compilación limpia sin advertencias
dotnet build SistemaFuneraria.sln --configuration Release --verbosity minimal

# 2. Recolección integral de cobertura
dotnet test SistemaFuneraria.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults

# 3. Auditoría de portabilidad de rutas locales en evidencias
# Validador de portabilidad CI (docs/evidencias sin rutas absolutas del host)
```

---

### 3. Salida de Terminal Consolidada
```text
Compilación correcta.
    0 Advertencia(s)
    0 Errores

SistemaFuneraria.Core: Line Coverage = 96.22%, Branch Coverage = 85.91%
SistemaFuneraria.Application: Line Coverage = 88.39%, Branch Coverage = 72.15%
SistemaFuneraria.Api: Line Coverage = 90.14%, Branch Coverage = 63.17%

Evidencias portables con rutas relativas verificadas exitosamente.
```

---

### 4. Criterio de Aceptación y DoD
- [x] Cobertura de código en `SistemaFuneraria.Core` $\ge 90\%$ (Alcanzado: **96.22%**).
- [x] Cobertura de código en `SistemaFuneraria.Application` $\ge 85\%$ (Alcanzado: **88.39%**).
- [x] Cero advertencias (`0 Warnings`) y cero errores (`0 Errors`) en compilación `Release`.
- [x] Cero pruebas marcadas con `Skip`.
- [x] Cero rutas absolutas en `docs/evidencias/`.
