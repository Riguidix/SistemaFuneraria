# ESPECIFICACIÓN DE REQUISITOS DE SOFTWARE (PRD)
## Sistema de Venta y Financiamiento de Lotes Funerarios

| Metadato | Detalle |
|---|---|
| **Proyecto** | Sistema de Venta y Financiamiento de Lotes Funerarios |
| **Materia / Contexto** | Estándares y Métricas para el Desarrollo de Software |
| **Versión del Documento** | 1.0 (Línea Base Congelada) |
| **Fecha de Aprobación** | 06 de Noviembre de 2026 |
| **Fecha Límite de Entrega** | 25 de Noviembre de 2026 |
| **Líder Técnico & Arquitecto** | Lead Fullstack |
| **Metodología** | TDD Estricto · Clean Architecture · SemVer (v0.0.1 → v1.0.0) |

---

### 1. Propósito y Alcance del Producto

#### 1.1 Objetivo del Sistema
Desarrollar una aplicación web integral que permita:
1. Gestionar la venta de lotes funerarios (2, 3 y 4 gavetas) bajo modalidades de contado y crédito financiado.
2. Calcular amortizaciones financieras fijas sobre saldos insolutos con precisión a dos decimales y ajuste en el último periodo.
3. Registrar pagos ordinarios, abonos extraordinarios a capital con recálculo de cuota periódica y liquidaciones anticipadas con descuento comercial de intereses futuros.
4. Identificar visualmente mensualidades vencidas sin generar recargos moratorios.
5. Soportar un proceso formal y auditable de verificación y validación (V&V) con trazabilidad bidireccional desde los requisitos hasta las evidencias de ejecución de pruebas.

#### 1.2 Límites Estrictos del Alcance (Scope Boundaries)
* **Dentro del Alcance (v1.0.0):**
  * Catálogo cerrado de 3 lotes funerarios fijos.
  * Flujo de venta de contado (pago del 100% inmediato sin generar tabla ni intereses).
  * Flujo de financiamiento con 3 opciones de enganche (10%, 20%, 30%) y 4 opciones de plazo (12, 18, 24, 48 meses).
  * Generador de tabla de amortización bajo sistema francés (intereses sobre saldo insoluto).
  * Registro de cobranza: mensualidad ordinaria, abono a capital y liquidación anticipada.
  * Consulta de estado de cuenta e identificación de atrasos.
* **Fuera del Alcance (Exclusiones v1.0.0):**
  * **Intereses moratorios** (especificado textualmente en Sección 8 del documento base).
  * CRUD o modificación de precios por parte del vendedor.
  * Pasarelas de cobro externas (Stripe, PayPal, terminal bancaria directa).
  * Facturación electrónica / Timbrado fiscal (CFDI).
  * Asignación física en mapa interactivo de cementerio.

---

### 2. Parámetros de Negocio Inmutables

#### 2.1 Catálogo de Lotes
Los precios son fijos y expresados en Pesos Mexicanos (MXN).

| ID Producto | Tipo de Lote | Capacidad | Precio de Contado (MXN) |
|---|---|---|---|
| `LOT-02` | Lote Básico | 2 gavetas | **$120,000.00** |
| `LOT-03` | Lote Familiar | 3 gavetas | **$150,000.00** |
| `LOT-04` | Lote Ampliado | 4 gavetas | **$180,000.00** |

#### 2.2 Opciones de Enganche
El cliente únicamente puede elegir una de las 3 opciones autorizadas:

| Nivel de Enganche | Porcentaje | Ejemplo Lote $120k | Ejemplo Lote $150k | Ejemplo Lote $180k |
|---|---|---|---|---|
| **Accesible** | 10% (0.10) | $12,000.00 | $15,000.00 | $18,000.00 |
| **Estándar** | 20% (0.20) | $24,000.00 | $30,000.00 | $36,000.00 |
| **Alto** | 30% (0.30) | $36,000.00 | $45,000.00 | $54,000.00 |

