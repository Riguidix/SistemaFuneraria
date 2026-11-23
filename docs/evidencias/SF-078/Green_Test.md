# Evidencia Green Test — SF-078: CobranzaController

## Implementación (`src/SistemaFuneraria.Api/Controllers/V1/CobranzaController.cs`)
- `[Authorize(Roles = "Administrador,Cajero")]` en `/api/v1/cobranza`.
- `GET /resumen-caja`, `GET /contrato/{folio}`, `POST /mensualidad` (valida `X-Idempotency-Key`), `GET /recibos/{folio}`.

## Verificación
Verificado en `CobranzaConcurrenciaTests`: `Vendedor` recibe `403 Forbidden`, ausencia de clave `X-Idempotency-Key` retorna `400 Bad Request` y `Cajero` aplica pago y consulta recibo con `200 OK`.
