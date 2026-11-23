# Evidencia Green Test — SF-098: UAT End-to-End Escenarios ST-01 a ST-05

## Suite UAT (`tests/SistemaFuneraria.Tests/Integration/Api/UatEscenariosSistemaTests.cs`)
- **`ST-01` (Contado):** Registro de venta de contado, bloqueo del lote (`Vendido`) y rechazo `409 Conflict` ante doble venta.
- **`ST-02` (Crédito + Abono):** Contrato a 12 meses ($120,000, 20% enganche), 3 mensualidades pagadas y abono a capital de `$10,000.00`.
- **`ST-03` (Liquidación anticipada):** Liquidación sin penalización cerrando `SaldoPosterior == $0.00`.
- **`ST-04` (Concurrencia):** Peticiones paralelas `Task.WhenAll` sobre misma cuota → exactamente un `200 OK` y un `409 Conflict`.
- **`ST-05` (CRUDs Administrador):** Actualización de sucursal, creación de `Cajero` con autenticación inmediata y cancelación administrativa liberando lote.

## Resultado
- Prueba integral `UAT_EscenariosST01aST05_DebenCompletarExitosamente` aprobada en verde (`0` defectos SEV-1/SEV-2).
