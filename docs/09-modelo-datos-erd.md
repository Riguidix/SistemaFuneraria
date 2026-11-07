# MODELO DE DATOS Y DIAGRAMA ENTIDAD-RELACIÓN (ERD)
## Sistema de Venta y Financiamiento de Lotes Funerarios

| Metadato | Detalle |
|---|---|
| **Proyecto** | Sistema de Venta y Financiamiento de Lotes Funerarios |
| **Materia / Contexto** | Estándares y Métricas para el Desarrollo de Software |
| **Documento** | Especificación de Arquitectura de Persistencia y ERD Oficial |
| **Versión del Modelo** | 1.1 (Calibrado con PM · 3 Hallazgos Resueltos) |
| **Motor de Base de Datos** | PostgreSQL 16 Alpine (Docker Compose) |
| **Mapeador ORM** | Entity Framework Core 8 (Npgsql.EntityFrameworkCore.PostgreSQL) |
| **Líder DBA / Persistencia** | Senior DBA & Site Reliability Engineer |
| **Fecha de Aprobación** | 07 de Noviembre de 2026 |

---

### 1. Propósito y Filosofía del Modelo de Datos

Este documento define la arquitectura de base de datos relacional para el sistema de comercialización y cobranza de lotes funerarios. Está diseñado bajo la premisa de **cero incidencias no programadas (resiliencia para operar sin interrupciones a las 2:00 AM)**, cumpliendo con:

1. **Integridad Financiera Absoluta (RNF-02):** Precisión decimal estricta mediante `NUMERIC(14,2)` en base de datos y `decimal` de 128 bits en C#. Cero tolerancia a errores de punto flotante.
2. **Idempotencia y Prevención de Doble Cobro (RNF-04, ST-04):** Llave única UUID `clave_idempotencia` en pagos para evitar cobros duplicados por concurrencia de cajeros o reintentos de red.
3. **Control de Concurrencia Optimista:** Token de concurrencia nativo de PostgreSQL (`xmin`) en contratos de financiamiento para evitar condiciones de carrera al actualizar saldos insolutos.
4. **Auditoría e Integridad Referencial Estricta:** Vínculos foráneos hacia usuarios de Identity (`AspNetUsers`), separando el rol de venta (`vendedor_id`) del rol de cobranza (`cajero_id`).
5. **Trazabilidad Contable Completa (RN-09):** Registro explícito de `descuento_aplicado` en liquidaciones anticipadas (10% de ahorro sobre intereses futuros).
6. **Catálogo de Productos Desacoplado (RN-01):** Relación 1:N entre el catálogo fijo de lotes y las transacciones de venta, permitiendo comercializar de forma ilimitada los 3 modelos autorizados ($120k, $150k, $180k).

---

### 2. Diagrama Entidad-Relación Oficial (Mermaid ERD)

