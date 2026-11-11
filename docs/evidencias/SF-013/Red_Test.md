# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-013 |
| **Título del Ticket** | Contrato ISimuladorService y DTOs en Application con tests unitarios en memoria |
| **Caso de Prueba Relacionado** | BDD Simulador / CP-06 / Especificación Backlog SF-013 |
| **Fecha y Hora de Ejecución** | 08/11/2026 18:35:15 |
| **Desarrollador / Tester** | Guillermo Sandoval (Backend) / Juan Romero (Frontend) |
| **Rama Git** | feature/SF-013-simulador-service-contrato |

---

### 1. Objetivo de la Prueba
Demostrar el estado de falla inicial en la fase ROJO (RED) debido a la ausencia de los tipos y contratos del simulador financiero en `SistemaFuneraria.Application.Servicios.Simulador`:
* `ISimuladorService`
* `SimuladorService`
* `SimularFinanciamientoRequest`
* `SimulacionFinanciamientoDto`

Se verifica que el compilador rechaza las pruebas unitarias en `tests/SistemaFuneraria.Tests/Application/SimuladorServiceTests.cs` con errores `CS0234` y `CS0246` antes de escribir cualquier línea de código de producción.

### 2. Comando de Terminal Ejecutado
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

### 3. Salida Real de Terminal (Falla Esperada / RED)
```text
  Determining projects to restore...
  All projects are up-to-date for restore.
  SistemaFuneraria.Core -> <REPO_ROOT>/src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> <REPO_ROOT>/src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> <REPO_ROOT>/src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> <REPO_ROOT>/src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
<REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/SimuladorServiceTests.cs(2,36): error CS0234: The type or namespace name 'Servicios' does not exist in the namespace 'SistemaFuneraria.Application' (are you missing an assembly reference?) [<REPO_ROOT>/tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj]
<REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/SimuladorServiceTests.cs(9,22): error CS0246: The type or namespace name 'ISimuladorService' could not be found (are you missing a using directive or an assembly reference?) [<REPO_ROOT>/tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj]

Build FAILED.

<REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/SimuladorServiceTests.cs(2,36): error CS0234: The type or namespace name 'Servicios' does not exist in the namespace 'SistemaFuneraria.Application' (are you missing an assembly reference?) [<REPO_ROOT>/tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj]
<REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/SimuladorServiceTests.cs(9,22): error CS0246: The type or namespace name 'ISimuladorService' could not be found (are you missing a using directive or an assembly reference?) [<REPO_ROOT>/tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj]
    0 Warning(s)
    2 Error(s)

Time Elapsed 00:00:05.79
```

### 4. Certificación
La fase ROJO queda certificada bajo TDD estricto ("Cero Fantasía"):
1. El compilador falla de manera determinista al no encontrar los contratos ni los DTOs en la capa de aplicación.
2. Los criterios BDD guían el diseño de las interfaces y modelos de datos.
