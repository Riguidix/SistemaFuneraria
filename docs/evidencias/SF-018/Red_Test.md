# Evidencia de Fase RED — Ticket SF-018: Implementación de ClienteService (Capa Application)

## 1. Metadatos de la Tarea

| Parámetro | Detalle |
| :--- | :--- |
| **Ticket** | `SF-018` — Implementación de `ClienteService` (Capa Application) |
| **Rama Git** | `feature/SF-018-cliente-service-impl` |
| **Fase TDD** | **RED** (Pruebas unitarias/integración de servicio compilando con fallas controladas) |
| **Responsable** | Memo Sandoval (Lead Fullstack) / Riguidix Rodríguez |
| **Fecha de Ejecución** | 11 de Noviembre de 2026, 23:11 CST |
| **Ambiente de Pruebas** | .NET SDK 8.0.32, EF Core InMemory, xUnit 2.5.3, FluentAssertions 6.12.0 |
| **Gobernanza / Regla** | Cobertura estricta BDD Gherkin, Clean Architecture (RF-ARCH-04), Cero Fantasía |

---

## 2. Resumen de la Fase RED

En apego al ciclo estricto TDD y las directrices del proyecto:
1. **Contratos y Abstracciones Limpias:**
   - Se diseñó la interfaz de persistencia [`IAppDbContext.cs`](file://<REPO_ROOT>/src/SistemaFuneraria.Application/Comun/Persistencia/IAppDbContext.cs) dentro de `SistemaFuneraria.Application` exponiendo `DbSet<Cliente> Clientes { get; }` y `Task<int> SaveChangesAsync(CancellationToken)`.
   - `SistemaFuneraria.Infrastructure.Persistencia.AppDbContext` implementa `IAppDbContext`, respetando la inversión de dependencias y asegurando que `Application` **jamás** dependa de `Infrastructure` (comprobado con `ReglasArquitecturaTests` 100% verde).
   - Se creó el stub de [`ClienteService.cs`](file://<REPO_ROOT>/src/SistemaFuneraria.Application/Servicios/Clientes/ClienteService.cs) implementando `IClienteService` inyectando `IAppDbContext`, con sus 4 métodos arrojando `NotImplementedException("SF-018: Fase RED...")`.
2. **Suite de Pruebas BDD / Criterios de Aceptación:**
   - Se implementaron 16 métodos de prueba en [`ClienteServiceTests.cs`](file://<REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/ClienteServiceTests.cs) con `TheoryData` expandido (37 ejecuciones unitarias totales).
   - Cubren de forma exhaustiva:
     * Registro con datos válidos y normalización de CURP (uppercase) y correo (lowercase).
     * Validación de obligatoriedad (Nombre, Primer Apellido, CURP, Teléfono, Correo).
     * Longitud máxima de campos (RF-VAL-01 / RF-CLI-01).
     * Formato regex estricto de CURP (18 caracteres alfanuméricos) y Correo.
     * Validación de teléfono (exactamente 10 dígitos numéricos).
     * Detección de duplicados por CURP y por Correo (evitando colisiones en BD).
     * Consulta por Id existente e inexistente.
     * Consulta por CURP existente e inexistente (incluyendo resiliencia a mayúsculas/minúsculas y espacios).
     * Listado general de clientes activos (vacío y con múltiples registros ordenados alfabéticamente).
     * Respeto a `CancellationToken`.
3. **Resultado:**
   - **37/37 pruebas de SF-018 fallan exactamente en RED** arrojando `NotImplementedException`.
   - **248/248 pruebas previas de la solución permanecen 100% en GREEN**.
   - Solución compila con 0 advertencias (`TreatWarningsAsErrors = true`).

---

## 3. Comandos Ejecutados y Salida de Terminal (Sanitizada)

### 3.1. Ejecución de Pruebas SF-018 (RED)

```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-018" --logger "console;verbosity=normal"
```

```text
Serie de pruebas para <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.09]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.20]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.21]   Starting:    SistemaFuneraria.Tests

  Con error Registro exitoso: Cliente válido debe persistirse en BD y retornar DTO con ID generado [55 ms]
  Mensaje de error:
   System.NotImplementedException : SF-018: Fase RED - RegistrarClienteAsync pendiente de implementación.
  Seguimiento de la pila:
     at SistemaFuneraria.Application.Servicios.Clientes.ClienteService.RegistrarClienteAsync(RegistrarClienteRequest request, CancellationToken cancellationToken) in <REPO_ROOT>/src/SistemaFuneraria.Application/Servicios/Clientes/ClienteService.cs:line 23
     at SistemaFuneraria.Tests.Application.ClienteServiceTests.RegistrarClienteAsync_ConDatosValidos_DebePersistirYRetornarDtoExitoso() in <REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/ClienteServiceTests.cs:line 47

  Con error Consulta por Id: Cliente existente retorna DTO exitoso con información completa [1 ms]
  Mensaje de error:
   System.NotImplementedException : SF-018: Fase RED - ObtenerPorIdAsync pendiente de implementación.
  Seguimiento de la pila:
     at SistemaFuneraria.Application.Servicios.Clientes.ClienteService.ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) in <REPO_ROOT>/src/SistemaFuneraria.Application/Servicios/Clientes/ClienteService.cs:line 29
     at SistemaFuneraria.Tests.Application.ClienteServiceTests.ObtenerPorIdAsync_ConClienteExistente_DebeRetornarClienteDtoExitoso() in <REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/ClienteServiceTests.cs:line 320

  Con error Consulta por CURP: Cliente existente retorna DTO exitoso incluso con espacios o minúsculas [< 1 ms]
  Mensaje de error:
   System.NotImplementedException : SF-018: Fase RED - ObtenerPorCurpAsync pendiente de implementación.
  Seguimiento de la pila:
     at SistemaFuneraria.Application.Servicios.Clientes.ClienteService.ObtenerPorCurpAsync(String curp, CancellationToken cancellationToken) in <REPO_ROOT>/src/SistemaFuneraria.Application/Servicios/Clientes/ClienteService.cs:line 35
     at SistemaFuneraria.Tests.Application.ClienteServiceTests.ObtenerPorCurpAsync_ConClienteExistente_DebeRetornarClienteDtoExitoso() in <REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/ClienteServiceTests.cs:line 377

  Con error Listado: Clientes activos retorna lista ordenada alfabéticamente por primer apellido [1 ms]
  Mensaje de error:
   System.NotImplementedException : SF-018: Fase RED - ListarTodosAsync pendiente de implementación.
  Seguimiento de la pila:
     at SistemaFuneraria.Application.Servicios.Clientes.ClienteService.ListarTodosAsync(CancellationToken cancellationToken) in <REPO_ROOT>/src/SistemaFuneraria.Application/Servicios/Clientes/ClienteService.cs:line 41
     at SistemaFuneraria.Tests.Application.ClienteServiceTests.ListarTodosAsync_ConClientesExistentes_DebeRetornarListaOrdenadaPorPrimerApellido() in <REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/ClienteServiceTests.cs:line 437

No se pudo ejecutar la serie de pruebas.
Pruebas totales: 37
     Incorrecto: 37
 Tiempo total: 1.3589 Segundos
```

### 3.2. Verificación de Regresión (248 pruebas del resto del sistema)

```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket!=SF-018" --logger "console;verbosity=minimal"
```

```text
Serie de pruebas para <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:   248, Omitido:     0, Total:   248, Duración: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```

---

## 4. Estado de Preparación para la Fase GREEN

El entorno queda preparado para que el desarrollador implemente el cuerpo de [`ClienteService.cs`](file://<REPO_ROOT>/src/SistemaFuneraria.Application/Servicios/Clientes/ClienteService.cs):
- **Dependencias inyectadas:** `IAppDbContext` listo para operaciones async de EF Core (`Clientes.AddAsync`, `Clientes.AnyAsync`, `Clientes.FirstOrDefaultAsync`, `Clientes.AsNoTracking().ToListAsync`).
- **Validaciones esperadas:** Usar `Resultado.Fallo<T>(...)` ante errores de entrada sin lanzar excepciones no controladas.
- **Normalización esperada:** CURP en mayúsculas (`Trim().ToUpperInvariant()`), correo en minúsculas (`Trim().ToLowerInvariant()`).
- **Respeto a contratos:** Todos los DTOs y Requests definidos en SF-017 encajan sin modificaciones.
