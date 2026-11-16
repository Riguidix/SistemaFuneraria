# Evidencia Green Test — SF-024

**Fecha:** 2026-11-15
**Rama:** `feature/SF-024-js-fetch-api-client-auth`
**Comandos ejecutados:**
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter ApiClientAuthWrapperTests
```

## Resultado (GREEN)
- **xUnit (`ApiClientAuthWrapperTests`):** **Total:** 2 | **Superado:** 2 | **Con error:** 0 | **Omitido:** 0 | **Duración:** 2 ms.
  - `SF-024: apiClient.js expone get, post, put, delete con Bearer JWT y ProblemDetails`: **Superada**.
  - `SF-024 (BDD): authApi.js expone login, olvideContrasena, logout, getSession y applyRolePermissions`: **Superada**.
- **Verificación de comportamiento en Node.js:** `ALL_NODE_BEHAVIOR_TESTS_PASSED` (validando `login` con `sessionStorage`, `GET` sin `body` + `Authorization: Bearer`, `ProblemDetails` $\rightarrow$ `ApiError` y `applyRolePermissions` ocultando `[data-roles="Administrador"]` para `Vendedor`).
