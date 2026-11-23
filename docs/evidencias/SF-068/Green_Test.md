# Evidencia Green Test — SF-068: Servicio ICarteraContratosService

## Implementación (`src/SistemaFuneraria.Application/Servicios/Contratos/CarteraContratosService.cs`)
- `ObtenerCarteraAsync`: calcula los 4 KPIs (`TotalActivos`, `MontoEnCartera`, `TotalAlCorriente`, `PorcentajeAlCorriente`, `TotalConAtraso`, `MontoPorRecuperar`, `TotalLiquidadosAnio`), filtra por `tab` (`Todos`, `Activos`, `Vencidos`, `Liquidados`) y `buscar`, proyectando `AccionSugerida = "Cobrar"` para contratos vencidos y `SaldoInsoluto = 0` / `"Liquidado"` para liquidados.
- `ObtenerDetalleAsync`: resuelve por UUID o `FolioContrato` incluyendo financiamiento y amortización.

## Pruebas (`tests/SistemaFuneraria.Tests/Unit/Application/CarteraContratosServiceTests.cs`)
- `ObtenerCarteraAsync_DebeCalcularKpisYFiltrarTabsCorrectamente`: en verde.
