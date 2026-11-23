# Evidencia Green Test: Ticket SF-095

## 1. Contexto de Ejecución
- **Ticket**: SF-095
- **Descripción**: Hardening de Base de Datos e Índices PostgreSQL (Auditoría Cero N+1, AsNoTracking y Tiempos < 200ms)
- **Fase**: GREEN (Implementación completada y verificada)
- **Fecha**: 2026-11-20
- **Entorno**: .NET 8 (x64), EF Core 8, PostgreSQL 16

---

## 2. Comando de Ejecución (Suite del Ticket SF-095)
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~DatabaseHardeningTests" --no-build -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

## 3. Salida de Ejecución (Fase Green - Suite SF-095)
```text
Serie de pruebas para tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.01] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.44]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.73]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.74]   Starting:    SistemaFuneraria.Tests
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.DatabaseHardeningTests.ConsultaLectura_ConAsNoTracking_DebeDejarCeroEntidadesEnChangeTracker [1 s]
[xUnit.net 00:00:03.94]   Finished:    SistemaFuneraria.Tests
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.DatabaseHardeningTests.MedicionRendimiento_SimulacionYConsultas_DebeEjecutarEnMenosDe200ms [151 ms]
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.DatabaseHardeningTests.ModeloEfCore_DebeRegistrarIndicesCompuestosDeHardening [737 ms]
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.DatabaseHardeningTests.ConsultaVentaConFinanciamiento_DebeResolverAtomaSinLazyLoading [436 ms]

La serie de pruebas se ejecutó correctamente.
Pruebas totales: 4
     Correcto: 4
 Tiempo total: 5.5837 Segundos
```

---

## 4. Comando de Ejecución (Suite Completa no-integración)
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

## 5. Salida de Ejecución (Suite Completa)
```text
La serie de pruebas se ejecutó correctamente.
Pruebas totales: 442
     Correcto: 442
 Tiempo total: 6.7331 Segundos
```

---

## 6. Verificación de Criterios de Aceptación
1. **Índices Compuestos de Hardening en PostgreSQL**:
   - `LoteConfiguration`: índice compuesto `(seccion, estado)` registrado como `ix_lotes_seccion_estado`.
   - `MensualidadConfiguration`: índice compuesto `(estado, fecha_vencimiento)` registrado como `ix_mensualidades_estado_vencimiento`.
   - `VentaConfiguration`: índice compuesto `(cliente_id, fecha_venta)` registrado como `ix_ventas_cliente_fecha`.
2. **Migración Reversible Rollback-First**:
   - Migración `AddHardeningIndices` (`src/SistemaFuneraria.Infrastructure/Persistencia/Migrations/20261120204033_AddHardeningIndices.cs`):
     - `Up()` crea los 3 índices en sus respectivas tablas.
     - `Down()` elimina limpiamente los 3 índices creados.
3. **Auditoría AsNoTracking y Cero N+1**:
   - Verificado que las consultas de lectura con `AsNoTracking()` dejan 0 entidades rastreadas en el `ChangeTracker`.
   - Verificado que consultas agregadas resuelven de forma atómica mediante proyección/include sin lazy loading ni consultas N+1.
4. **Tiempos de Respuesta Sub-200ms (RNF-01)**:
   - Verificado que las consultas de catálogo y amortizaciones ejecutan holgadamente por debajo del límite de 200 ms.
