# REGISTRO DE EJECUCIÓN DE PRUEBAS DE SOFTWARE
## Sistema de Venta y Financiamiento de Lotes Funerarios

| Metadato | Detalle |
|---|---|
| **Documento** | Registro Formal de Ejecución de Pruebas (Test Execution Log) |
| **Proyecto** | Sistema de Venta y Financiamiento de Lotes Funerarios |
| **Materia** | Estándares y Métricas para el Desarrollo de Software (Sección 12) |
| **Versión** | 1.0 (Línea Base) |
| **Responsable de Consolidación** | Edgar Acevedo (QA Lead) & Lead Fullstack |
| **Política de Edición** | **Escritor Único (Single-Writer):** Mantenido exclusivamente por QA/Lead para evitar conflictos en Git. |

---

## 1. Panel de Métricas de Ejecución de Pruebas

Este panel consolida el avance real de las corridas de pruebas sobre la rama de integración `dev` y staging `beta`:

| Métrica de Calidad | Valor Actual | Meta para Release v1.0.0 |
|---|---|---|
| **Total de Casos de Prueba Diseñados** | 10 base (CP-01 a CP-10) + 36 Oráculo + 4 Escenarios | 50 verificaciones |
| **Casos Ejecutados** | 0 | 50 (100%) |
| **Casos Aprobados (Passed)** | 0 | 50 (100%) |
| **Casos Fallidos (Failed)** | 0 | 0 (0%) |
| **Casos Pendientes de Ejecución** | 50 | 0 (0%) |
| **Porcentaje de Cobertura de Requisitos** | 100% trazado | 100% verificado |

---

## 2. Bitácora Maestra de Ejecución (Formato Sección 12)

Cada registro refleja una corrida formal. Cuando una prueba resulta `Fallida`, genera automáticamente un reporte en [`06-bitacora-incidencias.md`](file:///Users/riguidixrodriguez/RiderProjects/SistemaFuneraria/docs/06-bitacora-incidencias.md).

| ID Prueba | Fecha | Requisito | Resultado Esperado | Resultado Obtenido | Estado | Tester Responsable | Evidencia TDD (Pull Request / Run) |
|---|---|---|---|---|---|---|---|
| **CP-01** | *dd/mm/aaaa* | RF-01, RN-01 | Catálogo muestra 2, 3 y 4 gavetas ($120k, $150k, $180k) con precios fijos. | *[Pendiente de ejecución]* | `Pendiente` | Juan Romero | PR #___ (Evidencia RED y GREEN) |
| **CP-02** | *dd/mm/aaaa* | RF-03, RN-03 | Venta de contado por $150k en 1 sola exhibición sin tabla ni intereses. | *[Pendiente de ejecución]* | `Pendiente` | Reydescel Herrera | PR #___ (Evidencia RED y GREEN) |
| **CP-03** | *dd/mm/aaaa* | RF-04, RN-04 | Lote $120k con 20% enganche $\rightarrow$ Enganche $24k, Capital $96k. | *[Pendiente de ejecución]* | `Pendiente` | Gustavo Lucio | PR #___ (Evidencia RED y GREEN) |
| **CP-04** | *dd/mm/aaaa* | RF-05, RF-06 | Plazo 48 meses asigna tasa anual fija del 24% (2.0% mensual). | *[Pendiente de ejecución]* | `Pendiente` | Guillermo Sandoval | PR #___ (Evidencia RED y GREEN) |
| **CP-05** | *dd/mm/aaaa* | RF-07, RN-07 | Capital $96k, 12 meses, 16% $\rightarrow$ Mensualidad $8,710.16 e intereses $8,521.95. | *[Pendiente de ejecución]* | `Pendiente` | Gustavo Lucio | PR #___ (Evidencia RED y GREEN) |
| **CP-06** | *dd/mm/aaaa* | RF-08, RNF-02 | Tabla de 12 meses; periodo 12 ajusta a $8,710.19 y saldo final cierra en $0.00. | *[Pendiente de ejecución]* | `Pendiente` | Gustavo Lucio | PR #___ (Evidencia RED y GREEN) |
| **CP-07** | *dd/mm/aaaa* | RF-10, RF-11 | Abono $10k en periodo 3 reduce capital a $63,410.99 y recalcula cuota a $7,523.51. | *[Pendiente de ejecución]* | `Pendiente` | Reydescel Herrera | PR #___ (Evidencia RED y GREEN) |
| **CP-08** | *dd/mm/aaaa* | RF-12, RN-09 | Liquidación tras periodo 3 descuenta 10% en intereses futuros $\rightarrow$ Pago $77,893.39. | *[Pendiente de ejecución]* | `Pendiente` | Guillermo Sandoval | PR #___ (Evidencia RED y GREEN) |
| **CP-09** | *dd/mm/aaaa* | RF-14, RN-10 | Mensualidad atrasada identificada como `Vencida` con moratorio en $0.00. | *[Pendiente de ejecución]* | `Pendiente` | Edgar Acevedo | PR #___ (Evidencia RED y GREEN) |
| **CP-10** | *dd/mm/aaaa* | RN-04, RN-05 | Rechazo de enganche 15% o plazo 36m con mensaje de error descriptivo. | *[Pendiente de ejecución]* | `Pendiente` | Guillermo Sandoval | PR #___ (Evidencia RED y GREEN) |

---

## 3. Registro de Pruebas Automatizadas del Oráculo (36 Combinaciones)

| Suite Automatizada | Tipo de Test | Archivo xUnit | Métodos Evaluados | Total Pruebas | Estado |
|---|---|---|---|---|---|
| **Oráculo Financiero DT-01 a DT-36** | Unit / Theory | `CalculadoraFinanciamientoTests.cs` | `CalcularAmortizacion_TodasLasCombinaciones_DebenCoincidirConOraculo` | 36 combinaciones | `Pendiente` |
| **Casos Límite y Redondeo BVA** | Unit | `TablaAmortizacionTests.cs` | `GenerarTabla_UltimoPeriodo_DebeCerrarSaldoEnCeroExacto` | 5 pruebas | `Pendiente` |
| **Validación de Entradas EP** | Unit | `ValidadorFinanciamientoTests.cs` | `ValidarParametros_ConValoresFueraDeRegla_DebeRetornarError` | 6 pruebas | `Pendiente` |
| **Transaccionalidad y Concurrencia** | Integration | `RegistroPagoConcurrencyTests.cs` | `RegistrarPago_DosPeticionesSimultaneas_SoloUnaDebeProsperar` | 3 pruebas | `Pendiente` |

---

## 4. Criterio de Actualización
* Los desarrolladores entregan su evidencia visual de tests (fases **ROJO** y **VERDE**) en el cuerpo de su Pull Request.
* El QA Lead (Edgar Acevedo) y el Tech Lead consolidan periódicamente esta tabla en la rama `dev` tras cada merge, garantizando que ningún desarrollador sufra conflictos de Git.
