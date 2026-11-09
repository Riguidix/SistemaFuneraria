# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-006 |
| **Caso de Prueba Relacionado** | RNF-02 / Criterio de Aceptación SF-006 |
| **Fecha y Hora de Ejecución** | 07/11/2026 15:31 |
| **Desarrollador / Tester** | Lead Fullstack / Gustavo Lucio |
| **Rama Git** | feature/SF-006-value-object-dinero |

---

### 1. Objetivo de la Prueba
Demostrar que la implementación completa de `Dinero`, `RedondeoBancario` y `ReglaNegocioException` satisface las reglas de negocio, la invariante de no negatividad, la sobrecarga completa de operadores monetarios y la precisión financiera estipulada en RNF-02 (`MidpointRounding.ToEven`), aprobando el 100% de la suite de pruebas unitarias (`Unit/Core`) en verde.

### 2. Comando de Terminal Ejecutado
```bash
dotnet test --filter "FullyQualifiedName~Unit.Core"
```

### 3. Salida de Terminal (Pase Exitoso)
```text
Serie de pruebas para /Users/riguidixrodriguez/RiderProjects/SistemaFuneraria/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:    39, Omitido:     0, Total:    39, Duración: 12 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 4. Explicación del Resultado
La suite de pruebas unitarias pasó al 100% (39 de 39 pruebas en verde bajo el namespace `SistemaFuneraria.Tests.Unit.Core`):
1. **`DineroTests` (21 pruebas):**
   - El Value Object `Dinero` rechaza montos negativos lanzando `ReglaNegocioException`.
   - Cumple el criterio BDD de SF-006: `$10.00 - $5.00 = $5.00` y arroja `ReglaNegocioException` cuando el resultado es negativo (`$5.00 - $10.00`).
   - Aplica de manera consistente el redondeo bancario a 2 decimales vía `RedondeoBancario.Redondear` en constructor y operaciones aritméticas (`+`, `-`, `*`, `/`).
   - Soporta comparaciones relacionales (`<`, `<=`, `>`, `>=`, `==`, `!=`), ordenamiento natural `IComparable<Dinero>` y conversiones explícitas/implícitas con `decimal`.
2. **`RedondeoBancarioTests` (18 pruebas):**
   - Valida la regla RNF-02 con casos de prueba de valor límite (BVA): `10.555` $\rightarrow$ `10.56` (hacia par), `10.545` $\rightarrow$ `10.54` (hacia par), `0.005` $\rightarrow$ `0.00`, `0.015` $\rightarrow$ `0.02`.
   - Valida simetría para valores negativos: `-10.555` $\rightarrow$ `-10.56`, `-10.545` $\rightarrow$ `-10.54`.
   - Valida sobrecarga con precisión configurable (`RedondearConNDecimales`) para 0 y 4 decimales.
