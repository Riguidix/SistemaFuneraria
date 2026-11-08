-- ==============================================================================
-- SISTEMA DE VENTA Y FINANCIAMIENTO DE LOTES FUNERARIOS
-- Script DDL de Inicialización de Persistencia (PostgreSQL 16)
-- Especificación Oficial: docs/09-modelo-datos-erd.md (Versión 1.1)
-- Ticket: SF-002: Docker Compose con PostgreSQL 16 y scripts de inicialización
-- ==============================================================================

-- 0. Extensiones
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 1. Secuencia de Folios para Caja (Zero-Contention, Sección 5 ERD)
CREATE SEQUENCE IF NOT EXISTS seq_pagos_folio 
    START WITH 1 
    INCREMENT BY 1 
    NO CYCLE;

-- 2. Tabla: lotes (Catálogo Inmutable de Productos, Sección 3.1 ERD)
CREATE TABLE IF NOT EXISTS lotes (
    id VARCHAR(10) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    gavetas INTEGER NOT NULL CHECK (gavetas IN (2, 3, 4)),
    precio_contado NUMERIC(14,2) NOT NULL CHECK (precio_contado > 0),
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- 3. Tabla: clientes (Padrón de Compradores, Sección 3.2 ERD)
CREATE TABLE IF NOT EXISTS clientes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    curp VARCHAR(18) NOT NULL UNIQUE,
    correo VARCHAR(255) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    telefono VARCHAR(10) NOT NULL CHECK (telefono ~ '^[0-9]{10}$'),
    direccion TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 4. Tabla: AspNetUsers (Identity RBAC Vendedor / Cajero, Sección 2 ERD)
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" VARCHAR(450) NOT NULL PRIMARY KEY,
    "UserName" VARCHAR(256) NULL,
    "NormalizedUserName" VARCHAR(256) NULL,
    "Email" VARCHAR(256) NULL,
    "NormalizedEmail" VARCHAR(256) NULL,
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" VARCHAR(50) NULL,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "LockoutEnd" TIMESTAMPTZ NULL,
    "LockoutEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "AccessFailedCount" INTEGER NOT NULL DEFAULT 0
);

-- 5. Tabla: ventas (Operaciones Comerciales Cerradas, Sección 3.3 ERD)
CREATE TABLE IF NOT EXISTS ventas (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID NOT NULL REFERENCES clientes(id) ON DELETE RESTRICT,
    lote_id VARCHAR(10) NOT NULL REFERENCES lotes(id) ON DELETE RESTRICT,
    vendedor_id VARCHAR(450) NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    tipo_venta VARCHAR(20) NOT NULL CHECK (tipo_venta IN ('Contado', 'Financiado')),
    monto_total NUMERIC(14,2) NOT NULL CHECK (monto_total > 0),
    fecha_venta TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    estado VARCHAR(20) NOT NULL DEFAULT 'Completada' CHECK (estado IN ('Completada', 'Cancelada'))
);

-- 6. Tabla: financiamientos (Crédito y Amortización Francesa, Sección 3.4 ERD)
-- Nota: La columna de concurrencia 'xmin' es un atributo del sistema nativo en PostgreSQL.
CREATE TABLE IF NOT EXISTS financiamientos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    venta_id UUID NOT NULL UNIQUE REFERENCES ventas(id) ON DELETE RESTRICT,
    enganche NUMERIC(14,2) NOT NULL CHECK (enganche > 0),
    capital_financiado NUMERIC(14,2) NOT NULL CHECK (capital_financiado > 0),
    plazo_meses INTEGER NOT NULL CHECK (plazo_meses IN (12, 18, 24, 48)),
    tasa_anual NUMERIC(5,4) NOT NULL CHECK (tasa_anual IN (0.1600, 0.1800, 0.2000, 0.2400)),
    saldo_insoluto_actual NUMERIC(14,2) NOT NULL CHECK (saldo_insoluto_actual >= 0),
    estado VARCHAR(20) NOT NULL CHECK (estado IN ('Activo', 'Liquidado'))
);

-- 7. Tabla: mensualidades (Calendario Financiero Proyectado, Sección 3.5 ERD)
CREATE TABLE IF NOT EXISTS mensualidades (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    financiamiento_id UUID NOT NULL REFERENCES financiamientos(id) ON DELETE CASCADE,
    numero_periodo INTEGER NOT NULL CHECK (numero_periodo >= 1),
    saldo_inicial NUMERIC(14,2) NOT NULL CHECK (saldo_inicial >= 0),
    interes NUMERIC(14,2) NOT NULL CHECK (interes >= 0),
    capital_amortizado NUMERIC(14,2) NOT NULL CHECK (capital_amortizado >= 0),
    cuota NUMERIC(14,2) NOT NULL CHECK (cuota > 0),
    saldo_final NUMERIC(14,2) NOT NULL CHECK (saldo_final >= 0),
    fecha_vencimiento DATE NOT NULL,
    estado VARCHAR(20) NOT NULL CHECK (estado IN ('Pendiente', 'Pagada', 'Vencida'))
);

