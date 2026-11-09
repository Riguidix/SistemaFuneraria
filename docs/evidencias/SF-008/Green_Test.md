# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-008 |
| **Caso de Prueba Relacionado** | CP-03, RN-04, RN-01, RF-04 |
| **Fecha y Hora de Ejecución** | 08/11/2026 15:38 |
| **Desarrollador / Tester** | Gustavo Lucio |
| **Rama Git** | feature/SF-008-calculo-enganche-tdd |

---

### 1. Objetivo de la Prueba
Demostrar que la implementación de `CalculadoraFinanciamiento.CalcularEnganche` satisface con exactitud matemática el cálculo del enganche y capital financiado (CP-03, RF-04), valida estrictamente el catálogo inmutable de precios (RN-01) y porcentajes de enganche (RN-04), y aprueba al 100% las 24 pruebas unitarias en fase VERDE (GREEN).

### 2. Datos de Entrada Utilizados
* **Escenario Gherkin CP-03:** Lote Básico $120,000.00 con 20% de enganche $\rightarrow$ Enganche = $24,000.00, Capital = $96,000.00.
* **Matriz exhaustiva de combinaciones autorizadas (3 lotes x 3 enganches = 9 casos):**
  * $120,000.00 con 10% ($12k / $108k), 20% ($24k / $96k), 30% ($36k / $84k).
  * $150,000.00 con 10% ($15k / $135k), 20% ($30k / $120k), 30% ($45k / $105k).
  * $180,000.00 con 10% ($18k / $162k), 20% ($36k / $144k), 30% ($54k / $126k).
* **Casos inválidos de porcentaje (8 casos):** 0%, 5%, 15%, 25%, 35%, 50%, 100%, -10% $\rightarrow$ Lanzan `ArgumentOutOfRangeException` (RN-04).
* **Casos inválidos de precio (6 casos):** $0.00, -$50,000.00, $100,000.00, $99,999.99, $120,000.01, $200,000.00 $\rightarrow$ Lanzan `ArgumentOutOfRangeException` (RN-01).

### 3. Comando de Terminal Ejecutado
`dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-008"`

### 4. Salida de Terminal (Pase Exitoso)
```text
Serie de pruebas para C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:    24, Omitido:     0, Total:    24, Duración: 70 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La prueba pasó exitosamente (24 pruebas en VERDE) demostrando que:
1. `CalcularEnganche(120000m, 20)` retorna exactamente Enganche = `$24,000.00` y Capital Financiado = `$96,000.00` conforme a CP-03.
2. La suma de Enganche + Capital Financiado es idéntica al precio de lista del lote en todas las combinaciones oficiales, preservando la conservación del capital sin pérdidas por redondeo.
3. Se rechazan valores de enganche no autorizados arrojando `ArgumentOutOfRangeException` con el nombre de parámetro `porcentajeEnganche` y mención explícita a la regla `RN-04`.
4. Se rechazan precios de lote no autorizados arrojando `ArgumentOutOfRangeException` con el nombre de parámetro `precioLote` y mención explícita a la regla `RN-01`.
5. Los cálculos se ejecutan en tiempo $O(1)$ con aritmética `decimal` de 128 bits y redondeo bancario `MidpointRounding.ToEven` (RNF-02).

---

### 6. Matriz de Cumplimiento de Criterios M.E.S.T. (Fase GREEN)

| Pilar M.E.S.T. | Criterio de Calidad | Evidencia Técnica en Fase GREEN | Veredicto |
|---|---|---|:---:|
| **M - Mantenible** | Código autodocumentado, tuplas descriptivas y mensajes de excepción claros. | Firma limpia `(decimal Enganche, decimal CapitalFinanciado)` con validaciones tempranas (guard clauses). | ✅ APROBADO |
| **E - Eficiente** | Cálculo O(1) con validaciones instantáneas usando el catálogo inmutable `ParametrosNegocio`. | Búsquedas O(1) sobre colecciones inmutables y cálculo directo sin iteraciones. | ✅ APROBADO |
| **S - Seguridad** | Invariantes protegidos contra inyecciones de precios o porcentajes arbitrarios. | Validación estricta con guard clauses y tipado fuerte decimal que previene inconsistencias financieras. | ✅ APROBADO |
| **T - Testeable** | 100% de caminos y bordes cubiertos con aserciones xUnit y FluentAssertions. | 24 pruebas unitarias pasando al 100% en verde sin advertencias ni dependencias externas. | ✅ APROBADO |
