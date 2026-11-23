# Evidencia QA: Fase VERDE (GREEN) — SF-096

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-096 |
| **Título del Ticket** | Auditoría Visual Pixel-Perfect (13 Capturas Figma "Sistema Funerario" + Login) y Accesibilidad WCAG 2.2 AA |
| **Milestone** | Milestone 10 (v1.0.0) |
| **Carril** | Carril 3 (Web UI) / Carril 4 (QA Gate) |
| **Fecha de Certificación** | 23/11/2026 |
| **Responsable QA** | Edgar Acevedo (Carril 4 — DevOps & QA) |

---

### 1. Resumen de Implementación y Certificación
- Auditoría integral de las 13 pantallas del diseño oficial de Figma:
  - Captura 1: Dashboard y Resumen Ejecutivo.
  - Captura 2: Catálogo e inventario de Lotes.
  - Capturas 3 y 4: Directorio y Alta de Clientes.
  - Capturas 5 a 8: Wizard de Nueva Venta (Pasos 1 a 4).
  - Captura 9: Cartera de Contratos y Estado de Cuenta.
  - Captura 10: Módulo de Caja y Cobranza.
  - Captura 11: Reportes Analíticos e Indicadores.
  - Capturas 12 y 13: Configuración de Sucursal y Planes.
  - Login y recuperación de acceso con lockout y jitter.
- Verificación de tokens de diseño oficiales (`--forest`, `--gold`, `--rose`, `--cream`).
- Evaluación de accesibilidad WCAG 2.2 AA: contraste de color $\ge 4.5:1$, jerarquía tipográfica accesible y navegación por teclado.

---

### 2. Comandos de Verificación
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~Figma|FullyQualifiedName~WizardPasos" --no-build
```

### 3. Criterios de Aceptación Cumplidos
- [x] Fidelidad visual pixel-perfect frente a especificación de Figma.
- [x] 0 errores críticos de accesibilidad WCAG 2.2 AA.
- [x] Consistencia completa de tokens de estilo y componentes UI.
