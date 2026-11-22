# Evidencia Fase RED — SF-063

**Ticket:** [SF-063] CRUD Admin de Ventas/Contratos (`PATCH /api/v1/ventas/{id}/estado`, `DELETE /api/v1/ventas/{id}` con Liberación de Lote) [#129](https://github.com/Riguidix/SistemaFuneraria/issues/129)  
**Fecha:** 2026-11-22  
**Autor:** Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`)  
**Metodología:** TDD Estricto ("Cero Fantasía") — Fase RED  

---

## 1. Comando Ejecutado

```powershell
$env:PATH = "C:\Users\Billy\AppData\Local\Microsoft\dotnet;$env:PATH"; $env:DOTNET_ROOT = "C:\Users\Billy\AppData\Local\Microsoft\dotnet"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~VentaAdminCrudServiceTests|FullyQualifiedName~VentasAdminControllerTests" -v normal
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
  Failed SF-063 (BDD): Administrador cancela venta errónea por PATCH y lote vuelve a estar Disponible [58 ms]
  Error Message:
   System.NotImplementedException : SF-063: Fase RED.
  Failed SF-063: Motivo menor a 10 caracteres debe retornar error de validación [707 ms]
  Error Message:
   System.NotImplementedException : SF-063: Fase RED.
  Failed SF-063: DELETE venta con motivo válido cancela y libera lote [25 ms]
  Error Message:
   System.NotImplementedException : SF-063: Fase RED.
  Failed SF-063: PATCH con motivo menor a 10 caracteres debe retornar 400 BadRequest [1 ms]
  Error Message:
   System.NotImplementedException : SF-063: Fase RED.
  Failed SF-063 (BDD): Cancelar venta errónea pasa contrato a Cancelado y regresa lote a Disponible [28 ms]
  Error Message:
   System.NotImplementedException : SF-063: Fase RED.
  Failed SF-063: Venta inexistente debe retornar No Encontrado [< 1 ms]
  Error Message:
   System.NotImplementedException : SF-063: Fase RED.
  Passed SF-063: Métodos CambiarEstado y CancelarVenta deben exigir rol Administrador [1 ms]

Test Run Failed.
Total tests: 7
     Passed: 1
     Failed: 6
 Total time: 2.0963 Seconds
```

---

## 3. Conclusión Fase RED
Se verifica que la lógica de cambio de estado administrativo, cancelación y endpoints HTTP fallan controladamente con `NotImplementedException`, garantizando la validez de la prueba antes de la implementación de la Fase GREEN.
