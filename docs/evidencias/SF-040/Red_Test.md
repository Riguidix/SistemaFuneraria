# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-040 |
| **Caso de Prueba Relacionado** | `LoteConfiguration`, migración `AddLotesCatalogo` y `LoteSeed` |
| **Fecha y Hora de Ejecución** | 16/11/2026 15:23 |
| **Desarrollador / Tester** | Master Senior Backend & DBA Architect |
| **Rama Git** | `feature/SF-040-lote-configuration-migracion` |

---

### 1. Objetivo de la Prueba
Demostrar la ausencia de `LoteConfiguration`, `AppDbContext.Lotes` y `LoteSeed` antes de la implementación en `Application` e `Infrastructure`.

### 2. Comando Ejecutado
```bash
dotnet build SistemaFuneraria.sln --no-restore -m:1 --disable-build-servers
```

### 3. Salida Real de Terminal (Falla Esperada)
```text
  SistemaFuneraria.Core -> ...\SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> ...\SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> ...\SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> ...\SistemaFuneraria.Web.dll
  SistemaFuneraria.Api -> ...\SistemaFuneraria.Api.dll
...\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteSeedTests.cs(31,15): error CS0103: El nombre 'LoteSeed' no existe en el contexto actual
...\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteSeedTests.cs(32,15): error CS0103: El nombre 'LoteSeed' no existe en el contexto actual
...\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteSeedTests.cs(34,30): error CS1061: "AppDbContext" no contiene una definición para "Lotes" ni un método de extensión accesible "Lotes" que acepte un primer argumento del tipo "AppDbContext"

ERROR al compilar.
    0 Advertencia(s)
    3 Errores
```
