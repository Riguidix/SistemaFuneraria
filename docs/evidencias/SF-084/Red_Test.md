# Evidencia Red Test: Ticket SF-084

## 1. Contexto de Ejecución
- **Ticket**: SF-084
- **Descripción**: Dominio `CalculadorAbonoCapital` (100% a Capital, Plazo Fijo, Oráculo `CP-07`)
- **Fase**: RED (Fallo de compilación por ausencia de entidad / servicio de dominio)
- **Fecha**: 2026-11-20
- **Entorno**: .NET 8 (x64), C# 12, Clean Architecture (Core puro sin dependencias externas)

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
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorAbonoCapitalTests.cs(26,25): error CS0103: El nombre 'CalculadorAbonoCapital' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorAbonoCapitalTests.cs(78,25): error CS0103: El nombre 'CalculadorAbonoCapital' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorAbonoCapitalTests.cs(96,31): error CS0103: El nombre 'CalculadorAbonoCapital' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorAbonoCapitalTests.cs(111,31): error CS0103: El nombre 'CalculadorAbonoCapital' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorAbonoCapitalTests.cs(124,31): error CS0103: El nombre 'CalculadorAbonoCapital' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Core/CalculadorAbonoCapitalTests.cs(157,31): error CS0103: El nombre 'CalculadorAbonoCapital' no existe en el contexto actual

ERROR al compilar.
    0 Advertencia(s)
    6 Errores
```

---

## 4. Diagnóstico Técnico
Las pruebas unitarias en `tests/SistemaFuneraria.Tests/Unit/Core/CalculadorAbonoCapitalTests.cs` no compilan debido a que el componente de dominio `CalculadorAbonoCapital` y su estructura de resultado `ResultadoAbonoCapital` aún no han sido implementados en `src/SistemaFuneraria.Core/Calculos/`.
Procedemos con la fase GREEN implementando el cálculo con amortización francesa pura, soporte para el oráculo CP-07 y validación estricta de invariantes de negocio.
