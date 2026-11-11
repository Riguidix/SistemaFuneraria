# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-012 |
| **Caso de Prueba Relacionado** | DT-01 a DT-36 / RNF-02 / CP-05 / CP-06 |
| **Fecha y Hora de Ejecución** | 11/11/2026 15:54 |
| **Desarrollador / Tester** | Lead Fullstack (Remediación QA) |
| **Rama Git** | feature/SF-012-oraculo-36-combinaciones-test |

---

### 1. Objetivo de la Prueba
Documentar el estado real de falla (**RED**) encontrado al remediar la rama `feature/SF-012-oraculo-36-combinaciones-test` (commit `f1e742b`):

1. **Bloqueo inicial de compilación y prueba vacía (tautológica):**
   - `tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj` contenía marcadores de conflicto Git sin resolver (`<<<<<<< HEAD` / `=======` / `>>>>>>> 1d45f133...`) mezclando la configuración limpia `.NET 8` + `Directory.Packages.props` con un intento incompatible de `.NET 10.0`.
   - `tests/SistemaFuneraria.Tests/Core/OraculoFinancieroTests.cs` no invocaba ningún método de `SistemaFuneraria.Core`; únicamente ejecutaba aserciones tautológicas sobre el CSV (`Assert.True(precio > 0)`, etc.).
   - `docs/evidencias/SF-012/` contenía dos archivos vacíos de 0 bytes (`SF-012-Red_Test.md` y `SF-012-Green_Test.md`).
2. **Defecto real detectado al conectar el Oráculo con `SistemaFuneraria.Core`:**
   Al restaurar `.csproj` desde `dev` y conectar `OraculoFinancieroTests.cs` contra `CalculadoraFinanciamiento` (`CalcularEnganche`, `ObtenerTasas`, `CalcularMensualidadOrdinaria`) y `GeneradorAmortizacion.Generar` con precisión exacta de `$0.00` (tolerancia cero), **32 combinaciones pasaron y 4 fallaron exactamente por `-0.01M` (1 centavo)** en `ultimaMensualidadEsperada` debido a discrepancia de estrategia de redondeo entre `docs/01-PRD-requisitos.md` (`RNF-02: MidpointRounding.ToEven` implementado en `Core`) y `docs/anexos/generar_oraculo.py` (`ROUND_HALF_UP` / `AwayFromZero` usado al generar `oraculo-36-combinaciones.csv` y `DT-01..DT-36`).

---

