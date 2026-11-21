# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-057 |
| **Caso de Prueba Relacionado** | Entidad `Venta` (`FolioContrato`, `ModalidadVenta`, `EstadoVenta`, `PrecioPactado`) |
| **Fecha y Hora de Ejecución** | 18/11/2026 14:16 |
| **Desarrollador / Tester** | Master Senior Backend & DBA Architect |
| **Rama Git** | `feature/SF-057-entidad-venta-core` |

---

### 1. Objetivo de la Prueba
Verificar que `ModalidadVenta`, `EstadoVenta` y `Venta` cumplen los invariantes de dominio, validaciones de folio `CTR-YYYY-XXXX`, transiciones de estado (`CrearContado`, `CrearFinanciada`, `CambiarEstadoAdmin`, `MarcarLiquidado`), compilación limpia con 0 advertencias y cero regresiones en la suite no-integración.

### 2. Archivos Productivos y Blast Radius
- `src/SistemaFuneraria.Core/Enums/ModalidadVenta.cs`: 10 líneas
- `src/SistemaFuneraria.Core/Enums/EstadoVenta.cs`: 12 líneas
- `src/SistemaFuneraria.Core/Entidades/Venta.cs`: 152 líneas
- **Total código productivo**: **174 LOCs** ($\le 180$ LOCs permitidos)

### 3. Comandos Ejecutados
```bash
dotnet build SistemaFuneraria.sln --no-restore -m:1 --disable-build-servers
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-057" --no-build -m:1 --disable-build-servers
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers
```

### 4. Salida Real de Terminal
```text
$ dotnet build SistemaFuneraria.sln --no-restore -m:1 --disable-build-servers
Compilación correcta.
    0 Advertencia(s)
    0 Errores

$ dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-057" --no-build -m:1 --disable-build-servers
Correctas! - Con error:     0, Superado:    18, Omitido:     0, Total:    18, Duración: 47 ms - SistemaFuneraria.Tests.dll (net8.0)

$ dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers
Correctas! - Con error:     0, Superado:   399, Omitido:     0, Total:   399, Duración: 3 s - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Resultado
- **Build**: 0 errores, 0 advertencias.
- **`VentaTests` (`SF-057`)**: 18/18 pruebas superadas.
- **Suite no-integración**: 399/399 pruebas superadas sin regresiones.
