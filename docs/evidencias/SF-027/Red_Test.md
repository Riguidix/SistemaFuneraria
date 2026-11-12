# Evidencia Fase RED - [SF-027] Validación Estricta de Límites en SimuladorService (CP-10)

## 1. Identificación del Caso de Prueba y Ticket
* **Ticket:** SF-027
* **Caso de Prueba:** CP-10 · Rechazo de Parámetros No Permitidos (Enganche 15% / Plazo 36 Meses)
* **Rama:** `feature/SF-027-simulador-service-limites`
* **Desarrollador:** Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`)
* **Fecha de Ejecución:** 12 de Noviembre de 2026
* **Comando:**
  ```powershell
  dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~SimuladorLimitesTests"
  ```

---

## 2. Salida Real de Terminal (Fase RED)

```text
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.93]     SF-027 / CP-10: Enganches fuera de {10, 20, 30}% deben ser rechazados(porcentajeInvalido: 35) [FAIL]
[xUnit.net 00:00:00.94]     SF-027 / CP-10: Enganches fuera de {10, 20, 30}% deben ser rechazados(porcentajeInvalido: 15) [FAIL]
[xUnit.net 00:00:00.94]     SF-027 / CP-10: Enganches fuera de {10, 20, 30}% deben ser rechazados(porcentajeInvalido: 25) [FAIL]
[xUnit.net 00:00:00.94]     SF-027 / CP-10: Enganches fuera de {10, 20, 30}% deben ser rechazados(porcentajeInvalido: 50) [FAIL]
[xUnit.net 00:00:00.94]     SF-027 / CP-10: Enganches fuera de {10, 20, 30}% deben ser rechazados(porcentajeInvalido: 5) [FAIL]
[xUnit.net 00:00:00.94]     SF-027 / CP-10: Enganches fuera de {10, 20, 30}% deben ser rechazados(porcentajeInvalido: 0) [FAIL]
[xUnit.net 00:00:00.94]     SF-027 / CP-10: Plazos fuera de {12, 18, 24, 48} meses deben ser rechazados(plazoInvalido: 0) [FAIL]
[xUnit.net 00:00:00.95]     SF-027 / CP-10: Plazos fuera de {12, 18, 24, 48} meses deben ser rechazados(plazoInvalido: 6) [FAIL]
[xUnit.net 00:00:00.95]     SF-027 / CP-10: Plazos fuera de {12, 18, 24, 48} meses deben ser rechazados(plazoInvalido: 36) [FAIL]

  Failed CP-10 Caso A: Enganche 15% debe ser rechazado con mensaje explícito [1 ms]
  Error Message:
   Expected resultado.Errores {"El porcentaje de enganche de 15% no está autorizado (RN-04)."} to have an item matching e.Contains("Enganche no permitido. Solo se acepta 10%, 20% o 30%").
  
  Failed CP-10 Caso B: Plazo 36 meses debe ser rechazado con mensaje explícito [3 ms]
  Error Message:
   Expected resultado.Errores {"El plazo de 36 meses no está autorizado (RN-05)."} to have an item matching e.Contains("Plazo inválido. Solo se admiten plazos de 12, 18, 24 o 48 meses").

Failed!  - Failed:    19, Passed:     1, Skipped:     0, Total:    20, Duration: 98 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

## 3. Diagnóstico de Falla
* **Causa:** `SimuladorService.cs` arrojaba mensajes genéricos formateados como `El porcentaje de enganche de 15% no está autorizado (RN-04).` en lugar de los mensajes canónicos estipulados en la especificación **CP-10** del documento de casos de prueba (`Enganche no permitido. Solo se acepta 10%, 20% o 30%` y `Plazo inválido. Solo se admiten plazos de 12, 18, 24 o 48 meses`).
* **Acción para Fase GREEN:** Actualizar las cadenas de error de validación en `SimuladorService.cs` para cumplir al 100% con los mensajes requeridos por CP-10 y RN-01/RN-04/RN-05.