#### 2.3 Plazos y Tasas de Interés
La tasa anual es fija durante toda la vigencia del contrato:

| Plazo ($n$) | Tasa Anual Fija | Tasa Mensual Aplicable ($i = \text{Tasa Anual} / 12$) |
|---|---|---|
| **12 meses** | 16% (0.16) | 1.333333% (0.01333333) |
| **18 meses** | 18% (0.18) | 1.500000% (0.01500000) |
| **24 meses** | 20% (0.20) | 1.666667% (0.01666667) |
| **48 meses** | 24% (0.24) | 2.000000% (0.02000000) |

---

### 3. Fórmulas Matemáticas y Reglas de Cálculo Financiero

#### 3.1 Desglose Inicial
* $\text{Enganche} = \text{Precio del Lote} \times \text{Porcentaje Enganche}$
* $\text{Capital Financiado } (P) = \text{Precio del Lote} - \text{Enganche}$
* $\text{Tasa Mensual } (i) = \frac{\text{Tasa Anual}}{12}$

#### 3.2 Cuota Mensual Fija (Fórmula de Amortización Francesa)
$$\text{Mensualidad} = P \times \left[ \frac{i(1 + i)^n}{(1 + i)^n - 1} \right]$$

#### 3.3 Distribución Periódica de la Cuota
Para cada periodo $k \in \{1, 2, \dots, n\}$:
* $\text{Interés del Periodo } (I_k) = \text{Saldo Inicial}_k \times i$
* $\text{Capital Amortizado } (A_k) = \text{Mensualidad} - I_k$
* $\text{Saldo Final}_k = \text{Saldo Inicial}_k - A_k$