```mermaid
erDiagram
    ASPNET_USERS ||--o{ VENTAS : "registra como vendedor (1:N)"
    ASPNET_USERS ||--o{ PAGOS : "cobra en caja (1:N)"
    LOTES ||--o{ VENTAS : "producto comercializado (1:N)"
    CLIENTES ||--o{ VENTAS : "adquiere (1:N)"
    VENTAS ||--o| FINANCIAMIENTOS : "origina (1:0..1)"
    FINANCIAMIENTOS ||--|{ MENSUALIDADES : "desglosa en (1:N)"
    FINANCIAMIENTOS ||--o{ PAGOS : "recibe cobranza (1:N)"
    MENSUALIDADES ||--o| PAGOS : "aplica pago ordinario (1:0..1)"

    ASPNET_USERS {
        varchar(450) id PK
        varchar(256) user_name
        varchar(256) email
        varchar(50) role "Vendedor / Cajero"
    }

    LOTES {
        varchar(10) id PK "LOT-02, LOT-03, LOT-04"
        varchar(50) nombre "Básico, Familiar, Ampliado"
        int gavetas "2, 3, 4"
        numeric(14_2) precio_contado "$120k, $150k, $180k"
        boolean is_active "Estado del SKU en catálogo"
    }

    CLIENTES {
        uuid id PK
        varchar(18) curp UK "18 caracteres alfanuméricos"
        varchar(255) correo UK
        varchar(100) nombre
        varchar(100) apellidos
        varchar(10) telefono "10 dígitos numéricos"
        text direccion
        timestamptz created_at
    }

    VENTAS {
        uuid id PK
        uuid cliente_id FK
        varchar(10) lote_id FK "Catálogo 1:N"
        varchar(450) vendedor_id FK "Usuario Identity Vendedor"
        varchar(20) tipo_venta "Contado / Financiado"
        numeric(14_2) monto_total
        timestamptz fecha_venta
        varchar(20) estado "Completada / Cancelada"
    }

    FINANCIAMIENTOS {
        uuid id PK
        uuid venta_id FK_UK "Relación 1:1 con Venta"
        numeric(14_2) enganche "10%, 20% o 30%"
        numeric(14_2) capital_financiado "Precio - Enganche"
        int plazo_meses "12, 18, 24, 48"
        numeric(5_4) tasa_anual "0.1600, 0.1800, 0.2000, 0.2400"
        numeric(14_2) saldo_insoluto_actual
        varchar(20) estado "Activo / Liquidado"
        uint xmin "Token Concurrencia Optimista PostgreSQL"
    }

    MENSUALIDADES {
        uuid id PK
        uuid financiamiento_id FK
        int numero_periodo "1 a n"
        numeric(14_2) saldo_inicial
        numeric(14_2) interes
        numeric(14_2) capital_amortizado
        numeric(14_2) cuota
        numeric(14_2) saldo_final
        date fecha_vencimiento
        varchar(20) estado "Pendiente / Pagada / Vencida"
    }

    PAGOS {
        uuid id PK
        varchar(20) folio UK "PAG-2026-000001 (Sequence)"
        uuid financiamiento_id FK
        uuid mensualidad_id FK_NULLABLE "Null si es Abono/Liquidacion"
        varchar(450) cajero_id FK "Usuario Identity Cajero"
        uuid clave_idempotencia UK "Prevención Doble Cobro"
        varchar(20) tipo_pago "Enganche/Ordinario/Abono/Liquidacion"
        numeric(14_2) monto "Efectivo real cobrado en caja"
        numeric(14_2) descuento_aplicado "Bonificación 10% RN-09"
        timestamptz fecha_pago
    }
```

---

### 3. Diccionario Detallado de Datos

#### 3.1 Tabla: `lotes` (Catálogo Inmutable de Productos)
Representa los tres SKUs comercializables de acuerdo con la regla RN-01.

| Columna | Tipo PostgreSQL | Tipo .NET | Nulo | Restricciones / Índices | Descripción Funcional |
|---|---|---|:---:|---|---|
| `id` | `VARCHAR(10)` | `string` | NO | `PRIMARY KEY` | Clave fija (`LOT-02`, `LOT-03`, `LOT-04`). |
| `nombre` | `VARCHAR(50)` | `string` | NO | — | Denominación comercial (Básico, Familiar, Ampliado). |
| `gavetas` | `INTEGER` | `int` | NO | `CHECK (gavetas IN (2, 3, 4))` | Capacidad de descanso del lote funerario. |
| `precio_contado`| `NUMERIC(14,2)`| `decimal` | NO | `CHECK (precio_contado > 0)` | Precio de lista ($120,000, $150,000, $180,000 MXN). |
| `is_active` | `BOOLEAN` | `bool` | NO | `DEFAULT TRUE` | Bandera de activación del producto. |

#### 3.2 Tabla: `clientes`
Almacena el padrón de compradores con validaciones estrictas de unicidad e identidad.

| Columna | Tipo PostgreSQL | Tipo .NET | Nulo | Restricciones / Índices | Descripción Funcional |
|---|---|---|:---:|---|---|
| `id` | `UUID` | `Guid` | NO | `PRIMARY KEY` | Identificador único universal. |
| `curp` | `VARCHAR(18)` | `string` | NO | `UNIQUE`, `ix_clientes_curp` | Clave Única de Registro de Población oficial. |
| `correo` | `VARCHAR(255)`| `string` | NO | `UNIQUE`, `ix_clientes_correo`| Correo electrónico para notificaciones. |
| `nombre` | `VARCHAR(100)`| `string` | NO | — | Nombres del titular. |
| `apellidos` | `VARCHAR(100)`| `string` | NO | — | Apellidos paterno y materno. |
| `telefono` | `VARCHAR(10)` | `string` | NO | `CHECK (telefono ~ '^[0-9]{10}$')` | Teléfono a 10 dígitos obligatorios. |
| `direccion` | `TEXT` | `string` | NO | — | Domicilio completo del cliente. |
| `created_at` | `TIMESTAMPTZ` | `DateTimeOffset` | NO | `DEFAULT NOW()` | Fecha y hora UTC de alta en sistema. |

