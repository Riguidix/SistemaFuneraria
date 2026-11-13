# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-030 |
| **Caso de Prueba Relacionado** | ERD v1.1 Secciones 3.3, 3.4 y 3.5, CP-05, CP-06, RF-07, RF-08 |
| **Fecha y Hora de Ejecución** | 13/11/2026 12:40 |
| **Desarrollador / Tester** | Gustavo Lucio |
| **Rama Git** | feature/SF-030-entidades-venta-financiamiento |

---

### 1. Objetivo de la Prueba
Demostrar que las entidades de dominio `Venta`, `Financiamiento` y `Mensualidad` satisfacen completamente el modelo de agregados DDD, encapsulamiento de colecciones inmutables, constructores protegidos para EF Core, métodos factoría y validación de invariantes de negocio (ERD v1.1 Secciones 3.3, 3.4 y 3.5), aprobando al 100% las 29 pruebas unitarias en fase VERDE (GREEN).

### 2. Datos de Entrada Utilizados
* **Escenario de Contado (Happy Path):**
  * ClienteId = Guid, LoteId = "LOT-02", VendedorId = "VEND-001", MontoTotal = $120,000.00.
* **Escenario Financiado Canónico (CP-05 / CP-06):**
  * Precio Lote = $120,000.00 | Enganche = $24,000.00 (20%) | Capital Financiado = $96,000.00 | Plazo = 12 meses | Tasa Anual = 16%.
  * Calendario de 12 cuotas generado mediante `GeneradorAmortizacion.Generar`.
* **Prueba de Transición de Estado:**
  * Cuota periodo 1: `RegistrarPago()` transiciona de `Pendiente` a `Pagada`. Rechazo por reintento de pago sobre cuota ya saldada.
* **Validaciones de Invariantes Rechazadas con `ReglaNegocioException`:**
  * Venta con MontoTotal <= 0, ClienteId vacío, LoteId vacío, VendedorId vacío.
  * Venta financiada con financiamiento nulo o discrepancia (`MontoTotal != Enganche + CapitalFinanciado`).
  * Financiamiento con plazo no autorizado (0, 6, 36, 60), montos no positivos o número de cuotas != plazo.
  * Mensualidad con periodo <= 0, cuota <= 0 o saldos negativos.

### 3. Comando de Terminal Ejecutado
`dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-030" --no-build`

### 4. Salida de Terminal (Pase Exitoso)
```text
Serie de pruebas para C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:    29, Omitido:     0, Total:    29, Duración: 49 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La prueba pasó exitosamente (29 de 29 pruebas en VERDE) demostrando que:
1. `Venta.CrearContado` genera ventas completas sin financiamiento asociado, asegurando que `Financiamiento` sea nulo.
2. `Venta.CrearFinanciada` vincula bidireccionalmente la venta con el financiamiento (`Financiamiento.VentaId == Venta.Id`), validando que la suma del enganche y capital financiado coincida exactamente con el precio pactado de la operación.
3. `Financiamiento.Crear` valida que el número de cuotas suministradas coincida de manera estricta con el plazo autorizado (12, 18, 24, 48 meses) y asigna `FinanciamientoId` a cada una de las mensualidades en memoria.
4. `Mensualidad.RegistrarPago` gestiona adecuadamente la transición de estado de cobro, impidiendo duplicidad de pagos con `ReglaNegocioException`.
5. Se garantiza la pureza arquitectónica de `SistemaFuneraria.Core` con cero dependencias externas y cumplimiento de invariantes de negocio.

---

### 6. Matriz de Cumplimiento de Criterios M.E.S.T. (Fase GREEN)

| Pilar M.E.S.T. | Criterio de Calidad | Evidencia Técnica en Fase GREEN | Veredicto |
|---|---|---|:---:|
| **M - Mantenible** | Entidades de dominio puras con colecciones inmutables (`IReadOnlyCollection<Mensualidad>`) y constructores protegidos para EF Core. | Encapsulamiento con `private set`, `AsReadOnly()`, `protected` constructors y Factory Methods. | ✅ APROBADO |
| **E - Eficiente** | Inicialización eficiente en memoria sin copias redundantes ni conversiones costosas. | Colecciones respaldadas por `List<Mensualidad>`, IDs directos `Guid` y sin dependencias pesadas. | ✅ APROBADO |
| **S - Seguridad** | Blindaje contra estados de financiamiento corruptos mediante guard clauses e invariantes matemáticos. | Verificación de `MontoTotal == Enganche + CapitalFinanciado`, `Cuotas.Count == PlazoMeses` y montos positivos. | ✅ APROBADO |
| **T - Testeable** | Cobertura integral de caminos válidos, ciclo de vida de cuotas e invariantes de negocio. | 29 pruebas unitarias pasando al 100% en verde con etiquetas de trazabilidad `[Trait("Ticket", "SF-030")]`. | ✅ APROBADO |
