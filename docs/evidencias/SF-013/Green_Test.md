# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-013 |
| **Título del Ticket** | Contrato ISimuladorService y DTOs en Application con tests unitarios en memoria |
| **Caso de Prueba Relacionado** | BDD Simulador / CP-06 / Especificación Backlog SF-013 |
| **Fecha y Hora de Ejecución** | 11/11/2026 15:24:14 |
| **Desarrollador / Tester** | Guillermo Sandoval (Backend) / Juan Romero (Frontend) |
| **Rama Git** | feature/SF-013-simulador-service-contrato |

---

### 1. Objetivo de la Prueba
Demostrar que tras la implementación y refactorización limpia de los contratos y servicios en memoria en `SistemaFuneraria.Application.Servicios.Simulador`:
* `ISimuladorService`
* `SimuladorService` (orquestando directamente `CalculadoraFinanciamiento` y `GeneradorAmortizacion` de `Core`, libre de punto flotante `double`)
* `SimularFinanciamientoRequest`
* `SimulacionFinanciamientoDto` y `CuotaAmortizacionDto`

La suite de pruebas en `tests/SistemaFuneraria.Tests/Application/SimuladorServiceTests.cs` compila limpiamente con advertencias tratadas como errores (`TreatWarningsAsErrors: true`) y valida:
1. Simulación exitosa con precio `$120,000`, enganche `20%` y plazo de `12 meses`.
2. Mensualidad ordinaria calculada en `$8,710.16` delegando al motor decimal de `Core` (RNF-02).
3. Tabla de amortización con 12 cuotas exactas, saldo final en `$0.00` y absorción al centavo en la última mensualidad (`$8,710.19`) generada por `GeneradorAmortizacion`.
4. Rechazo controlado de parámetros no autorizados retornando `Resultado<T>` fallido sin lanzar excepciones de control de flujo.

### 2. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-013" --logger "console;verbosity=detailed"
```

### 3. Salida Real de Terminal (Pase Exitoso / GREEN)
```text
  Determinando los proyectos que se van a restaurar...
  Todos los proyectos están actualizados para la restauración.
  SistemaFuneraria.Core -> <REPO_ROOT>/src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> <REPO_ROOT>/src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> <REPO_ROOT>/src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> <REPO_ROOT>/src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
  SistemaFuneraria.Tests -> <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll
Serie de pruebas para <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
<REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.06]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.13]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.14]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.21]   Finished:    SistemaFuneraria.Tests
  Correctas SF-013: Parámetros no autorizados deben retornar Resultado fallido con error de negocio(precio: 120000, enganche: 15, plazo: 12) [16 ms]
  Correctas SF-013: Parámetros no autorizados deben retornar Resultado fallido con error de negocio(precio: 100000, enganche: 20, plazo: 12) [1 ms]
  Correctas SF-013: Parámetros no autorizados deben retornar Resultado fallido con error de negocio(precio: 120000, enganche: 20, plazo: 36) [< 1 ms]
  Correctas SF-013 / BDD: Simular financiamiento $120k, 20% enganche, 12 meses debe retornar cuota $8,710.16 y 12 cuotas [7 ms]

La serie de pruebas se ejecutó correctamente.
Pruebas totales: 4
     Correcto: 4
 Tiempo total: 0.6767 Segundos
```

### 4. Certificación
1. **4/4 pruebas unitarias pasadas en verde (100% de éxito).**
2. **Tiempo de ejecución total:** 0.68 segundos (por debajo del SLA de $< 5.0$ s).
3. **Cero advertencias y cero errores** en la compilación estricta con `TreatWarningsAsErrors`.
4. La capa `Application` queda limpia, sin duplicación de fórmulas ni uso de flotantes (`double`), orquestando directamente al motor `Core` y publicando `ISimuladorService` para Razor Pages.
