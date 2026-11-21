# Evidencia TDD: Fase ROJA (RED) — SF-072

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-072 |
| **Título del Ticket** | Semilla CarteraDemoSeed (Contratos de Muestra CTR-2025-0189, CTR-2025-0182, CTR-2024-0118, CTR-2025-0144 Idénticos a Figma) |
| **Milestone** | v0.7.0 |
| **Carril** | Carril 1 (Infrastructure) |
| **Fecha de Ejecución** | 20/11/2026 |
| **Criterio BDD** | Carga idempotente de contratos semilla de Figma en entorno de desarrollo |

---

### 1. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~CarteraDemoSeedTests" -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

---

### 2. Salida de Falla de Pruebas (Fase Roja)
```text
Serie de pruebas para tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.12]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.35]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.36]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.45]     SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_ConServiceProvider_DebePoblarCorrectamente [FAIL]
[xUnit.net 00:00:00.45]       System.NotImplementedException : The method or operation is not implemented.
[xUnit.net 00:00:00.45]       Stack Trace:
[xUnit.net 00:00:00.45]         src/SistemaFuneraria.Infrastructure/Persistencia/CarteraDemoSeed.cs(16,0): at SistemaFuneraria.Infrastructure.Persistencia.CarteraDemoSeed.SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
[xUnit.net 00:00:00.45]         tests/SistemaFuneraria.Tests/Integration/CarteraDemoSeedTests.cs(120,0): at SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_ConServiceProvider_DebePoblarCorrectamente()
[xUnit.net 00:00:00.54]     SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_EjecutadoDosVeces_DebeSerIdempotenteYCrearExactamente4ContratosFigma [FAIL]
[xUnit.net 00:00:00.54]       System.NotImplementedException : The method or operation is not implemented.
[xUnit.net 00:00:00.54]       Stack Trace:
[xUnit.net 00:00:00.54]         src/SistemaFuneraria.Infrastructure/Persistencia/CarteraDemoSeed.cs(24,0): at SistemaFuneraria.Infrastructure.Persistencia.CarteraDemoSeed.SeedAsync(AppDbContext db, CancellationToken cancellationToken)
[xUnit.net 00:00:00.54]         tests/SistemaFuneraria.Tests/Integration/CarteraDemoSeedTests.cs(34,0): at SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_EjecutadoDosVeces_DebeSerIdempotenteYCrearExactamente4ContratosFigma()
[xUnit.net 00:00:00.55]   Finished:    SistemaFuneraria.Tests
  Con error SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_ConServiceProvider_DebePoblarCorrectamente [23 ms]
  Mensaje de error:
   System.NotImplementedException : The method or operation is not implemented.
  Seguimiento de la pila:
     at SistemaFuneraria.Infrastructure.Persistencia.CarteraDemoSeed.SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) in src/SistemaFuneraria.Infrastructure/Persistencia/CarteraDemoSeed.cs:line 16
   at SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_ConServiceProvider_DebePoblarCorrectamente() in tests/SistemaFuneraria.Tests/Integration/CarteraDemoSeedTests.cs:line 120
  Con error SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_EjecutadoDosVeces_DebeSerIdempotenteYCrearExactamente4ContratosFigma [94 ms]
  Mensaje de error:
   System.NotImplementedException : The method or operation is not implemented.
  Seguimiento de la pila:
     at SistemaFuneraria.Infrastructure.Persistencia.CarteraDemoSeed.SeedAsync(AppDbContext db, CancellationToken cancellationToken) in src/SistemaFuneraria.Infrastructure/Persistencia/CarteraDemoSeed.cs:line 24
   at SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_EjecutadoDosVeces_DebeSerIdempotenteYCrearExactamente4ContratosFigma() in tests/SistemaFuneraria.Tests/Integration/CarteraDemoSeedTests.cs:line 34

No se pudo ejecutar la serie de pruebas.
Pruebas totales: 2
     Incorrecto: 2
 Tiempo total: 1.2342 Segundos
```
