# Evidencia Green Test — SF-087: Abono a Capital, Liquidación y 3 Tarjetas en /Caja

## Implementación
- Endpoints en `src/SistemaFuneraria.Api/Controllers/V1/CobranzaController.cs`: `POST /api/v1/cobranza/abono-capital` y `POST /api/v1/cobranza/liquidacion` con idempotencia `X-Idempotency-Key` y `[Authorize(Roles = "Administrador,Cajero")]`.
- Conectores Web en `src/SistemaFuneraria.Web/wwwroot/js/api/cobranzaApi.js` (`aplicarAbonoCapital`, `aplicarLiquidacion`) y `src/SistemaFuneraria.Web/wwwroot/js/pages/cajaPage.js` habilitando las 3 tarjetas de operación (`Mensualidad`, `Abono a capital`, `Liquidación`).

## Verificación
- Validado en `Milestone9WebTests.CajaWeb_DebeConectarTresTarjetasDeOperacion` y pruebas HTTP en `AbonoYLiquidacionControllerTests`.
