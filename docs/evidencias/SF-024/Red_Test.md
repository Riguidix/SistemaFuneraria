# Evidencia Red Test — SF-024

**Fecha:** 2026-11-15
**Rama:** `feature/SF-024-js-fetch-api-client-auth`

## Estado inicial (RED)
- `src/SistemaFuneraria.Web/wwwroot/js/api/apiClient.js` se encontraba vacío (`0 bytes`) y `authApi.js` no existía.
- En el primer borrador de `apiClient.js` se detectaron fallos de contrato y ejecución:
  - Envío de `body` en peticiones `GET`, doble llamada a `.json()`, uso de palabra reservada `delete`, `Authorization: Bearer undefined`, falta de manejo de `204 No Content` y `ProblemDetails` RFC 7807, y advertencias de JSHint ES5.
- `tests/SistemaFuneraria.Tests/Unit/Web/ApiClientAuthWrapperTests.cs` requería la implementación completa de `apiClient.js` y `authApi.js`.
