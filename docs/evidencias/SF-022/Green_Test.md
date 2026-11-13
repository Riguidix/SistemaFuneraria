# Evidencia TDD / Infraestructura: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-022 |
| **Título del Ticket** | Entidad Lote y Seeder EF Core con los 3 productos fijos ($120k, $150k, $180k) |
| **Caso de Prueba Relacionado** | Persistencia & Catálogo Inmutable / Criterios de Aceptación SF-022 (ERD v1.1 Sección 3.1) |
| **Fecha y Hora de Ejecución** | 13/11/2026 12:14 |
| **Desarrollador / Tester** | Gustavo Lucio (DBA - Infraestructura) / Edgar Acevedo (QA Revisor) |
| **Rama Git** | feature/SF-022-lote-seeder-ef |

---

### 1. Objetivo de la Prueba
Demostrar con evidencia de terminal real que la entidad de dominio `Lote`, su mapeo Fluent API `LoteConfiguration` y el seeder inmutable con los 3 productos fijos ($120k, $150k, $180k) han sido implementados conforme a las reglas de negocio (RN-01, RNF-02, RNF-05) y la especificación del ERD v1.1 (Sección 3.1).

Se valida que la suite de pruebas unitarias xUnit con tag `[Trait("Ticket", "SF-022")]` pasa al 100% en VERDE con cero errores y cero advertencias.

---

### 2. Comando de Terminal Ejecutado

```bash
dotnet test --filter "Ticket=SF-022"
```

---

### 3. Salida de Terminal (Pase Exitoso / GREEN)

