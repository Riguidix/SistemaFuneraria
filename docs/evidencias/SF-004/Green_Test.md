# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-004 |
| **Caso de Prueba Relacionado** | RNF-06 / RN-12 (Formato Monetario en Pesos Mexicanos es-MX) |
| **Fecha y Hora de Ejecución** | 07/11/2026 01:02 |
| **Desarrollador / Tester** | Juan Romero (Web-UI) / Lead Fullstack |
| **Rama Git** | feature/SF-004-layout-bootstrap-es-mx |

---

### 1. Objetivo de la Prueba
Demostrar que la implementación de `RequestLocalizationOptions` con la cultura `es-MX` en `src/SistemaFuneraria.Web/Program.cs` y el layout responsivo base en `_Layout.cshtml` con Bootstrap 5.3.8 satisfacen plenamente los criterios de aceptación de SF-004. Todo valor monetario se formatea homogéneamente con signo de pesos (`$`), comas de separación de miles y dos decimales (`.00`), renderizando montos de lista como `$120,000.00`, `$150,000.00` y `$180,000.00`.

### 2. Comando de Terminal Ejecutado
```bash
dotnet test --filter "FullyQualifiedName~CulturaEsMxTests"
```

### 3. Salida de Terminal (Pase Exitoso)
```text
$ dotnet test --filter "FullyQualifiedName~CulturaEsMxTests"
Determinando los proyectos que se van a restaurar...
Todos los proyectos están actualizados para la restauración.

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

[xUnit.net 00:00:00.65]   Descubriendo: SistemaFuneraria.Tests
[xUnit.net 00:00:00.72]   Descubrimiento finalizado: 7 pruebas encontradas
[xUnit.net 00:00:00.74]   Iniciando:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.89]   Finalizado:   SistemaFuneraria.Tests
  Aprobada SistemaFuneraria.Tests.Unit.CulturaEsMxTests.CulturaEsMx_SeparadorDecimal_DebeSerPuntoYSeparadorMiles_DebeSerComa [1 ms]
  Aprobada SistemaFuneraria.Tests.Unit.CulturaEsMxTests.FormatoFecha_EnCulturaEsMx_DebeTenerEstructuraDiaMesAnio [1 ms]
  Aprobada SistemaFuneraria.Tests.Unit.CulturaEsMxTests.Moneda_EnCulturaEsMx_DebeFormatearConSignoPesosComaDeMilesYDosDecimales(valor: 120000.00, formatoEsperado: "$120,000.00") [1 ms]
  Aprobada SistemaFuneraria.Tests.Unit.CulturaEsMxTests.Moneda_EnCulturaEsMx_DebeFormatearConSignoPesosComaDeMilesYDosDecimales(valor: 150000.00, formatoEsperado: "$150,000.00") [< 1 ms]
  Aprobada SistemaFuneraria.Tests.Unit.CulturaEsMxTests.Moneda_EnCulturaEsMx_DebeFormatearConSignoPesosComaDeMilesYDosDecimales(valor: 180000.00, formatoEsperado: "$180,000.00") [< 1 ms]
  Aprobada SistemaFuneraria.Tests.Unit.CulturaEsMxTests.Moneda_EnCulturaEsMx_DebeFormatearConSignoPesosComaDeMilesYDosDecimales(valor: 8710.16, formatoEsperado: "$8,710.16") [< 1 ms]
  Aprobada SistemaFuneraria.Tests.Unit.CulturaEsMxTests.Moneda_EnCulturaEsMx_DebeFormatearConSignoPesosComaDeMilesYDosDecimales(valor: 0.00, formatoEsperado: "$0.00") [< 1 ms]

Correctas! - Con error:     0, Superado:     7, Omitido:     0, Total:     7, Duración: 38 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 4. Explicación del Resultado
La suite de pruebas pasó al 100%:
1. Se validaron las propiedades numéricas de `es-MX`: símbolo de moneda `$`, punto decimal `.` y separador de miles `,`.
2. Se comprobaron los precios de lista ($120k, $150k, $180k) y las cuotas ordinarias ($8,710.16) obteniendo exactamente el formato monetario exigido por la regla de negocio RN-12.
3. El layout base `_Layout.cshtml` incorpora Bootstrap 5.3.8, navegación accesible y contenedor de alertas para la interacción de los roles `Vendedor` y `Cajero`.
