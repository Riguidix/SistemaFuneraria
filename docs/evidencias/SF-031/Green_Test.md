# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-031 |
| **Caso de Prueba Relacionado** | RF-02 / ST-05 (Ampliación CRUD en Cliente e IClienteService) |
| **Fecha y Hora de Ejecución** | 16/11/2026 20:23 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-031-app-cliente-crud-completo` |

---

### 1. Objetivo de la Prueba
Verificar que la implementación completa de las funcionalidades requeridas en SF-031 satisface al 100% las pruebas unitarias y los escenarios BDD sin regresiones:
1. `BuscarClientesAsync`: filtrado insensible a mayúsculas/minúsculas por nombre, apellidos, CURP o teléfono, con ordenamiento alfabético (`Apellidos, Nombre`).
2. `ActualizarClienteAsync`: mutación de datos con validación de invariantes, validación de unicidad de CURP y correo contra otros clientes registrados en la base de datos.
3. `EliminarClienteAsync`: eliminación limpia del padrón para clientes sin contratos y bloqueo con error de conflicto (`409`) para clientes con contratos activos.
4. `Cliente.Actualizar`: invariantes de dominio en el modelo `Core`.
5. `ClienteDto`: cálculo dinámico de iniciales de avatar y formateo de fecha (`"Cliente desde MMM. yyyy"`).

---

### 2. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClienteCrudCompletoTests" -v normal
```

---

### 3. Salida Real de Terminal (Fase GREEN - Pruebas del Ticket SF-031)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.05]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.13]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.13]   Starting:    SistemaFuneraria.Tests
  Passed SF-031 (BDD): ActualizarClienteAsync con teléfono válido y correo único debe persistir [385 ms]
  Passed SF-031: Cliente.Actualizar aplica validaciones de invariantes [< 1 ms]
  Passed SF-031: ActualizarClienteAsync con Correo duplicado en otro cliente debe fallar [20 ms]
  Passed SF-031: BuscarClientesAsync por CURP o teléfono debe retornar coincidencias [25 ms]
  Passed SF-031: BuscarClientesAsync con filtro nulo o vacío debe retornar todos [4 ms]
  Passed SF-031: BuscarClientesAsync por nombre debe retornar coincidencias [< 1 ms]
  Passed SF-031: EliminarClienteAsync sobre cliente con contratos activos debe ser rechazado [5 ms]
  Passed SF-031 (BDD): EliminarClienteAsync sobre cliente sin contratos debe eliminar del padrón [9 ms]
  Passed SF-031: ClienteDto mapea campos de resumen de cartera e iniciales [5 ms]
  Passed SF-031: ActualizarClienteAsync con CURP duplicada en otro cliente debe fallar [< 1 ms]
[xUnit.net 00:00:00.63]   Finished:    SistemaFuneraria.Tests

Test Run Successful.
Total tests: 10
     Passed: 10
 Total time: 0.9475 Seconds

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### 4. Salida Real de Verificación Global (Regresión Cero)
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName!~Integration"
```
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   345, Skipped:     0, Total:   345, Duration: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Conclusión y Dictamen
El ciclo TDD ha concluido satisfactoriamente pasando de 9 pruebas en fallo (`NotImplementedException`) a 10 pruebas exitosas en la suite específica y 345 pruebas exitosas en la suite unitaria global sin ninguna advertencia del compilador. Cumple con la regla de Blast Radius y con la arquitectura limpia.
