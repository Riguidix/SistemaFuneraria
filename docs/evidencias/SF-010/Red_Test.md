# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-010 |
| **Caso de Prueba Relacionado** | CP-05, RF-07, RN-05, RN-07, RNF-02 |
| **Fecha y Hora de Ejecución** | 08/11/2026 16:17 |
| **Desarrollador / Tester** | Gustavo Lucio |
| **Rama Git** | feature/SF-010-cuota-france-tdd |

---

### 1. Objetivo de la Prueba
Demostrar que los métodos `CalculadoraFinanciamiento.CalcularMensualidadOrdinaria` y `CalculadoraFinanciamiento.CalcularCuota` no poseen implementación operativa y lanzan intencionalmente `System.NotImplementedException`, garantizando la fase ROJO (RED) del ciclo TDD estricto antes de escribir el código de producción.

### 2. Datos de Entrada Utilizados
* **Escenario Canónico Gherkin CP-05:** Capital = $96,000.00, Tasa Anual = 16% ($i = 0.16/12$), Plazo = 12 meses.
* **Casos del Oráculo Financiero:**
  * Capital = $96,000.00 a 18 meses (18% anual), 24 meses (20% anual), 48 meses (24% anual).
  * Capital = $108,000.00 a 12 meses (16% anual) y 24 meses (20% anual).
  * Capital = $84,000.00 a 12 meses (16% anual).
* **Casos inválidos de prueba:**
  * Capitales inválidos: $0.00, -$1.00, -$96,000.00 (RN-04, RNF-02).
  * Plazos inválidos: 0, 6, 15, 36, 60, -12 meses (RN-05).
  * Tasas mensuales inválidas: 0, -0.01, -0.16.

### 3. Comando de Terminal Ejecutado
`dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-010" --no-build`

### 4. Salida de Terminal (Falla Esperada)
```text
Serie de pruebas para C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Con error! - Con error:    22, Superado:     0, Omitido:     0, Total:    22, Duración: 101 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La suite de pruebas falló al 100% (22 pruebas en ROJO) como exige el estándar TDD. La firma de los métodos `CalcularMensualidadOrdinaria` y `CalcularCuota` compila pero arroja `NotImplementedException`, dejando el requerimiento formalmente definido en pruebas antes de la implementación de la fórmula francesa en la fase VERDE.

---

### 6. Matriz de Cumplimiento de Criterios M.E.S.T. (Fase RED)

| Pilar M.E.S.T. | Criterio de Calidad | Evidencia Técnica en Fase RED | Veredicto |
|---|---|---|:---:|
| **M - Mantenible** | Pruebas organizadas con nomenclatura BDD (Given-When-Then) y aserciones legibles. | Estructura en regiones temáticas, documentación XML y mensajes de validación explícitos. | ✅ APROBADO |
| **E - Eficiente** | Definición de sobrecargas y contratos sin alocaciones innecesarias. | Métodos estáticos puros que reciben y retornan `decimal`. | ✅ APROBADO |
| **S - Seguridad** | Cobertura de validaciones de integridad de límites (capital > 0, plazos en catálogo RN-05, tasas > 0). | 14 pruebas de excepción parametrizadas para rechazo con `ArgumentOutOfRangeException`. | ✅ APROBADO |
| **T - Testeable** | 100% de caminos y bordes cubiertos con aserciones xUnit y FluentAssertions. | 22 pruebas unitarias etiquetadas con `[Trait("Ticket", "SF-010")]`. | ✅ APROBADO |
