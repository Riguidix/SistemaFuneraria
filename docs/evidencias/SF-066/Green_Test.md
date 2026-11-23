# Evidencia Green Test — SF-066: VentaService.RegistrarVentaFinanciadaAsync (ACID)

## Implementación (`src/SistemaFuneraria.Application/Servicios/Ventas/VentaService.cs`)
- `RegistrarVentaFinanciadaAsync`: valida porcentaje de enganche (`10`, `20`, `30`) y plazo (`12`, `18`, `24`, `48`), verifica cliente y lote `Disponible` (`409` si vendido/mantenimiento), ejecuta la amortización francesa exacta mediante `CalculadoraFinanciamiento` y `GeneradorAmortizacion`, persiste atómicamente `Venta` (`ModalidadVenta.Financiado`), `Financiamiento`, sus `N` `Mensualidades` y marca el lote como `Vendido`.

## Pruebas (`tests/SistemaFuneraria.Tests/Unit/Application/VentaFinanciadaServiceTests.cs`)
- `RegistrarVentaFinanciadaAsync_Con20PorcientoY24Meses_DebePersistirTodoAtomicamente`
- `RegistrarVentaFinanciadaAsync_SobreLoteVendido_DebeRetornarConflicto`
- `RegistrarVentaFinanciadaAsync_ParametrosInvalidos_DebeRetornarFallo` (2 casos `InlineData`)

Todas las pruebas unitarias en verde (0 errores).
