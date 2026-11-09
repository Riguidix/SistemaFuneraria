# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-009 |
| **Caso de Prueba Relacionado** | CP-04 (RF-05, RF-06, RN-05, RN-06), CP-10 (RN-05) |
| **Fecha y Hora de Ejecución** | 07/11/2026 01:38 |
| **Desarrollador / Tester** | Juan Romero (Dev) / Lead Fullstack |
| **Rama Git** | feature/SF-009-tasa-mensual-tdd |

---

### 1. Objetivo de la Prueba
Demostrar la falla previa en fase ROJO antes de escribir la implementación de producción. Se valida que al intentar compilar y ejecutar la suite unitaria `CalculadoraTasasTests`, el motor financiero en `SistemaFuneraria.Core` carece de la clase `CalculadoraFinanciamiento` y del contrato `ObtenerTasas(int plazoMeses)`.

### 2. Datos de Entrada Utilizados
* Plazos autorizados a verificar: 12, 18, 24 y 48 meses.
* Caso CP-04: Plazo de 48 meses esperando Tasa Anual 24% (`0.24m`) y Tasa Mensual 2.0% (`0.0200m`).
* Casos no autorizados (CP-10): 36, 6, 0, -12, 60 y 15 meses esperando `ArgumentOutOfRangeException`.

### 3. Comando de Terminal Ejecutado
```bash
dotnet test --filter "FullyQualifiedName~CalculadoraTasasTests"
```

### 4. Salida de Terminal (Falla Esperada)
```text
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Core\CalculadoraTasasTests.cs(26,38): error CS0103: El nombre 'CalculadoraFinanciamiento' no existe en el contexto actual. [C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Core\CalculadoraTasasTests.cs(44,38): error CS0103: El nombre 'CalculadoraFinanciamiento' no existe en el contexto actual. [C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Core\CalculadoraTasasTests.cs(61,42): error CS0103: El nombre 'CalculadoraFinanciamiento' no existe en el contexto actual. [C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Core\CalculadoraTasasTests.cs(79,32): error CS0103: El nombre 'CalculadoraFinanciamiento' no existe en el contexto actual. [C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]

Error al compilar.
    0 Advertencia(s)
    4 Errores

Tiempo transcurrido 00:00:01.42
```

### 5. Explicación del Resultado
La fase ROJO se certifica como exitosa:
1. La prueba unitaria no compila debido a la ausencia deliberada de la clase `CalculadoraFinanciamiento` en `SistemaFuneraria.Core.Calculos`.
2. Se confirma que no existe lógica preexistente para la asignación de tasas anuales indexadas ni tasas mensuales, garantizando un flujo TDD estricto sin falso positivo.
