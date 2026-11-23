# Evidencia Green Test — SF-070: Pantalla /Contratos/Index conectada a Cartera real

## Implementación
- `src/SistemaFuneraria.Web/Pages/Contratos/Index.cshtml`: mantiene el diseño Figma (*Captura 9*) con IDs para los 4 KPIs, tabs (`Todos`, `Activos`, `Vencidos`, `Liquidados`), buscador y enlace `Cobrar` → `/Caja?contrato={folio}` / `Detalle` → `/Contratos/Detalle?folio={folio}`.
- `src/SistemaFuneraria.Web/wwwroot/js/pages/contratosIndexPage.js`: consume `contratosApi.obtenerCartera`.

## Pruebas (`tests/SistemaFuneraria.Tests/Unit/Web/ContratosYFinanciadaWebTests.cs`)
- `ContratosIndex_DebeConectarKpisTabsYContratosApi` y `Paso4YContratoDetalle_DebenSoportarCreditoYAmortizacion` en verde.
- Validación visual en navegador pendiente por solicitud explícita del usuario (pruebas browser a cargo del usuario).
