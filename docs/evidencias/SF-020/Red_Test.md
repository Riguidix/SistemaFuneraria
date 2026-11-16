# Evidencia Red Test — SF-020

**Fecha:** 2026-11-14
**Rama:** `feature/SF-020-api-controller-base-cors`
**Comando ejecutado:**
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter ApiControllerBaseTests
```

## Resultado (RED)
- **Estado:** Error de compilación (fase TDD Rojo esperada).
- **Errores observados:**
  - `error CS0234: El tipo o el nombre del espacio de nombres 'Api' no existe en el espacio de nombres 'SistemaFuneraria'`
  - `error CS0246: El nombre del tipo o del espacio de nombres 'ApiControllerBase' no se encontró`
- **Archivo de pruebas:** `tests/SistemaFuneraria.Tests/Unit/Api/ApiControllerBaseTests.cs` (5 casos cubriendo `200 OK` tipado/no tipado, `400 Bad Request` ProblemDetails, `409 Conflict` con `[CONFLICTO]` y `404 Not Found` con `[NO_ENCONTRADO]`).
