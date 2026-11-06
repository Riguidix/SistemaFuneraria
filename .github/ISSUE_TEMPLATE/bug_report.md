---
name: "Reporte de Incidencia (INC-XXX)"
about: "Reportar un defecto, fallo de prueba o regresión en el sistema."
title: "[INC-___] Descripción corta del defecto"
labels: ["bug", "incidencia"]
assignees: []
---

### 1. Metadatos de la Incidencia (docs/06-bitacora-incidencias.md)
- **Incidencia ID:** `INC-___`
- **Caso de Prueba Relacionado:** CP-___ (o RNF-___ / RN-___ / DT-___)
- **Severidad:** SEV1-Crítica (Blocker) | SEV2-Mayor | SEV3-Menor
- **Prioridad:** P0 (Inmediata) | P1 (Alta) | P2 (Media)
- **Story Points corrección:** 1 | 2 | 3
- **Rama de corrección:** `bugfix/INC-___-slug-descriptivo`
- **Desarrollador asignado:** @___
- **Revisor asignado:** @___

---

### 2. Descripción del Problema
<!-- Explica qué ocurrió y bajo qué condiciones específicas -->

---

### 3. Pasos para Reproducir
1. Paso 1...
2. Paso 2...
3. Paso 3...

---

### 4. Comportamiento Observado vs. Esperado
- **Comportamiento Esperado:** [Lo que la regla matemática o de negocio dicta]
- **Comportamiento Real:** [Lo que arrojó el sistema erróneamente]

---

### 5. Evidencia de Falla (Fase ROJO)
```text
[Pegar traza de error de terminal o aserción fallida de xUnit]
```

---

### 6. Plan de Corrección y Evidencia VERDE
- [ ] Prueba unitaria o de integración de regresión escrita en `SistemaFuneraria.Tests`
- [ ] Evidencia **VERDE** documentada en `docs/06-bitacora-incidencias.md`
