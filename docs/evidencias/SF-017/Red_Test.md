# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-017 |
| **Título del Ticket** | Contrato IClienteService y DTOs planos (RegistrarClienteRequest, ClienteDto) |
| **Caso de Prueba Relacionado** | BDD Contrato Application / Especificación Backlog SF-017 |
| **Fecha y Hora de Ejecución** | 11/11/2026 17:53:30 |
| **Desarrollador / Tester** | Guillermo Sandoval (Backend) / Juan Romero (Frontend) |
| **Rama Git** | feature/SF-017-cliente-service-contrato |

---

### 1. Objetivo de la Prueba
Demostrar el estado inicial de falla en la fase ROJO (RED) ante la ausencia de los contratos y DTOs de clientes en `SistemaFuneraria.Application.Servicios.Clientes`:
* `IClienteService`
* `RegistrarClienteRequest`
* `ClienteDto`

Se verifica que el compilador rechaza las pruebas unitarias en `tests/SistemaFuneraria.Tests/Application/ClienteContratosTests.cs` con error `CS0234` al no existir el espacio de nombres ni los tipos especificados.

### 2. Comando de Terminal Ejecutado
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

### 3. Salida Real de Terminal (Falla Esperada / RED)
```text
  Determining projects to restore...
  Restored C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\SistemaFuneraria.Application.csproj (in 1.19 sec).
  Restored C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Web\SistemaFuneraria.Web.csproj (in 660 ms).
  Restored C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Infrastructure\SistemaFuneraria.Infrastructure.csproj (in 509 ms).
  Restored C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Core\SistemaFuneraria.Core.csproj (in 133 ms).
  Restored C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj (in 4.89 sec).
  SistemaFuneraria.Core -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Core\bin\Debug\net8.0\SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\bin\Debug\net8.0\SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Infrastructure\bin\Debug\net8.0\SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Web\bin\Debug\net8.0\SistemaFuneraria.Web.dll
C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Application\ClienteContratosTests.cs(4,46): error CS0234: The type or namespace name 'Clientes' does not exist in the namespace 'SistemaFuneraria.Application.Servicios' (are you missing an assembly reference?) [C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]

Build FAILED.

C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Application\ClienteContratosTests.cs(4,46): error CS0234: The type or namespace name 'Clientes' does not exist in the namespace 'SistemaFuneraria.Application.Servicios' (are you missing an assembly reference?) [C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
    0 Warning(s)
    1 Error(s)

Time Elapsed 00:00:29.05
```

### 4. Certificación
La fase ROJO queda certificada de manera irrefutable:
1. El compilador falla limpiamente por ausencia de los contratos en la capa de aplicación.
2. Se asegura que la implementación futura responda estrictamente a las especificaciones del backlog y a las necesidades del consumidor (capa Web).
