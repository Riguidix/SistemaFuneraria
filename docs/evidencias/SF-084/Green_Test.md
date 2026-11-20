# Evidencia Green Test: Ticket SF-084

## 1. Contexto de Ejecución
- **Ticket**: SF-084
- **Descripción**: Dominio `CalculadorAbonoCapital` (100% a Capital, Plazo Fijo, Oráculo `CP-07`)
- **Fase**: GREEN (Implementación completada y verificada)
- **Fecha**: 2026-11-20
- **Entorno**: .NET 8 (x64), C# 12, Clean Architecture (Core puro con 0 dependencias externas)

---

## 2. Comando de Ejecución (Suite del Ticket SF-084)
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~CalculadorAbonoCapitalTests" --no-build -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

## 3. Salida de Ejecución (Fase Green - Suite SF-084)
```text
Serie de pruebas para tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.88]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:01.24]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:01.26]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:01.44]   Finished:    SistemaFuneraria.Tests
  Correctas SistemaFuneraria.Tests.Unit.Core.CalculadorAbonoCapitalTests.Recalcular_ConFinanciamientoEnMora_DebeLanzarReglaNegocioException [61 ms]
  Correctas SistemaFuneraria.Tests.Unit.Core.CalculadorAbonoCapitalTests.Recalcular_ConMontoAbonoInvalido_DebeLanzarReglaNegocioException(abonoInvalido: -500) [1 ms]
  Correctas SistemaFuneraria.Tests.Unit.Core.CalculadorAbonoCapitalTests.Recalcular_ConMontoAbonoInvalido_DebeLanzarReglaNegocioException(abonoInvalido: 0) [1 ms]
  Correctas SistemaFuneraria.Tests.Unit.Core.CalculadorAbonoCapitalTests.Recalcular_CasoCP07_ConSaldo74078_DebeDescontarCapitalYFijarCuotaEn752351 [4 ms]
  Correctas SistemaFuneraria.Tests.Unit.Core.CalculadorAbonoCapitalTests.Recalcular_ConMensualidadesVencidas_DebeLanzarReglaNegocioException [2 ms]
  Correctas SistemaFuneraria.Tests.Unit.Core.CalculadorAbonoCapitalTests.Recalcular_ConMontoAbonoMayorOIgualASaldoInsoluto_DebeLanzarReglaNegocioException(saldo: 50000, abono: 55000) [1 ms]
  Correctas SistemaFuneraria.Tests.Unit.Core.CalculadorAbonoCapitalTests.Recalcular_ConMontoAbonoMayorOIgualASaldoInsoluto_DebeLanzarReglaNegocioException(saldo: 50000, abono: 50000) [1 ms]
  Correctas SistemaFuneraria.Tests.Unit.Core.CalculadorAbonoCapitalTests.Recalcular_CasoCP07_DesdeContratoFinanciado_DebeAlinearConOraculoCsv [3 ms]

La serie de pruebas se ejecutó correctamente.
Pruebas totales: 8
     Correcto: 8
 Tiempo total: 3.1933 Segundos
```

---

## 4. Comando de Ejecución (Suite Completa no-integración)
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

## 5. Salida de Ejecución (Suite Completa)
```text
La serie de pruebas se ejecutó correctamente.
Pruebas totales: 426
     Correcto: 426
 Tiempo total: 6.2602 Segundos
```

---

## 6. Verificación de Criterios de Aceptación
1. **Pureza Arquitectónica**:
   - `src/SistemaFuneraria.Core/Calculos/CalculadorAbonoCapital.cs` y `ResultadoAbonoCapital.cs` poseen 0 dependencias externas y 0 acoplamiento a EF Core.
2. **Invariantes y Reglas de Dominio**:
   - Validación estricta `MontoAbono > 0` con `ReglaNegocioException`.
   - Validación `MontoAbono < SaldoInsoluto`; si es `>=` se lanza `ReglaNegocioException` requiriendo liquidación anticipada.
   - Prohibición de abono extraordinario ante mensualidades vencidas (`RF-ABO-01`), lanzando `ReglaNegocioException`.
3. **Descuento íntegro a Capital (100%)**:
   - El abono reduce el saldo deudor directamente: `NuevoSaldo = SaldoInsoluto - MontoAbono`.
4. **Oráculo CP-07 Certificado**:
   - Certificado con SaldoInsoluto de $74,078.00 y abono de $10,000.00 dejando saldo en $64,078.00 y cuotas en $7,523.51 (con ajuste al centavo en cuota 12 a $0.00 de saldo final).
   - Certificado también contra el contrato financiado oficial CP-06 ($96,000 inicial, $73,410.99 tras 3 pagos, nuevo saldo $63,410.99 y cuotas de $7,523.67 alineadas a `docs/anexos/tabla-cp07-abono-10000-tras-periodo-3.csv`).
