# Evidencia Red Test — SF-039

**Fecha:** 2026-11-16
**Rama:** `feature/SF-039-entidad-lote-core`
**Ticket:** `SF-039` — Entidad `Lote` (`Clave`, `Seccion`, `AndadorFila`, `Gavetas` 2/3/4, `PrecioContado` & `EstadoLote`)

## Comando ejecutado (Fase ROJO)
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj -m:1 --disable-build-servers --no-restore
```

## Salida real (Fallo esperado por ausencia de implementación en `Core`)
```text
C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Core\LoteTests.cs(3,29): error CS0234: El tipo o el nombre del espacio de nombres 'Enums' no existe en el espacio de nombres 'SistemaFuneraria.Core' (¿falta alguna referencia de ensamblado?) [C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Core\LoteTests.cs(20,20): error CS0246: El nombre del tipo o del espacio de nombres 'Lote' no se encontró (¿falta una directiva using o una referencia de ensamblado?) [C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]

ERROR al compilar.
    0 Advertencia(s)
    2 Errores
```
