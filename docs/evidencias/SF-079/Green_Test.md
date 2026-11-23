# Evidencia Green Test — SF-079: Wrapper JS cobranzaApi.js y Conexión /Caja

## Implementación
- `src/SistemaFuneraria.Web/wwwroot/js/api/cobranzaApi.js`: genera `crypto.randomUUID()` para `X-Idempotency-Key` y `claveIdempotencia` y expone `obtenerResumenCaja`, `obtenerContratoParaCaja`, `aplicarPagoMensualidad` y `obtenerRecibo`.
- `src/SistemaFuneraria.Web/wwwroot/js/pages/cajaPage.js` conectado a `src/SistemaFuneraria.Web/Pages/Caja/Index.cshtml`.

## Verificación
- Validado en Node.js (`UUID_LEN: 36`) y mediante `CajaYReciboWebTests.cs` en verde.
