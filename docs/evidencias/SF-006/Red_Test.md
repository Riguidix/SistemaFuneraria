# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-006 |
| **Caso de Prueba Relacionado** | RNF-02 / Criterio de Aceptación SF-006 |
| **Fecha y Hora de Ejecución** | 07/11/2026 14:50 |
| **Desarrollador / Tester** | Lead Fullstack / Gustavo Lucio |
| **Rama Git** | feature/SF-006-value-object-dinero |

---

### 1. Objetivo de la Prueba
Demostrar el estado de falla previo a la implementación del Value Object `Dinero`, la utilidad centralizada `RedondeoBancario` y la excepción de dominio `ReglaNegocioException`. Se verifica que el contrato del Value Object inmutable `Dinero` y la estrategia de redondeo bancario RNF-02 (`MidpointRounding.ToEven`) están definidos en `SistemaFuneraria.Core` pero su comportamiento de negocio (invariante de no negatividad, redondeo a 2 decimales, operaciones sobrecargadas `+`, `-`, `*`, `/`, comparaciones y conversiones) aún no está implementado y falla la suite de pruebas unitarias ubicada en `tests/SistemaFuneraria.Tests/Unit/Core/`.

### 2. Datos de Entrada Utilizados
* Casos de prueba BDD:
  - Importes `$10.00` y `$5.00` para resta y regla de no negatividad (`$5.00 - $10.00`).
  - Montos negativos: `-0.01m`.
  - Redondeo bancario a 2 decimales (Banker's Rounding): `10.555m` $\rightarrow$ `10.56m`, `10.545m` $\rightarrow$ `10.54m`, `10.554m` $\rightarrow$ `10.55m`.
  - Escalares y divisores: `factor = 0.20m`, `precioLote = $120,000.00`, divisor `3m` y divisor `0m`.

### 3. Comando de Terminal Ejecutado
```bash
dotnet test --filter "FullyQualifiedName~DineroTests"
```

### 4. Salida de Terminal (Falla Esperada)
```text
Serie de pruebas para /Users/riguidixrodriguez/RiderProjects/SistemaFuneraria/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.06]     SistemaFuneraria.Tests.Unit.Core.DineroTests.OperadorMultiplicacion_EscalarPorDinero_DebeSerConmutativo [FAIL]
[xUnit.net 00:00:00.07]     SistemaFuneraria.Tests.Unit.Core.DineroTests.OperadorMultiplicacion_ConFactorNegativo_DebeLanzarReglaNegocioException [FAIL]
[xUnit.net 00:00:00.07]     SistemaFuneraria.Tests.Unit.Core.DineroTests.Constructor_ConMasDeDosDecimales_DebeAplicarRedondeoBancarioToEven(montoEntrada: 10.554, montoEsperado: 10.55) [FAIL]
[xUnit.net 00:00:00.07]     SistemaFuneraria.Tests.Unit.Core.DineroTests.Constructor_ConMasDeDosDecimales_DebeAplicarRedondeoBancarioToEven(montoEntrada: 10.545, montoEsperado: 10.54) [FAIL]
[xUnit.net 00:00:00.07]     SistemaFuneraria.Tests.Unit.Core.DineroTests.Constructor_ConMasDeDosDecimales_DebeAplicarRedondeoBancarioToEven(montoEntrada: 10.555, montoEsperado: 10.56) [FAIL]
[xUnit.net 00:00:00.07]     SistemaFuneraria.Tests.Unit.Core.DineroTests.Constructor_ConMasDeDosDecimales_DebeAplicarRedondeoBancarioToEven(montoEntrada: 10.556, montoEsperado: 10.56) [FAIL]
[xUnit.net 00:00:00.07]     SistemaFuneraria.Tests.Unit.Core.DineroTests.PropiedadZero_DebeTenerMontoCero [FAIL]
[xUnit.net 00:00:00.07]     SistemaFuneraria.Tests.Unit.Core.DineroTests.OperadorResta_CuandoResultadoSeriaNegativo_DebeLanzarReglaNegocioException [FAIL]
[xUnit.net 00:00:00.07]     SistemaFuneraria.Tests.Unit.Core.DineroTests.OperadorDivision_DineroEntreDinero_DebeRetornarRazonDecimal [FAIL]

Con error! - Con error:    21, Superado:     0, Omitido:     0, Total:    21, Duración: 35 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La fase ROJO se certifica con éxito total (falla controlada):
1. La solución y proyectos compilan sin advertencias ni errores (`TreatWarningsAsErrors = true`).
2. El contrato público del Value Object `Dinero` y la excepción `ReglaNegocioException` están definidos con sus firmas correspondientes en `SistemaFuneraria.Core`.
3. Las 21 pruebas unitarias en `SistemaFuneraria.Tests.Unit.Core` que cubren los requisitos de negocio (criterio BDD de resta, invariante de no negatividad, redondeo bancario RNF-02, sobrecarga de operadores y comparaciones) fallan con `System.NotImplementedException`, dejando el escenario listo para la implementación de la fase VERDE.
