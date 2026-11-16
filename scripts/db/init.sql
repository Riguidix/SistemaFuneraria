-- ==============================================================================
-- SISTEMA DE VENTA Y FINANCIAMIENTO DE LOTES FUNERARIOS
-- Script DDL de Inicialización de Persistencia (PostgreSQL 16)
-- Nota: El esquema de tablas (clientes, AspNet*, etc.) es administrado
-- exclusivamente por las migraciones de EF Core (SistemaFuneraria.Infrastructure).
-- ==============================================================================

-- 0. Extensiones requeridas
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 1. Secuencia de Folios para Caja (Zero-Contention, Sección 5 ERD)
CREATE SEQUENCE IF NOT EXISTS seq_pagos_folio
    START WITH 1
    INCREMENT BY 1
    NO CYCLE;
