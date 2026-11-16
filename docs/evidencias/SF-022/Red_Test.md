# Evidencia Red Test — SF-022

**Fecha:** 2026-11-14
**Rama:** `features/SF-022-identity-seed-3-roles`
**Comando ejecutado:**
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter IdentitySeedTests
```

## Resultado (RED)
- **Estado:** Error de compilación (fase TDD Rojo).
- **Error observado:**
  `src/SistemaFuneraria.Infrastructure/Persistencia/Seed/IdentitySeed.cs(9,38): error CS1002: Se esperaba ;`
- **Archivo de pruebas:** `tests/SistemaFuneraria.Tests/Integration/IdentitySeedTests.cs` (verifica constantes de rol en `ParametrosNegocio`, ejecución idempotente doble de `IdentitySeed.SeedAsync(provider)`, los 3 roles y los 3 usuarios oficiales con contraseña `"Sistema Funerario2026!"` y claims `NombreCompleto`, `Iniciales`, `Sucursal`).
