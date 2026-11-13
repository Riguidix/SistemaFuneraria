# Evidencia TDD / Infraestructura: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-022 |
| **Título del Ticket** | Entidad Lote y Seeder EF Core con los 3 productos fijos ($120k, $150k, $180k) |
| **Caso de Prueba Relacionado** | Persistencia & Catálogo Inmutable / Criterios de Aceptación SF-022 (ERD v1.1 Sección 3.1) |
| **Fecha y Hora de Ejecución** | 13/11/2026 12:10 |
| **Desarrollador / Tester** | Gustavo Lucio (DBA - Infraestructura) / Edgar Acevedo (QA Revisor) |
| **Rama Git** | feature/SF-022-lote-seeder-ef |

---

### 1. Objetivo de la Prueba
Demostrar el estado inicial de fallo y ausencia de la entidad de dominio `Lote` en `SistemaFuneraria.Core`, su mapeo Fluent API `LoteConfiguration` en `SistemaFuneraria.Infrastructure` y su exposición como `DbSet<Lote>` en `AppDbContext`.

Se valida mediante la suite xUnit `LoteConfigurationTests.cs` (etiquetada con `[Trait("Ticket", "SF-022")]`), certificando que el sistema no puede compilar ni satisfacer los criterios de aceptación del catálogo oficial sin la implementación de las clases correspondientes.

---

### 2. Comando de Terminal Ejecutado

```bash
dotnet test --filter "Ticket=SF-022"
```

---

### 3. Salida de Terminal (Falla Esperada / RED)

```text
  Determinando los proyectos que se van a restaurar...
  Todos los proyectos están actualizados para la restauración.
  SistemaFuneraria.Core -> <REPO>/src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> <REPO>/src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> <REPO>/src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> <REPO>/src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(33,60): error CS0246: El nombre del tipo o del espacio de nombres 'Lote' no se encontró (¿falta una directiva using o una referencia de ensamblado?) [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(48,69): error CS0103: El nombre 'Lote' no existe en el contexto actual [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(51,53): error CS0103: El nombre 'Lote' no existe en el contexto actual [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(64,53): error CS0103: El nombre 'Lote' no existe en el contexto actual [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(70,54): error CS0103: El nombre 'Lote' no existe en el contexto actual [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(75,60): error CS0103: El nombre 'Lote' no existe en el contexto actual [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(80,55): error CS0103: El nombre 'Lote' no existe en el contexto actual [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(112,97): error CS0103: El nombre 'Lote' no existe en el contexto actual [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(140,20): error CS0103: El nombre 'Lote' no existe en el contexto actual [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
<REPO>\tests\SistemaFuneraria.Tests\Unit\Infrastructure\LoteConfigurationTests.cs(160,31): error CS0103: El nombre 'Lote' no existe en el contexto actual [<REPO>\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]
```

---

### 4. Diagnóstico Técnico del Callejón de Falla
1. **Entidad Ausente:** La clase `SistemaFuneraria.Core.Entidades.Lote` no existe en la capa de dominio.
2. **Mapeo Ausente:** La clase `LoteConfiguration` no está creada en `Infrastructure/Persistencia/Configuraciones`.
3. **Persistencia Incompleta:** `AppDbContext` carece del `DbSet<Lote> Lotes` necesario para que el modelo incluya la tabla relacional `lotes` con el seeder `HasData`.
