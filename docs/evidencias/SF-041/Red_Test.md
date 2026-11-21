# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-041 |
| **Caso de Prueba Relacionado** | CP-01 / RF-01 (ILoteService — Catálogo Paginado, Resumen KPIs y CRUD Admin) |
| **Fecha y Hora de Ejecución** | 20/11/2026 17:26 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-041-app-lote-service` |

---

### 1. Objetivo de la Prueba
Demostrar la falla controlada inicial (`NotImplementedException: SF-041: Fase RED.`) en los métodos del servicio de aplicación `LoteService`:
1. `ObtenerCatalogoAsync`: cálculo de los 3 KPIs de cabecera (`Disponibles`, `Precio desde`, `Sección con más espacio`) y filtrado paginado por sección, estado y texto libre.
2. `ObtenerPorIdAsync`: consulta detallada por identificador único.
3. `CrearLoteAsync`: alta de lote con validación de clave única y rechazo ante duplicidad (`[CONFLICTO]`).
4. `ActualizarLoteAsync`: actualización administrativa de ubicación, capacidad, precio y estado.
5. `CambiarEstadoLoteAsync`: transiciones de estado operativo de inventario.

---

### 2. Comando de Terminal Ejecutado
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~LoteServiceTests"
```

---

### 3. Salida Real de Terminal (Fase RED)
```text
Test run for tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:02.28]     SF-041: CrearLoteAsync con clave única debe registrar y retornar Id [FAIL]
  Failed SF-041: CrearLoteAsync con clave única debe registrar y retornar Id [418 ms]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.CrearLoteAsync(CrearLoteCommand command, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 39
  Failed SF-041: ObtenerPorId con lote existente debe retornar DTO [1 s]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 29
  Failed SF-041 (BDD): Cálculo de KPIs y filtrado por sección y estado (CP-01) [52 ms]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.ObtenerCatalogoAsync(String buscar, String seccion, String estado, Nullable`1 gavetas, Int32 pagina, Int32 tamanoPagina, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 24
  Failed SF-041: ObtenerPorId inexistente debe retornar [NO_ENCONTRADO] [1 ms]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 29
  Failed SF-041: Búsqueda reactiva por texto libre en clave o gavetas [25 ms]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.ObtenerCatalogoAsync(String buscar, String seccion, String estado, Nullable`1 gavetas, Int32 pagina, Int32 tamanoPagina, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 24
  Failed SF-041: Cálculo de KPIs de cabecera en catálogo completo [3 ms]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.ObtenerCatalogoAsync(String buscar, String seccion, String estado, Nullable`1 gavetas, Int32 pagina, Int32 tamanoPagina, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 24
  Failed SF-041: CrearLoteAsync con clave duplicada debe retornar [CONFLICTO] [2 ms]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.CrearLoteAsync(CrearLoteCommand command, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 39
  Failed SF-041: Catálogo sin lotes disponibles debe retornar PrecioDesde = 0m [2 ms]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.ObtenerCatalogoAsync(String buscar, String seccion, String estado, Nullable`1 gavetas, Int32 pagina, Int32 tamanoPagina, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 24
  Failed SF-041: CambiarEstadoLoteAsync cambia estado operativo del lote [3 ms]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.CambiarEstadoLoteAsync(Guid id, EstadoLote nuevoEstado, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 49
  Failed SF-041: ActualizarLoteAsync con datos válidos debe persistir cambios [4 ms]
  Error Message:
   System.NotImplementedException : SF-041: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Lotes.LoteService.ActualizarLoteAsync(Guid id, ActualizarLoteCommand command, CancellationToken cancellationToken) in src\SistemaFuneraria.Application\Servicios\Lotes\LoteService.cs:line 44

Failed!  - Failed:    10, Passed:     0, Skipped:     0, Total:    10, Duration: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Conclusión
Fase RED verificada: Las 10 pruebas unitarias fallan de manera controlada con `NotImplementedException`, estableciendo la base para la implementación funcional en `LoteService.cs`.
