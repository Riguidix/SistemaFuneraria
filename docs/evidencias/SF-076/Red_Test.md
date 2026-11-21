# Evidencia TDD: Fase ROJA (RED) — SF-076

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-076 |
| **Título del Ticket** | EF Core PagoConfiguration, Secuencia seq_pagos_folio y Migración AddPagosCaja |
| **Milestone** | v0.8.0 |
| **Carril** | Carril 1 (Infrastructure) |
| **Fecha de Ejecución** | 20/11/2026 |
| **Criterio BDD** | Generación secuencial de folios PAG-2026-XXXXXX, mapeo de tabla pagos y unicidad de idempotencia |

---

### 1. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~PagoConfigurationTests" -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

---

### 2. Salida de Falla de Pruebas (Fase Roja)
```text
Serie de pruebas para tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.73]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:01.05]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:01.06]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:01.63]     SistemaFuneraria.Tests.Unit.Infrastructure.PagoConfigurationTests.ObtenerSiguienteFolioPagoAsync_DebeGenerarFoliosSecuencialesConFormatoOficial [FAIL]
[xUnit.net 00:00:01.63]       System.NotImplementedException : The method or operation is not implemented.
[xUnit.net 00:00:01.63]       Stack Trace:
[xUnit.net 00:00:01.63]         src/SistemaFuneraria.Infrastructure/Persistencia/AppDbContext.cs(26,0): at SistemaFuneraria.Infrastructure.Persistencia.AppDbContext.ObtenerSiguienteFolioPagoAsync(Int32 anio, CancellationToken cancellationToken)
[xUnit.net 00:00:01.64]         tests/SistemaFuneraria.Tests/Unit/Infrastructure/PagoConfigurationTests.cs(112,0): at SistemaFuneraria.Tests.Unit.Infrastructure.PagoConfigurationTests.ObtenerSiguienteFolioPagoAsync_DebeGenerarFoliosSecuencialesConFormatoOficial()
[xUnit.net 00:00:02.46]     SistemaFuneraria.Tests.Unit.Infrastructure.PagoConfigurationTests.ModeloEfCore_DebeRegistrarDbSetPagosYSecuenciaPagosFolio [FAIL]
[xUnit.net 00:00:02.46]       System.InvalidOperationException : The property 'Pago.MontoCapital' could not be mapped because it is of type 'Dinero', which is not a supported primitive type or a valid entity type. Either explicitly map this property, or ignore it using the '[NotMapped]' attribute or by using 'EntityTypeBuilder.Ignore' in 'OnModelCreating'.
[xUnit.net 00:00:02.46]       Stack Trace:
[xUnit.net 00:00:02.46]            at Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.ValidatePropertyMapping(IModel model, IDiagnosticsLogger`1 logger)
[xUnit.net 00:00:02.46]            at Microsoft.EntityFrameworkCore.Infrastructure.ModelValidator.Validate(IModel model, IDiagnosticsLogger`1 logger)
[xUnit.net 00:00:02.46]            at tests/SistemaFuneraria.Tests/Unit/Infrastructure/PagoConfigurationTests.cs:line 45
[xUnit.net 00:00:02.47]     SistemaFuneraria.Tests.Unit.Infrastructure.PagoConfigurationTests.ModeloEfCore_PagoConfiguration_DebeMapearTablaColumnasCheckConstraintsYFks [FAIL]
[xUnit.net 00:00:02.47]       System.InvalidOperationException : The property 'Pago.MontoCapital' could not be mapped because it is of type 'Dinero', which is not a supported primitive type or a valid entity type.
[xUnit.net 00:00:02.47]       Stack Trace:
[xUnit.net 00:00:02.47]            at tests/SistemaFuneraria.Tests/Unit/Infrastructure/PagoConfigurationTests.cs:line 61

No se pudo ejecutar la serie de pruebas.
Pruebas totales: 3
     Incorrecto: 3
 Tiempo total: 4.0752 Segundos
```