#### 3.4 Regla de Redondeo y Ajuste de Cierre (RNF-02)
* Toda operación monetaria interna debe mantener precisión decimal (`decimal` en C# / `NUMERIC(14,2)` en BD).
* La última mensualidad ($k = n$) **debe absorber la diferencia residual** por redondeo para forzar:
$$\text{Saldo Final}_n = \$0.00$$
$$\text{Capital Amortizado}_n = \text{Saldo Inicial}_n$$
$$\text{Mensualidad}_n = \text{Saldo Inicial}_n + I_n$$

#### 3.5 Recálculo por Abono Extraordinario a Capital (RN-08)
Cuando el cliente realiza un abono extraordinario $E$:
1. $E$ se descuenta íntegramente del saldo de capital insoluto al inicio del periodo siguiente:
   $$\text{Nuevo Saldo Insoluto } (P') = \text{Saldo Final}_{k} - E$$
2. Se conserva la fecha final original. El número de periodos restantes es $n' = n - k$.
3. Se recalcula la nueva mensualidad fija reducida para los $n'$ periodos:
   $$\text{Nueva Mensualidad} = P' \times \left[ \frac{i(1 + i)^{n'}}{(1 + i)^{n'} - 1} \right]$$

#### 3.6 Liquidación Anticipada con Descuento (RN-09, RF-12)
Si el cliente liquida tras el periodo $k$:
1. $\text{Capital Pendiente} = \text{Saldo Final}_k$
2. $\text{Intereses Futuros Programados} = \sum_{j=k+1}^n I_j$
3. Descuento comercial aplicado: $10\%$ sobre los intereses futuros.
4. **Importe de Liquidación:**
   $$\text{Monto Liquidación} = \text{Capital Pendiente} + (0.90 \times \text{Intereses Futuros Programados})$$

---

### 4. Requisitos Funcionales (RF)

| ID | Requisito | Entradas | Salidas / Efecto | Criterio de Aceptación Verificable |
|---|---|---|---|---|
| **RF-01** | Catálogo de lotes | Solicitud de consulta | Lista de 3 lotes: ID, gavetas, precio unitario. | Muestra exactamente 2 ($120k), 3 ($150k) y 4 ($180k) gavetas. |
| **RF-02** | Registro de clientes | Nombre, Apellidos, Teléfono (10 dígitos), Correo, Dirección | Registro persistido, ID de cliente generado | Rechaza campos vacíos y formatos de correo/teléfono inválidos. |
| **RF-03** | Selección de modalidad | Elección: Contado vs. Financiamiento | Determina la bifurcación del flujo de venta | Contado omite simulación; Financiamiento exige enganche y plazo. |
| **RF-04** | Cálculo de enganche | Precio de lote y porcentaje (10%, 20%, 30%) | Monto en $ del enganche y Capital financiado | Cálculo exacto según porcentaje; rechaza cualquier otro valor. |
| **RF-05** | Validación de plazos | Selección de plazo (12, 18, 24, 48) | Aceptación del plazo o error de validación | Solo se permiten 12, 18, 24 y 48 meses; otros valores devuelven error. |
| **RF-06** | Asignación de tasa anual | Plazo seleccionado | Tasa anual y tasa mensual calculada | 12m $\rightarrow$ 16%; 18m $\rightarrow$ 18%; 24m $\rightarrow$ 20%; 48m $\rightarrow$ 24%. |
| **RF-07** | Cálculo financiero integral | Lote, enganche y plazo | Mensualidad, total intereses, costo total | Los valores coinciden exactamente con la fórmula de amortización. |
| **RF-08** | Generador de amortización | ID financiamiento | Tabla con periodos 1 a $n$, desglose y saldo $0.00 | Presenta todas las columnas y el saldo final cierra en cero con ajuste. |
| **RF-09** | Registro de pago ordinario | ID contrato, número de mensualidad, monto | Recibo generado, cuota marcada pagada, saldo actualizado | Reduce saldo insoluto; no permite pagos duplicados del mismo periodo. |
| **RF-10** | Abono extraordinario capital | ID contrato, monto de abono ($E$) | Abono aplicado directo a capital insoluto | Valida que $E > 0$ y $E < \text{Saldo Insoluto}$. |
| **RF-11** | Recálculo de mensualidad | Abono registrado en periodo $k$ | Nueva tabla de amortización versionada | Conserva plazo final; reduce la mensualidad de los periodos $k+1$ a $n$. |
| **RF-12** | Cotización y pago de liquidación | ID contrato | Desglose: Capital + 90% intereses futuros = Monto Total | Aplica descuento exacto del 10% a intereses futuros; liquida el contrato. |
| **RF-13** | Consulta de estado de cuenta | ID cliente o contrato | Historial de pagos, saldo insoluto actual, cuotas pendientes | Información 100% consistente con la tabla y los pagos aplicados. |
| **RF-14** | Identificación de atraso | Fecha actual vs. fecha de vencimiento | Indicador visual de mora / días de retraso | Marca cuota como vencida sin calcular intereses moratorios ($0.00). |

---

### 5. Requisitos No Funcionales (RNF)

| ID | Dimensión | Requisito Técnico Medible |
|---|---|---|
| **RNF-01** | **Usabilidad** | El flujo de venta debe completarse en máximo 4 pasos guiados. Formularios con validación en tiempo real y mensajes claros de error. |
| **RNF-02** | **Integridad Financiera** | Todo cálculo financiero debe utilizar precisión decimal de punto fijo (`decimal` de 128 bits en .NET, `numeric(14,2)` en PostgreSQL). Prohibido el uso de flotantes (`float` o `double`). Redondeo final a 2 decimales vía `MidpointRounding.ToEven` (Banker's Rounding) en la presentación. |
| **RNF-03** | **Seguridad** | Autenticación y control de acceso basado en roles (RBAC): `Vendedor` (registro de clientes, cotizaciones y ventas) y `Cajero` (registro de pagos ordinarios, abonos y liquidaciones). Protección contra CSRF en todos los formularios. |
| **RNF-04** | **Confiabilidad Transaccional** | Toda operación de venta o cobranza debe ejecutarse bajo una transacción de base de datos con aislamiento ACID. Un fallo intermedio revierte todo (`ROLLBACK`). |
| **RNF-05** | **Mantenibilidad** | Arquitectura Limpia (Clean Architecture) en capas estrictas: `Core` (cero dependencias externas), `Application`, `Infrastructure` y `Web`. Catálogo de parámetros centralizado. |
| **RNF-06** | **Compatibilidad** | La aplicación web debe renderizarse de forma responsiva en Chrome, Firefox, Safari y Edge (versiones modernas) utilizando Bootstrap 5.3.8. |

---

### 6. Reglas de Negocio Codificadas (RN)

* **RN-01:** Solo existen 3 productos en catálogo (2, 3 y 4 gavetas) con precios fijos inalterables ($120k, $150k, $180k).
* **RN-02:** Una venta no puede persistirse sin estar vinculada a un cliente válido existente y a un lote disponible.
* **RN-03:** La venta de contado se liquida en una sola exhibición por el 100% del precio de catálogo y no genera tabla de amortización ni intereses.
* **RN-04:** En financiamiento, el enganche únicamente puede ser del 10%, 20% o 30%. Cualquier otro valor es rechazado.
* **RN-05:** Los plazos autorizados son exclusivamente 12, 18, 24 y 48 meses.
* **RN-06:** La tasa anual está indexada al plazo (12m: 16%, 18m: 18%, 24m: 20%, 48m: 24%) y permanece inmutable durante todo el contrato.
* **RN-07:** Los intereses ordinarios se calculan exclusivamente sobre el saldo insoluto al inicio de cada mes.
* **RN-08:** Todo abono extraordinario a capital reduce el saldo insoluto y recalcula la cuota mensual para los periodos restantes, conservando la fecha de vencimiento original.
* **RN-09:** La liquidación anticipada otorga un descuento obligatorio del 10% sobre la suma de los intereses futuros programados.
* **RN-10:** Las mensualidades cuya fecha de vencimiento sea anterior a la fecha de consulta se marcarán en estado `Vencida`, sin aplicar penalización ni interés moratorio.
* **RN-11:** Ningún pago puede registrarse por importes menores o iguales a $0.00 ni por cantidades que superen el saldo insoluto total.
* **RN-12:** Todos los valores monetarios de salida deben formatearse bajo el estándar de moneda mexicana (`es-MX`, `$#,##0.00`).

---

### 7. Matriz de Criterios de Aceptación vs. Rechazo de Sistema

| Prueba de Sistema | Criterio de Aceptación (Pasa) | Criterio de Rechazo (Falla / Bloqueador) |
|---|---|---|
| **Catálogo** | 3 lotes visibles con precios exactos de lista. | Falta algún lote o los precios son editables. |
| **Validación de Entradas** | Enganches fuera de {10, 20, 30}% o plazos fuera de {12, 18, 24, 48} son bloqueados con mensaje claro. | Acepta valores no autorizados (e.g. 15% o 36 meses). |
| **Tabla de Amortización** | Concluye exactamente con saldo final en $0.00 tras el ajuste del último periodo. | Termina con centavos a favor o en contra ($0.03 o -$0.05). |
| **Abono a Capital** | Reduce saldo insoluto y baja la mensualidad restante manteniendo el mes final de término. | Reduce el número de meses o no actualiza las cuotas futuras. |
| **Liquidación Anticipada** | Cobro = Saldo Capital + 90% de intereses futuros. | Cobra el 100% de los intereses futuros o calcula descuento erróneo. |
| **Vencimientos** | Identifica mora visualmente, intereses moratorios = $0.00. | Aplica cargos por mora o recargos no autorizados. |
| **Integridad de Pagos** | Registro atómico; pagos duplicados o importes negativos son rechazados. | Permite pagar dos veces la misma cuota o saldos negativos. |
