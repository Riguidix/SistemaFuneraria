# Evidencia Red Test — SF-030

**Fecha:** 2026-11-16
**Rama:** `feature/SF-030-qa-gate-v020-auth-health`
**Comando ejecutado:**
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter AuthYUsuariosAdminHttpTests
```

## Resultado (RED)
- **Total:** 4 | **Superado:** 3 | **Con error:** 1 | **Duración:** 1 s
- **Prueba fallida:** `SF-030: GET /health debe responder 200 OK`
- **Mensaje de error:**
  `Expected response.StatusCode to be HttpStatusCode.OK {value: 200}, but found HttpStatusCode.NotFound {value: 404}.`
- **Motivo:** `SistemaFuneraria.Api/Program.cs` aún no registraba `AddHealthChecks()` ni `MapHealthChecks("/health")`.
