# Evidencia Fase GREEN — SF-060

**Ticket:** [SF-060] `IVentaService.RegistrarVentaContadoAsync` (Transacción ACID: Venta + Lote Vendido + Pago Único CP-02) [#126](https://github.com/Riguidix/SistemaFuneraria/issues/126)  
**Fecha:** 2026-11-22  
**Autor:** Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`)  
**Metodología:** TDD Estricto ("Cero Fantasía") — Fase GREEN  

---

## 1. Comando Ejecutado

```powershell
$env:PATH = "%DOTNET_HOME%;$env:PATH"; $env:DOTNET_ROOT = "%DOTNET_HOME%"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~VentaContadoServiceTests" -v normal
```

---

## 2. Salida de Terminal (100% Exitoso)

```text
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.49]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.59]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.59]   Starting:    SistemaFuneraria.Tests
  Passed SF-060: Venta de contado sobre lote en Mantenimiento debe retornar conflicto [798 ms]
  Passed SF-060: Venta de contado con cliente inexistente debe retornar No Encontrado [16 ms]
[xUnit.net 00:00:01.53]   Finished:    SistemaFuneraria.Tests
  Passed SF-060: Venta de contado sobre lote Reservado debe permitir venta y marcar como Vendido [72 ms]
  Passed SF-060 (BDD / CP-02): Venta de contado debe crear Venta Liquidada, lote Vendido y Pago único [18 ms]
  Passed SF-060: Venta de contado con lote inexistente debe retornar No Encontrado [1 ms]
  Passed SF-060: Venta de contado sobre lote ya Vendido debe retornar conflicto y no duplicar venta [1 ms]

Test Run Successful.
Total tests: 6
     Passed: 6
 Total time: 1.8305 Seconds
```

---

## 3. Escenarios BDD Validados

1. **Escenario Canónico (CP-02 / ST-01):**
   - Venta de contado sobre lote `JN-A-042` ($120,000.00) crea la entidad `Venta` en estado `Liquidado` con folio `CTR-2026-0190`.
   - Lote pasa automáticamente a estado `Vendido`.
   - Se crea el registro inmutable de `Pago` tipo `Contado` por $120,000.00 con folio `PAG-2026-XXXXXX` y saldo posterior $0.00.
2. **Lote Reservado:**
   - Permite formalizar venta y pasa a `Vendido`.
3. **Lote Vendido:**
   - Retorna error de negocio `[CONFLICTO]` y no altera registros en base de datos.
4. **Lote en Mantenimiento:**
   - Retorna error de negocio `[CONFLICTO]`.
5. **Cliente Inexistente:**
   - Retorna error `[NO_ENCONTRADO]`.
6. **Lote Inexistente:**
   - Retorna error `[NO_ENCONTRADO]`.
