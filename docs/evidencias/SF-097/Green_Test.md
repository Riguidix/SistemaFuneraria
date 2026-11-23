# Evidencia Green Test — SF-097: Ejecución Integral CI/CD y Cobertura XPlat

## Métricas Verificadas (Release + XPlat Code Coverage)
1. **Compilación limpia:** `dotnet build --configuration Release` → `0` advertencias, `0` errores.
2. **Suite automatizada (sin Docker local):** `688` pruebas superadas, `0` fallidas, `0` omitidas (`[Fact(Skip = ...)]` = `0`).
3. **Cobertura de líneas medida (`coverage.cobertura.xml`):**
   - `SistemaFuneraria.Core`: **95.94%** (meta DoD `>= 90%`)
   - `SistemaFuneraria.Application`: **93.18%** (meta DoD `>= 85%`)
   - `SistemaFuneraria.Api`: **86.83%**
   - `SistemaFuneraria.Infrastructure`: **84.50%**
   - `SistemaFuneraria.Web`: **70.46%**
4. **Higiene de evidencias:** `./scripts/validate-evidence.sh --all` exitoso con `0` rutas absolutas de host.
