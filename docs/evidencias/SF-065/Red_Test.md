# Evidencia Red Test — SF-065: Suite QA HTTP CP-02 / ST-01 (Venta de Contado E2E y Bloqueo de Lote)

## Estado Inicial (Antes del Cambio)
No existía la suite `VentasContadoControllerTests.cs` verificando por HTTP real el ciclo completo `CP-02` / `ST-01` (lote `Disponible` → cliente registrado → `POST /api/v1/ventas/contado` → lote `Vendido` → rechazo `409 Conflict` en doble venta y liberación `Disponible` al cancelar como Administrador).

## Fallo Esperado
```text
No test matches filter FullyQualifiedName~Integration.Api.VentasContadoControllerTests
```