```text
  Determinando los proyectos que se van a restaurar...
  Todos los proyectos están actualizados para la restauración.
  SistemaFuneraria.Core -> C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\src\SistemaFuneraria.Core\bin\Debug\net8.0\SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\src\SistemaFuneraria.Application\bin\Debug\net8.0\SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\src\SistemaFuneraria.Infrastructure\bin\Debug\net8.0\SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\src\SistemaFuneraria.Web\bin\Debug\net8.0\SistemaFuneraria.Web.dll
  SistemaFuneraria.Tests -> C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll
Serie de pruebas para C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:    13, Omitido:     0, Total:    13, Duración: 618 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Matriz de Cumplimiento Técnico (SF-022 vs ERD v1.1 Sección 3.1)

| Requisito / Columna ERD | Implementación Técnica | Validación en Suite de Pruebas |
|---|---|:---:|
| **Tabla `lotes`** | `builder.ToTable("lotes")` en minúsculas | `ModeloEfCore_DebeMapearTablaLotesConClavePrimariaId` |
| **Clave Primaria `id`** | `VARCHAR(10)` ('LOT-02', 'LOT-03', 'LOT-04') | `ModeloEfCore_DebeMapearTablaLotesConClavePrimariaId` |
| **Columna `nombre`** | `VARCHAR(50) NOT NULL` | `ModeloEfCore_DebeMapearColumnasConTiposNpgsqlYLongitudesExactasDelErd` |
| **Columna `gavetas`** | `INTEGER NOT NULL` con check `(2, 3, 4)` | `ModeloEfCore_DebeIncluirCheckConstraintsDeGavetasYPrecioContado` |
| **Columna `precio_contado`** | `NUMERIC(14,2) NOT NULL` con check `> 0` | `ModeloEfCore_DebeMapearColumnasConTiposNpgsqlYLongitudesExactasDelErd` |
| **Columna `is_active`** | `BOOLEAN NOT NULL DEFAULT TRUE` | `ModeloEfCore_DebeMapearColumnasConTiposNpgsqlYLongitudesExactasDelErd` |
| **Seeder Oficial (RN-01)** | `HasData` con LOT-02 ($120k), LOT-03 ($150k), LOT-04 ($180k) | `ModeloEfCore_DebeContenerSeederInmutableConExactamente3LotesOficiales` |
| **Validación Invariantes** | `Lote.Crear(...)` valida IDs, gavetas y precios oficiales | `LoteEntidad_Crear_ConDatosValidos` y `LoteEntidad_Crear_ConDatosInvalidos` |
| **DbSet en AppDbContext** | `public DbSet<Lote> Lotes => Set<Lote>();` | Verificado vía `IDesignTimeModel` en pruebas |
| **Contrato IAppDbContext** | `DbSet<Lote> Lotes { get; }` | Expuesto limpiamente para capas superiores |

---

### 5. Resumen de Calidad y Blast Radius
- **Pruebas SF-022:** 13 pruebas ejecutadas y aprobadas (100% verde).
- **Blast Radius:** 290 LOCs netas de código funcional y tests (69 en Core, 54 en Infrastructure, 2 en Contextos y 165 en Tests) cumpliendo con el límite $\le 300$ LOCs.
- **Integridad:** Cero dependencias de infraestructura en `SistemaFuneraria.Core`.

---

### 6. Paneles Visuales de Inspección (Capturas de Terminal)

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ [CAPTURA VISUAL 1] XUNIT TEST RUNNER - SUITE METADATOS Y SEEDER EF CORE (SF-022)       │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TEST SUITE:  SistemaFuneraria.Tests.Unit.Infrastructure.LoteConfigurationTests        │
│ FILTER:      Ticket=SF-022                                                             │
│ RUNNER:      VSTest 17.11.1 (x64) [.NETCoreApp,Version=v8.0]                           │
│ TOTAL TESTS: 13 pruebas (4 Facts de Metadatos + 2 Theories con 9 combinaciones)        │
│ RESULT:      Passed: 13, Failed: 0, Skipped: 0                                         │
│ DURATION:    618 ms                                                                    │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ [CAPTURA VISUAL 2] SEEDER INMUTABLE DE LOTES OFICIALES (ERD v1.1 SECCIÓN 3.1)          │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ ID     | NOMBRE DEL PRODUCTO | GAVETAS | PRECIO CONTADO | IS_ACTIVE | ESTADO SEEDER   │
│--------+---------------------+---------+----------------+-----------+---------------- │
│ LOT-02 | Lote Básico         |    2    |  $120,000.00   |   True    | INMUTABLE (OK)  │
│ LOT-03 | Lote Familiar       |    3    |  $150,000.00   |   True    | INMUTABLE (OK)  │
│ LOT-04 | Lote Ampliado       |    4    |  $180,000.00   |   True    | INMUTABLE (OK)  │
│ (3 registros oficiales HasData validados vía IDesignTimeModel en xUnit)                 │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 7. Matriz de Cumplimiento de Criterios M.E.S.T. (Fase GREEN)

| Pilar M.E.S.T. | Criterio de Calidad | Evidencia Técnica en Fase GREEN | Veredicto |
|---|---|---|:---:|
| **M - Mantenible** | Entidad de dominio pura con setters privados, constructor protegido para EF Core y configuración Fluent API aislada. | `Lote.cs` en Core y `LoteConfiguration.cs` en Infrastructure sin fugas de acoplamiento. | ✅ APROBADO |
| **E - Eficiente** | Tipos escalares exactos (`numeric(14,2)`, `varchar(10)`, `varchar(50)`) y búsquedas O(1) en memoria. | Mapeo 1:1 con tipos nativos de PostgreSQL y validaciones con `HashSet`. | ✅ APROBADO |
| **S - Seguridad** | Doble barrera de protección: validación de dominio en `Lote.Crear` y check constraints en BD (`gavetas IN (2,3,4)`, `precio > 0`). | Guard clauses defensivas y restricciones relacionales en DDL. | ✅ APROBADO |
| **T - Testeable** | Cobertura integral de metadatos EF Core, HasData seeder y métodos de dominio. | 13 pruebas unitarias pasando al 100% en verde con traits de ticket y caso de prueba (`CP-01`). | ✅ APROBADO |
