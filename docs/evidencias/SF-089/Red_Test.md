# Evidencia TDD / Frontend UI: Fase ROJA (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-089 |
| **Título del Ticket** | Dashboard Operativo Resumen (Captura 1 de Figma): Saludo, 4 KPIs, Gráfica Ventas vs Cobranza, Disponibilidad y Actividad Reciente |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 15/11/2026 23:00 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaDashboard |

---

### 1. Resumen de la Falla Inicial (RED)
Antes de la implementación, la página principal `Pages/Index.cshtml` contenía un placeholder genérico que redirigía a `/Cuenta/Login` y carecía de los componentes requeridos por la **Captura 1 de Figma**:
1. Sin encabezado operativo ni saludo personalizado *"Buenos días, Andrea"*.
2. Sin las 4 tarjetas KPI de cartera (`Ventas del mes $486,500`, `Cobranza mensual $238,240`, `Contratos activos 128`, `Pagos vencidos 8`).
3. Sin la gráfica de barras de Ventas y Cobranza de los últimos 6 meses.
4. Sin la dona central de disponibilidad de lotes (64 disponibles: 28, 21, 15) ni barra de ocupación al 64%.
5. Sin la tabla de actividad reciente con movimientos de caja.

### 2. Ejecución del Test en Rojo
```text
[xUnit.net] DashboardPageTests.Dashboard_OnGet_DebeCargarKpisGraficasYActividadRecienteConformeFigma [FAIL]
  Mensaje de error:
   Expected pageModel.VentasMes to be 486500m because "Ventas del mes según Figma Captura 1", but found 0m.
```
Resultado: **Prueba fallida (Fase ROJA confirmada).**
