# Evidencia Green Test — SF-088: Dashboard Resumen y Reportes Analíticos

## Implementación
- `src/SistemaFuneraria.Application/Servicios/Dashboard/DashboardReportesService.cs`: `ObtenerResumenDashboardAsync` (4 KPIs, serie de 6 meses, disponibilidad por 2/3/4 gavetas y actividad reciente) y `ObtenerIndicadoresReportesAsync` (conversión, ticket promedio, morosidad, desglose por gavetas y meta mensual) con `AsNoTracking()` y protección ante división por cero.
- `src/SistemaFuneraria.Api/Controllers/V1/DashboardReportesController.cs`: `GET /api/v1/dashboard/resumen` y `GET /api/v1/reportes/indicadores` (`[Authorize(Roles = "Administrador")]`).
- Conectores Web: `reportesApi.js`, `resumenPage.js` (`/Resumen/Index.cshtml`) y `reportesPage.js` (`/Reportes/Index.cshtml`).

## Verificación
- `DashboardReportesServiceTests.cs`, `VencimientosYReportesTests.cs` y `Milestone9WebTests.cs` en verde.
