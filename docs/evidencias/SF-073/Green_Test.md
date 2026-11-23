# Evidencia Green Test — SF-073: Conector Completo del Wizard `/Ventas/Nueva` Modalidad Crédito

## Implementación
- `src/SistemaFuneraria.Web/wwwroot/js/ventas/paso4Confirmacion.js`: `construirPayloadVentaFinanciada(draft)` conectado a `ventasApi.registrarVentaFinanciada` (`POST /api/v1/ventas/financiada`) y redirección automática a `/Contratos/Detalle?id={ventaId}`.
- Verificado mediante `ContratosYFinanciadaWebTests.Paso4YContratoDetalle_DebenSoportarCreditoYAmortizacion` y pruebas HTTP en `VentasFinanciadasControllerTests`.
