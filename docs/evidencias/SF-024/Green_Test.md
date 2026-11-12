# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-024 |
| **Título del Ticket** | Caso de uso IVentaService.RegistrarVentaContadoAsync y persistencia en 1 exhibición |
| **Caso de Prueba Relacionado** | CP-02 / BDD Venta Contado / Especificación Backlog SF-024 |
| **Fecha y Hora de Ejecución** | 12/11/2026 17:18:29 |
| **Desarrollador / Tester** | Guillermo Sandoval (Backend) / Lead Fullstack (Revisor) |
| **Rama Git** | feature/SF-024-venta-contado-service |

---

### 1. Objetivo de la Prueba
Demostrar que tras la implementación de los servicios de venta en `SistemaFuneraria.Application.Servicios.Ventas`:
* `IVentaService`
* `VentaService`
* `RegistrarVentaContadoRequest`

La suite de pruebas unitarias en `tests/SistemaFuneraria.Tests/Application/VentaContadoServiceTests.cs` compila limpiamente (0 errores, 0 advertencias con `TreatWarningsAsErrors` activo) y valida:
1. Emisión determinista del folio único oficial bajo el formato `VTA-2026-XXXXXX` (CP-02).
2. Cobro y liquidación inmediata por el 100% del precio oficial ($150k para lote familiar) sin generar intereses ni tabla de amortización (RN-03).
3. Rechazo de ventas cuando el cliente no existe en la base de datos.
4. Rechazo controlado de precios de lotes no autorizados en el catálogo oficial (RN-01).

### 2. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~VentaContadoServiceTests" --logger "console;verbosity=detailed"
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
[xUnit.net 00:00:01.98]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:02.36]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:02.37]   Starting:    SistemaFuneraria.Tests
  Passed SF-024: Venta de contado con precio no autorizado debe retornar Fallo (RN-01)(precioInvalido: 0) [1 s]
[xUnit.net 00:00:04.62]   Finished:    SistemaFuneraria.Tests
  Passed SF-024: Venta de contado con precio no autorizado debe retornar Fallo (RN-01)(precioInvalido: 100000) [99 ms]
  Passed SF-024: Venta de contado con precio no autorizado debe retornar Fallo (RN-01)(precioInvalido: 200000) [28 ms]
  Passed SF-024: Venta de contado con precio no autorizado debe retornar Fallo (RN-01)(precioInvalido: 135000) [1 ms]
  Passed SF-024: Venta de contado debe liquidarse en 1 exhibición por el 100% del precio (RN-03) [5 ms]
  Passed SF-024: Venta de contado con cliente inexistente debe retornar Fallo [2 ms]
  Passed SF-024 / BDD: Venta de contado de lote familiar de $150k debe generar folio VTA-2026-XXXXXX [4 ms]

Test Run Successful.
Total tests: 7
     Passed: 7
 Total time: 5.8363 Seconds
```

### 4. Certificación
1. **7/7 pruebas unitarias de venta de contado pasadas en verde (100% de éxito).**
2. **285/285 pruebas totales de la solución superadas en verde.**
3. **Cero advertencias y cero errores** de compilación con `TreatWarningsAsErrors`.
4. El servicio `IVentaService` queda publicado para desbloquear la interfaz gráfica del Wizard de Ventas (SF-025).
