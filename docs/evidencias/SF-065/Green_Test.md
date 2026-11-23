# Evidencia Green Test — SF-065: Suite QA HTTP CP-02 / ST-01 (Venta de Contado E2E y Bloqueo de Lote)

## Pruebas Implementadas (`tests/SistemaFuneraria.Tests/Integration/Api/VentasContadoControllerTests.cs`)
1. `ST01_CP02_VentaContadoE2E_DebeRetornar201MarcarLoteVendidoYRechazarDobleVenta409`:
   - Selecciona lote `Disponible` vía `GET /api/v1/lotes`, crea cliente vía `POST /api/v1/clientes` y registra venta Contado (`POST /api/v1/ventas/contado` con `metodoPago: "Efectivo"`).
   - Valida `201 Created`, `Estado == "Liquidado"`, `Modalidad == "Contado"`, folios `CTR-*` y `PAG-*`, lote actualizado a `Vendido` vía `GET /api/v1/lotes/{id}`, consulta `GET /api/v1/ventas/{id}` y rechazo `409 Conflict` al reintentar venta sobre el mismo lote.
2. `CancelacionAdmin_DebeBloquearVendedor403RechazarMotivoCorto400YLiberarLoteDisponible`:
   - Verifica `403 Forbidden` para `Vendedor`, `400 Bad Request` para motivo menor a 10 caracteres, y `200 OK` para `Administrador` devolviendo contrato `Cancelado` y lote nuevamente `Disponible`.
3. `SeguridadYNotFound_DebeRetornar401Y404`:
   - Verifica `401 Unauthorized` sin token y `404 NotFound` para UUID inexistente.

## Comando y Resultado
```bash
dotnet test SistemaFuneraria.sln --configuration Release --filter "FullyQualifiedName~VentasContadoControllerTests" --verbosity minimal
```
Todas las pruebas de integración HTTP en verde (0 fallos, 0 advertencias).
