# Evidencia Green Test: Ticket SF-076

## 1. Contexto de Ejecución
- **Ticket**: SF-076
- **Descripción**: EF Core `PagoConfiguration`, Secuencia `seq_pagos_folio` y Migración `AddPagosCaja`
- **Fase**: GREEN (Implementación completada y verificada)
- **Fecha**: 2026-11-20
- **Entorno**: .NET 8 (x64), PostgreSQL 16 / InMemory provider para suite unitaria

---

## 2. Comando de Ejecución (Suite del Ticket SF-076)
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~PagoConfigurationTests" --no-build -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

## 3. Salida de Ejecución (Fase Green - Suite SF-076)
```text
Serie de pruebas para tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.30]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.56]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.57]   Starting:    SistemaFuneraria.Tests
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.PagoConfigurationTests.ObtenerSiguienteFolioPagoAsync_DebeGenerarFoliosSecuencialesConFormatoOficial [1 s]
[xUnit.net 00:00:02.78]   Finished:    SistemaFuneraria.Tests
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.PagoConfigurationTests.ModeloEfCore_DebeRegistrarDbSetPagosYSecuenciaPagosFolio [612 ms]
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.PagoConfigurationTests.ModeloEfCore_PagoConfiguration_DebeMapearTablaColumnasCheckConstraintsYFks [43 ms]

La serie de pruebas se ejecutó correctamente.
Pruebas totales: 3
     Correcto: 3
 Tiempo total: 4.3191 Segundos
```

---

## 4. Comando de Ejecución (Suite Completa no-integración)
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

## 5. Salida de Ejecución (Suite Completa)
```text
La serie de pruebas se ejecutó correctamente.
Pruebas totales: 418
     Correcto: 418
 Tiempo total: 7.9088 Segundos
```

---

## 6. Verificación de Criterios de Aceptación
1. **Entidad y Enums de Dominio**:
   - `TipoPago` y `MetodoPago` creados en `src/SistemaFuneraria.Core/Enums/`.
   - `Pago` creado en `src/SistemaFuneraria.Core/Entidades/Pago.cs` con validación rigurosa de folios (`PAG-YYYY-XXXXXX`), idempotencia, estados y desglose de montos.
2. **Configuración Fluent API (`PagoConfiguration`)**:
   - Mapeo a tabla `pagos`.
   - Conversión de `Dinero` a `numeric(18,2)` para MontoTotal, MontoCapital, MontoInteres, SaldoAnterior y SaldoPosterior.
   - Conversión string para los enums `TipoPago` y `MetodoPago`.
   - Check constraints para tipos, métodos, montos y saldos válidos.
   - Índices únicos `ix_pagos_folio` y `ix_pagos_clave_idempotencia`.
   - Claves foráneas restrictivas (`DeleteBehavior.Restrict`).
3. **AppDbContext y Secuencia**:
   - Registro de `DbSet<Pago> Pagos`.
   - Registro de secuencia `seq_pagos_folio` con `HasSequence<long>("seq_pagos_folio").StartsAt(1).IncrementsBy(1)`.
   - Implementación de `ObtenerSiguienteFolioPagoAsync` soportando PostgreSQL vía `SELECT nextval('seq_pagos_folio');` y fallback atómico in-memory para tests.
4. **Migración Reversible (`AddPagosCaja`)**:
   - Migración generada limpiamente con creación de secuencia y tabla en `Up()`, y reversión completa en `Down()`.
