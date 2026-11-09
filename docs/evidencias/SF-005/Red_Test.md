# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-005 |
| **Caso de Prueba Relacionado** | RNF-05, RN-01, RN-04, RN-05 |
| **Fecha y Hora de Ejecución** | 08/11/2026 14:30 |
| **Desarrollador / Tester** | Gustavo Lucio |
| **Rama Git** | feature/SF-005-Parametros-negocios-core |

---

### 1. Objetivo de la Prueba
Demostrar que los métodos de validación de negocio O(1) (`EsPrecioAutorizado`, `EsEngancheAutorizado`), la colección inmutable de plazos y las aserciones de parámetros no existen o provocan errores de compilación/ejecución antes de completar la implementación formal en `ParametrosNegocio`.

### 2. Datos de Entrada Utilizados
* Precios de lote: $120,000.00, $150,000.00, $180,000.00 (inválido: $99,999.00).
* Enganches: 10%, 20%, 30% (inválido: 15%).
* Plazos y tasas anuales: 12m (16%), 18m (18%), 24m (20%), 48m (24%) (inválidos: 6m, 36m, 60m).

### 3. Comando de Terminal Ejecutado
`dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-005"`

### 4. Salida de Terminal (Falla Esperada)
```text
C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\src\SistemaFuneraria.Core\Comun\ParametrosNegocio.cs(37,20): error CS1002: Se esperaba ; [C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\src\SistemaFuneraria.Core\SistemaFuneraria.Core.csproj]
C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Core\ParametrosNegocioTests.cs(14,26): error CS0117: 'ParametrosNegocio' no contiene una definición para 'EsPrecioAutorizado'
C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Core\ParametrosNegocioTests.cs(24,26): error CS0117: 'ParametrosNegocio' no contiene una definición para 'EsEngancheAutorizado'

ERROR al compilar.
    0 Advertencia(s)
    3 Errores
```

### 5. Explicación del Resultado
La suite de pruebas falló en rojo en la fase RED debido a que la clase `ParametrosNegocio` aún no tenía implementados correctamente los métodos canónicos de consulta O(1) (`EsPrecioAutorizado`, `EsEngancheAutorizado`), existían errores de tipografía en la firma y faltaban las colecciones de validación rápida.
