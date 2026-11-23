# Evidencia Red Test — SF-064: Vista Preliminar de Contrato Emitido y Comprobante Imprimible de Contado

## Estado Inicial (Antes del Cambio)
1. `Pages/Ventas/Partials/_Paso4Confirmacion.cshtml` y `paso4Confirmacion.js` usaban datos demo (`CTR-2026-0190`) con `setTimeout` sin consumir `ventasApi.registrarVentaContado`, sin selector de método de pago ni manejo de `409 Conflict`.
2. `Pages/Contratos/Detalle.cshtml` solo mostraba datos estáticos de amortización y no consultaba `GET /api/v1/ventas/{id}` al recibir `?id={ventaId}`, ni mostraba estado `404` ni comprobante específico de `Contado`.

## Fallo Esperado
```text
FAIL: Paso4Confirmacion_DebeConsumirVentasApiYSoportarConflicto409
FAIL: ContratosDetalle_DebeIncluirComprobanteContadoPrintY404
```
