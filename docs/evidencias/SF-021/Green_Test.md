# Evidencia Green Test — SF-021

**Fecha:** 2026-11-14
**Rama:** `feature/SF-021-identity-dbcontext-migration`
**Comando ejecutado:**
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-021"
```

## Resultado (GREEN)
- **Pruebas (`Ticket=SF-021`):** **Total:** 2 | **Superado:** 2 | **Con error:** 0 | **Omitido:** 0 | **Duración:** 2 s.
  - `SF-021: AppDbContext debe heredar de IdentityDbContext<IdentityUser>`: **Superada**.
  - `SF-021 (BDD): EsquemaIdentity crea AspNetUsers, AspNetRoles y AspNetUserRoles y revierte sin perder clientes`: **Superada** contra PostgreSQL 16 real (`20261115035548_EsquemaIdentity`).
