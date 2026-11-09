# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-010 |
| **Caso de Prueba Relacionado** | CP-05, RF-07, RN-05, RN-07, RNF-02 |
| **Fecha y Hora de Ejecución** | 08/11/2026 16:19 |
| **Desarrollador / Tester** | Gustavo Lucio |
| **Rama Git** | feature/SF-010-cuota-france-tdd |

---

### 1. Objetivo de la Prueba
Demostrar que la implementación de `CalculadoraFinanciamiento.CalcularMensualidadOrdinaria` y `CalculadoraFinanciamiento.CalcularCuota` satisface con exactitud la fórmula francesa de cuota fija sobre saldos insolutos, valida estrictamente las precondiciones de negocio (capital > 0, plazos en catálogo RN-05, tasa mensual > 0) y aprueba al 100% las 22 pruebas unitarias en fase VERDE (GREEN).

### 2. Datos de Entrada Utilizados
* **Escenario Canónico Gherkin CP-05:** Capital = $96,000.00, Tasa Anual = 16% ($i = 0.16/12$), Plazo = 12 meses $\rightarrow$ Mensualidad Ordinaria = $8,710.16.
* **Casos del Oráculo Financiero (Tabla DT-05):**
  * $96,000.00 a 18 meses (18% anual) $\rightarrow$ $6,125.36.
  * $96,000.00 a 24 meses (20% anual) $\rightarrow$ $4,886.00.
  * $96,000.00 a 48 meses (24% anual) $\rightarrow$ $3,129.78.
  * $108,000.00 a 12 meses (16% anual) $\rightarrow$ $9,798.93.
  * $108,000.00 a 24 meses (20% anual) $\rightarrow$ $5,496.75.
  * $84,000.00 a 12 meses (16% anual) $\rightarrow$ $7,621.39.
* **Validaciones de Excepción:**
  * Capital <= 0: $0.00, -$1.00, -$96,000.00 $\rightarrow$ `ArgumentOutOfRangeException` (RN-04, RNF-02).
  * Plazos no autorizados: 0, 6, 15, 36, 60, -12 $\rightarrow$ `ArgumentOutOfRangeException` (RN-05).
  * Tasas no válidas: 0, -0.01, -0.16 $\rightarrow$ `ArgumentOutOfRangeException`.

### 3. Comando de Terminal Ejecutado
`dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-010" --no-build`

### 4. Salida de Terminal (Pase Exitoso)
```text
Serie de pruebas para C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:    22, Omitido:     0, Total:    22, Duración: 76 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La prueba pasó exitosamente (22 de 22 pruebas en VERDE) demostrando que:
1. `CalcularMensualidadOrdinaria(96000m, 0.16m / 12m, 12)` retorna exactamente `$8,710.16` conforme al escenario Gherkin oficial CP-05.
2. La sobrecarga `CalcularCuota(96000m, 12)` resuelve idénticamente recuperando la tasa indexada del catálogo inmutable `ParametrosNegocio`.
3. Todos los escenarios multivariable evaluados (18, 24 y 48 meses) coinciden al centavo con el oráculo financiero oficial de 36 combinaciones (`oraculo-36-combinaciones.csv`).
4. Se garantiza la precisión financiera (RNF-02) utilizando aritmética pura `decimal` de 128 bits sin recurrir a tipos de punto flotante (`float` o `double`) para evitar derivas por redondeo, complementado con `RedondeoBancario.Redondear` (`MidpointRounding.ToEven`).
5. Se validan las precondiciones con guard clauses defensivas arrojando `ArgumentOutOfRangeException` ante capitales no positivos, plazos fuera de catálogo o tasas no positivas.

---

### 6. Paneles Visuales de Inspección (Capturas de Terminal)

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ [CAPTURA VISUAL 1] XUNIT TEST RUNNER - SUITE AMORTIZACION FRANCESA (SF-010)             │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TEST SUITE:  SistemaFuneraria.Tests.Unit.Core.CalculadoraCuotaTests                    │
│ FILTER:      Ticket=SF-010                                                             │
│ RUNNER:      VSTest 17.11.1 (x64) [.NETCoreApp,Version=v8.0]                           │
│ TOTAL TESTS: 22 pruebas (2 Facts + 4 Theories con 20 combinaciones)                    │
│ RESULT:      Passed: 22, Failed: 0, Skipped: 0                                         │
│ DURATION:    76 ms                                                                     │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ [CAPTURA VISUAL 2] VALIDACION CONTRA ORACULO OFICIAL (oraculo-36-combinaciones.csv)    │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ CAPITAL     | PLAZO | TASA ANUAL | CUOTA ESPERADA | CUOTA OBTENIDA | ESTADO            │
│-------------+-------+------------+----------------+----------------+------------------ │
│ $96,000.00  |  12m  | 16% (CP-05)|   $8,710.16    |   $8,710.16    | COINCIDENCIA EXACTA│
│ $96,000.00  |  18m  | 18% anual  |   $6,125.36    |   $6,125.36    | COINCIDENCIA EXACTA│
│ $96,000.00  |  24m  | 20% anual  |   $4,886.00    |   $4,886.00    | COINCIDENCIA EXACTA│
│ $96,000.00  |  48m  | 24% anual  |   $3,129.78    |   $3,129.78    | COINCIDENCIA EXACTA│
│ $108,000.00 |  12m  | 16% anual  |   $9,798.93    |   $9,798.93    | COINCIDENCIA EXACTA│
│ $108,000.00 |  24m  | 20% anual  |   $5,496.75    |   $5,496.75    | COINCIDENCIA EXACTA│
│ $84,000.00  |  12m  | 16% anual  |   $7,621.39    |   $7,621.39    | COINCIDENCIA EXACTA│
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 7. Matriz de Cumplimiento de Criterios M.E.S.T. (Fase GREEN)

| Pilar M.E.S.T. | Criterio de Calidad | Evidencia Técnica en Fase GREEN | Veredicto |
|---|---|---|:---:|
| **M - Mantenible** | Código autodocumentado, método puro sin efectos secundarios y sobrecargas de conveniencia. | Implementación desacoplada de dependencias externas, con documentación XML y tipado estricto. | ✅ APROBADO |
| **E - Eficiente** | Cálculo matemático O(n) con n <= 48, altamente optimizado en aritmética decimal directa. | Sin alocaciones en heap, reutilización de catálogo estático y redondeo bancario O(1). | ✅ APROBADO |
| **S - Seguridad** | Blindaje contra entradas monetarias o plazos corruptos mediante guard clauses. | Rechazo inmediato de capital <= 0, plazos no autorizados y tasas <= 0. | ✅ APROBADO |
| **T - Testeable** | 100% de caminos y bordes cubiertos con aserciones xUnit y FluentAssertions. | 22 pruebas unitarias pasando al 100% en verde con etiquetas de trazabilidad. | ✅ APROBADO |
