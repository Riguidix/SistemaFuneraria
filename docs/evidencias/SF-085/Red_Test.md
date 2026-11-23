# Evidencia Red Test: Ticket SF-085

## 1. Contexto de Ejecución
- **Ticket**: SF-085
- **Descripción**: Dominio `CalculadorLiquidacion` (Descuento 10% Intereses Futuros, Cero Penalización, Oráculo `CP-08`)
- **Fase**: RED (Fallo de compilación por ausencia de `CalculadorLiquidacion` y `ResultadoLiquidacion`)
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
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorLiquidacionTests.cs(24,25): error CS0103: El nombre 'CalculadorLiquidacion' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorLiquidacionTests.cs(58,25): error CS0103: El nombre 'CalculadorLiquidacion' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorLiquidacionTests.cs(92,9): error CS0103: El nombre 'CalculadorLiquidacion' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorLiquidacionTests.cs(118,9): error CS0103: El nombre 'CalculadorLiquidacion' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorLiquidacionTests.cs(120,31): error CS0103: El nombre 'CalculadorLiquidacion' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorLiquidacionTests.cs(131,31): error CS0103: El nombre 'CalculadorLiquidacion' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorLiquidacionTests.cs(145,31): error CS0103: El nombre 'CalculadorLiquidacion' no existe en el contexto actual

ERROR al compilar.
    0 Advertencia(s)
    7 Errores
```

---

## 4. Diagnóstico Técnico
La suite de pruebas en `tests/SistemaFuneraria.Tests/Unit/Core/CalculadorLiquidacionTests.cs` no compila porque `CalculadorLiquidacion` y `ResultadoLiquidacion` no existen en `src/SistemaFuneraria.Core/Calculos/`.
Procedemos con la fase GREEN implementando los componentes requeridos en la capa de dominio.
