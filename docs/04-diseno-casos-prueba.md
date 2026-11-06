# DISEÑO DE CASOS DE PRUEBA DE SOFTWARE
## Sistema de Venta y Financiamiento de Lotes Funerarios

| Metadato | Detalle |
|---|---|
| **Documento** | Diseño Detallado de Casos de Prueba (Test Case Specification) |
| **Proyecto** | Sistema de Venta y Financiamiento de Lotes Funerarios |
| **Materia** | Estándares y Métricas para el Desarrollo de Software (Sección 10) |
| **Versión** | 1.0 (Línea Base Congelada) |
| **Fecha de Diseño** | 06 de Noviembre de 2026 |
| **Alcance** | Casos CP-01 a CP-10 desarrollados a detalle con criterios de aceptación |

---

### CP-01 · Consulta del Catálogo de Lotes Funerarios

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-01** |
| **Requisito(s)** | RF-01, RN-01 |
| **Técnica Aplicada** | Tabla de Decisiones (DT) / Inspección Visual |
| **Objetivo** | Comprobar que el catálogo público despliegue exactamente los 3 productos autorizados con sus capacidades de gavetas y precios fijos oficiales. |
| **Precondición** | La aplicación web se encuentra en ejecución y la base de datos contiene los 3 lotes precargados vía seeder. |
| **Datos de Entrada** | Ninguno (Acceso directo a la página de Catálogo). |
| **Pasos de Ejecución** | 1. Abrir el navegador e ingresar a la URL `/Catalogo`.<br>2. Localizar la sección de productos disponibles.<br>3. Verificar la presencia de cada tarjeta de producto, número de gavetas y precio en pesos mexicanos (`es-MX`). |
| **Resultado Esperado** | El catálogo muestra exactamente:<br>1. *Lote Básico:* 2 gavetas — **$120,000.00**<br>2. *Lote Familiar:* 3 gavetas — **$150,000.00**<br>3. *Lote Ampliado:* 4 gavetas — **$180,000.00**<br>Los precios no son editables por el usuario. |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura de pantalla de la interfaz web en `/Catalogo` mostrando los tres productos con sus gavetas y precios formateados con signo de pesos y comas de miles. |

---

### CP-02 · Venta de Contado de Lote Familiar de 3 Gavetas

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-02** |
| **Requisito(s)** | RF-02, RF-03, RN-02, RN-03 |
| **Técnica Aplicada** | Pruebas Basadas en Escenarios (ST-01) |
| **Objetivo** | Verificar que al seleccionar la modalidad de contado se cobre el precio total del lote en una sola exhibición sin generar financiamiento, intereses ni tabla de amortización. |
| **Precondición** | Usuario con rol `Vendedor` autenticado; lote familiar de 3 gavetas disponible en inventario. |
| **Datos de Entrada** | Cliente: *"María Gómez"* (RFC: `GOMM800101`, Tel: `5512345678`).<br>Producto: Lote Familiar (3 gavetas, $150,000.00).<br>Modalidad de Compra: `Contado`. |
| **Pasos de Ejecución** | 1. Iniciar el wizard de venta en `/Ventas/Nueva`.<br>2. Registrar o seleccionar los datos de la cliente María Gómez.<br>3. Seleccionar el Lote Familiar de 3 gavetas ($150,000.00).<br>4. Elegir la opción de compra *"Contado"*.<br>5. Confirmar y registrar la venta en caja. |
| **Resultado Esperado** | 1. El sistema totaliza el importe a pagar en exactamente **$150,000.00**.<br>2. Se emite recibo de pago total en una sola exhibición.<br>3. El estado del lote cambia a `Vendido`.<br>4. El contrato queda marcado como `Liquidado` con saldo deudor en **$0.00**.<br>5. No se genera tabla de amortización ni cobro de intereses. |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura de pantalla del resumen de venta de contado y del recibo emitido por $150,000.00 confirmando saldo en $0.00. |

