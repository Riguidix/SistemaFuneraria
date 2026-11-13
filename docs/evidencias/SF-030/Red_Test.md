# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-030 |
| **Caso de Prueba Relacionado** | ERD v1.1 Secciones 3.3, 3.4 y 3.5, CP-05, CP-06, RF-07, RF-08 |
| **Fecha y Hora de Ejecución** | 13/11/2026 12:37 |
| **Desarrollador / Tester** | Gustavo Lucio |
| **Rama Git** | feature/SF-030-entidades-venta-financiamiento |

---

### 1. Objetivo de la Prueba
Demostrar que los métodos factoría de las entidades de dominio `Venta`, `Financiamiento` y `Mensualidad`, así como las operaciones de ciclo de vida (`RegistrarPago`), no poseen implementación operativa y lanzan intencionalmente `System.NotImplementedException`, garantizando la fase ROJO (RED) del ciclo TDD estricto antes de escribir el código de producción.

### 2. Datos de Entrada Utilizados
* **Venta de Contado:** Lote "LOT-02", Vendedor "VEND-001", MontoTotal $120,000.00.
* **Venta Financiada y Crédito (Escenario Canónico CP-05 / CP-06):**
  * Precio Lote: $120,000.00
  * Enganche: $24,000.00 (20%)
  * Capital Financiado: $96,000.00
  * Plazo: 12 meses
  * Tasa Anual: 16% (0.16m)
  * Calendario: 12 cuotas mensuales calculadas mediante `GeneradorAmortizacion.Generar`.
* **Casos inválidos de prueba:**
  * Venta con MontoTotal <= 0, ClienteId vacío, LoteId o VendedorId vacíos.
  * Venta financiada con financiamiento nulo o discrepancia aritmética (`MontoTotal != Enganche + CapitalFinanciado`).
  * Financiamiento con plazo no autorizado (0, 6, 36, 60), montos no positivos o número de cuotas discrepante del plazo acordado.
  * Mensualidad con periodo <= 0, cuota <= 0 o saldos negativos.

### 3. Comando de Terminal Ejecutado
`dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-030" --no-build`

### 4. Salida de Terminal (Falla Esperada)
```text
Serie de pruebas para C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Con error! - Con error:    29, Superado:     0, Omitido:     0, Total:    29, Duración: 88 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La suite de pruebas falló al 100% (29 de 29 pruebas en ROJO) como exige el estándar TDD. Las firmas de las entidades `Venta`, `Financiamiento` y `Mensualidad` compilan adecuadamente con sus enums asociados (`TipoVenta`, `EstadoVenta`, `EstadoFinanciamiento`, `EstadoMensualidad`), pero arrojan `NotImplementedException`, dejando el contrato y las invariantes de negocio listos para su implementación en la fase VERDE.

---

### 6. Matriz de Cumplimiento de Criterios M.E.S.T. (Fase RED)

| Pilar M.E.S.T. | Criterio de Calidad | Evidencia Técnica en Fase RED | Veredicto |
|---|---|---|:---:|
| **M - Mantenible** | Entidades ricas en dominio con colecciones inmutables para el exterior y constructores protegidos para EF Core. | Encapsulamiento con `IReadOnlyCollection<Mensualidad>`, constructores protegidos y Factory Methods. | ✅ APROBADO |
| **E - Eficiente** | Modelado en memoria sin copias innecesarias y vinculación relacional bidireccional limpia. | IDs tipados `Guid`, enums nativos y colecciones respaldadas por `List<T>`. | ✅ APROBADO |
| **S - Seguridad** | Validación estricta de invariantes (monto total = enganche + capital, cuotas == plazo, montos > 0). | 25 pruebas de invariantes parametrizadas para rechazo con `ReglaNegocioException`. | ✅ APROBADO |
| **T - Testeable** | 100% de caminos y bordes cubiertos con aserciones xUnit y FluentAssertions. | 29 pruebas unitarias agrupadas bajo `[Trait("Ticket", "SF-030")]`. | ✅ APROBADO |
