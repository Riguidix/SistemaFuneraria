# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-048 |
| **Caso de Prueba Relacionado** | `ISimuladorService.Cotizar` y `SimulacionCotizacionDto` |
| **Fecha y Hora de Ejecución** | 19/11/2026 11:11 |
| **Desarrollador / Tester** | Master Senior Backend & DBA Architect |
| **Rama Git** | `feature/SF-048-simulador-cotizacion-app` |

---

### 1. Objetivo de la Prueba
Demostrar el fallo esperado de compilación antes de crear `DTOs/Simulador/SimulacionCotizacionDto.cs` e implementar `ISimuladorService.Cotizar(SimularCotizacionRequest)`.

### 2. Comando Ejecutado
```bash
dotnet build SistemaFuneraria.sln --no-restore -m:1 --disable-build-servers
```

### 3. Salida Real de Terminal (Falla Esperada)
```text
  SistemaFuneraria.Core -> src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
  SistemaFuneraria.Api -> src/SistemaFuneraria.Api/bin/Debug/net8.0/SistemaFuneraria.Api.dll
tests/SistemaFuneraria.Tests/Unit/Application/SimuladorCotizacionTests.cs(2,41): error CS0234: El tipo o el nombre del espacio de nombres 'Simulador' no existe en el espacio de nombres 'SistemaFuneraria.Application.DTOs'

ERROR al compilar.
    0 Advertencia(s)
    1 Errores
```