---

### CP-03 · Cálculo de Enganche Estándar (20%) y Capital Financiado

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-03** |
| **Requisito(s)** | RF-04, RN-04 |
| **Técnica Aplicada** | Partición de Equivalencia (EP-02) / Análisis de Valores Límite |
| **Objetivo** | Comprobar que al seleccionar un enganche del 20% sobre un lote de $120,000.00, el sistema calcule con exactitud matemática el monto del enganche y el capital restante a financiar. |
| **Precondición** | Simulador de financiamiento disponible en `/Simulador` o en el paso 2 del wizard de venta. |
| **Datos de Entrada** | Producto: Lote Básico (2 gavetas, Precio: $120,000.00).<br>Modalidad: `Financiamiento`.<br>Porcentaje de Enganche: `20%` (Enganche Estándar). |
| **Pasos de Ejecución** | 1. Ingresar al simulador financiero.<br>2. Seleccionar el Lote Básico ($120,000.00).<br>3. Marcar la opción de enganche del 20%.<br>4. Observar los importes desglosados en pantalla. |
| **Resultado Esperado** | El sistema calcula y muestra de forma inmediata:<br>* $\text{Enganche (20%)} = \$120,000.00 \times 0.20 = \mathbf{\$24,000.00}$<br>* $\text{Capital Financiado} = \$120,000.00 - \$24,000.00 = \mathbf{\$96,000.00}$ |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura de pantalla del formulario del simulador mostrando el lote seleccionado, el porcentaje del 20% y las etiquetas de Enganche ($24,000.00) y Capital Financiado ($96,000.00). |

---

### CP-04 · Asignación de Tasa Anual Indexada al Plazo (48 Meses)

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-04** |
| **Requisito(s)** | RF-05, RF-06, RN-05, RN-06 |
| **Técnica Aplicada** | Tabla de Decisiones (DT-04, DT-08, DT-12) / Valores Límite (BVA-09) |
| **Objetivo** | Comprobar que al seleccionar el plazo máximo de 48 meses, el sistema asigne de manera fija y automática la tasa anual del 24% y su correspondiente tasa mensual del 2.0%. |
| **Precondición** | Simulador financiero abierto con lote y enganche previamente seleccionados. |
| **Datos de Entrada** | Plazo seleccionado: `48 meses`. |
| **Pasos de Ejecución** | 1. En el simulador, ubicarse en el selector de plazos.<br>2. Seleccionar la opción *"48 meses"*.<br>3. Verificar el campo informativo de tasa de interés en pantalla. |
| **Resultado Esperado** | 1. El sistema asigna automáticamente la **Tasa Anual Fija del 24%**.<br>2. La tasa mensual interna utilizada para el cálculo es exactamente $24\% / 12 = \mathbf{2.0000\%}$ ($0.0200$).<br>3. El usuario no puede modificar manualmente el porcentaje de tasa. |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura de pantalla del simulador donde se aprecie la selección de 48 meses y la tasa anual del 24% (2.0% mensual) reflejada en el resumen. |

---

### CP-05 · Cálculo de Cuota Mensual e Intereses (Capital $96,000, 12 Meses, 16% Tasa)

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-05** |
| **Requisito(s)** | RF-07, RN-07 |
| **Técnica Aplicada** | Tabla de Decisiones (DT-05) / Oráculo Financiero |
| **Objetivo** | Verificar que la cuota fija mensual y el total de intereses se calculen con precisión matemática bajo el sistema de amortización francesa sobre saldos insolutos. |
| **Precondición** | Simulador financiero configurado con Capital = $96,000.00, Plazo = 12 meses, Tasa Anual = 16% ($i = 0.01333333$). |
| **Datos de Entrada** | $P = \$96,000.00$; $n = 12$; Tasa Anual $= 16\%$. |
| **Pasos de Ejecución** | 1. Introducir los parámetros de cotización en el simulador.<br>2. Solicitar el cálculo del financiamiento.<br>3. Verificar la mensualidad regular y el costo total proyectado. |
| **Resultado Esperado** | Según la fórmula: $\text{Mensualidad} = 96000 \times \left[ \frac{0.01333333(1.01333333)^{12}}{(1.01333333)^{12} - 1} \right]$<br>* **Mensualidad Ordinaria:** **$8,710.16**<br>* **Intereses Totales Proyectados:** **$8,521.95**<br>* **Total a Pagar (Financiamiento):** **$104,521.95**<br>* **Costo Total con Enganche:** **$128,521.95** |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura del simulador web o salida de prueba unitaria xUnit mostrando los importes de Mensualidad ($8,710.16) e Intereses Totales ($8,521.95). |

