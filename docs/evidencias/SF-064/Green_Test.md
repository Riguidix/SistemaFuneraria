# Evidencia Green Test — SF-064: Vista Preliminar de Contrato Emitido y Comprobante Imprimible de Contado

## Cambios Implementados
1. **`src/SistemaFuneraria.Api/Program.cs`:** Registro de `JsonStringEnumConverter` en `AddControllers()` para aceptar enums como string (`MetodoPago`, `EstadoVenta`) desde frontend.
2. **Paso 4 del Wizard (`SF-062`):** `Nueva.cshtml`, `_Paso4Confirmacion.cshtml` y `paso4Confirmacion.js` conectados a `ventasApi.registrarVentaContado()`, limpiando `wizardState`, manejando `409 Conflict` con botón `"Elegir otro lote"` y redirigiendo a `/Contratos/Detalle?id={ventaId}`.
3. **`Pages/Contratos/Detalle.cshtml` + `contratoDetallePage.js` (`SF-064`):**
   - Consulta real `GET /api/v1/ventas/{id}`, muestra tarjeta `#comprobanteContadoCard` en modalidad `Contado`, estilos `@media print`, vista `#contratoNoEncontrado` (`404`) y modal Admin conectado a `ventasApi.cambiarEstadoVenta` / `cancelarVenta`.
4. **Pruebas:** `VentaContadoDetalleWebTests` + validación en Node de helpers JS en verde.
