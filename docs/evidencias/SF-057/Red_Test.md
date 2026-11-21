# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-057 |
| **Caso de Prueba Relacionado** | Entidad `Venta` (`FolioContrato`, `ModalidadVenta`, `EstadoVenta`, `PrecioPactado`) |
| **Fecha y Hora de Ejecución** | 18/11/2026 14:14 |
| **Desarrollador / Tester** | Master Senior Backend & DBA Architect |
| **Rama Git** | `feature/SF-057-entidad-venta-core` |

---

### 1. Objetivo de la Prueba
Demostrar el fallo esperado de compilación antes de implementar `ModalidadVenta`, `EstadoVenta` y `Venta` en `src/SistemaFuneraria.Core/`.

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
tests/SistemaFuneraria.Tests/Unit/Core/VentaTests.cs(27,21): error CS0103: El nombre 'Venta' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/VentaTests.cs(35,37): error CS0103: El nombre 'ModalidadVenta' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/VentaTests.cs(36,34): error CS0103: El nombre 'EstadoVenta' no existe en el contexto actual

ERROR al compilar.
    0 Advertencia(s)
    31 Errores
```
