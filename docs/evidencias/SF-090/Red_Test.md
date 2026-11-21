# Evidencia TDD / Frontend UI: Fase ROJA (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-090 |
| **Título del Ticket** | Pantalla Oficial de Reportes y Analítica (Captura 11 de Figma): Filtros, 3 KPIs, Barras por Lote y Dona Meta Mensual |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 15/11/2026 23:40 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaReportes |

---

### 1. Resumen de la Falla Inicial (RED)
Antes de la implementación, la ruta `/Reportes` no existía en el proyecto (`Pages/Reportes/Index.cshtml` inexistente), el ítem "Reportes" en el Sidebar estaba marcado como inerte sin navegación y no existían los componentes analíticos de la **Captura 11 de Figma**:
1. Sin encabezado operativo ni kicker `ANALÍTICA`.
2. Sin barra de filtros de periodo (`Noviembre 2025`) y sucursal (`Jardines del Valle`).
3. Sin las 3 tarjetas KPI superiores (`Conversión 32.8%`, `Ticket promedio $147,420`, `Índice de morosidad 6.2%`).
4. Sin las 3 barras horizontales en verde bosque de ventas por tipo de lote (`Lote 2 gavetas $198,000`, `Lote 3 gavetas $168,500`, `Lote 4 gavetas $120,000`).
5. Sin la dona circular de meta mensual al `81%` ni el resumen con acumulado (`$486,500`) y objetivo (`$600,000`).

### 2. Ejecución del Test en Rojo
```text
[xUnit.net] ReportesPageTests.Reportes_OnGet_DebeCargarKpisAnaliticaYMetasConformeFigmaCaptura11 [FAIL]
  Mensaje de error:
   The type or namespace name 'Reportes' does not exist in the namespace 'SistemaFuneraria.Web.Pages'
```
Resultado: **Prueba fallida (Fase ROJA confirmada).**
