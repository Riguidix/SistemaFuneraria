# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-009 |
| **Caso de Prueba Relacionado** | CP-04 (RF-05, RF-06, RN-05, RN-06), CP-10 (RN-05), RNF-02, RNF-05 |
| **Fecha y Hora de Ejecución** | 07/11/2026 16:15 |
| **Desarrollador / Tester** | Juan Romero (Dev) / Gustavo Lucio (Reviewer) |
| **Rama Git** | feature/SF-009-tasa-mensual-tdd |

---

### 1. Objetivo de la Prueba
Demostrar que la implementación en `CalculadoraFinanciamiento` conectada al catálogo inmutable `ParametrosNegocio` satisface al 100% las observaciones del Code Review:
1. **Cero Falsos Positivos:** Las aserciones comparan directamente contra los valores literales oraculares (ej. 2 + 2 = 4, sin recalculación interna en los tests).
2. **Cero `double`:** Toda la suite opera exclusivamente con precisión de punto fijo `decimal` de 128 bits, sin conversiones de punto flotante ni redondeos mágicos.
3. **Catálogo Inmutable Centralizado (RNF-05):** `ParametrosNegocio` centraliza las tasas y plazos oficiales de manera inmutable.

### 2. Comando de Terminal Ejecutado
```bash
dotnet test --filter "FullyQualifiedName~CalculadoraTasasTests"
```

### 3. Salida de Terminal (Pase Exitoso)
```text
Determinando los proyectos que se van a restaurar...
Todos los proyectos están actualizados para la restauración.
SistemaFuneraria.Core -> C:\Users\PC\SistemaFuneraria\src\SistemaFuneraria.Core\bin\Debug\net8.0\SistemaFuneraria.Core.dll
SistemaFuneraria.Tests -> C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll
Serie de pruebas para C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Superado! - Con error: 0, Superado: 17, Omitido: 0, Total: 17, Duración: 21 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 4. Explicación del Resultado
La fase VERDE se certifica con éxito total:
1. **Caso CP-04 Aprobado:** El plazo de 48 meses retorna exactamente `TasaAnual = 0.24m` (24%) y `TasaMensual = 0.0200m` (2.0%) contra el valor esperado oracular.
2. **Cumplimiento RN-06 & RNF-02:** Los 4 plazos autorizados (12m $\rightarrow$ 16%, 18m $\rightarrow$ 18%, 24m $\rightarrow$ 20%, 48m $\rightarrow$ 24%) se comparan contra valores literales `decimal` de punto fijo de 28 decimales (`0.0133333333333333333333333333m`), eliminando cualquier uso de `double` y evitando falsos positivos de recalculación.
3. **Cumplimiento RN-05 & CP-10:** Los plazos inválidos (36, 6, 0, -12, 60, 15 meses) lanzan invariablemente `ArgumentOutOfRangeException` con el parámetro `plazoMeses`.
4. **Cumplimiento RNF-05:** Se integró el catálogo inmutable `ParametrosNegocio` en `SistemaFuneraria.Core.Comun` para garantizar consistencia con los demás tickets del backlog.
