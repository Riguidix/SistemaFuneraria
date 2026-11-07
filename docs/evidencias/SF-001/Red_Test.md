# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-001 |
| **Caso de Prueba Relacionado** | Arquitectura Base / Criterio de Aceptación SF-001 |
| **Fecha y Hora de Ejecución** | 05/11/2026 23:29 |
| **Desarrollador / Tester** | Lead Fullstack / Edgar Acevedo (QA-DevOps) |
| **Rama Git** | feat/SF-001-Scaffold_de_Solucion |

---

### 1. Objetivo de la Prueba
Demostrar el estado inicial de falla y ausencia de la arquitectura base de la solución. Se valida que al intentar compilar o listar proyectos en `SistemaFuneraria.sln`, no existe la estructura de 5 proyectos (.NET 8) ni los archivos de configuración centralizada (`Directory.Build.props`, `Directory.Packages.props`, `global.json`), violando el criterio de aceptación que exige 5 proyectos compilados limpiamente con 0 advertencias y 0 errores.

### 2. Datos de Entrada Utilizados
* Archivo de solución objetivo: `SistemaFuneraria.sln`
* Estructura esperada ausente:
  - `src/SistemaFuneraria.Core/SistemaFuneraria.Core.csproj`
  - `src/SistemaFuneraria.Application/SistemaFuneraria.Application.csproj`
  - `src/SistemaFuneraria.Infrastructure/SistemaFuneraria.Infrastructure.csproj`
  - `src/SistemaFuneraria.Web/SistemaFuneraria.Web.csproj`
  - `tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj`
  - `Directory.Build.props`
  - `Directory.Packages.props`
  - `global.json`

### 3. Comando de Terminal Ejecutado
```bash
dotnet sln SistemaFuneraria.sln list
dotnet build SistemaFuneraria.sln
```

### 4. Salida de Terminal (Falla Esperada)
```text
$ dotnet sln SistemaFuneraria.sln list
No se han encontrado proyectos en la solución.

$ dotnet build SistemaFuneraria.sln
  Determinando los proyectos que se van a restaurar...
<DOTNET_SDK_ROOT>/NuGet.targets(198,5): warning : No se puede encontrar un proyecto para restaurar. [<REPO_ROOT>/SistemaFuneraria.sln]

Compilación correcta.

<DOTNET_SDK_ROOT>/NuGet.targets(198,5): warning : No se puede encontrar un proyecto para restaurar. [<REPO_ROOT>/SistemaFuneraria.sln]
    1 Advertencia(s)
    0 Errores

Tiempo transcurrido 00:00:00.10
```

### 5. Explicación del Resultado
La fase ROJO se certifica como exitosa (falla detectada):
1. El comando `dotnet sln list` confirma que `SistemaFuneraria.sln` no contiene ningún proyecto (`No se han encontrado proyectos en la solución`).
2. El comando `dotnet build` emite una advertencia de NuGet (`warning : No se puede encontrar un proyecto para restaurar`), incumpliendo el criterio de aceptación de SF-001 que exige compilar 5 proyectos con cero advertencias y cero errores.
3. No existen las capas arquitectónicas (`Core`, `Application`, `Infrastructure`, `Web`, `Tests`) ni las directivas de compilación estricta (`TreatWarningsAsErrors`, `ManagePackageVersionsCentrally`).
