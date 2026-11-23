# Evidencia Green Test — SF-083: Suite QA HTTP y Estrés Concurrente ST-04

## Pruebas (`tests/SistemaFuneraria.Tests/Integration/Api/CobranzaConcurrenciaTests.cs`)
1. `CobranzaHttp_DebeValidarRolesIdempotenciaYReciboOficial`:
   - `Vendedor` → `403 Forbidden`, sin `X-Idempotency-Key` → `400 Bad Request`.
   - `Cajero` cobra cuota `#1` → `200 OK`, reintento con misma `ClaveIdempotencia` → mismo folio `PAG-*` sin doble descuento, consulta `GET /api/v1/cobranza/recibos/{folio}` y `GET /api/v1/cobranza/resumen-caja`.
2. `ST04_CobroConcurrenteTaskWhenAll_DebePermitirUnoYRechazarOtroCon409`:
   - Dos peticiones HTTP paralelas (`Task.WhenAll`) sobre el mismo contrato y cuota `#1` con distinta `ClaveIdempotencia` resultan en exactamente un `200 OK` y un `409 Conflict`.

## Resultado Final
```bash
dotnet build --configuration Release
dotnet test --configuration Release --no-build --filter "FullyQualifiedName~Cobranza|FullyQualifiedName~Caja|FullyQualifiedName~Pago|FullyQualifiedName~Venta|FullyQualifiedName~Contrato"
```
Compilación sin advertencias ni errores (`0/0`) y **119/119** pruebas en verde.
