# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-016 |
| **Caso de Prueba Relacionado** | ERD v1.1 Sección 3.2 y 4 / RF-02 / Rollback-First |
| **Fecha y Hora de Ejecución** | 11/11/2026 18:47 |
| **Desarrollador / Tester** | Lead Fullstack / Edgar Acevedo |
| **Rama Git** | feature/SF-016-ef-cliente-configuration |

---

### 1. Objetivo de la Prueba
Certificar el estado **ROJO (RED)** previo a la implementación Fluent API de `ClienteConfiguration`, al registro de configuraciones en `AppDbContext.OnModelCreating` y a la generación de la migración inicial `*_Inicial` para PostgreSQL 16.

La batería de pruebas valida dos frentes complementarios bajo TDD estricto:
1. **Pruebas Unitarias de Metadatos EF Core (`Unit/Infrastructure/ClienteConfigurationTests.cs`):** Verifican sin abrir conexión de red el mapeo de tabla `clientes`, clave primaria `id (uuid)`, tipos `VARCHAR(18/255/100/10)`, `TEXT`, `TIMESTAMPTZ`, índices `ix_clientes_curp`, `ix_clientes_correo`, `ix_clientes_apellidos_nombre` (`text_pattern_ops`), check constraint `clientes_telefono_check` y existencia de la migración `*_Inicial`.
2. **Pruebas de Integración contra PostgreSQL 16 (`Integration/ClientePersistenciaTests.cs`):** Ejecutan sobre una base de datos efímera aislada en PostgreSQL 16 la migración EF Core, la verificación directa de catálogo (`information_schema.columns` y `pg_indexes`), persistencia real con `Cliente.Crear`, rechazo de CURP y correo duplicados (`SQLSTATE 23505`), validación del `CHECK constraint` (`SQLSTATE 23514`) y reversibilidad completa mediante rollback (`IMigrator.MigrateAsync("0")`).

---

### 2. Escenarios Cubiertos en Fase RED
* **Unit (6 pruebas):**
  - `ModeloEfCore_DebeMapearTablaClientesConClavePrimariaUuid`
  - `ModeloEfCore_DebeMapearColumnasConTiposNpgsqlYLongitudesExactasDelErd`
  - `ModeloEfCore_DebeConfigurarIndicesUnicosYBusquedaPorApellidosNombre`
  - `ModeloEfCore_DebeIncluirCheckConstraintDeTelefono10Digitos`
  - `ClienteConfiguration_AlAplicarseDirectamente_DebeEjecutarseSinExcepcion` (`NotImplementedException`)
  - `AppDbContext_DebeContenerMigracionInicialCompilada`
* **Integration (6 pruebas en PostgreSQL 16):**
  - `MigracionInicial_DebeCrearTablaClientesConEsquemaExactoEIndicesEnPostgres`
  - `ClientesDbSet_DebePersistirYConsultarClienteValidoCorrectamente`
  - `ClientesDbSet_ConCurpDuplicado_DebeRechazarPorIndiceUnico`
  - `ClientesDbSet_ConCorreoDuplicado_DebeRechazarPorIndiceUnico`
  - `BaseDatos_DebeRechazarTelefonoInvalidoMedianteCheckConstraint`
  - `MigracionInicial_DebeSerEstrictamenteReversibleConRollbackClean`

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-016"
```

---

### 4. Salida de Terminal (Falla Esperada — 0 Superadas / 12 Con Error)
```text
  Determinando los proyectos que se van a restaurar...
  Todos los proyectos están actualizados para la restauración.
  SistemaFuneraria.Core -> <REPO_ROOT>/src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> <REPO_ROOT>/src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> <REPO_ROOT>/src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> <REPO_ROOT>/src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
  SistemaFuneraria.Tests -> <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll
