# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-017 |
| **Título del Ticket** | Contrato IClienteService y DTOs planos (RegistrarClienteRequest, ClienteDto) |
| **Caso de Prueba Relacionado** | BDD Contrato Application / Especificación Backlog SF-017 |
| **Fecha y Hora de Ejecución** | 11/11/2026 17:54:36 |
| **Desarrollador / Tester** | Guillermo Sandoval (Backend) / Juan Romero (Frontend) |
| **Rama Git** | feature/SF-017-cliente-service-contrato |

---

### 1. Objetivo de la Prueba
Demostrar que tras la implementación de los contratos y DTOs en `SistemaFuneraria.Application.Servicios.Clientes`:
* `IClienteService`
* `RegistrarClienteRequest`
* `ClienteDto`

La suite de pruebas unitarias en `tests/SistemaFuneraria.Tests/Application/ClienteContratosTests.cs` compila limpiamente (0 errores, 0 advertencias con `TreatWarningsAsErrors` activo) y valida:
1. `IClienteService` expone métodos asíncronos (`RegistrarClienteAsync`, `ObtenerPorIdAsync`, `ObtenerPorCurpAsync`, `ListarClientesAsync`) con soporte obligatorio de `CancellationToken`.
2. Las operaciones retornan el monad canónico `Resultado<T>` desacoplado de excepciones y de la infraestructura de persistencia.
3. Los DTOs `RegistrarClienteRequest` y `ClienteDto` son inmutables y transportan con precisión los tipos de datos requeridos por el PRD y el ERD (CURP, Nombre, Apellidos, Teléfono, Correo, Dirección, Id y CreatedAt).

### 2. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClienteContratosTests" --logger "console;verbosity=detailed"
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
[xUnit.net 00:00:00.64]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.77]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.78]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:01.16]   Finished:    SistemaFuneraria.Tests
  Passed SF-017: RegistrarClienteRequest debe ser inmutable y contener campos obligatorios de negocio [250 ms]
  Passed SF-017: ClienteDto debe contener todos los campos de visualización incluyendo Id y CreatedAt [2 ms]
  Passed SF-017 / BDD: IClienteService debe exponer contratos asíncronos con CancellationToken y Resultado<T> [3 ms]

Test Run Successful.
Total tests: 3
     Passed: 3
 Total time: 2.0368 Seconds
```

### 4. Certificación
1. **3/3 pruebas unitarias de contrato pasadas en verde (100% de éxito).**
2. **Tiempo de ejecución total:** 2.03 s (por debajo del SLA de $< 5.0$ s).
3. **Cero advertencias y cero errores** en la compilación estricta con `TreatWarningsAsErrors`.
4. El contrato `IClienteService` queda publicado para desbloquear la implementación de `ClienteService` (SF-018) y las pantallas de clientes en Web Razor (SF-019).