#### 3.3 Tabla: `ventas`
Encabeza toda operación comercial cerrada por un vendedor.

| Columna | Tipo PostgreSQL | Tipo .NET | Nulo | Restricciones / Índices | Descripción Funcional |
|---|---|---|:---:|---|---|
| `id` | `UUID` | `Guid` | NO | `PRIMARY KEY` | Folio interno único de la venta. |
| `cliente_id` | `UUID` | `Guid` | NO | `FK -> clientes(id)`, `ix_ventas_cliente_id` | Cliente comprador. |
| `lote_id` | `VARCHAR(10)` | `string` | NO | `FK -> lotes(id)`, `ix_ventas_lote_id` | Producto del catálogo (Relación 1:N). |
| `vendedor_id` | `VARCHAR(450)`| `string` | NO | `FK -> AspNetUsers(Id)`, `ix_ventas_vendedor_id`| Vendedor de Identity que cerró la venta. |
| `tipo_venta` | `VARCHAR(20)` | `string` | NO | `CHECK (tipo_venta IN ('Contado', 'Financiado'))` | Modalidad comercial pactada. |
| `monto_total` | `NUMERIC(14,2)`| `decimal` | NO | `CHECK (monto_total > 0)` | Importe total convenido de la operación. |
| `fecha_venta` | `TIMESTAMPTZ` | `DateTimeOffset` | NO | `DEFAULT NOW()`, `ix_ventas_fecha` | Fecha y hora del cierre de venta. |
| `estado` | `VARCHAR(20)` | `string` | NO | `DEFAULT 'Completada'` | Estado legal de la venta. |

#### 3.4 Tabla: `financiamientos`
Gobierna el crédito otorgado, amortización y saldo deudor.

| Columna | Tipo PostgreSQL | Tipo .NET | Nulo | Restricciones / Índices | Descripción Funcional |
|---|---|---|:---:|---|---|
| `id` | `UUID` | `Guid` | NO | `PRIMARY KEY` | Identificador único del contrato de crédito. |
| `venta_id` | `UUID` | `Guid` | NO | `FK -> ventas(id) UNIQUE` | Venta que dio origen al crédito (1:1). |
| `enganche` | `NUMERIC(14,2)`| `decimal` | NO | `CHECK (enganche > 0)` | Monto pagado por enganche (10%, 20% o 30%). |
| `capital_financiado`| `NUMERIC(14,2)`| `decimal` | NO | `CHECK (capital_financiado > 0)` | Monto insoluto original a amortizar. |
| `plazo_meses`| `INTEGER` | `int` | NO | `CHECK (plazo_meses IN (12, 18, 24, 48))` | Plazo acordado en meses. |
| `tasa_anual` | `NUMERIC(5,4)` | `decimal` | NO | `CHECK (tasa_anual IN (0.1600, 0.1800, 0.2000, 0.2400))` | Tasa de interés anual inmutable. |
| `saldo_insoluto_actual`| `NUMERIC(14,2)`| `decimal` | NO | `CHECK (saldo_insoluto_actual >= 0)` | Capital pendiente a la fecha. |
| `estado` | `VARCHAR(20)` | `string` | NO | `CHECK (estado IN ('Activo', 'Liquidado'))` | Estado de exigibilidad del crédito. |
| `xmin` | `XMIN (uint)` | `uint` | NO | `System Column` | Concurrency token optimista nativo. |

#### 3.5 Tabla: `mensualidades`
Almacena el calendario financiero proyectado periodo a periodo.

