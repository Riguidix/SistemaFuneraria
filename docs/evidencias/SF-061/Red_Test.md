# Evidencia Fase RED — SF-061

**Ticket:** [SF-061] `VentasController` (`POST /api/v1/ventas/contado`, `GET /api/v1/ventas/{id}`) y Wrapper `ventasApi.js` [#127](https://github.com/Riguidix/SistemaFuneraria/issues/127)  
**Fecha:** 2026-11-22  
**Autor:** Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`)  
**Metodología:** TDD Estricto ("Cero Fantasía") — Fase RED  

---

## 1. Comando Ejecutado

```powershell
$env:PATH = "%DOTNET_HOME%;$env:PATH"; $env:DOTNET_ROOT = "%DOTNET_HOME%"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~VentasControllerTests|FullyQualifiedName~VentasApiWrapperTests" -v normal
```

---

## 2. Salida de Terminal (Falla Comprobada)

```text
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:01.07]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:01.18]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:01.19]   Starting:    SistemaFuneraria.Tests
  Failed SF-061: ventasApi.js debe utilizar la ruta base /api/v1/ventas [18 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo '<repo>/src\SistemaFuneraria.Web\wwwroot\js\api\ventasApi.js', but found False.
  Failed SF-061: ventasApi.js debe soportar UMD (window.ventasApi y module.exports) [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo '<repo>/src\SistemaFuneraria.Web\wwwroot\js\api\ventasApi.js', but found False.
  Failed SF-061: ventasApi.js debe existir físicamente en wwwroot/js/api/ [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because el archivo ventasApi.js debe existir en el directorio wwwroot/js/api, but found False.
  Failed SF-061: ventasApi.js debe exponer registrarVentaContado, registrarVentaFinanciada y obtenerVentaPorId [< 1 ms]
  Error Message:
   Expected File.Exists(path) to be true because debe existir el archivo '<repo>/src\SistemaFuneraria.Web\wwwroot\js\api\ventasApi.js', but found False.
  Failed SF-061: Registrar contado con ClienteId o LoteId vacío responde 400 BadRequest [883 ms]
  Error Message:
   System.NotImplementedException : SF-061: Fase RED.
  Failed SF-061 (BDD): Intento de vender por API un lote ya Vendido responde 409 Conflict [335 ms]
  Error Message:
   System.NotImplementedException : SF-061: Fase RED.
  Failed SF-061: Registrar venta contado con datos válidos responde 201 Created [16 ms]
  Error Message:
   System.NotImplementedException : SF-061: Fase RED.
  Failed SF-061: ObtenerPorId con venta existente responde 200 OK [141 ms]
  Error Message:
   System.NotImplementedException : SF-061: Fase RED.
  Failed SF-061: ObtenerPorId con venta inexistente responde 404 NotFound [< 1 ms]
  Error Message:
   System.NotImplementedException : SF-061: Fase RED.

Test Run Failed.
Total tests: 9
     Failed: 9
 Total time: 2.8109 Seconds
```

---

## 3. Conclusión Fase RED
Se verifica que tanto el endpoint REST `VentasController` como el wrapper JavaScript `ventasApi.js` fallan controladamente antes de escribir el código productivo, asegurando la trazabilidad del Quality Gate.
