# Evidencia Fase GREEN — SF-063

**Ticket:** [SF-063] CRUD Admin de Ventas/Contratos (`PATCH /api/v1/ventas/{id}/estado`, `DELETE /api/v1/ventas/{id}` con Liberación de Lote) [#129](https://github.com/Riguidix/SistemaFuneraria/issues/129)  
**Fecha:** 2026-11-22  
**Autor:** Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`)  
**Metodología:** TDD Estricto ("Cero Fantasía") — Fase GREEN  

---

## 1. Comando Ejecutado

```powershell
$env:PATH = "%DOTNET_HOME%;$env:PATH"; $env:DOTNET_ROOT = "%DOTNET_HOME%"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~VentaAdminCrudServiceTests|FullyQualifiedName~VentasAdminControllerTests" -v normal
```

---

## 2. Salida de Terminal (100% Exitoso)

```text
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.58]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.68]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.68]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:01.64]   Finished:    SistemaFuneraria.Tests
  Passed SF-063: Motivo menor a 10 caracteres debe retornar error de validación [904 ms]
  Passed SF-063 (BDD): Administrador cancela venta errónea por PATCH y lote vuelve a estar Disponible [920 ms]
  Passed SF-063: DELETE venta con motivo válido cancela y libera lote [10 ms]
  Passed SF-063 (BDD): Cancelar venta errónea pasa contrato a Cancelado y regresa lote a Disponible [25 ms]
  Passed SF-063: Venta inexistente debe retornar No Encontrado [< 1 ms]
  Passed SF-063: PATCH con motivo menor a 10 caracteres debe retornar 400 BadRequest [2 ms]
  Passed SF-063: Métodos CambiarEstado y CancelarVenta deben exigir rol Administrador [1 ms]

Test Run Successful.
Total tests: 7
     Passed: 7
 Total time: 1.9717 Seconds
```

---

## 3. Escenarios BDD y Validaciones Verificadas

1. **Escenario BDD (Gherkin):**
   - Administradora cancela un contrato por `PATCH /api/v1/ventas/{id}/estado` con motivo `"Error de captura en titular"`.
   - El contrato pasa a estado `Cancelado` registrando el motivo en base de datos.
   - El lote asociado (`JN-A-042`) es liberado de manera atómica ACID y regresa automáticamente a estado `Disponible`.
2. **Baja Lógica / DELETE (`DELETE /api/v1/ventas/{id}`):**
   - Con motivo válido de cancelación (≥ 10 caracteres), ejecuta la cancelación administrativa y libera el lote.
3. **Validación de Motivo Administrativo:**
   - Motivo menor a 10 caracteres o vacío es rechazado con `400 Bad Request`.
4. **Control de Acceso Basado en Roles (RBAC):**
   - Los endpoints `PATCH /api/v1/ventas/{id}/estado` y `DELETE /api/v1/ventas/{id}` exigen explícitamente el rol `Administrador` (`[Authorize(Roles = "Administrador")]`).
