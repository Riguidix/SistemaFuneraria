# Evidencia Green Test — SF-030

**Fecha:** 2026-11-16
**Rama:** `feature/SF-030-qa-gate-v020-auth-health`
**Comandos ejecutados:**
```bash
dotnet build SistemaFuneraria.sln
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter AuthYUsuariosAdminHttpTests
```

## Resultado (GREEN)
- **Compilación (`dotnet build SistemaFuneraria.sln`):** Correcta — 6 proyectos compilados, **0 Advertencias, 0 Errores**.
- **Pruebas (`AuthYUsuariosAdminHttpTests`):** **Total:** 4 | **Superado:** 4 | **Con error:** 0 | **Omitido:** 0 | **Duración:** 1 s.
  - `SF-030: GET /health debe responder 200 OK`: **Superada**.
  - `SF-030 (BDD): Login Administrador exitoso y control RBAC en /api/v1/admin/usuarios`: **Superada** (`200 OK` para `Administrador`; `403 Forbidden` para `Vendedor` y `Cajero`).
  - `SF-030 (BDD): 5 intentos fallidos bloquean la cuenta con 423 Locked (15 minutos)`: **Superada**.
  - `SF-030 (BDD): /api/v1/auth/olvide-contrasena aplica jitter anti-timing >= 200ms`: **Superada**.
- **Validador CI de evidencias:** Sin rutas locales (`/Users/` o `C:\`) en `docs/evidencias/SF-030`.
