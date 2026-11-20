# Evidencia Red Test: Ticket SF-086

## 1. Contexto de Ejecución
- **Ticket**: SF-086
- **Descripción**: Dominio `EvaluadorVencimientos` con `TimeProvider` ($0.00 recargo moratorio, `CP-09`)
- **Fase**: RED (Fallo de compilación por ausencia de `EvaluadorVencimientos`)
- **Fecha**: 2026-11-20
- **Entorno**: .NET 8 (x64), C# 12, Clean Architecture (Core puro)

---

## 2. Comando de Ejecución
```bash
dotnet build -m:1 --disable-build-servers
```

## 3. Salida de Ejecución (Fase Red)
```text
Determinando los proyectos que se van a restaurar...
Todos los proyectos están actualizados para la restauración.
SistemaFuneraria.Core -> src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
SistemaFuneraria.Application -> src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
SistemaFuneraria.Infrastructure -> src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
SistemaFuneraria.Web -> src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
SistemaFuneraria.Api -> src/SistemaFuneraria.Api/bin/Debug/net8.0/SistemaFuneraria.Api.dll
tests/SistemaFuneraria.Tests/Unit/Core/EvaluadorVencimientosTests.cs(54,30): error CS0103: El nombre 'EvaluadorVencimientos' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/EvaluadorVencimientosTests.cs(83,30): error CS0103: El nombre 'EvaluadorVencimientos' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/EvaluadorVencimientosTests.cs(114,30): error CS0103: El nombre 'EvaluadorVencimientos' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/EvaluadorVencimientosTests.cs(141,30): error CS0103: El nombre 'EvaluadorVencimientos' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/EvaluadorVencimientosTests.cs(151,31): error CS0103: El nombre 'EvaluadorVencimientos' no existe en el contexto actual

ERROR al compilar.
    0 Advertencia(s)
    5 Errores
```

---

## 4. Diagnóstico Técnico
La suite de pruebas `tests/SistemaFuneraria.Tests/Unit/Core/EvaluadorVencimientosTests.cs` no compila porque el componente `EvaluadorVencimientos` aún no ha sido implementado en `src/SistemaFuneraria.Core/Calculos/`.
Procedemos con la fase GREEN implementando el evaluador con `System.TimeProvider` nativo y política estricta de cero recargos moratorios ($0.00).
