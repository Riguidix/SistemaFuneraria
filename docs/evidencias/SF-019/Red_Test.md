# Evidencia Red Test — SF-019

**Fecha:** 2026-11-14
**Rama:** `feature/SF-019-scaffold-api-project`
**Comando ejecutado:**
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter ReglasArquitecturaTests
```

## Resultado (RED)
- **Total:** 7 | **Superado:** 6 | **Con error:** 1 | **Duración:** 44 ms
- **Prueba fallida:** `RF-ARCH-07: Web no debe referenciar Infrastructure, EntityFrameworkCore ni Npgsql (SF-019)`
- **Motivo:** `src/SistemaFuneraria.Web/SistemaFuneraria.Web.csproj` aún contenía `<ProjectReference Include="..\SistemaFuneraria.Infrastructure\SistemaFuneraria.Infrastructure.csproj" />` y `<PackageReference Include="Microsoft.EntityFrameworkCore.Design" />`.
