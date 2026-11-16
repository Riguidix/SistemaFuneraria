# Evidencia de Regresión: Sobreingeniería Rechazada en Code Review

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-004 (Layout Razor Pages, Bootstrap 5.3.8 y Cultura es-MX) |
| **Tipo de Evento** | Detección de Sobreingeniería y Regresión en CI/CD (Quality Gate) |
| **Fecha de Detección** | 07/11/2026 17:42 |
| **Auditor de Calidad (QA)** | Lead Fullstack / Edgar Acevedo (DevOps) |
| **Desarrollador / Autor** | Juan Romero (con asistencia de Copilot) |
| **Rama Git** | `feature/SF-004-layout-bootstrap-es-mx` |
| **Pull Request** | PR #58 |
| **Veredicto de Auditoría** | **RECHAZADO Y REMOVIDO POR ROLLBACK (Cero Parches)** |

---

### 1. Resumen Ejecutivo del Incidente
Durante la revisión de calidad (Quality Gate) del ticket **SF-004**, se detectó la introducción de un segundo commit (`fd224a6`: *test(web): add SF-004 HTTP integration coverage*) que intentó implementar una solución con **alto grado de sobreingeniería y fragilidad**, la cual quebró el pipeline de Integración Continua (GitHub Actions) al compilar en modo `Release`. 

Siguiendo el principio de **Mínimo Cambio y Calidad Cero Fantasía**, se rechazó cualquier intento de "parchar" el código viciado y se procedió a **eliminar el commit mediante `git reset --hard`**, restableciendo la estabilidad de la rama a su versión limpia y probada.

---

### 2. Anatomía de la Sobreingeniería Introducida

En lugar de utilizar pruebas unitarias puras sobre la configuración de cultura o el estándar oficial de Microsoft para ASP.NET Core (`WebApplicationFactory<Program>` en memoria), el commit introdujo una clase `Sf004WebFixture` en `tests/SistemaFuneraria.Tests/Integration/SF004LayoutTests.cs` con los siguientes vicios arquitectónicos:

1. **Subprocesos del Sistema Operativo en Pruebas:**
   Lanzamiento manual de un proceso externo con `System.Diagnostics.Process` ejecutando `dotnet run --no-build`.
2. **Apertura de Sockets TCP en Red Local:**
   Búsqueda dinámica de puertos libres con `System.Net.Sockets.TcpListener` y sondeo mediante polling con `HttpClient` durante un timeout de 30 segundos.
3. **Falsa Certidumbre en Entorno Local:**
   La prueba pasaba en la máquina local del desarrollador únicamente porque existían binarios previos en `Debug`.

```csharp
// ❌ CÓDIGO DE SOBREINGENIERÍA DETENIDO EN CODE REVIEW:
var startInfo = new ProcessStartInfo("dotnet");
startInfo.ArgumentList.Add("run");
startInfo.ArgumentList.Add("--no-build"); // Trampa: busca binarios en bin/Debug
startInfo.ArgumentList.Add("--project");
startInfo.ArgumentList.Add(webProject);
_process = new Process { StartInfo = startInfo };
_process.Start(); // Levanta subproceso del SO vulnerable a entornos CI
```

---

### 3. La Falla Provocada en Integración Continua (GitHub Actions)

El pipeline automatizado de CI compila en modo estricto de producción:
`dotnet build --configuration Release`
`dotnet test --no-build --configuration Release`

Al ejecutarse en el runner limpio de Ubuntu en GitHub Actions:
* `dotnet run --no-build` buscaba por omisión la carpeta `src/SistemaFuneraria.Web/bin/Debug/net8.0/`.
* Como el runner solo había compilado en **`Release`**, la carpeta `Debug` no existía.
* El proceso abortó arrojando una excepción de sistema operativo `Win32Exception (2)`, quebrando 3 pruebas HTTP y deteniendo el pipeline.

#### Salida Real del Fallo en CI (Run #42422739707):
```text
[xUnit.net] SistemaFuneraria.Tests.Integration.SF004LayoutTests.HomePage_DeclaresMexicanSpanishCulture [FAIL]
System.InvalidOperationException : La aplicación terminó antes de estar disponible.
Unhandled exception: System.ComponentModel.Win32Exception (2): 
An error occurred trying to start process '<REPO_ROOT>/src/SistemaFuneraria.Web/bin/Debug/net8.0/SistemaFuneraria.Web' 
with working directory '<REPO_ROOT>/src/SistemaFuneraria.Web'. No such file or directory

Failed! - Failed: 3, Passed: 8, Skipped: 0, Total: 11
##[error]Process completed with exit code 1.
```

---

### 4. Acción de Contención Aplicada (Rollback-First)

En cumplimiento de las políticas de QA de este proyecto, **no se aceptó aplicar parches sobre una base de código viciada**:

1. **Rollback Determinista:**  
   Se ejecutó un retroceso forzado al commit original limpio de Juan Romero:
   ```bash
   git reset --hard 2d8e3e9
   git push --force-with-lease origin feature/SF-004-layout-bootstrap-es-mx
   ```
2. **Eliminación Total de Archivos Espurios:**  
   Se eliminó por completo `tests/SistemaFuneraria.Tests/Integration/SF004LayoutTests.cs`.
3. **Verificación de Suite Canónica:**  
   Se ejecutó la suite de pruebas unitarias en modo `Release` localmente confirmando 8 pruebas aprobadas en 1 ms.

---

### 5. Certificación de Regresión Resuelta en GitHub Actions

Tras el force-push limpio, GitHub Actions reejecutó el workflow `CI - Build & Test Gatekeeper` (Run #42427252378), certificando el paso a **100% verde**:

```text
$ gh pr checks 58
All checks were successful
0 cancelled, 0 failing, 1 successful, 0 skipped, and 0 pending checks

   NAME                         DESCRIPTION  ELAPSED
✓  CI - Build & Test Gatekeeper              1m3s
```

---

### 6. Lecciones Aprendidas para la Escuadra (Post-Mortem)

1. **Principio YAGNI & Anti-Sobreingeniería:** No crear infraestructura de red ni subprocesos para validar lo que un test unitario enfocado puede verificar en 2 milisegundos.
2. **Independencia de Configuración:** Ningún test debe asumir la existencia de carpetas `Debug` o `Release` en disco.
3. **Estándar para Pruebas Web:** Si en tickets futuros (ej. SF-026) se requieren pruebas de endpoints web, se utilizará exclusivamente `WebApplicationFactory<Program>` (servidor en memoria), tal como exige la Sección 3 del Plan de Pruebas (`docs/03-plan-pruebas.md`).
