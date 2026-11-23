# Evidencia Green Test — SF-093: Suite QA HTTP CP-07 y CP-08

## Pruebas (`tests/SistemaFuneraria.Tests/Integration/Api/AbonoYLiquidacionControllerTests.cs`)
1. `CP07_AbonoCapital_10k_DebeReducirSaldoYRecalcularCuotas`: lote de 2 gavetas (`$120,000.00`), 20% enganche, 12 meses; paga 3 cuotas, aplica abono de `$10,000.00` y valida `MontoCapital = $10,000.00`, `MontoInteres = $0.00`, reducción exacta de `SaldoPosterior`, recálculo de cuotas `4..11` menor a la cuota original y conservación de capital (`$96,000.00`).
2. `CP08_LiquidacionAnticipada_ConDescuento10Porciento_DebeCerrarContrato`: liquida anticipadamente dejando `SaldoPosterior = $0.00`, `EstadoVisual = "Liquidado"`, 12 cuotas `Pagada` y rechazo `409 Conflict` ante segunda liquidación.
3. `AbonoCapital_ConCuotaVencida_DebeRetornar409Conflict`: valida bloqueo `409` sobre `CTR-2024-0118`.
4. `AbonoCapital_Validaciones_DebeRetornar400BadRequest`: rechazo `400` sin clave de idempotencia o datos inválidos.

Todas las pruebas de `AbonoYLiquidacionControllerTests` en verde (4/4).
