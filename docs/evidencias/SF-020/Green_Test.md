# Evidencia Green Test — SF-020

**Fecha:** 2026-11-14
**Rama:** `feature/SF-020-api-controller-base-cors`
**Comandos ejecutados:**
```bash
dotnet build SistemaFuneraria.sln
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter ApiControllerBaseTests
```

## Resultado (GREEN)
- **Compilación (`dotnet build SistemaFuneraria.sln`):** Correcta — 6 proyectos compilados, **0 Advertencias, 0 Errores**.
- **Pruebas (`ApiControllerBaseTests`):** **Total:** 5 | **Superado:** 5 | **Con error:** 0 | **Omitido:** 0 | **Duración:** 6 ms.
  - `FromResult` con `Resultado<T>.Exito` $\rightarrow$ `200 OK` con `Valor`.
  - `FromResult` con `Resultado.Exito` $\rightarrow$ `200 OK`.
  - `FromResult` con error de negocio $\rightarrow$ `400 Bad Request` (`ProblemDetails`).
  - `FromResult` con `[CONFLICTO]` $\rightarrow$ `409 Conflict` (`ProblemDetails`).
  - `FromResult` con `[NO_ENCONTRADO]` $\rightarrow$ `404 Not Found` (`ProblemDetails`).
