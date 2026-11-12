# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-024 |
| **Título del Ticket** | Caso de uso IVentaService.RegistrarVentaContadoAsync y persistencia en 1 exhibición |
| **Caso de Prueba Relacionado** | CP-02 / BDD Venta Contado / Especificación Backlog SF-024 |
| **Fecha y Hora de Ejecución** | 12/11/2026 17:17:08 |
| **Desarrollador / Tester** | Guillermo Sandoval (Backend) / Lead Fullstack (Revisor) |
| **Rama Git** | feature/SF-024-venta-contado-service |

---

### 1. Objetivo de la Prueba
Demostrar el estado inicial de falla en la fase ROJO (RED) ante la ausencia de los tipos y servicios de venta de contado en `SistemaFuneraria.Application.Servicios.Ventas`:
* `IVentaService`
* `VentaService`
* `RegistrarVentaContadoRequest`

Se verifica que el compilador rechaza las pruebas unitarias en `tests/SistemaFuneraria.Tests/Application/VentaContadoServiceTests.cs` con error `CS0234` al no existir el espacio de nombres ni los tipos especificados.

### 2. Comando de Terminal Ejecutado
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

### 3. Salida Real de Terminal (Falla Esperada / RED)
```text
  Determining projects to restore...
  Restored C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Core\SistemaFuneraria.Core.csproj (in 218 ms).
  Restored C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\SistemaFuneraria.Application.csproj (in 5.48 sec).
  Restored C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj (in 7.89 sec).
  Restored C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Web\SistemaFuneraria.Web.csproj (in 11.49 sec).
  Restored C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Infrastructure\SistemaFuneraria.Infrastructure.csproj (in 11.49 sec).
  SistemaFuneraria.Core -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Core\bin\Debug\net8.0\SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\bin\Debug\net8.0\SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Infrastructure\bin\Debug\net8.0\SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Web\bin\Debug\net8.0\SistemaFuneraria.Web.dll
C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Application\VentaContadoServiceTests.cs(3,46): error CS0234: The type or namespace name 'Ventas' does not exist in the namespace 'SistemaFuneraria.Application.Servicios' (are you missing an assembly reference?) [C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]

Build FAILED.

C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Application\VentaContadoServiceTests.cs(3,46): error CS0234: The type or namespace name 'Ventas' does not exist in the namespace 'SistemaFuneraria.Application.Servicios' (are you missing an assembly reference?) [C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
    0 Warning(s)
    1 Error(s)

Time Elapsed 00:00:34.82
```

### 4. Certificación
La fase ROJO queda certificada bajo TDD estricto ("Cero Fantasía"):
1. El compilador falla de manera determinista ante la ausencia de los contratos de venta de contado en `Application`.
2. Las pruebas unitarias definen con exactitud los criterios de aceptación de negocio (CP-02, RF-03, RN-01, RN-03).
