# Evidencia Green Test — SF-069: ContratosController y contratosApi.js

## Implementación
- `src/SistemaFuneraria.Api/Controllers/V1/ContratosController.cs`:
  - `GET /api/v1/contratos?buscar=&tab=Todos&page=1&pageSize=10`
  - `GET /api/v1/contratos/{folioOrId}` (resuelve `Guid` o `CTR-YYYY-XXXX`)
  - `GET /api/v1/contratos/{id:guid}/amortizacion`
- `src/SistemaFuneraria.Web/wwwroot/js/api/contratosApi.js`: `obtenerCartera`, `obtenerDetalleContrato`, `obtenerTablaAmortizacion`, `cambiarEstadoAdmin`.

## Verificación
Verificado vía `VentasFinanciadasControllerTests` (`ContratosCartera_DebeFiltrarTabsYResolverPorFolioYGuid`) y carga directa en Node.js.
