# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-008 |
| **Caso de Prueba Relacionado** | CP-03, RN-04, RN-01, RF-04 |
| **Fecha y Hora de Ejecución** | 08/11/2026 15:30 |
| **Desarrollador / Tester** | Gustavo Lucio |
| **Rama Git** | feature/SF-008-calculo-enganche-tdd |

---

### 1. Objetivo de la Prueba
Demostrar que la función `CalculadoraFinanciamiento.CalcularEnganche` no posee aún implementación operativa y lanza `System.NotImplementedException` al ser invocada, garantizando la fase ROJO (RED) del ciclo TDD estricto antes de escribir el código de producción.

### 2. Datos de Entrada Utilizados
* **Escenario Gherkin CP-03:** Lote Básico $120,000.00 con 20% de enganche.
* **Matriz de combinaciones autorizadas:**
  * Lotes oficiales: $120,000.00, $150,000.00, $180,000.00 (RN-01).
  * Porcentajes oficiales: 10%, 20%, 30% (RN-04).
* **Casos de prueba inválidos:**
  * Porcentajes no autorizados: 0%, 5%, 15%, 25%, 35%, 50%, 100%, -10%.
  * Precios de lote no autorizados: $0.00, -$50,000.00, $100,000.00, $99,999.99, $120,000.01, $200,000.00.

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-008" --logger "console;verbosity=detailed"
```

### 4. Salida de Terminal (Falla Esperada)
```text
Serie de pruebas para C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

[xUnit.net 00:00:00.62]     SistemaFuneraria.Tests.Unit.Core.CalculadoraEngancheTests.CalcularEnganche_ConLoteBasicoYEngancheVeintePorCiento_DebeCalcularEngancheYCapitalExactos [FAIL]
  Error:
    System.NotImplementedException : The method or operation is not implemented.
  Seguimiento de la pila:
    at SistemaFuneraria.Core.Calculos.CalculadoraFinanciamiento.CalcularEnganche(Decimal precioLote, Int32 porcentajeEnganche) in C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\src\SistemaFuneraria.Core\Calculos\CalculadoraFinanciamiento.cs:line 41
    at SistemaFuneraria.Tests.Unit.Core.CalculadoraEngancheTests.CalcularEnganche_ConLoteBasicoYEngancheVeintePorCiento_DebeCalcularEngancheYCapitalExactos() in C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Core\CalculadoraEngancheTests.cs:line 26

[xUnit.net 00:00:00.63]     SistemaFuneraria.Tests.Unit.Core.CalculadoraEngancheTests.CalcularEnganche_ConCombinacionesCatalogoAutorizado_DebeCalcularEngancheYCapitalAlCentavo(precioLote: 120000.00, porcentajeEnganche: 10, engancheEsperado: 12000.00, capitalEsperado: 108000.00) [FAIL]
  Error:
    System.NotImplementedException : The method or operation is not implemented.

[xUnit.net 00:00:00.64]     SistemaFuneraria.Tests.Unit.Core.CalculadoraEngancheTests.CalcularEnganche_ConPorcentajeNoAutorizado_DebeLanzarArgumentOutOfRangeException(porcentajeInvalido: 15) [FAIL]
  Error:
    Expected a <System.ArgumentOutOfRangeException> to be thrown, but found <System.NotImplementedException>: The method or operation is not implemented.

Con error! - Con error:    24, Superado:     0, Omitido:     0, Total:    24, Duración: 145 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La suite de pruebas falló al 100% (24 pruebas en ROJO) como se esperaba en la metodología TDD. La firma del método `CalcularEnganche` compila con total corrección sintáctica y tipado fuerte, pero lanza intencionalmente `NotImplementedException`, dejando el terreno listo para la implementación del código de producción en la fase VERDE (GREEN).

---

### 6. Matriz de Cumplimiento de Criterios M.E.S.T. (Fase RED)

| Pilar M.E.S.T. | Criterio de Calidad | Evidencia Técnica en Fase RED | Veredicto |
|---|---|---|:---:|
| **M - Mantenible** | Pruebas legibles con nomenclatura BDD (Given-When-Then) y tipado estricto. | Pruebas organizadas por regiones temáticas, sin números mágicos y con mensajes explícitos en FluentAssertions. | ✅ APROBADO |
| **E - Eficiente** | Definición de tupla por valor `(decimal, decimal)` sin alocaciones innecesarias en heap. | Contrato ligero de tupla nombrada `(decimal Enganche, decimal CapitalFinanciado)`. | ✅ APROBADO |
| **S - Seguridad** | Cobertura de validaciones de integridad de catálogo (precios RN-01 y enganches RN-04). | Tests específicos para rechazo con `ArgumentOutOfRangeException` ante porcentajes o precios arbitrarios. | ✅ APROBADO |
| **T - Testeable** | Pruebas parametrizadas (`[Theory]`) y etiquetadas (`[Trait]`). | 24 pruebas unitarias agrupadas bajo `[Trait("Ticket", "SF-008")]` cubriendo el 100% de caminos y bordes. | ✅ APROBADO |
