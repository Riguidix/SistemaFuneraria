# Scripts de Base de Datos y Ciclo de Vida de Persistencia (PostgreSQL 16)

Este directorio contiene los scripts DDL canónicos para la inicialización y rollback de la base de datos `funeraria_dev`, cumpliendo con la política **Rollback-First** de ingeniería.

---

## 📁 Archivos Disponibles

| Script | Propósito | Descripción |
|---|---|---|
| `init.sql` | **Migración UP** | Inicializa extensiones, secuencias (`seq_pagos_folio`), tablas de dominio (`lotes`, `clientes`, `ventas`, `financiamientos`, `mensualidades`, `pagos`, `AspNetUsers`), índices de alto rendimiento, triggers de inmutabilidad append-only para `pagos`, y realiza el seeding idempotente de los 3 lotes autorizados (`LOT-02`, `LOT-03`, `LOT-04`). |
| `down.sql` | **Rollback DOWN** | Desmantela de forma ordenada e inversa todos los triggers, funciones, tablas dependientes y secuencias sin dejar objetos huérfanos. |

---

## 🔄 Ciclo de Vida Local: Up $\rightarrow$ Down $\rightarrow$ Up

Para validar la reversibilidad estricta de la base de datos en el contenedor Docker:

### 1. Inicialización (Up)
El script `init.sql` se ejecuta automáticamente en el primer arranque limpio del contenedor al montarse en `/docker-entrypoint-initdb.d/01-init.sql`.

Si se desea ejecutar manualmente sobre una base de datos existente:
```bash
docker exec -i funeraria-db psql -U postgres -d funeraria_dev -f - < scripts/db/init.sql
```

### 2. Reversión / Rollback (Down)
Ejecuta la eliminación ordenada y limpia del esquema:
```bash
docker exec -i funeraria-db psql -U postgres -d funeraria_dev -f - < scripts/db/down.sql
```

### 3. Re-inicialización y Verificación (Up)
Vuelve a aplicar `init.sql` para comprobar que la regeneración es 100% idempotente y no genera colisiones:
```bash
docker exec -i funeraria-db psql -U postgres -d funeraria_dev -f - < scripts/db/init.sql
```
