# Evidencia TDD: Fase VERDE (GREEN) — SF-072

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-072 |
| **Título del Ticket** | Semilla CarteraDemoSeed (Contratos de Muestra CTR-2025-0189, CTR-2025-0182, CTR-2024-0118, CTR-2025-0144 Idénticos a Figma) |
| **Milestone** | v0.7.0 |
| **Carril** | Carril 1 (Infrastructure) |
| **Fecha de Ejecución** | 20/11/2026 |
| **Criterio BDD** | Carga idempotente de contratos semilla de Figma en entorno de desarrollo |

---

### 1. Resumen de Implementación
* **`src/SistemaFuneraria.Core/Entidades/Financiamiento.cs`**:
  * Incorporación del método de dominio puro `MarcarEnMora()` que permite transicionar de `Activo` a `EnMora` cuando se detectan mensualidades vencidas, manteniendo encapsuladas las invariantes de negocio con cero dependencias externas.
* **`src/SistemaFuneraria.Infrastructure/Persistencia/CarteraDemoSeed.cs`**:
  * Implementación del seeder idempotente `CarteraDemoSeed` con las firmas estáticas `SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)` y `SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)`.
  * Verificación previa de existencia de los 4 folios oficiales (`CTR-2025-0189`, `CTR-2025-0182`, `CTR-2024-0118`, `CTR-2025-0144`) para asegurar idempotencia estricta.
  * Creación y asignación de clientes oficiales de muestra (`Laura Castañeda`, `Roberto Méndez`, `Elena Vargas`, `Jorge Salinas`) con CURPs, teléfonos y correos válidos de prueba.
  * Vinculación a sus respectivos lotes de catálogo (`JC-C-118`, `JN-A-036`, `RO-B-027`, `JN-D-065`) y actualización de estado a `Vendido`.
  * Generación de amortización francesa oficial a 24 meses y simulación de cobranza mediante métodos de dominio (`AplicarPagoCuota`, `MarcarVencida`, `MarcarEnMora`, `CambiarEstadoAdmin`) para que cuadren los saldos y estados de Figma:
    * `CTR-2025-0189`: Saldo $127,500.00, Próximo pago $6,248.50, Estado Venta Activo.
    * `CTR-2025-0182`: Saldo $58,743.20, Próximo pago $6,248.50, Estado Venta Activo.
    * `CTR-2024-0118`: Saldo $76,180.40, 1 vencido ($6,248.50), Estado Venta Activo, Financiamiento EnMora, cuota 7 en Estado Vencida.
    * `CTR-2025-0144`: Saldo $94,265.80, Próximo pago $5,612.30, Estado Venta EnRevision.
* **`src/SistemaFuneraria.Api/Program.cs`**:
  * Invocación de `CarteraDemoSeed.SeedAsync(app.Services)` junto con `LoteSeed.SeedAsync(app.Services)` en el bloque condicional de entorno de desarrollo (`seedDemoUsers`).
* **`tests/SistemaFuneraria.Tests/Integration/CarteraDemoSeedTests.cs`**:
  * Suite de pruebas de integración con base de datos en memoria validando la idempotencia en doble ejecución y la consistencia de datos de los 4 contratos.

---

### 2. Evidencia de Compilación (`dotnet build`)
```bash
dotnet build -m:1 --disable-build-servers
```

```text
Determinando los proyectos que se van a restaurar...
Todos los proyectos están actualizados para la restauración.
SistemaFuneraria.Core -> src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
SistemaFuneraria.Application -> src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
SistemaFuneraria.Infrastructure -> src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
SistemaFuneraria.Web -> src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
SistemaFuneraria.Api -> src/SistemaFuneraria.Api/bin/Debug/net8.0/SistemaFuneraria.Api.dll
SistemaFuneraria.Tests -> tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll

Compilación correcta.
    0 Advertencia(s)
    0 Errores
```

---

### 3. Evidencia de Pruebas Unitarias / Integración del Ticket (`dotnet test`)
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~CarteraDemoSeedTests" --no-build -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

```text
Serie de pruebas para tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.32)
[xUnit.net 00:00:00.11]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.26]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.27]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:01.67]   Finished:    SistemaFuneraria.Tests
  Correctas SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_ConServiceProvider_DebePoblarCorrectamente [1 s]
  Correctas SistemaFuneraria.Tests.Integration.CarteraDemoSeedTests.SeedAsync_EjecutadoDosVeces_DebeSerIdempotenteYCrearExactamente4ContratosFigma [207 ms]

La serie de pruebas se ejecutó correctamente.
Pruebas totales: 2
     Correcto: 2
 Tiempo total: 2.3030 Segundos
```

---

### 4. Evidencia de Suite Completa No-Integración (Regresión Cero)
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Tipo!=Integration" --no-build -m:1 --disable-build-servers --logger "console;verbosity=normal"
```

```text
La serie de pruebas se ejecutó correctamente.
Pruebas totales: 415
     Correcto: 415
 Tiempo total: 4.2796 Segundos
```

---

### 5. Control de Blast Radius y Clean Architecture
* **`src/SistemaFuneraria.Infrastructure/Persistencia/CarteraDemoSeed.cs`**: 193 LOCs ($\le 210$ LOCs permitidas).
* **`src/SistemaFuneraria.Core/Entidades/Financiamiento.cs`**: +11 LOCs en `Core` (cero dependencias externas).
* **Rutas relativas**: Cumplimiento del 100% de la política anti-rutas absolutas para CI.
* **Control Git**: Cambios mantenidos estrictamente en el working tree sin commit ni push.
