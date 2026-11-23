# Evidencia Green Test — SF-067: Endpoints Venta Financiada y Amortización

## Implementación (`src/SistemaFuneraria.Api/Controllers/V1/VentasController.cs`)
- `POST /api/v1/ventas/financiada` (`RegistrarFinanciada`) → `201 Created` (`ContratoFinanciadoResumenDto`), `400 BadRequest`, `404 NotFound`, `409 Conflict`.
- `GET /api/v1/ventas/{id:guid}/amortizacion` (`ObtenerAmortizacion`) → `200 OK` (`CuotaAmortizacionContratoDto[]`) o `404 NotFound`.
- Conector Web en `src/SistemaFuneraria.Web/wwwroot/js/ventas/paso4Confirmacion.js` (`SF-073`) enlazado con `ventasApi.registrarVentaFinanciada`.

## Verificación
Verificado mediante `VentasFinanciadasControllerTests` y `ContratosYFinanciadaWebTests` en verde.
