# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-059 |
| **Caso de Prueba Relacionado** | `VentaConfiguration`, `FinanciamientoConfiguration` (`xmin`), `MensualidadConfiguration` y `AddVentasFinanciamientos` |
| **Fecha y Hora de Ejecución** | 19/11/2026 10:35 |
| **Desarrollador / Tester** | Master Senior Backend & DBA Architect |
| **Rama Git** | `feature/SF-059-ventas-financiamiento-migracion` |

---

### 1. Objetivo de la Prueba
Demostrar el fallo esperado de compilación antes de registrar `Ventas`, `Financiamientos`, `Mensualidades`, la secuencia `seq_contratos_folio` y sus configuraciones Fluent API en `Application` e `Infrastructure`.

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
tests/SistemaFuneraria.Tests/Unit/Infrastructure/VentasFinanciamientosConfigurationTests.cs(35,12): error CS1061: "AppDbContext" no contiene una definición para "Ventas"
tests/SistemaFuneraria.Tests/Unit/Infrastructure/VentasFinanciamientosConfigurationTests.cs(36,12): error CS1061: "AppDbContext" no contiene una definición para "Financiamientos"
tests/SistemaFuneraria.Tests/Unit/Infrastructure/VentasFinanciamientosConfigurationTests.cs(37,12): error CS1061: "AppDbContext" no contiene una definición para "Mensualidades"

ERROR al compilar.
    0 Advertencia(s)
    3 Errores
```
