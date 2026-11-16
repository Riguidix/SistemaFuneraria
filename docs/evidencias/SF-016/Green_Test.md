# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-016 |
| **Caso de Prueba Relacionado** | ERD v1.1 Sección 3.2 y 4 / RF-02 / Rollback-First |
| **Fecha y Hora de Ejecución** | 11/11/2026 18:54 |
| **Desarrollador / Tester** | Lead Fullstack / Edgar Acevedo |
| **Rama Git** | feature/SF-016-ef-cliente-configuration |

---

### 1. Objetivo de la Prueba
Certificar que el mapeo Fluent API de `ClienteConfiguration`, el registro de configuraciones en `AppDbContext` y la migración `Inicial` crean la tabla `clientes` en PostgreSQL 16 exactamente como define el ERD v1.1 y que la migración es reversible.

### 2. Cambios Implementados
* `Persistencia/Configuraciones/ClienteConfiguration.cs`: tabla `clientes`, PK `id (uuid)`, `curp varchar(18)`, `correo varchar(255)`, `nombre`/`apellidos varchar(100)`, `telefono varchar(10)`, `direccion text`, `created_at timestamptz DEFAULT NOW()`; índices `ix_clientes_curp` (UNIQUE), `ix_clientes_correo` (UNIQUE), `ix_clientes_apellidos_nombre` (`text_pattern_ops`); CHECK `clientes_telefono_check`.
* `Persistencia/AppDbContext.cs`: `ApplyConfigurationsFromAssembly(...)`.
* `Persistencia/Migrations/20261112005406_Inicial.cs` (+ `Designer` y `AppDbContextModelSnapshot`): `Up()` crea tabla, check e índices; `Down()` hace `DropTable("clientes")`.

### 3. Comandos Ejecutados
```bash
dotnet ef migrations add Inicial --project src/SistemaFuneraria.Infrastructure --startup-project src/SistemaFuneraria.Infrastructure --output-dir Persistencia/Migrations
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-016"
dotnet test --no-build
```

### 4. Salida de Terminal (Pase Exitoso)
```text
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'

Correctas! - Con error:     0, Superado:    12, Omitido:     0, Total:    12, Duración: 730 ms - SistemaFuneraria.Tests.dll (net8.0)

    0 Advertencia(s)
    0 Errores
Correctas! - Con error:     0, Superado:   245, Omitido:     0, Total:   245, Duración: 948 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
1. **6/6 Unit:** el modelo de diseño EF Core expone tabla, columnas, tipos, índices, operadores `text_pattern_ops`, check constraint y la migración `*_Inicial`.
2. **6/6 Integration (PostgreSQL 16, BD efímera):** la migración se aplica; `information_schema` y `pg_indexes` coinciden con el ERD; se persiste y lee un `Cliente`; CURP y correo duplicados devuelven `23505`; teléfono inválido devuelve `23514`; `MigrateAsync("0")` elimina la tabla (Rollback-First).
3. **Regresión:** suite completa 245/245 en verde con 0 advertencias. Las bases temporales `sf016_test_*` se eliminan al terminar (0 restantes).