---

### CP-06 · Generación de Tabla de Amortización y Ajuste en Último Periodo

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-06** |
| **Requisito(s)** | RF-08, RNF-02 |
| **Técnica Aplicada** | Análisis de Valores Límite (BVA-11) / Tabla de Decisiones (DT-05) |
| **Objetivo** | Comprobar que la tabla de amortización se genere con los 12 periodos completos, desglosando saldo inicial, interés, capital amortizado, pago y saldo final, garantizando que el periodo 12 absorba el ajuste de centavos y el saldo final cierre en exactamente $0.00. |
| **Precondición** | Venta financiada cotizada para Lote Básico ($120k), enganche 20% ($24k), plazo 12 meses. |
| **Datos de Entrada** | Capital: $96,000.00; Plazo: 12 meses; Tasa Anual: 16%; Tasa Mensual: 1.333333%. |
| **Pasos de Ejecución** | 1. Generar la tabla de amortización en el sistema.<br>2. Inspeccionar la fila del periodo 1 al 11.<br>3. Inspeccionar detalladamente la fila del periodo 12 (último mes).<br>4. Comprobar la columna de Saldo Final en el periodo 12. |
| **Resultado Esperado** | La tabla contiene 12 filas exactamente. La fila 12 aplica el ajuste al centavo:<br>* Cuotas 1 a 11: **$8,710.16** cada una.<br>* Cuota 12 (Ajustada): Saldo Inicial 12 ($8,595.58) + Interés 12 ($114.61) = **$8,710.19** (+0.03 centavos).<br>* **Saldo Final Periodo 12:** **$0.00** (cero centavos restantes). |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura de pantalla de la tabla completa de amortización mostrando la última fila (Periodo 12) con pago de $8,710.19 y saldo final en $0.00. |

#### Desglose Matemático Oficial de la Tabla CP-06:
| Periodo | Saldo Inicial | Interés (1.3333%) | Capital Amortizado | Pago Mensual | Saldo Final |
|:---:|:---:|:---:|:---:|:---:|:---:|
| 1 | $96,000.00 | $1,280.00 | $7,430.16 | $8,710.16 | $88,569.84 |
| 2 | $88,569.84 | $1,180.93 | $7,529.23 | $8,710.16 | $81,040.61 |
| 3 | $81,040.61 | $1,080.54 | $7,629.62 | $8,710.16 | $73,410.99 |
| 4 | $73,410.99 | $978.81 | $7,731.35 | $8,710.16 | $65,679.64 |
| 5 | $65,679.64 | $875.73 | $7,834.43 | $8,710.16 | $57,845.21 |
| 6 | $57,845.21 | $771.27 | $7,938.89 | $8,710.16 | $49,906.32 |
| 7 | $49,906.32 | $665.42 | $8,044.74 | $8,710.16 | $41,861.58 |
| 8 | $41,861.58 | $558.15 | $8,152.01 | $8,710.16 | $33,709.57 |
| 9 | $33,709.57 | $449.46 | $8,260.70 | $8,710.16 | $25,448.87 |
| 10 | $25,448.87 | $339.32 | $8,370.84 | $8,710.16 | $17,078.03 |
| 11 | $17,078.03 | $227.71 | $8,482.45 | $8,710.16 | $8,595.58 |
| **12 (Ajuste)** | **$8,595.58** | **$114.61** | **$8,595.58** | **$8,710.19** | **$0.00** |

