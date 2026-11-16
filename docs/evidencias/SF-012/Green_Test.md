# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-012 |
| **Caso de Prueba Relacionado** | DT-01 a DT-36 / RNF-02 / CP-05 / CP-06 |
| **Fecha y Hora de Ejecución** | 11/11/2026 15:57 |
| **Desarrollador / Tester** | Lead Fullstack (Remediación QA) |
| **Rama Git** | feature/SF-012-oraculo-36-combinaciones-test |

---

### 1. Objetivo de la Prueba
Certificar la fase **VERDE** tras resolver los bloqueos de la rama `feature/SF-012-oraculo-36-combinaciones-test`:
1. Restauración de `tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj` al proyecto limpio `.NET 8` heredado de `dev`.
2. Ejecución real del motor financiero de `SistemaFuneraria.Core` (`CalculadoraFinanciamiento` y `GeneradorAmortizacion`) en `tests/SistemaFuneraria.Tests/Core/OraculoFinancieroTests.cs` contra las **36 combinaciones cartesianas** de `docs/anexos/oraculo-36-combinaciones.csv` con tolerancia estricta de **`$0.00`** al centavo.
3. Alineación del generador `docs/anexos/generar_oraculo.py`, del CSV oficial y de la matriz `docs/02-matriz-trazabilidad.md` (`DT-06`, `DT-08`, `DT-12`, `DT-36`) con la regla normativa del PRD **`RNF-02`** (`MidpointRounding.ToEven` / Banker's Rounding).

---

### 2. Comandos de Terminal Ejecutados
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~OraculoFinanciero"
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName!~PostgresConexionTests"
```

---

### 3. Salida de Terminal (Pase Exitoso)

#### A. Ejecución específica del Oráculo de 36 Combinaciones (`SF-012`)
```text
  Determinando los proyectos que se van a restaurar...
  Todos los proyectos están actualizados para la restauración.
  SistemaFuneraria.Core -> <REPO_ROOT>/src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> <REPO_ROOT>/src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> <REPO_ROOT>/src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> <REPO_ROOT>/src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
  SistemaFuneraria.Tests -> <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll
Serie de pruebas para <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:    36, Omitido:     0, Total:    36, Duración: 30 ms - SistemaFuneraria.Tests.dll (net8.0)
```

#### B. Regresión completa de la suite no dependiente de PostgreSQL
```text
Serie de pruebas para <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:   192, Omitido:     0, Total:   192, Duración: 87 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Explicación del Resultado
Las **36 combinaciones** aprobaron con **`$0.00` de discrepancia** en **`30 ms`**, verificando simultáneamente en cada escenario (`3 Lotes × 3 Enganches × 4 Plazos`):
1. `CalculadoraFinanciamiento.CalcularEnganche(precio, enganchePct)` coincide exactamente con `engancheEsperado` y `capitalFinanciadoEsperado`.
2. `CalculadoraFinanciamiento.ObtenerTasas(plazoMeses)` coincide exactamente con `tasaAnualPct / 100m`.
3. `CalculadoraFinanciamiento.CalcularMensualidadOrdinaria(capital, tasaMensual, plazoMeses)` coincide exactamente con `mensualidadEsperada`.
4. `GeneradorAmortizacion.Generar(capital, tasaMensual, plazoMeses)` produce exactamente `plazoMeses` renglones y verifica al centavo:
   - Última cuota ajustada (`ultimoPeriodo.PagoMensual == ultimaMensualidadEsperada`).
   - Cierre de deuda exacto (`ultimoPeriodo.SaldoFinal == 0.00m`).
   - Suma total de intereses (`tabla.Sum(r => r.Interes) == interesesTotalesEsperados`).
   - Costo financiero total (`enganche + tabla.Sum(r => r.PagoMensual) == totalPagadoEsperado`).
5. La suite completa sin dependencia de Docker (`Core`, `Application`, `Arquitectura`, cultura `es-MX` y el propio Oráculo) pasó **`192/192` en `87 ms`** sin advertencias (`TreatWarningsAsErrors = true`).
