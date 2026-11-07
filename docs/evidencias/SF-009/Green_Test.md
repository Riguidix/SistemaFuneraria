# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-009 |
| **Caso de Prueba Relacionado** | CP-04 (RF-05, RF-06, RN-05, RN-06), CP-10 (RN-05) |
| **Fecha y Hora de Ejecución** | 07/11/2026 01:45 |
| **Desarrollador / Tester** | Juan Romero (Dev) / Gustavo Lucio (Reviewer) |
| **Rama Git** | feature/SF-009-tasa-mensual-tdd |

---

### 1. Objetivo de la Prueba
Demostrar que la implementación mínima y rigurosa en `CalculadoraFinanciamiento` satisface el 100% de los criterios de aceptación de SF-009: asignación inmutable de tasas según el plazo, cálculo de tasa mensual con precisión decimal de 128 bits y rechazo inmediato de plazos no autorizados.

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

Superado! - Con error: 0, Superado: 13, Omitido: 0, Total: 13, Duración: 24 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 4. Explicación del Resultado
La fase VERDE se certifica formalmente con éxito:
1. **Caso CP-04 Aprobado:** El plazo de 48 meses retorna exactamente `TasaAnual = 0.24m` (24%) y `TasaMensual = 0.0200m` (2.0%).
2. **Cumplimiento RN-06:** Se verificaron los 4 plazos autorizados (12m → 16%, 18m → 18%, 24m → 20%, 48m → 24%), confirmando que la tasa mensual se obtiene mediante división decimal exacta `tasaAnual / 12m`.
3. **Cumplimiento RN-05 & CP-10:** Los plazos inválidos (36, 6, 0, -12, 60, 15 meses) lanzan invariablemente `ArgumentOutOfRangeException` con el parámetro `plazoMeses`.
4. **Pureza Arquitectónica:** Compilación limpia con cero advertencias y cero errores bajo `TreatWarningsAsErrors`.