### 2. Datos de Entrada Utilizados
* Archivo fuente: `docs/anexos/oraculo-36-combinaciones.csv` (**36 combinaciones cartesianas**: 3 tipos de lote × 3 porcentajes de enganche × 4 plazos autorizados).
* Combinaciones en conflicto detectadas (diferencia `-0.01M` en cuota final, intereses totales y total pagado):
  - **DT-06:** `Lote basico`, 20% enganche, 18 meses (18% anual) — CSV `6125.28` vs Core `6125.27`
  - **DT-08:** `Lote basico`, 20% enganche, 48 meses (24% anual) — CSV `3129.44` vs Core `3129.43`
  - **DT-12:** `Lote basico`, 30% enganche, 48 meses (24% anual) — CSV `2738.87` vs Core `2738.86`
  - **DT-36:** `Lote ampliado`, 30% enganche, 48 meses (24% anual) — CSV `4107.92` vs Core `4107.91`

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~OraculoFinanciero" --logger "console;verbosity=detailed"
```

---

### 4. Salida de Terminal (Falla Real Observada — 32 Correctas / 4 Incorrectas)
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
<REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.08]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.18]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.19]   Starting:    SistemaFuneraria.Tests
...
  Con error SistemaFuneraria.Tests.Core.OraculoFinancieroTests.ValidarCombinacionOraculoFinanciero(lote: "Lote basico", gavetas: 2, precio: 120000.00, enganchePct: 20, engancheEsperado: 24000.00, capitalFinanciadoEsperado: 96000.00, plazoMeses: 18, tasaAnualPct: 18, mensualidadEsperada: 6125.36, ultimaMensualidadEsperada: 6125.28, interesesTotalesEsperados: 14256.40, totalPagadoEsperado: 134256.40) [74 ms]
  Mensaje de error:
   Expected ultimoPeriodo.PagoMensual to be 6125.28M because (Lote basico, 20%, 18m) cuota final ajustada debe coincidir con el oráculo, but found 6125.27M (difference of -0.01).

  Con error SistemaFuneraria.Tests.Core.OraculoFinancieroTests.ValidarCombinacionOraculoFinanciero(lote: "Lote basico", gavetas: 2, precio: 120000.00, enganchePct: 20, engancheEsperado: 24000.00, capitalFinanciadoEsperado: 96000.00, plazoMeses: 48, tasaAnualPct: 24, mensualidadEsperada: 3129.78, ultimaMensualidadEsperada: 3129.44, interesesTotalesEsperados: 54229.10, totalPagadoEsperado: 174229.10) [2 ms]
  Mensaje de error:
   Expected ultimoPeriodo.PagoMensual to be 3129.44M because (Lote basico, 20%, 48m) cuota final ajustada debe coincidir con el oráculo, but found 3129.43M (difference of -0.01).

  Con error SistemaFuneraria.Tests.Core.OraculoFinancieroTests.ValidarCombinacionOraculoFinanciero(lote: "Lote ampliado", gavetas: 4, precio: 180000.00, enganchePct: 30, engancheEsperado: 54000.00, capitalFinanciadoEsperado: 126000.00, plazoMeses: 48, tasaAnualPct: 24, mensualidadEsperada: 4107.83, ultimaMensualidadEsperada: 4107.92, interesesTotalesEsperados: 71175.93, totalPagadoEsperado: 251175.93) [1 ms]
  Mensaje de error:
   Expected ultimoPeriodo.PagoMensual to be 4107.92M because (Lote ampliado, 30%, 48m) cuota final ajustada debe coincidir con el oráculo, but found 4107.91M (difference of -0.01).

  Con error SistemaFuneraria.Tests.Core.OraculoFinancieroTests.ValidarCombinacionOraculoFinanciero(lote: "Lote basico", gavetas: 2, precio: 120000.00, enganchePct: 30, engancheEsperado: 36000.00, capitalFinanciadoEsperado: 84000.00, plazoMeses: 48, tasaAnualPct: 24, mensualidadEsperada: 2738.55, ultimaMensualidadEsperada: 2738.87, interesesTotalesEsperados: 47450.72, totalPagadoEsperado: 167450.72) [1 ms]
  Mensaje de error:
   Expected ultimoPeriodo.PagoMensual to be 2738.87M because (Lote basico, 30%, 48m) cuota final ajustada debe coincidir con el oráculo, but found 2738.86M (difference of -0.01).

No se pudo ejecutar la serie de pruebas.
Pruebas totales: 36
     Correcto: 32
     Incorrecto: 4
 Tiempo total: 0.9816 Segundos
```

---

### 5. Explicación del Resultado
1. **Valor real de SF-012:** Al sustituir la prueba vacía original por la verificación real contra `SistemaFuneraria.Core`, el oráculo cumplió su función de control de calidad y detectó de inmediato una divergencia especificada de **1 centavo (`-$0.01`)** en exactamente 4 de las 36 combinaciones.
2. **Causa raíz matemática verificada:**
   - En `docs/01-PRD-requisitos.md` (`RNF-02`) y en los tickets previos aprobados (`SF-006`, `SF-008`, `SF-010`, `SF-011`), todo el motor financiero aplica **Banker's Rounding (`MidpointRounding.ToEven`)**.
   - En contraste, el generador `docs/anexos/generar_oraculo.py` utilizó `ROUND_HALF_UP` (`MidpointRounding.AwayFromZero`, citado en su encabezado como `"ADR-001 en FUN-007"`), produciendo esos mismos 4 valores en `oraculo-36-combinaciones.csv` y en la tabla `DT-01..DT-36` de `docs/02-matriz-trazabilidad.md`.
3. Se mantiene estrictamente el estado **ROJO** sin relajar aserciones (`$0.00` de tolerancia) hasta alinear oficialmente la fuente de verdad entre PRD `RNF-02` y el oráculo de referencia para pasar a **VERDE (36/36)**.