---

### CP-07 · Abono Extraordinario a Capital y Recálculo de Mensualidad

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-07** |
| **Requisito(s)** | RF-10, RF-11, RN-08 |
| **Técnica Aplicada** | Pruebas Basadas en Escenarios (ST-02) / Partición de Equivalencia |
| **Objetivo** | Comprobar que tras aplicar un abono extraordinario a capital de $10,000.00 al concluir el periodo 3, el sistema reduzca el saldo insoluto y recalcule una nueva cuota periódica menor conservando el plazo final de 12 meses. |
| **Precondición** | Contrato activo CP-06 con periodos 1, 2 y 3 pagados (Saldo insoluto al cierre del periodo 3: $73,410.99). |
| **Datos de Entrada** | Monto de Abono Extraordinario a Capital: **$10,000.00**.<br>Periodo de aplicación: Periodo 3.<br>Periodos restantes por amortizar: 9 meses (periodos 4 al 12). |
| **Pasos de Ejecución** | 1. Ingresar al módulo de Cobranza en `/Caja/AbonoCapital`.<br>2. Localizar el contrato activo de Carlos Ruiz.<br>3. Registrar el pago ordinario de la cuota 3 ($8,710.16).<br>4. Registrar un abono extraordinario a capital por $10,000.00.<br>5. Confirmar la operación y consultar la nueva tabla de amortización recalculada. |
| **Resultado Esperado** | 1. El saldo de capital al inicio del periodo 4 se reduce a: $\$73,410.99 - \$10,000.00 = \mathbf{\$63,410.99}$.<br>2. El plazo final se mantiene intacto (vence en el mes 12).<br>3. La nueva cuota para los periodos 4 a 11 se recalcula a: **$7,523.51** (ahorro mensual de $1,186.65).<br>4. La cuota 12 ajustada queda en **$7,523.50** y el saldo final cierra exactamente en **$0.00**. |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura de pantalla de la vista de caja con el recibo del abono por $10,000.00 y la tabla recalculada comparando la cuota anterior ($8,710.16) vs la nueva cuota ($7,523.51). |

---

### CP-08 · Liquidación Anticipada con Descuento del 10% sobre Intereses Futuros

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-08** |
| **Requisito(s)** | RF-12, RN-09 |
| **Técnica Aplicada** | Pruebas Basadas en Escenarios (ST-03) |
| **Objetivo** | Comprobar que al solicitar la liquidación total anticipada tras el periodo 3, el sistema calcule el saldo deudor como la suma del capital pendiente más el 90% de los intereses futuros programados (descuento comercial del 10%). |
| **Precondición** | Contrato CP-06 con cuotas 1, 2 y 3 pagadas puntualmente. Saldo de capital pendiente: **$73,410.99**. |
| **Datos de Entrada** | Solicitud de cotización de Liquidación Anticipada al inicio del periodo 4. |
| **Pasos de Ejecución** | 1. Ingresar a la consulta del contrato en `/Caja/Liquidacion`.<br>2. Solicitar la cotización formal de liquidación anticipada.<br>3. Comprobar el desglose matemático en pantalla.<br>4. Registrar el pago y liquidar el contrato. |
| **Resultado Esperado** | El sistema desglosa:<br>* $\text{Capital Pendiente} = \mathbf{\$73,410.99}$<br>* $\text{Intereses Futuros Programados (Periodos 4 a 12)} = \sum_{j=4}^{12} I_j = \mathbf{\$4,980.45}$<br>* $\text{Descuento Comercial (10%)} = \$4,980.45 \times 0.10 = \mathbf{\$498.05}$<br>* $\text{Intereses a Pagar (90%)} = \$4,980.45 - \$498.05 = \mathbf{\$4,482.40}$<br>* **Monto Total de Liquidación:** $\$73,410.99 + \$4,482.40 = \mathbf{\$77,893.39}$<br>Tras pagar, el contrato cambia a estado `Liquidado` y la deuda se cancela a $0.00. |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura de pantalla de la pantalla de cotización de liquidación anticipada mostrando el desglose de capital, descuento del 10% en intereses ($498.05) y total a pagar de $77,893.39. |

