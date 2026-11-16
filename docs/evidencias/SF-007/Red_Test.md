# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-007 |
| **Título del Ticket** | Patrón Resultado y Resultado<T> canónico en Application.Comun |
| **Caso de Prueba Relacionado** | CA-01 a CA-04 / Especificación Backlog SF-007 |
| **Fecha y Hora de Ejecución** | 08/11/2026 18:12:49 |
| **Desarrollador / Tester** | Guillermo Sandoval (Backend) / Lead Fullstack (Revisor) |
| **Rama Git** | feature/SF-007-patron-resultado |

---

### 1. Objetivo de la Prueba
Demostrar el estado inicial de falla (fase RED) por ausencia de la implementación de los tipos `Resultado` y `Resultado<T>` en el espacio de nombres `SistemaFuneraria.Application.Comun`. Se valida que la suite de pruebas unitarias en `tests/SistemaFuneraria.Tests/Application/ResultadoTests.cs` no compila debido a la falta de definición de estos tipos en el proyecto `Application`.

### 2. Comando de Terminal Ejecutado
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

### 3. Salida Real de Terminal (Falla Esperada / RED)
```text
  Determining projects to restore...
  Restored <REPO_ROOT>/src/SistemaFuneraria.Application/SistemaFuneraria.Application.csproj (in 184 ms).
  Restored <REPO_ROOT>/src/SistemaFuneraria.Infrastructure/SistemaFuneraria.Infrastructure.csproj (in 184 ms).
  Restored <REPO_ROOT>/src/SistemaFuneraria.Web/SistemaFuneraria.Web.csproj (in 184 ms).
  Restored <REPO_ROOT>/src/SistemaFuneraria.Core/SistemaFuneraria.Core.csproj (in 167 ms).
  Restored <REPO_ROOT>/tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj (in 4.12 sec).
  SistemaFuneraria.Core -> <REPO_ROOT>/src/SistemaFuneraria.Core/bin/Debug/net8.0/SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> <REPO_ROOT>/src/SistemaFuneraria.Application/bin/Debug/net8.0/SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> <REPO_ROOT>/src/SistemaFuneraria.Infrastructure/bin/Debug/net8.0/SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> <REPO_ROOT>/src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web.dll
<REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/ResultadoTests.cs(4,24): error CS0234: The type or namespace name 'Application' does not exist in the namespace 'SistemaFuneraria' (are you missing an assembly reference?) [<REPO_ROOT>/tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj]

Build FAILED.

<REPO_ROOT>/tests/SistemaFuneraria.Tests/Application/ResultadoTests.cs(4,24): error CS0234: The type or namespace name 'Application' does not exist in the namespace 'SistemaFuneraria' (are you missing an assembly reference?) [<REPO_ROOT>/tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj]
    0 Warning(s)
    1 Error(s)

Time Elapsed 00:00:19.19
```

### 4. Certificación
La fase ROJO queda formalmente certificada bajo la metodología TDD estricto ("Cero Fantasía"):
1. El compilador falla limpiamente por ausencia de la implementación de `Resultado` y `Resultado<T>` en `SistemaFuneraria.Application.Comun`.
2. Las pruebas unitarias definen con precisión los contratos requeridos antes de escribir el código de producción.
