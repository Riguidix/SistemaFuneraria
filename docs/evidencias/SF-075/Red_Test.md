# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-075 |
| **Caso de Prueba Relacionado** | Entidad `Pago` (`Folio`, `TipoPago`, `MetodoPago`, `Referencia`, `ClaveIdempotencia`) |
| **Fecha y Hora de Ejecución** | 19/11/2026 11:22 |
| **Desarrollador / Tester** | Master Senior Backend & DBA Architect |
| **Rama Git** | `feature/SF-075-entidad-pago-core` |

---

### 1. Objetivo de la Prueba
Demostrar el fallo esperado de compilación antes de implementar `TipoPago`, `MetodoPago` y `Pago` en `src/SistemaFuneraria.Core/`.

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
tests/SistemaFuneraria.Tests/Unit/Core/PagoTests.cs(30,9): error CS0246: El nombre del tipo o del espacio de nombres 'TipoPago' no se encontró
tests/SistemaFuneraria.Tests/Unit/Core/PagoTests.cs(31,9): error CS0246: El nombre del tipo o del espacio de nombres 'MetodoPago' no se encontró

ERROR al compilar.
    0 Advertencia(s)
    20 Errores
```
