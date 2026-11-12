# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-011 |
| **Caso de Prueba Relacionado** | CP-06 / RF-08 / RNF-02 / BVA-11 |
| **Fecha y Hora de Ejecución** | 09/11/2026 17:09 |
| **Desarrollador / Tester** | Lead Fullstack / Gustavo Lucio |
| **Rama Git** | feature/SF-011-generador-amortizacion-ajuste-centavos |

---

### 1. Objetivo de la Prueba
Demostrar el defecto de redondeo por acumulación de decimales e incumplimiento de la regla RNF-02 previo a la implementación del ajuste de cierre. Se comprueba que, al generar la amortización francesa de forma estándar sin absorber el desbalance residual en la cuota $n$, el saldo final del periodo 12 concluye en un valor distinto de cero ($0.03) y la suma de capital amortizado queda incompleta ($95,999.97 en lugar de los $96,000.00 financiados), provocando la falla en la suite de pruebas unitarias.

### 2. Datos de Entrada Utilizados
* Caso canónico CP-06:
  - Capital Financiado: $96,000.00 MXN (Lote Básico $120,000 con 20% enganche).
  - Plazo: 12 meses.
  - Tasa Anual: 16% (RN-05).
  - Tasa Mensual: 1.333333% ($0.16 / 12$).
* Casos límite en otros plazos autorizados:
  - 18 meses al 18% anual.
  - 24 meses al 20% anual.
  - 48 meses al 24% anual.

### 3. Comando de Terminal Ejecutado
```bash
dotnet test --filter "FullyQualifiedName~GeneradorAmortizacionTests"
```

### 4. Salida de Terminal (Falla Esperada)
```text
Serie de pruebas para /Users/riguidixrodriguez/RiderProjects/SistemaFuneraria/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.29]     SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EnCualquierPlazoAutorizado_UltimoPeriodoDebeCerrarSaldoEnCeroExacto(capital: 108000, plazoMeses: 24) [FAIL]
[xUnit.net 00:00:00.30]     SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EnCualquierPlazoAutorizado_UltimoPeriodoDebeCerrarSaldoEnCeroExacto(capital: 96000, plazoMeses: 24) [FAIL]
[xUnit.net 00:00:00.30]     SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EnCualquierPlazoAutorizado_UltimoPeriodoDebeCerrarSaldoEnCeroExacto(capital: 96000, plazoMeses: 48) [FAIL]
[xUnit.net 00:00:00.30]     SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EnCualquierPlazoAutorizado_UltimoPeriodoDebeCerrarSaldoEnCeroExacto(capital: 108000, plazoMeses: 12) [FAIL]
[xUnit.net 00:00:00.30]     SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EnCualquierPlazoAutorizado_UltimoPeriodoDebeCerrarSaldoEnCeroExacto(capital: 126000, plazoMeses: 48) [FAIL]
[xUnit.net 00:00:00.30]     SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EnCualquierPlazoAutorizado_UltimoPeriodoDebeCerrarSaldoEnCeroExacto(capital: 96000, plazoMeses: 18) [FAIL]
[xUnit.net 00:00:00.39]     SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EscenarioCanonicoCP06_SumaCapitalAmortizado_DebeSerExactamenteCapitalInicial [FAIL]
[xUnit.net 00:00:00.40]     SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EscenarioCanonicoCP06_Periodo12_DebeAbsorberAjusteCentavosYSaldoFinalCero [FAIL]

  Con error SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EscenarioCanonicoCP06_Periodo12_DebeAbsorberAjusteCentavosYSaldoFinalCero
  Mensaje de error:
   Expected renglon12.CapitalAmortizado to be 8595.58M because el capital amortizado debe cubrir el 100% del saldo inicial remanente, but found 8595.55M (difference of -0.03).

  Con error SistemaFuneraria.Tests.Unit.Core.GeneradorAmortizacionTests.Generar_EscenarioCanonicoCP06_SumaCapitalAmortizado_DebeSerExactamenteCapitalInicial
  Mensaje de error:
   Expected totalAmortizado to be 96000.00M because la suma de capital amortizado no puede diferir por centavos del capital inicial financiado, but found 95999.97M (difference of -0.03).

Con error! - Con error:     8, Superado:    11, Omitido:     0, Total:    19, Duración: 108 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La fase ROJO se certifica con precisión empírica:
1. El contrato `GeneradorAmortizacion.Generar` y el registro inmutable `RenglonAmortizacion` compilan limpiamente (`TreatWarningsAsErrors = true`).
2. Las pruebas unitarias confirman la existencia del descuadre financiero por falta de absorción al centavo:
   - El periodo 12 concluye con un saldo remanente de `$0.03` (o desviaciones negativas en otros plazos) en lugar de `$0.00`.
   - El pago mensual del periodo 12 se mantiene en `$8,710.16` en vez de absorber los centavos para cerrar en `$8,710.19`.
   - El capital total amortizado sumó `$95,999.97` faltando 3 centavos respecto al capital financiado de `$96,000.00`.
3. El escenario queda formalmente en ROJO, listo para implementar la lógica de absorción de la cuota $n$ en la fase VERDE.
