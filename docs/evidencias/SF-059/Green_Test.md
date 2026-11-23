# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-059 |
| **Caso de Prueba Relacionado** | `VentaConfiguration`, `FinanciamientoConfiguration` (`xmin`), `MensualidadConfiguration` y `AddVentasFinanciamientos` |
| **Fecha y Hora de Ejecución** | 19/11/2026 10:37 |
| **Desarrollador / Tester** | Master Senior Backend & DBA Architect |
| **Rama Git** | `feature/SF-059-ventas-financiamiento-migracion` |

---

### 1. Objetivo de la Prueba
Verificar que `IAppDbContext`, `AppDbContext` (`seq_contratos_folio` `START WITH 190 INCREMENT BY 1`), `VentaConfiguration`, `FinanciamientoConfiguration` (`xmin`), `MensualidadConfiguration` y la migración reversible `AddVentasFinanciamientos` compilan con 0 advertencias y superan las pruebas unitarias de `SF-059` y la suite no-integración.

*(Nota de alcance: las pruebas de concurrencia optimista con `DbUpdateConcurrencyException` contra PostgreSQL 16 real pertenecen al carril de integración `Tipo=Integration` con base activa; en esta sesión se validaron el mapeo EF Core de `VersionFila`/`xmin`, `IsConcurrencyToken`, `DeleteBehavior.Restrict`, `uq_ventas_lote_activo` y el DDL generado en `Up()`/`Down()`.)*

### 2. Archivos Productivos y Blast Radius
- `src/SistemaFuneraria.Application/Comun/Persistencia/IAppDbContext.cs`: `+3` líneas
- `src/SistemaFuneraria.Infrastructure/Persistencia/AppDbContext.cs`: `+8` líneas
- `src/SistemaFuneraria.Infrastructure/Persistencia/Configuraciones/VentaConfiguration.cs`: `48` líneas
- `src/SistemaFuneraria.Infrastructure/Persistencia/Configuraciones/FinanciamientoConfiguration.cs`: `49` líneas
- `src/SistemaFuneraria.Infrastructure/Persistencia/Configuraciones/MensualidadConfiguration.cs`: `45` líneas
- **Total código productivo (sin migración autogenerada ni snapshot)**: **153 LOCs** ($\le 250$ LOCs permitidos)

### 3. Comandos Ejecutados
```bash
dotnet build SistemaFuneraria.sln --no-restore -m:1 --disable-build-servers
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-059" --no-build -m:1 --disable-build-servers
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers
```

### 4. Salida Real de Terminal
```text
$ dotnet build SistemaFuneraria.sln --no-restore -m:1 --disable-build-servers
Compilación correcta.
    0 Advertencia(s)
    0 Errores

$ dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-059" --no-build -m:1 --disable-build-servers
Correctas! - Con error:     0, Superado:     4, Omitido:     0, Total:     4, Duración: 75 ms - SistemaFuneraria.Tests.dll (net8.0)

$ dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers
Correctas! - Con error:     0, Superado:   415, Omitido:     0, Total:   415, Duración: 3 s - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Resultado
- **Build**: 0 errores, 0 advertencias.
- **`SF-059` (`VentasFinanciamientosConfigurationTests`)**: 4/4 pruebas superadas.
- **Suite no-integración**: 415/415 pruebas superadas sin regresiones.
- **Migración `20261119163710_AddVentasFinanciamientos`**: `Up()` crea secuencia `seq_contratos_folio` (`startValue: 190`) y tablas `ventas`, `financiamientos`, `mensualidades` con `ReferentialAction.Restrict` e índice único filtrado `uq_ventas_lote_activo`; `Down()` revierte en orden inverso con `DropTable` y `DropSequence`.
