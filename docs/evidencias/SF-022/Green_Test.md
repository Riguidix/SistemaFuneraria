# Evidencia Green Test — SF-022

**Fecha:** 2026-11-15
**Rama:** `features/SF-022-identity-seed-3-roles`
**Comandos ejecutados:**
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter IdentitySeedTests
```

## Resultado (GREEN)
- **Pruebas (`IdentitySeedTests`):** **Total:** 1 | **Superado:** 1 | **Con error:** 0 | **Omitido:** 0.
  - `SF-022 (BDD): IdentitySeed siembra de forma idempotente los 3 roles y las 3 cuentas oficiales`: **Superada** contra PostgreSQL 16 real (doble ejecución consecutiva sin duplicar roles, usuarios ni claims).
