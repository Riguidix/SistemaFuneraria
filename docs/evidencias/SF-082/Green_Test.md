# Evidencia Green Test — SF-082: Recibo Oficial /Caja/Recibo

## Implementación
- `src/SistemaFuneraria.Web/Pages/Caja/Recibo.cshtml`, `Recibo.cshtml.cs` y `src/SistemaFuneraria.Web/wwwroot/js/pages/reciboPagoPage.js`.
- Membrete institucional, folio `PAG-YYYY-XXXXXX`, desglose `Saldo anterior`, `Aplicado a Capital`, `Aplicado a Interés`, `Importe total pagado`, `Nuevo saldo insoluto` y hoja de impresión limpia (`@media print`).

## Verificación
- `CajaYReciboWebTests.CajaRecibo_DebeRenderizarDesgloseEImpresion` en verde. Pruebas manuales de navegador reservadas al usuario.