---

### CP-09 · Identificación de Mensualidades Vencidas sin Intereses Moratorios

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-09** |
| **Requisito(s)** | RF-13, RF-14, RN-10 |
| **Técnica Aplicada** | Pruebas Basadas en Escenarios (ST-03) / Regla de Alcance v1.0.0 |
| **Objetivo** | Comprobar que cuando transcurre la fecha límite de vencimiento de una mensualidad sin recibir pago, el sistema identifique visualmente el atraso pero **no genere ni cobre intereses moratorios** ($0.00). |
| **Precondición** | Contrato activo con Periodo 1 pagado. La fecha de corte del sistema se sitúa 5 días después de la fecha de vencimiento del Periodo 2. |
| **Datos de Entrada** | Consulta de Estado de Cuenta del cliente en fecha posterior al vencimiento. |
| **Pasos de Ejecución** | 1. Modificar la fecha de consulta del sistema (`TimeProvider`) a una fecha posterior al vencimiento de la cuota 2.<br>2. Ingresar a `/EstadoCuenta` del contrato.<br>3. Verificar el estado visual de la cuota 2 y el total exigible. |
| **Resultado Esperado** | 1. La mensualidad 2 cambia su estado a `Vencida` (destacada en color rojo o con insignia de alerta).<br>2. Se indica el número de días de atraso acumulados.<br>3. **El importe de Intereses Moratorios es exactamente $0.00**.<br>4. El importe total requerido para regularizar sigue siendo la cuota ordinaria intacta ($8,710.16). |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura de pantalla del Estado de Cuenta mostrando la mensualidad en estado `Vencida` con el campo de recargos moratorios mostrando expresamente `$0.00`. |

---

### CP-10 · Rechazo de Parámetros No Permitidos (Enganche 15% / Plazo 36 Meses)

| Campo | Especificación |
|---|---|
| **Identificador** | **CP-10** |
| **Requisito(s)** | RF-04, RF-05, RN-04, RN-05 |
| **Técnica Aplicada** | Partición de Equivalencia Inválida (EP-04, EP-11) |
| **Objetivo** | Comprobar que el sistema rechace con mensajes claros de validación cualquier intento de registrar una venta o simulación con porcentajes de enganche o plazos que no formen parte de las opciones autorizadas. |
| **Precondición** | Formulario de simulación de financiamiento abierto. |
| **Datos de Entrada** | Caso A: Enganche ingresado = `15%`.<br>Caso B: Plazo ingresado = `36 meses`. |
| **Pasos de Ejecución** | 1. Intentar enviar una petición al cotizador con enganche del 15%.<br>2. Observar la respuesta del cliente y del servidor.<br>3. Intentar enviar una petición con plazo de 36 meses.<br>4. Observar la respuesta del cliente y del servidor. |
| **Resultado Esperado** | 1. Caso A: Petición rechazada con error de validación: *"Enganche no permitido. Solo se acepta 10%, 20% o 30%"*.<br>2. Caso B: Petición rechazada con error de validación: *"Plazo inválido. Solo se admiten plazos de 12, 18, 24 o 48 meses"*. |
| **Resultado Obtenido** | *[Pendiente de ejecución]* |
| **Estado** | `Pendiente` |
| **Evidencia Requerida** | Captura de pantalla de la interfaz web mostrando las alertas de validación en rojo al ingresar 15% de enganche o 36 meses de plazo. |
