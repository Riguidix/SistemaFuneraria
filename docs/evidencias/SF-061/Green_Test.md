# Evidencia Fase GREEN — SF-061

**Ticket:** [SF-061] `VentasController` (`POST /api/v1/ventas/contado`, `GET /api/v1/ventas/{id}`) y Wrapper `ventasApi.js` [#127](https://github.com/Riguidix/SistemaFuneraria/issues/127)  
**Fecha:** 2026-11-22  
**Autor:** Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`)  
**Metodología:** TDD Estricto ("Cero Fantasía") — Fase GREEN  

---

## 1. Comando Ejecutado

```powershell
$env:PATH = "%DOTNET_HOME%;$env:PATH"; $env:DOTNET_ROOT = "%DOTNET_HOME%"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~VentasControllerTests|FullyQualifiedName~VentasApiWrapperTests" -v normal
```

---

## 2. Salida de Terminal (100% Exitoso)

```text
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.55]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.65]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.65]   Starting:    SistemaFuneraria.Tests
  Passed SF-061: ventasApi.js debe existir físicamente en wwwroot/js/api/ [324 ms]
  Passed SF-061: ventasApi.js debe utilizar la ruta base /api/v1/ventas [< 1 ms]
  Passed SF-061: ventasApi.js debe soportar UMD (window.ventasApi y module.exports) [< 1 ms]
  Passed SF-061: ventasApi.js debe exponer registrarVentaContado, registrarVentaFinanciada y obtenerVentaPorId [< 1 ms]
  Passed SF-061: Registrar contado con ClienteId o LoteId vacío responde 400 BadRequest [381 ms]
  Passed SF-061 (BDD): Intento de vender por API un lote ya Vendido responde 409 Conflict [408 ms]
[xUnit.net 00:00:01.58]   Finished:    SistemaFuneraria.Tests
  Passed SF-061: Registrar venta contado con datos válidos responde 201 Created [79 ms]
  Passed SF-061: ObtenerPorId con venta existente responde 200 OK [33 ms]
  Passed SF-061: ObtenerPorId con venta inexistente responde 404 NotFound [< 1 ms]

Test Run Successful.
Total tests: 9
     Passed: 9
 Total time: 1.8880 Seconds
```

---

## 3. Escenarios BDD y Casos Límite Verificados

1. **Escenario BDD (Gherkin):**
   - Intento de venta de contado sobre un lote en estado `Vendido` (`LM-A-014`) es rechazado con código HTTP `409 Conflict` y `ProblemDetails` con detalle `"El lote seleccionado ya fue vendido"`.
2. **Registro Exitoso de Contado (CP-02 / ST-01):**
   - Solicitud válida con cliente y lote disponible responde HTTP `201 Created` (`CreatedAtAction`) con `ContratoResumenDto`, folio `CTR-YYYY-XXXX` y estado `Liquidado`.
3. **Validación de Identificadores (Edge Cases):**
   - Envío de `ClienteId` o `LoteId` vacíos (`Guid.Empty`) es interceptado respondiendo `400 Bad Request`.
4. **Consulta por Identificador (GET /api/v1/ventas/{id}):**
   - Venta existente retorna HTTP `200 OK` con datos del contrato.
   - Venta inexistente retorna HTTP `404 NotFound`.
5. **Wrapper Cliente JS (`ventasApi.js`):**
   - Existencia física en `wwwroot/js/api/ventasApi.js`.
   - Exposición de métodos `registrarVentaContado`, `registrarVentaFinanciada` y `obtenerVentaPorId`.
   - Rutas canónicas `/api/v1/ventas`, `/contado`, `/financiada`.
   - Soporte universal UMD (`window.ventasApi` y `module.exports`).
