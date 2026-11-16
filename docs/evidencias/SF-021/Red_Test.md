# Evidencia Red Test — SF-021

**Fecha:** 2026-11-14
**Rama:** `feature/SF-021-identity-dbcontext-migration`
**Comando ejecutado:**
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter AppDbContextIdentityContratoTests
```

## Resultado (RED)
- **Total:** 1 | **Superado:** 0 | **Con error:** 1
- **Prueba fallida:** `SF-021: AppDbContext debe heredar de IdentityDbContext<IdentityUser>`
- **Mensaje de error:**
  `Expected baseType!.Name to start with "IdentityDbContext" because AppDbContext debe heredar de IdentityDbContext<IdentityUser> para exponer las tablas AspNet*., but "DbContext" is too short.`
- **Archivo de pruebas:** `tests/SistemaFuneraria.Tests/Integration/IdentityEsquemaTests.cs` (incluye además la prueba de integración PostgreSQL 16 para `Up()` y `Down()` de `EsquemaIdentity`).
