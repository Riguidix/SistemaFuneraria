# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-048 |
| **Caso de Prueba Relacionado** | `ISimuladorService.Cotizar` y `SimulacionCotizacionDto` |
| **Fecha y Hora de Ejecución** | 19/11/2026 11:15 |
| **Desarrollador / Tester** | Master Senior Backend & DBA Architect |
| **Rama Git** | `feature/SF-048-simulador-cotizacion-app` |

---

### 1. Objetivo de la Prueba
Verificar que `SimulacionCotizacionDto`, `ISimuladorService.Cotizar` y `SimuladorService` orquestan el motor financiero de `Core` (`CalculadoraFinanciamiento` y `GeneradorAmortizacion`) y `TimeProvider` sin romper `Simular(SimularFinanciamientoRequest)` de `v0.1.0`.

*(Nota matemática: para `$120,000.00`, `20%` enganche y `24` meses al `20%` anual, la fuente oficial `docs/anexos/oraculo-36-combinaciones.csv` línea 8 certifica `$4,886.00` —en lugar de `$4,887.54` mostrado en el texto de mockup de `docs/08-backlog.md`—, coincidiendo al céntimo con el motor `CalculadoraFinanciamiento`.)*

### 2. Archivos Productivos y Blast Radius
- `src/SistemaFuneraria.Application/DTOs/Simulador/SimulacionCotizacionDto.cs`: `40` líneas
- `src/SistemaFuneraria.Application/Servicios/Simulador/ISimuladorService.cs`: `+8` líneas
- `src/SistemaFuneraria.Application/Servicios/Simulador/SimuladorService.cs`: `+99/-2` líneas
- **Total código productivo**: **145 LOCs netas** ($\le 210$ LOCs permitidos)

### 3. Comandos Ejecutados
```bash
dotnet build SistemaFuneraria.sln --no-restore -m:1 --disable-build-servers
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-048" --no-build -m:1 --disable-build-servers
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers
```

### 4. Salida Real de Terminal
```text
$ dotnet build SistemaFuneraria.sln --no-restore -m:1 --disable-build-servers
Compilación correcta.
    0 Advertencia(s)
    0 Errores

$ dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-048" --no-build -m:1 --disable-build-servers
Correctas! - Con error:     0, Superado:    10, Omitido:     0, Total:    10, Duración: 63 ms - SistemaFuneraria.Tests.dll (net8.0)

$ dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers
Correctas! - Con error:     0, Superado:   391, Omitido:     0, Total:   391, Duración: 2 s - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Resultado
- **Build**: 0 errores, 0 advertencias.
- **`SimuladorCotizacionTests` (`SF-048`)**: 10/10 pruebas superadas.
- **Suite no-integración**: 391/391 pruebas superadas sin regresiones (incluyendo `SimuladorServiceTests` y `SimuladorLimitesTests` de `v0.1.0`).
