-- ==============================================================================
-- SISTEMA DE VENTA Y FINANCIAMIENTO DE LOTES FUNERARIOS
-- Script DDL de Rollback / Desmantelamiento (PostgreSQL 16)
-- Política: Rollback-First (Reversibilidad Estricta)
-- Ticket: SF-002: Docker Compose con PostgreSQL 16 y scripts de inicialización
-- ==============================================================================

-- 1. Eliminar Triggers y Funciones
DROP TRIGGER IF EXISTS trg_pagos_inmutables ON pagos;
DROP FUNCTION IF EXISTS fn_prevenir_modificacion_pagos();

-- 2. Eliminar Tablas en Orden Inverso de Dependencia Referencial
DROP TABLE IF EXISTS pagos CASCADE;
DROP TABLE IF EXISTS mensualidades CASCADE;
DROP TABLE IF EXISTS financiamientos CASCADE;
DROP TABLE IF EXISTS ventas CASCADE;
DROP TABLE IF EXISTS "AspNetUsers" CASCADE;
DROP TABLE IF EXISTS clientes CASCADE;
DROP TABLE IF EXISTS lotes CASCADE;

-- 3. Eliminar Secuencias de Folios
DROP SEQUENCE IF EXISTS seq_pagos_folio CASCADE;
