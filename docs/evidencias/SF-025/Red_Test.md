# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-025 |
| **Título del Ticket** | Pantalla Figma `/ParametrosUi` (Captura 13) y hoja `sistema-tokens.css` |
| **Caso de Prueba Relacionado** | RNF-01, RNF-06, RN-12 / Captura 13 Figma |
| **Fecha y Hora de Ejecución** | 15/11/2026 19:54 |
| **Rama Git** | `feature/SF-025-figma-parametros-ui-tokens` |

---

### 1. Objetivo de la Prueba (Estado Inicial RED)
Verificar previo a la integración de `sistema-tokens.css` y `ParametrosUi.cshtml` desde `feature/web` que `dev` no contaba con las variables CSS institucionales (`--forest`, `--forest-2`, `--gold`, `--rose`, `--canvas`, `--ink`) ni con la vista `/ParametrosUi`.

### 2. Escenario Esperado en Falla
Al ejecutar `dotnet test --filter "Ticket=SF-025"` antes de incorporar los archivos de UI, los archivos `wwwroot/css/sistema-tokens.css` y `Pages/ParametrosUi.cshtml` no existían en `dev`, provocando la falla de aserción (`File.Exists(...)` = `false`).
