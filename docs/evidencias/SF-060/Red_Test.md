# Evidencia Fase RED — SF-060

**Ticket:** [SF-060] `IVentaService.RegistrarVentaContadoAsync` (Transacción ACID: Venta + Lote Vendido + Pago Único CP-02) [#126](https://github.com/Riguidix/SistemaFuneraria/issues/126)  
**Fecha:** 2026-11-22  
**Autor:** Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`)  
**Metodología:** TDD Estricto ("Cero Fantasía") — Fase RED  

---

## 1. Comando Ejecutado

```powershell
$env:PATH = "C:\Users\Billy\AppData\Local\Microsoft\dotnet;$env:PATH"; $env:DOTNET_ROOT = "C:\Users\Billy\AppData\Local\Microsoft\dotnet"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~VentaContadoServiceTests" -v normal
```

---

## 2. Salida de Terminal (Falla Comprobada)

```text
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:02.76]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:02.88]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:02.89]   Starting:    SistemaFuneraria.Tests
  Failed SF-060: Venta de contado sobre lote en Mantenimiento debe retornar conflicto [< 1 ms]
  Error Message:
   System.NotImplementedException : SF-060: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Ventas.VentaService.RegistrarVentaContadoAsync(RegistrarVentaContadoCommand command, CancellationToken cancellationToken) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\Servicios\Ventas\VentaService.cs:line 20
     at SistemaFuneraria.Tests.Unit.Application.VentaContadoServiceTests.RegistrarVentaContadoAsync_ConLoteMantenimiento_DebeRetornarConflicto() in C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Application\VentaContadoServiceTests.cs:line 176

  Failed SF-060: Venta de contado con cliente inexistente debe retornar No Encontrado [17 ms]
  Error Message:
   System.NotImplementedException : SF-060: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Ventas.VentaService.RegistrarVentaContadoAsync(RegistrarVentaContadoCommand command, CancellationToken cancellationToken) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\Servicios\Ventas\VentaService.cs:line 20
     at SistemaFuneraria.Tests.Unit.Application.VentaContadoServiceTests.RegistrarVentaContadoAsync_ConClienteInexistente_DebeRetornarNoEncontrado() in C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Application\VentaContadoServiceTests.cs:line 191

  Failed SF-060: Venta de contado sobre lote Reservado debe permitir venta y marcar como Vendido [9 ms]
  Error Message:
   System.NotImplementedException : SF-060: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Ventas.VentaService.RegistrarVentaContadoAsync(RegistrarVentaContadoCommand command, CancellationToken cancellationToken) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\Servicios\Ventas\VentaService.cs:line 20
     at SistemaFuneraria.Tests.Unit.Application.VentaContadoServiceTests.RegistrarVentaContadoAsync_ConLoteReservado_DebePermitirVentaYPasarAVendido() in C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Application\VentaContadoServiceTests.cs:line 139

  Failed SF-060 (BDD / CP-02): Venta de contado debe crear Venta Liquidada, lote Vendido y Pago único [2 ms]
  Error Message:
   System.NotImplementedException : SF-060: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Ventas.VentaService.RegistrarVentaContadoAsync(RegistrarVentaContadoCommand command, CancellationToken cancellationToken) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\Servicios\Ventas\VentaService.cs:line 20
     at SistemaFuneraria.Tests.Unit.Application.VentaContadoServiceTests.RegistrarVentaContadoAsync_ConDatosValidos_DebeCrearVentaLiquidadaLoteVendidoYPagoUnico() in C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Application\VentaContadoServiceTests.cs:line 97

  Failed SF-060: Venta de contado con lote inexistente debe retornar No Encontrado [< 1 ms]
  Error Message:
   System.NotImplementedException : SF-060: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Ventas.VentaService.RegistrarVentaContadoAsync(RegistrarVentaContadoCommand command, CancellationToken cancellationToken) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\Servicios\Ventas\VentaService.cs:line 20
     at SistemaFuneraria.Tests.Unit.Application.VentaContadoServiceTests.RegistrarVentaContadoAsync_ConLoteInexistente_DebeRetornarNoEncontrado() in C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Application\VentaContadoServiceTests.cs:line 206

  Failed SF-060: Venta de contado sobre lote ya Vendido debe retornar conflicto y no duplicar venta [1 ms]
  Error Message:
   System.NotImplementedException : SF-060: Fase RED.
  Stack Trace:
     at SistemaFuneraria.Application.Servicios.Ventas.VentaService.RegistrarVentaContadoAsync(RegistrarVentaContadoCommand command, CancellationToken cancellationToken) in C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\Servicios\Ventas\VentaService.cs:line 20
     at SistemaFuneraria.Tests.Unit.Application.VentaContadoServiceTests.RegistrarVentaContadoAsync_ConLoteYaVendido_DebeRetornarConflictoYNoModificarEstado() in C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Application\VentaContadoServiceTests.cs:line 156

Test Run Failed.
Total tests: 6
     Failed: 6
 Total time: 5.0139 Seconds
```

---

## 3. Conclusión Fase RED
Se valida que el contrato `IVentaService` y las pruebas unitarias fallan de manera controlada con `NotImplementedException`, garantizando la ausencia de falsos positivos antes de proceder a la implementación en la Fase GREEN.
