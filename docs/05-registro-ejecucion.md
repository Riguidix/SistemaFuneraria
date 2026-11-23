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

| Métrica de Calidad | Valor Actual (Milestone 1) | Meta para Release v1.0.0 |
|---|---|---|
| **Total de Casos de Prueba Diseñados** | 10 base (CP-01 a CP-10) + 36 Oráculo + 4 Escenarios | 50 verificaciones |
| **Casos Ejecutados** | 42 (CP-01, 03, 04, 05, 06, 10 + DT-01..36) | 50 (100%) |
| **Casos Aprobados (Passed)** | 42 | 50 (100%) |
| **Casos Fallidos (Failed)** | 0 | 0 (0%) |
| **Casos Pendientes de Ejecución (Sprint 2 & 3)** | 8 (CP-02, CP-07, CP-08, CP-09 + ST-01..04) | 0 (0%) |
| **Porcentaje de Cobertura de Requisitos Milestone 1** | 100% verificado (Cimientos & Motor TDD) | 100% verificado |

---

## 2. Bitácora Maestra de Ejecución (Formato Sección 12)

Cada registro refleja una corrida formal. Cuando una prueba resulta `Fallida`, genera automáticamente un reporte en [`06-bitacora-incidencias.md`](file:///Users/riguidixrodriguez/RiderProjects/SistemaFuneraria/docs/06-bitacora-incidencias.md).

| ID Prueba | Fecha | Requisito | Resultado Esperado | Resultado Obtenido | Estado | Tester Responsable | Evidencia TDD (Pull Request / Run) |
|---|---|---|---|---|---|---|---|
| **CP-01** | 08/11/2026 | RF-01, RN-01 | Catálogo muestra 2, 3 y 4 gavetas ($120k, $150k, $180k) con precios fijos. | Validación $O(1)$ en memoria con catálogo inmutable aprobada. | `Aprobado` | Gustavo Lucio | PR #64 (SF-005) |
| **CP-02** | *Pendiente* | RF-03, RN-03 | Venta de contado por $150k en 1 sola exhibición sin tabla ni intereses. | *[Pendiente de ejecución Sprint 2]* | `Pendiente` | Reydescel Herrera | PR #___ (SF-024) |
| **CP-03** | 08/11/2026 | RF-04, RN-04 | Lote $120k con 20% enganche $\rightarrow$ Enganche $24k, Capital $96k. | Cálculos decimales exactos con redondeo bancario to-even aprobados. | `Aprobado` | Gustavo Lucio | PR #65 (SF-008) |
| **CP-04** | 07/11/2026 | RF-05, RF-06 | Plazo 48 meses asigna tasa anual fija del 24% (2.0% mensual). | Tasas indexadas al plazo con precisión decimal estricta aprobadas. | `Aprobado` | Juan Romero | PR #59 (SF-009) |
| **CP-05** | 08/11/2026 | RF-07, RN-07 | Capital $96k, 12 meses, 16% $\rightarrow$ Mensualidad $8,710.16 e intereses $8,521.95. | Cuota francesa ordinaria $8,710.16 calculada en 128-bit decimal aprobada. | `Aprobado` | Gustavo Lucio | PR #66 (SF-010) |
| **CP-06** | 09/11/2026 | RF-08, RNF-02 | Tabla de 12 meses; periodo 12 ajusta a $8,710.19 y saldo final cierra en $0.00. | Absorción al centavo en cuota final con saldo $0.00 aprobada. | `Aprobado` | Lead Fullstack | PR #70 (SF-011) |
| **CP-07** | 23/11/2026 | RF-10, RF-11 | Abono $10k en periodo 3 reduce capital a $63,410.99 y recalcula cuota a $7,523.51. | Abono aplicado íntegro a capital con recibo oficial y cuotas restantes recalculadas aprobadas. | `Aprobado` | Edgar Acevedo | PR #___ (SF-093) |
| **CP-08** | 23/11/2026 | RF-12, RN-09 | Liquidación tras periodo 3 descuenta 10% en intereses futuros $\rightarrow$ Pago $77,893.39. | Liquidación anticipada con 10% bonificación comercial y saldo en $0.00 aprobada. | `Aprobado` | Edgar Acevedo | PR #___ (SF-093) |
| **CP-09** | 23/11/2026 | RF-14, RN-10 | Mensualidad atrasada identificada como `Vencida` con moratorio en $0.00. | Cuota atrasada con FakeTimeProvider identificada como Vencida y cobro sin recargos ($0.00 mora) aprobado. | `Aprobado` | Edgar Acevedo | PR #___ (SF-094) |
| **CP-10** | 08/11/2026 | RN-04, RN-05 | Rechazo de enganche 15% o plazo 36m con mensaje de error descriptivo. | Excepciones `ArgumentOutOfRangeException` y `Resultado.Fallo` aprobadas. | `Aprobado` | Guillermo Sandoval / Gustavo Lucio | PR #64 (SF-005) y PR #69 (SF-013) |

---

## 3. Registro de Pruebas Automatizadas del Oráculo (36 Combinaciones)

| Suite Automatizada | Tipo de Test | Archivo xUnit | Métodos Evaluados | Total Pruebas | Estado |
|---|---|---|---|---|---|
| **Oráculo Financiero DT-01 a DT-36** | Unit / Theory | `OraculoFinancieroTests.cs` | `ValidarCombinacionOraculoFinanciero` | 36 combinaciones | `Aprobado` (PR #72 / SF-012) |
| **Casos Límite y Redondeo BVA** | Unit | `GeneradorAmortizacionTests.cs` | `Generar_EnCualquierPlazoAutorizado_UltimoPeriodoDebeCerrarSaldoEnCeroExacto` | 5 pruebas | `Aprobado` (PR #70 / SF-011) |
| **Validación de Entradas EP** | Unit | `SimuladorServiceTests.cs` | `Simular_ConParametrosNoAutorizados_DebeRetornarFallo` | 6 pruebas | `Aprobado` (PR #69 / SF-013) |
| **Transaccionalidad y Concurrencia** | Integration | `PostgresConexionTests.cs` | `CanConnect_PostgresContainer_DebeAbrirConexion` | 1 prueba | `Aprobado` (PR #63 / SF-002) |
| **QA Gate v0.2.0 (Auth, Lockout, RBAC y /health)** | Integration HTTP | `AuthYUsuariosAdminHttpTests.cs` | `Health_DebeRetornar200Ok`, `AdminUsuarios_DebePermitirAdministradorYRechazarVendedorYCajero`, `Login_AlQuintoIntentoFallido_DebeRetornar423Locked`, `OlvideContrasena_DebeAplicarJitterMinimo200Ms` | 4 pruebas | `Aprobado` (SF-030) |
| **QA Gate v0.3.0 (CRUD Clientes, BVA y RBAC)** | Integration HTTP | `ClientesCrudHttpTests.cs` | `CicloCompleto_CrudClientes_DebeCrearBuscarActualizarYExportar`, `ValidacionBva_TelefonoYCurp_DebeRetornar400BadRequest`, `SeguridadRbac_EliminarCliente_VendedorDebeRecibir403YAdminDebeEliminar` | 3 pruebas | `Aprobado` (SF-038) |
| **QA Gate v0.5.0 (Auditoría Wizard Pasos 1–3 y Regresión)** | Unit / Web | `WizardPasosAuditoriaTests.cs` | `AuditoriaFigma_Paso1a3_ComponentesYResumenCotizacion_DebeCumplirEspecificacionFigma`, `AuditoriaBDD_RegresionPasos_AnteriorYSiguiente_DebeConservarDatosDeCotizacionYFinanciamiento`, `AuditoriaFrontend_ScriptsReactivosYPersistenciaDraft_DebenEstarRegistrados`, `AuditoriaFigma_ModalidadContado_DebeProyectarPagoUnicoInmediato` | 4 pruebas | `Aprobado` (SF-056) |
| **Suite QA HTTP Simulador (CP-03..06, CP-10)** | Integration HTTP | `SimuladorControllerTests.cs` | `CP03_CotizarCredito_12Meses2Gavetas_DebeCalcularCuotasYAmortizacionExacta`, `CP04_CotizarCredito_24Meses2Gavetas_DebeCoincidirConCaptura7Figma`, `CP05_CotizarCredito_24Meses3Gavetas_DebeCalcularCuotasYAmortizacionExacta`, `CP06_Oraculo36Combinaciones_TodasDebenRetornar200OkYCapitalCuadrarExacto`, `CP10_FronterasInvalidas_DebeRetornar400BadRequest`, `ObtenerPlanes_DebeRetornar200OkConPlanesAutorizados`, `CotizarContado_DebeRetornar200OkSinAmortizacion` | 14 pruebas | `Aprobado` (SF-055) |

---

## 4. Criterio de Actualización
* Los desarrolladores entregan su evidencia visual de tests (fases **ROJO** y **VERDE**) en el cuerpo de su Pull Request.
* El QA Lead (Edgar Acevedo) y el Tech Lead consolidan periódicamente esta tabla en la rama `dev` tras cada merge, garantizando que ningún desarrollador sufra conflictos de Git.