| Columna | Tipo PostgreSQL | Tipo .NET | Nulo | Restricciones / Índices | Descripción Funcional |
|---|---|---|:---:|---|---|
| `id` | `UUID` | `Guid` | NO | `PRIMARY KEY` | Identificador único de la mensualidad. |
| `financiamiento_id`| `UUID` | `Guid` | NO | `FK -> financiamientos(id) ON DELETE CASCADE`| Contrato padre al que pertenece la cuota. |
| `numero_periodo` | `INTEGER` | `int` | NO | `CHECK (numero_periodo >= 1)` | Número de cuota periódica ($1 \dots n$). |
| `saldo_inicial`| `NUMERIC(14,2)`| `decimal` | NO | `CHECK (saldo_inicial >= 0)` | Deuda antes de aplicar la cuota. |
| `interes` | `NUMERIC(14,2)`| `decimal` | NO | `CHECK (interes >= 0)` | Interés del periodo ($Saldo \times i$). |
| `capital_amortizado`| `NUMERIC(14,2)`| `decimal`| NO | `CHECK (capital_amortizado >= 0)` | Amortización aplicada al principal. |
| `cuota` | `NUMERIC(14,2)`| `decimal` | NO | `CHECK (cuota > 0)` | Importe total exigible en el periodo. |
| `saldo_final` | `NUMERIC(14,2)`| `decimal` | NO | `CHECK (saldo_final >= 0)` | Saldo posterior a la cuota ($0.00 en $n$). |
| `fecha_vencimiento`| `DATE` | `DateOnly` | NO | `ix_mensualidades_vencimiento` | Fecha límite para cubrir el pago sin retraso. |
| `estado` | `VARCHAR(20)` | `string` | NO | `CHECK (estado IN ('Pendiente', 'Pagada', 'Vencida'))` | Estado administrativo de cobro. |

#### 3.6 Tabla: `pagos`
Libro mayor de ingresos en caja con auditoría contable.

| Columna | Tipo PostgreSQL | Tipo .NET | Nulo | Restricciones / Índices | Descripción Funcional |
|---|---|---|:---:|---|---|
| `id` | `UUID` | `Guid` | NO | `PRIMARY KEY` | Identificador universal del movimiento. |
| `folio` | `VARCHAR(20)` | `string` | NO | `UNIQUE`, `ix_pagos_folio` | Folio oficial imprimible (`PAG-2026-XXXXXX`). |
| `financiamiento_id`| `UUID` | `Guid` | NO | `FK -> financiamientos(id)`, `ix_pagos_financiamiento_id`| Crédito impactado por el ingreso. |
| `mensualidad_id` | `UUID` | `Guid` | SÍ | `FK -> mensualidades(id)`, `ix_pagos_mensualidad_id` | Cuota saldada (nulo en abono/liquidación). |
| `cajero_id` | `VARCHAR(450)`| `string` | NO | `FK -> AspNetUsers(Id)`, `ix_pagos_cajero_id` | Cajero de Identity receptor del pago. |
| `clave_idempotencia`| `UUID` | `Guid` | NO | `UNIQUE`, `ix_pagos_idempotencia` | Token de seguridad contra cobros duplicados. |
| `tipo_pago` | `VARCHAR(20)` | `string` | NO | `CHECK (tipo_pago IN ('Enganche', 'Ordinario', 'AbonoCapital', 'Liquidacion'))` | Concepto del ingreso en caja. |
| `monto` | `NUMERIC(14,2)`| `decimal` | NO | `CHECK (monto > 0)` | Dinero real efectivamente cobrado. |
| `descuento_aplicado`| `NUMERIC(14,2)`| `decimal`| NO | `DEFAULT 0.00`, `CHECK (descuento_aplicado >= 0)` | Bonificación comercial del 10% (RN-09). |
| `fecha_pago` | `TIMESTAMPTZ` | `DateTimeOffset` | NO | `DEFAULT NOW()`, `ix_pagos_fecha` | Marca de tiempo inmutable del cobro. |

---

### 4. Matriz de Índices de Alto Rendimiento

Para asegurar que el motor de base de datos no ejecute escaneos secuenciales destructivos bajo carga, se configuran los siguientes índices:

