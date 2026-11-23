# Evidencia Green Test — SF-074: Suite QA HTTP Ventas Financiadas y Cartera

## Pruebas (`tests/SistemaFuneraria.Tests/Integration/Api/VentasFinanciadasControllerTests.cs`)
1. `RegistrarFinanciada_Lote150k20Porciento24Meses_DebePersistir24CuotasExactasYAparecerEnCartera`:
   - `POST /api/v1/ventas/financiada` sobre lote de `$150,000.00` (3 gavetas), `20%` enganche y `24` meses → `201 Created`.
   - `GET /api/v1/ventas/{id}/amortizacion` devuelve 24 cuotas `Pendiente`, suma de capital `= $120,000.00` y saldo final `= $0.00` (cuotas 1..23 de `$6,107.50`).
   - Verifica aparición en `GET /api/v1/contratos?tab=Activos` y rechazo `409 Conflict` en segunda venta sobre el mismo lote.
2. `ContratosCartera_DebeFiltrarTabsYResolverPorFolioYGuid`:
   - Verifica `GET /api/v1/contratos?tab=Vencidos&buscar=Elena` (`CTR-2024-0118` con `Estado == "Vencido"` y `AccionSugerida == "Cobrar"`), resolución por folio y UUID, y `404` para folio inexistente.
3. `RegistrarFinanciada_ParametrosFueraDeMatriz_DebeRetornar400`:
   - Enganche `15%` y plazo `36` meses retornan `400 Bad Request`.

## Comando y Resultado Final
```bash
dotnet build --configuration Release
dotnet test --configuration Release --no-build --filter "FullyQualifiedName~Venta|FullyQualifiedName~Contrato|FullyQualifiedName~Cartera|FullyQualifiedName~Paso4|FullyQualifiedName~Wizard"
```
Compilación sin advertencias ni errores (`0/0`) y **96/96** pruebas superadas.