-- 8. Tabla: pagos (Libro Mayor de Caja e Idempotencia, Sección 3.6 ERD)
CREATE TABLE IF NOT EXISTS pagos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    folio VARCHAR(20) NOT NULL UNIQUE DEFAULT CONCAT('PAG-', TO_CHAR(NOW(), 'YYYY'), '-', LPAD(nextval('seq_pagos_folio')::text, 6, '0')),
    financiamiento_id UUID NOT NULL REFERENCES financiamientos(id) ON DELETE RESTRICT,
    mensualidad_id UUID NULL REFERENCES mensualidades(id) ON DELETE RESTRICT,
    cajero_id VARCHAR(450) NOT NULL REFERENCES "AspNetUsers"("Id") ON DELETE RESTRICT,
    clave_idempotencia UUID NOT NULL UNIQUE,
    tipo_pago VARCHAR(20) NOT NULL CHECK (tipo_pago IN ('Enganche', 'Ordinario', 'AbonoCapital', 'Liquidacion')),
    monto NUMERIC(14,2) NOT NULL CHECK (monto > 0),
    descuento_aplicado NUMERIC(14,2) NOT NULL DEFAULT 0.00 CHECK (descuento_aplicado >= 0),
    fecha_pago TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 8.1 Inmutabilidad de Pagos (Append-Only / Auditoría Contable Estricta)
CREATE OR REPLACE FUNCTION fn_prevenir_modificacion_pagos()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'Operación no permitida: La tabla pagos es estrictamente append-only para garantizar trazabilidad y auditoría contable.';
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_pagos_inmutables ON pagos;
CREATE TRIGGER trg_pagos_inmutables
    BEFORE UPDATE OR DELETE ON pagos
    FOR EACH ROW
    EXECUTE FUNCTION fn_prevenir_modificacion_pagos();

-- ==============================================================================
-- 9. Matriz de Índices de Alto Rendimiento (Sección 4 ERD)
-- ==============================================================================

-- 9.1 Unicidad y Búsqueda de Clientes
CREATE UNIQUE INDEX IF NOT EXISTS ix_clientes_curp ON clientes (curp);
CREATE UNIQUE INDEX IF NOT EXISTS ix_clientes_correo ON clientes (correo);
CREATE INDEX IF NOT EXISTS ix_clientes_apellidos_nombre ON clientes (apellidos text_pattern_ops, nombre text_pattern_ops);

-- 9.2 Índices de Llaves Foráneas y Filtros en Ventas
CREATE INDEX IF NOT EXISTS ix_ventas_cliente_id ON ventas (cliente_id);
CREATE INDEX IF NOT EXISTS ix_ventas_lote_id ON ventas (lote_id);
CREATE INDEX IF NOT EXISTS ix_ventas_vendedor_id ON ventas (vendedor_id);
CREATE INDEX IF NOT EXISTS ix_ventas_fecha ON ventas (fecha_venta DESC);

-- 9.3 Índices de Contratos y Periodos de Cuotas
CREATE UNIQUE INDEX IF NOT EXISTS uq_financiamientos_venta_id ON financiamientos (venta_id);
CREATE UNIQUE INDEX IF NOT EXISTS uq_mensualidades_periodo ON mensualidades (financiamiento_id, numero_periodo);

-- 9.4 Índice Parcial Crítico: Detección Instantánea de Mora / Vencimientos (CP-09, RF-14)
CREATE INDEX IF NOT EXISTS ix_mensualidades_mora_parcial 
ON mensualidades (fecha_vencimiento) 
WHERE estado = 'Pendiente';

-- 9.5 Índices de Caja e Idempotencia
CREATE UNIQUE INDEX IF NOT EXISTS uq_pagos_idempotencia ON pagos (clave_idempotencia);
CREATE UNIQUE INDEX IF NOT EXISTS uq_pagos_folio ON pagos (folio);
CREATE INDEX IF NOT EXISTS ix_pagos_financiamiento_id ON pagos (financiamiento_id);
CREATE INDEX IF NOT EXISTS ix_pagos_cajero_id ON pagos (cajero_id);
CREATE INDEX IF NOT EXISTS ix_pagos_fecha ON pagos (fecha_pago DESC);

-- ==============================================================================
-- 10. Datos Maestros Iniciales y Seeders Idempotentes
-- ==============================================================================

-- 10.1 Catálogo Cerrado de 3 Lotes Funerarios Autorizados (RN-01)
INSERT INTO lotes (id, nombre, gavetas, precio_contado, is_active)
VALUES 
    ('LOT-02', 'Lote Básico', 2, 120000.00, TRUE),
    ('LOT-03', 'Lote Familiar', 3, 150000.00, TRUE),
    ('LOT-04', 'Lote Ampliado', 4, 180000.00, TRUE)
ON CONFLICT (id) DO NOTHING;

-- 10.2 Usuarios Iniciales de Identity para Desarrollo y Pruebas
INSERT INTO "AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "SecurityStamp", "ConcurrencyStamp")
VALUES 
    ('usr-vendedor-01', 'vendedor@funeraria.com', 'VENDEDOR@FUNERARIA.COM', 'vendedor@funeraria.com', 'VENDEDOR@FUNERARIA.COM', TRUE, 'sec-stamp-vendedor-01', 'con-stamp-vendedor-01'),
    ('usr-cajero-01', 'cajero@funeraria.com', 'CAJERO@FUNERARIA.COM', 'cajero@funeraria.com', 'CAJERO@FUNERARIA.COM', TRUE, 'sec-stamp-cajero-01', 'con-stamp-cajero-01')
ON CONFLICT ("Id") DO NOTHING;
