# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-049 |
| **Caso de Prueba Relacionado** | CP-10 / RF-01 (SimuladorController: POST /api/v1/simulador/cotizar & GET /api/v1/simulador/planes) |
| **Fecha y Hora de Ejecución** | 21/11/2026 23:23 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-049-api-simulador-controller` |

---

### 1. Resumen de la Implementación
Se implementó `SimuladorController` en `src/SistemaFuneraria.Api/Controllers/V1/SimuladorController.cs` con estricta adherencia a Clean Architecture y Blast Radius ($\le 160$ LOCs):
1. **Controlador REST**:
   - Hereda de `ApiControllerBase`.
   - Protegido con `[Authorize]` y expuesto en la ruta base `api/v1/simulador`.
2. **Endpoint `GET /api/v1/simulador/planes`**:
   - Devuelve el catálogo oficial de porcentajes de enganche autorizados (`10%`, `20%`, `30%`) y plazos/tasas (`12 meses · 16% anual`, `18 meses · 18% anual`, `24 meses · 20% anual`, `48 meses · 24% anual`).
   - Sirve directamente a los selectores de la *Captura 7* del Wizard de Ventas.
3. **Endpoint `POST /api/v1/simulador/cotizar`**:
   - Ejecuta la simulación y cálculo de amortización francesa sin mutar ni bloquear inventario en BD.
   - Si se proporciona `LoteId`, resuelve en inventario la capacidad/gavetas del lote o retorna `404 Not Found` en caso de inexistencia.
   - En modalidad `Contado`, proyecta liquidación total inmediata sin tabla de amortización.
   - En modalidad `Credito`, valida las 36 combinaciones permitidas (`CP-10`).
   - Rechaza plazos no autorizados (ej. `PlazoMeses = 15`) retornando `400 Bad Request` con `ProblemDetails`.

---

### 2. Comando de Terminal Ejecutado
```powershell
$env:PATH = "C:\Users\Billy\AppData\Local\Microsoft\dotnet;$env:PATH"; $env:DOTNET_ROOT = "C:\Users\Billy\AppData\Local\Microsoft\dotnet"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~SimuladorControllerTests" -v normal
```

---

### 3. Salida Real de Terminal (Fase GREEN)
```text
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.14]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.35]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.35]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:01.56]   Finished:    SistemaFuneraria.Tests
  Passed SF-049: Cotizar con LoteId existente debe consultar gavetas del lote y retornar 200 OK [1 s]
  Passed SF-049: SimuladorController debe tener Route api/v1/simulador, ApiController y Authorize [5 ms]
  Passed SF-049: ObtenerPlanes debe retornar 200 OK con enganches y plazos autorizados de Figma Captura 7 [44 ms]
  Passed SF-049 (BDD / CP-10): Rechazo de plazo inválido de 15 meses por API debe retornar 400 Bad Request [3 ms]
  Passed SF-049: Cotizar modalidad Crédito Captura 7 ($120k, 20%, 24 meses) debe retornar 200 OK con cuota exacta $4,887.54 [1 ms]
  Passed SF-049: Cotizar con LoteId inexistente debe retornar 404 Not Found [32 ms]
  Passed SF-049: Cotizar modalidad Contado debe retornar 200 OK con liquidación sin tabla [2 ms]

Test Run Successful.
Total tests: 7
     Passed: 7
 Total time: 2.1803 Seconds
```

---

### 4. Prueba de Regresión Global
```text
Passed!  - Failed:     0, Passed:   554, Skipped:     0, Total:   554, Duration: 2 s - SistemaFuneraria.Tests.dll (net8.0)
```
100% de la suite de pruebas unitarias exitosa sin regresiones en ninguna capa arquitectónica.
