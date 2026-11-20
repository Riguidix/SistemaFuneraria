# Evidencia Red Test: Ticket SF-095

## 1. Contexto de Ejecución
- **Ticket**: SF-095
- **Descripción**: Hardening de Base de Datos e Índices PostgreSQL (Auditoría Cero N+1, AsNoTracking y Tiempos < 200ms)
- **Fase**: RED (Fallo de aserción por ausencia de índices compuestos de hardening en el IModel de EF Core)
- **Fecha**: 2026-11-20
- **Entorno**: .NET 8 (x64), EF Core 8, PostgreSQL 16

---

## 2. Comando de Ejecución
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~DatabaseHardeningTests" --no-build -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

## 3. Salida de Ejecución (Fase Red)
```text
Serie de pruebas para tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.75]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:01.07]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:01.08]   Starting:    SistemaFuneraria.Tests
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.DatabaseHardeningTests.ConsultaLectura_ConAsNoTracking_DebeDejarCeroEntidadesEnChangeTracker [2 s]
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.DatabaseHardeningTests.MedicionRendimiento_SimulacionYConsultas_DebeEjecutarEnMenosDe200ms [205 ms]
  Con error SistemaFuneraria.Tests.Unit.Infrastructure.DatabaseHardeningTests.ModeloEfCore_DebeRegistrarIndicesCompuestosDeHardening [918 ms]
  Mensaje de error:
   Expected idxLote not to be <null>.
  Seguimiento de la pila:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
   at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
   at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(String message)
   at FluentAssertions.Primitives.ReferenceTypeAssertions`2.NotBeNull(String because, Object[] becauseArgs)
   at SistemaFuneraria.Tests.Unit.Infrastructure.DatabaseHardeningTests.ModeloEfCore_DebeRegistrarIndicesCompuestosDeHardening() in tests/SistemaFuneraria.Tests/Unit/Infrastructure/DatabaseHardeningTests.cs:line 49
  Correctas SistemaFuneraria.Tests.Unit.Infrastructure.DatabaseHardeningTests.ConsultaVentaConFinanciamiento_DebeResolverAtomaSinLazyLoading [474 ms]

No se pudo ejecutar la serie de pruebas.
Pruebas totales: 4
     Correcto: 3
     Incorrecto: 1
 Tiempo total: 6.7348 Segundos
```

---

## 4. Diagnóstico Técnico
La prueba `ModeloEfCore_DebeRegistrarIndicesCompuestosDeHardening` falló con `Expected idxLote not to be <null>` debido a que los índices compuestos `ix_lotes_seccion_estado`, `ix_mensualidades_estado_vencimiento` y `ix_ventas_cliente_fecha` aún no han sido configurados en sus respectivas clases `LoteConfiguration`, `MensualidadConfiguration` y `VentaConfiguration`.
Procedemos con la fase GREEN aplicando las configuraciones y generando la migración reversible `AddHardeningIndices`.
