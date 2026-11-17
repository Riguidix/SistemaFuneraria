# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-031 |
| **Caso de Prueba Relacionado** | RF-02 / ST-05 (Ampliación CRUD en Cliente e IClienteService) |
| **Fecha y Hora de Ejecución** | 16/11/2026 20:14 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-031-app-cliente-crud-completo` |

---

### 1. Objetivo de la Prueba
Demostrar la falla inicial de las pruebas unitarias que verifican las operaciones ampliadas del ciclo de vida de clientes en `ClienteService` y `Cliente`:
1. `BuscarClientesAsync`: filtrado multicriterio por nombre, apellidos, CURP o teléfono (Captura 3 Figma).
2. `ActualizarClienteAsync`: verificación de invariantes, unicidad de CURP y correo electrónico frente a terceros.
3. `EliminarClienteAsync`: eliminación de clientes sin contratos y protección estricta contra borrado de clientes con contratos activos.
4. `Cliente.Actualizar`: mutación controlada con validación de reglas de negocio en la entidad pura de dominio.
5. `ClienteDto`: mapeo enriquecido con iniciales de avatar y resumen de cartera.

### 2. Datos de Entrada Utilizados
* Búsqueda por término `"Vega"` (nombre) y `"COML850202"` (CURP parcial).
* Petición `ActualizarClienteRequest` con teléfono `"5599998888"` y nuevo correo único.
* Intentos de actualización con colisión de CURP y correo con registros preexistentes.
* Intentos de eliminación de cliente con `ContratosActivos = 0` y `ContratosActivos = 2`.

### 3. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClienteCrudCompletoTests"
```

### 4. Salida Real de Terminal (Fase RED)
```text
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:04.04]     SF-031 (BDD): ActualizarClienteAsync con teléfono válido y correo único debe persistir [FAIL]
  Failed SF-031 (BDD): ActualizarClienteAsync con teléfono válido y correo único debe persistir [1 s]
  Error Message:
   System.NotImplementedException : SF-031: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Clientes.ClienteService.ActualizarClienteAsync(Guid id, ActualizarClienteRequest request, CancellationToken cancellationToken) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\Servicios\Clientes\ClienteService.cs:line 113
   at SistemaFuneraria.Tests.Application.ClienteCrudCompletoTests.ActualizarClienteAsync_ConDatosValidos_DebePersistirCambios() in C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Application\ClienteCrudCompletoTests.cs:line 114
[xUnit.net 00:00:04.08]     SF-031: Cliente.Actualizar aplica validaciones de invariantes [FAIL]
[xUnit.net 00:00:04.12]     SF-031: ActualizarClienteAsync con Correo duplicado en otro cliente debe fallar [FAIL]
[xUnit.net 00:00:04.15]     SF-031: BuscarClientesAsync por CURP o teléfono debe retornar coincidencias [FAIL]
[xUnit.net 00:00:04.15]     SF-031: BuscarClientesAsync con filtro nulo o vacío debe retornar todos [FAIL]
[xUnit.net 00:00:04.16]     SF-031: BuscarClientesAsync por nombre debe retornar coincidencias [FAIL]
[xUnit.net 00:00:04.16]     SF-031: EliminarClienteAsync sobre cliente con contratos activos debe ser rechazado [FAIL]
[xUnit.net 00:00:04.17]     SF-031 (BDD): EliminarClienteAsync sobre cliente sin contratos debe eliminar del padrón [FAIL]
[xUnit.net 00:00:04.49]     SF-031: ActualizarClienteAsync con CURP duplicada en otro cliente debe fallar [FAIL]

Failed!  - Failed:     9, Passed:     1, Skipped:     0, Total:    10, Duration: 442 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Diagnóstico Técnico
Los métodos `BuscarClientesAsync`, `ActualizarClienteAsync`, `EliminarClienteAsync` y `Cliente.Actualizar` se encuentran en stub arrojando `NotImplementedException`. Se procede a implementar la lógica completa en la fase GREEN.
