# Evidencia Green Test — SF-077: CobranzaService (Pagos Ordinarios, Concurrencia e Idempotencia)

## Implementación (`src/SistemaFuneraria.Application/Servicios/Cobranza/CobranzaService.cs`)
- `ObtenerResumenCajaAsync`: calcula `CorteDelDiaDto` (`SaldoInicial = $2,000.00`, `PagosRecibidosHoy`, `IngresosDelDia`, `TotalEnCaja`) y conteo `PagosVencidosCount`.
- `ObtenerContratoParaCajaAsync`: localiza por folio `CTR-YYYY-XXXX` o `Guid` devolviendo cuota vigente y liquidación estimada.
- `AplicarPagoMensualidadAsync`: transacción ACID, idempotencia por `ClaveIdempotencia`, guardia de concurrencia en curso, manejo de `DbUpdateConcurrencyException` (`[CONFLICTO]` `409`), folio `PAG-YYYY-XXXXXX` y actualización de `SaldoInsoluto`.

## Pruebas (`tests/SistemaFuneraria.Tests/Unit/Application/CobranzaServiceTests.cs`)
- 4 pruebas unitarias en verde (cobro ordinario + corte del día, idempotencia, monto inválido y `DbUpdateConcurrencyException`).
