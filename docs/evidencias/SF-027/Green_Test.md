# Evidencia Fase GREEN - [SF-027] Validación Estricta de Límites en SimuladorService (CP-10)

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

## 2. Salida Real de Terminal (Fase GREEN)

```text
  Determining projects to restore...
  All projects are up-to-date for restore.
  SistemaFuneraria.Core -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Core\bin\Debug\net8.0\SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\bin\Debug\net8.0\SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Infrastructure\bin\Debug\net8.0\SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Web\bin\Debug\net8.0\SistemaFuneraria.Web.dll
  SistemaFuneraria.Tests -> C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    20, Skipped:     0, Total:    20, Duration: 54 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

## 3. Verificación de Regresión (Suite Completa No-Integración)

```text
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   298, Skipped:     0, Total:   298, Duration: 941 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

## 4. Conclusión de Aceptación
* **CP-10 Caso A:** Validación de enganche rechaza 15% (y cualquier valor $\notin \{10, 20, 30\}\%$) con mensaje explícito `"Enganche no permitido. Solo se acepta 10%, 20% o 30% (RN-04)."`.
* **CP-10 Caso B:** Validación de plazo rechaza 36 meses (y cualquier valor $\notin \{12, 18, 24, 48\}$ meses) con mensaje explícito `"Plazo inválido. Solo se admiten plazos de 12, 18, 24 o 48 meses (RN-05)."`.
* **Partición de Precios:** Validación rechaza cualquier precio no autorizado $\notin \{\$120,000, \$150,000, \$180,000\}$ con mensaje `"Precio no permitido. Solo se autorizan lotes de $120,000.00, $150,000.00 y $180,000.00 (RN-01)."`.
* **Acumulación de Errores:** Se verificó que múltiples violaciones simultáneas acumulen y retornen todos los mensajes de error en la colección de fallos.
* **Criterio de Calidad:** 0 advertencias, 0 errores de compilación (`TreatWarningsAsErrors: true`), arquitectura limpia pura sin dependencias hacia infraestructura.