Serie de pruebas para <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.35]     SistemaFuneraria.Tests.Unit.Infrastructure.ClienteConfigurationTests.ModeloEfCore_DebeMapearTablaClientesConClavePrimariaUuid [FAIL]
[xUnit.net 00:00:00.36]     SistemaFuneraria.Tests.Unit.Infrastructure.ClienteConfigurationTests.ModeloEfCore_DebeConfigurarIndicesUnicosYBusquedaPorApellidosNombre [FAIL]
[xUnit.net 00:00:00.36]     SistemaFuneraria.Tests.Unit.Infrastructure.ClienteConfigurationTests.ModeloEfCore_DebeIncluirCheckConstraintDeTelefono10Digitos [FAIL]
[xUnit.net 00:00:00.37]     SistemaFuneraria.Tests.Unit.Infrastructure.ClienteConfigurationTests.AppDbContext_DebeContenerMigracionInicialCompilada [FAIL]
[xUnit.net 00:00:00.37]     SistemaFuneraria.Tests.Unit.Infrastructure.ClienteConfigurationTests.ModeloEfCore_DebeMapearColumnasConTiposNpgsqlYLongitudesExactasDelErd [FAIL]
[xUnit.net 00:00:00.37]     SistemaFuneraria.Tests.Unit.Infrastructure.ClienteConfigurationTests.ClienteConfiguration_AlAplicarseDirectamente_DebeEjecutarseSinExcepcion [FAIL]
[xUnit.net 00:00:00.43]     SistemaFuneraria.Tests.Integration.ClientePersistenciaTests.MigracionInicial_DebeCrearTablaClientesConEsquemaExactoEIndicesEnPostgres [FAIL]
[xUnit.net 00:00:00.59]     SistemaFuneraria.Tests.Integration.ClientePersistenciaTests.ClientesDbSet_ConCurpDuplicado_DebeRechazarPorIndiceUnico [FAIL]
[xUnit.net 00:00:00.65]     SistemaFuneraria.Tests.Integration.ClientePersistenciaTests.ClientesDbSet_DebePersistirYConsultarClienteValidoCorrectamente [FAIL]

  Con error SistemaFuneraria.Tests.Unit.Infrastructure.ClienteConfigurationTests.ModeloEfCore_DebeMapearTablaClientesConClavePrimariaUuid [250 ms]
  Mensaje de error:
   Expected entityType.GetTableName() to be "clientes" because el ERD v1.1 Sección 3.2 define el nombre de tabla en minúsculas 'clientes', but "Clientes" differs near "Cli" (index 0).

  Con error SistemaFuneraria.Tests.Unit.Infrastructure.ClienteConfigurationTests.ClienteConfiguration_AlAplicarseDirectamente_DebeEjecutarseSinExcepcion [1 ms]
  Mensaje de error:
   Did not expect any exception because ClienteConfiguration debe implementar el mapeo Fluent API completo de Cliente, but found System.NotImplementedException: SF-016: Implementar mapeo Fluent API para la tabla 'clientes' según ERD v1.1 Sección 3.2.

  Con error SistemaFuneraria.Tests.Integration.ClientePersistenciaTests.MigracionInicial_DebeCrearTablaClientesConEsquemaExactoEIndicesEnPostgres [220 ms]
  Mensaje de error:
   Expected aplicadas {empty} to have an item matching m.EndsWith("_Inicial", Ordinal) because la migración inicial '*_Inicial' debe haberse aplicado contra PostgreSQL 16.

  Con error SistemaFuneraria.Tests.Integration.ClientePersistenciaTests.ClientesDbSet_ConCurpDuplicado_DebeRechazarPorIndiceUnico [112 ms]
  Mensaje de error:
   Did not expect any exception because el primer cliente válido debe persistirse en PostgreSQL, but found Microsoft.EntityFrameworkCore.DbUpdateException -> Npgsql.PostgresException (0x80004005): 42P01: relation "Clientes" does not exist

  Con error SistemaFuneraria.Tests.Integration.ClientePersistenciaTests.MigracionInicial_DebeSerEstrictamenteReversibleConRollbackClean [35 ms]
  Mensaje de error:
   Expected (ExisteTablaClientesAsync(conn)) to be true because la migración Inicial debe crear la tabla clientes, but found False.

Con error! - Con error:    12, Superado:     0, Omitido:     0, Total:    12, Duración: 500 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
1. **Contratos compilando limpiamente:** La solución compila con 0 errores y 0 advertencias (`TreatWarningsAsErrors = true`), y los 233 tests previos del repositorio (`Ticket!=SF-016`) siguen aprobando en verde.
2. **Fase RED verificada al 100% (12/12 en falla):**
   - `ClienteConfiguration.Configure` lanza `NotImplementedException` al aplicarse.
   - `AppDbContext.OnModelCreating` aún no aplica `ApplyConfigurationsFromAssembly(...)`, exponiendo el modelo por convención (`Clientes` sin longitudes `VARCHAR`, sin índices ni check constraint).
   - `Persistencia/Migrations/` no contiene aún la migración inicial `*_Inicial`, provocando que las pruebas de integración contra PostgreSQL 16 fallen al validar el esquema y la reversibilidad.
3. El ticket queda formalmente listo para que el equipo desarrolle la fase **VERDE (GREEN)**.
