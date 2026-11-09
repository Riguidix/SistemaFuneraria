# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-013 |
| **Título del Ticket** | Contrato ISimuladorService y DTOs en Application con tests unitarios en memoria |
| **Caso de Prueba Relacionado** | BDD Simulador / CP-06 / Especificación Backlog SF-013 |
| **Fecha y Hora de Ejecución** | 08/11/2026 18:36:14 |
| **Desarrollador / Tester** | Guillermo Sandoval (Backend) / Juan Romero (Frontend) |
| **Rama Git** | feature/SF-013-simulador-service-contrato |

---

### 1. Objetivo de la Prueba
Demostrar que tras la implementación de los contratos y servicios en memoria en `SistemaFuneraria.Application.Servicios.Simulador`:
* `ISimuladorService`
* `SimuladorService`
* `SimularFinanciamientoRequest`
* `SimulacionFinanciamientoDto` y `CuotaAmortizacionDto`

La suite de pruebas en `tests/SistemaFuneraria.Tests/Application/SimuladorServiceTests.cs` compila limpiamente con advertencias tratadas como errores (`TreatWarningsAsErrors: true`) y valida:
1. Simulación exitosa con precio `$120,000`, enganche `20%` y plazo de `12 meses`.
2. Mensualidad ordinaria calculada en `$8,710.16`.
3. Tabla de amortización con 12 cuotas exactas, saldo final en `$0.00` y absorción al centavo en la última mensualidad (`$8,710.19`).
4. Rechazo controlado de parámetros no autorizados retornando `Resultado<T>` fallido sin lanzar excepciones de control de flujo.

### 2. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~SimuladorServiceTests" --logger "console;verbosity=detailed"
```

### 3. Salida Real de Terminal (Pase Exitoso / GREEN)
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
C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.11]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.15]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.15]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.33]   Finished:    SistemaFuneraria.Tests
  Passed SF-013: Parámetros no autorizados deben retornar Resultado fallido con error de negocio(precio: 120000, enganche: 15, plazo: 12) [145 ms]
  Passed SF-013: Parámetros no autorizados deben retornar Resultado fallido con error de negocio(precio: 100000, enganche: 20, plazo: 12) [1 ms]
  Passed SF-013: Parámetros no autorizados deben retornar Resultado fallido con error de negocio(precio: 120000, enganche: 20, plazo: 36) [< 1 ms]
  Passed SF-013 / BDD: Simular financiamiento $120k, 20% enganche, 12 meses debe retornar cuota $8,710.16 y 12 cuotas [2 ms]

Test Run Successful.
Total tests: 4
     Passed: 4
 Total time: 0.6712 Seconds
```

### 4. Certificación
1. **4/4 pruebas unitarias pasadas en verde (100% de éxito).**
2. **Tiempo de ejecución total:** 0.67 segundos (por debajo del SLA de $< 5.0$ s).
3. **Cero advertencias y cero errores** en la compilación estricta con `TreatWarningsAsErrors`.
4. El contrato `ISimuladorService` queda listo y publicado para su consumo seguro por la capa Web y Razor Pages.