```sql
-- 1. Unicidad de Clientes
CREATE UNIQUE INDEX ix_clientes_curp ON clientes (curp);
CREATE UNIQUE INDEX ix_clientes_correo ON clientes (correo);

-- 2. Búsqueda de Clientes en Interfaz (Trigramas / Pattern matching)
CREATE INDEX ix_clientes_apellidos_nombre ON clientes (apellidos text_pattern_ops, nombre text_pattern_ops);

-- 3. Llaves Foráneas de Ventas
CREATE INDEX ix_ventas_cliente_id ON ventas (cliente_id);
CREATE INDEX ix_ventas_lote_id ON ventas (lote_id);
CREATE INDEX ix_ventas_vendedor_id ON ventas (vendedor_id);
CREATE INDEX ix_ventas_fecha ON ventas (fecha_venta DESC);

-- 4. Contratos y Cuotas
CREATE UNIQUE INDEX uq_financiamientos_venta_id ON financiamientos (venta_id);
CREATE UNIQUE INDEX uq_mensualidades_periodo ON mensualidades (financiamiento_id, numero_periodo);

-- 5. Índice Parcial Crítico: Detección Instantánea de Mora / Vencimientos (CP-09, RF-14)
-- Escanea únicamente cuotas pendientes, ignorando el 95% de la tabla ya pagada
CREATE INDEX ix_mensualidades_mora_parcial 
ON mensualidades (fecha_vencimiento) 
WHERE estado = 'Pendiente';

-- 6. Caja e Idempotencia
CREATE UNIQUE INDEX uq_pagos_idempotencia ON pagos (clave_idempotencia);
CREATE UNIQUE INDEX uq_pagos_folio ON pagos (folio);
CREATE INDEX ix_pagos_financiamiento_id ON pagos (financiamiento_id);
CREATE INDEX ix_pagos_cajero_id ON pagos (cajero_id);
CREATE INDEX ix_pagos_fecha ON pagos (fecha_pago DESC);
```

---

### 5. Secuencia de Folios Segura (Zero-Contention)

Para cumplir con el requerimiento de folio contable sin bloquear la tabla `pagos`:

```sql
-- Secuencia PostgreSQL nativa
CREATE SEQUENCE seq_pagos_folio START WITH 1 INCREMENT BY 1 NO CYCLE;

-- Expresión por defecto aplicada en EF Core (SF-035)
-- Genera: PAG-2026-000001, PAG-2026-000002, etc.
ALTER TABLE pagos ALTER COLUMN folio 
SET DEFAULT CONCAT('PAG-', TO_CHAR(NOW(), 'YYYY'), '-', LPAD(nextval('seq_pagos_folio')::text, 6, '0'));
```

---

### 6. Configuración de Concurrencia Optimista en EF Core (`xmin`)

Para el contrato de financiamiento (`SF-031`), la configuración en Fluent API asegura la protección contra concurrencia sin sobrecarga de triggers:

```csharp
// FinanciamientoConfiguration.cs
public class FinanciamientoConfiguration : IEntityTypeConfiguration<Financiamiento>
{
    public void Configure(EntityTypeBuilder<Financiamiento> builder)
    {
        builder.ToTable("financiamientos");

        builder.HasKey(f => f.Id);

        // Mapeo del concurrency token nativo de PostgreSQL
        builder.UseXminAsConcurrencyVersion();

        builder.Property(f => f.SaldoInsolutoActual)
               .HasPrecision(14, 2)
               .IsRequired();

        builder.HasOne<Venta>()
               .WithOne()
               .HasForeignKey<Financiamiento>(f => f.VentaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

### 7. Trazabilidad de Requisitos PRD Cubiertos por el ERD

| Requisito PRD | Entidad / Columna en ERD | Mecanismo de Garantía Técnica |
|---|---|---|
| **RN-01 (3 Lotes fijos)** | `lotes` (ID, Precio) | Seeder con 3 filas inmutables + FK `Restrict`. |
| **RN-02 (Venta asociada)**| `ventas` (`cliente_id`, `lote_id`) | Claves foráneas obligatorias `NOT NULL`. |
| **RN-04 / RN-05 (Límites)**| `financiamientos` (`plazo_meses`, `enganche`) | Check constraints de valores autorizados en DDL. |
| **RN-08 (Abono Capital)**| `pagos` + `financiamientos.saldo_insoluto_actual` | Transacción ACID con token `xmin`. |
| **RN-09 (Liquidación 10%)**| `pagos.descuento_aplicado` | Auditoría de bonificación del 10% en intereses. |
| **RN-10 (Vencida $0 mora)**| `mensualidades.estado` | Índice parcial sobre `fecha_vencimiento`. |
| **RN-11 (No negativos)** | Check constraints monetarios | `CHECK (monto > 0)` y `CHECK (saldo >= 0)`. |
| **RNF-02 (Precisión)** | Columnas de dinero | Tipo `NUMERIC(14,2)` universal. |
| **RNF-03 (RBAC Identity)** | `ventas.vendedor_id`, `pagos.cajero_id` | Integridad referencial con `AspNetUsers`. |
| **RNF-04 (ACID & Idemp)** | `pagos.clave_idempotencia` | Índice `UNIQUE` + Aislamiento en EF Core. |
