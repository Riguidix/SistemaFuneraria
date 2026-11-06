---
name: "Ticket de Desarrollo (SF-XXX)"
about: "Crear una tarea de desarrollo o funcionalidad de negocio para el sprint y GitHub Projects."
title: "[SF-___] Título descriptivo del ticket"
labels: ["desarrollo", "sprint"]
assignees: []
---

### 1. Metadatos de Gobernanza y GitHub Projects
- **Ticket ID:** `SF-___`
- **Milestone:** Milestone 1 (Sprint 1) | Milestone 2 (Sprint 2) | Milestone 3 (Sprint 3) | Milestone 4 (Sprint 4)
- **Story Points:** 1 | 2 | 3 | 5 | 8
- **Prioridad:** P0 (Bloqueador) | P1 (Alta) | P2 (Media) | P3 (Baja)
- **Capa arquitectónica:** Core | Application | Infrastructure | Web | Tests
- **Requisitos trazados:** RF-___, RN-___, CP-___
- **Desarrollador asignado:** @___
- **Revisor asignado:** @___
- **Rama objetivo:** `feature/SF-___-slug-descriptivo`
- **Blast Radius estimado:** ≤ 250 LOCs (Límite estricto PR: ≤ 300 LOCs)

---

### 2. Contexto y Valor de Negocio
<!-- Explica qué problema de negocio resuelve y qué actor lo utiliza -->

---

### 3. Archivos Involucrados
<!-- Especifica las rutas de archivos que este ticket debe crear o modificar según docs/08-backlog.md -->
- `src/...`
- `tests/...`

---

### 4. Dependencias
- **Bloqueado por:** Ninguno <!-- o #ID -->
- **Desbloquea a:** Ninguno <!-- o #ID -->

---

### 5. Criterios de Aceptación (Gherkin BDD)
- [ ] **Escenario 1: Flujo Principal (Happy Path)**
  - **Dado** [precondición o estado inicial]
  - **Cuando** [acción ejecutada]
  - **Entonces** [resultado verificable]
- [ ] **Escenario 2: Caso Límite o Error**
  - **Dado** [condición inválida o frontera]
  - **Cuando** [intento de acción]
  - **Entonces** [código de error o mensaje de validación esperado]

---

### 6. Plan de Contingencia y Rollback (Rollback-First Obligatorio)
- **Comando o estrategia de reversión:** `[down(), git revert, etc.]`
- **Seguridad de datos:** Confirmación de no dejar datos huérfanos ni estados inconsistentes.

---

### 7. Evidencia TDD Requerida
- [ ] Evidencia **ROJO** generada en `docs/evidencias/SF-___/Red_Test.md`
- [ ] Evidencia **VERDE** generada en `docs/evidencias/SF-___/Green_Test.md`
