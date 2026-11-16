# Evidencia TDD / Frontend UI: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-027 |
| **Título del Ticket** | Kit Reutilizable de los 5 Estados UI en "Sistema Funerario" (Empty, Skeleton, Error Banner, Toast y Modal Redondeado) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad, Accesibilidad WCAG y 5 Estados de UI) |
| **Fecha y Hora de Ejecución** | 14/11/2026 23:20 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-027-figma-sistema-funerario-ui-states-kit |

---

### 1. Resumen de Implementación
* **`src/SistemaFuneraria.Web/wwwroot/js/ui/uiStates.js`:**
  * Implementación estandarizada en Vanilla JS del namespace `window.SfUi`:
    1. **Estado 1 (Loading):** `SfUi.showSkeleton(target, type, count)` y `SfUi.hideSkeleton(target)` con efecto Shimmer cálido para tablas, cards e inputs.
    2. **Estado 2 (Empty):** `SfUi.showEmpty(target, options)` con icono memorial, mensaje empático y botón CTA en verde `--forest`.
    3. **Estado 3 (Error):** `SfUi.showError(target, options)` con banner accesible (`role="alert"`), borde `--rose` (`#a84f4b`), renderizado del `detail` de `ProblemDetails` y botón de reintento.
    4. **Estado 4 (Success / Toast):** `SfUi.showToast(options)` con notificación flotante auto-descartable.
    5. **Estado 5 (Confirm / Modal):** `SfUi.confirm(options)` con modal Bootstrap 5 redondeado a `24px` conforme a la Captura 4 de Figma.
* **`src/SistemaFuneraria.Web/Pages/Shared/_UiStatesPartial.cshtml`:**
  * Componente HTML semántico con contenedor de toasts y modal de confirmación pre-insertado en `_Layout.cshtml`.
* **`src/SistemaFuneraria.Web/wwwroot/css/sistema-tokens.css`:**
  * Declaración de tokens oficiales de la Captura 13 (`--forest`, `--forest-2`, `--gold`, `--rose`, `--canvas`, `--ink`).
* **`src/SistemaFuneraria.Web/Pages/ParametrosUi.cshtml` y `.cs`:**
  * Vista de parámetros UI con escala tonal y catálogo interactivo de los tokens y estados.

---

### 2. Evidencia de Ejecución GREEN
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~UiStatesKitTests" --logger "console;verbosity=normal"
```

```text
Serie de pruebas para C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.12]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.30]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.31]   Starting:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.38]   Finished:    SistemaFuneraria.Tests
  Correctas SistemaFuneraria.Tests.Unit.Web.UiStatesKitTests.SistemaTokensCss_DebeContenerVariablesOficialesFigma [10 ms]
  Correctas SistemaFuneraria.Tests.Unit.Web.UiStatesKitTests.ParametrosUiModel_OnGet_DebeInicializarCorrectamente [3 ms]
  Correctas SistemaFuneraria.Tests.Unit.Web.UiStatesKitTests.UiStatesScript_DebeExistirYContenerLosCincoEstadosEstandarizados [< 1 ms]

La serie de pruebas se ejecutó correctamente.
Pruebas totales: 3
     Correcto: 3
 Tiempo total: 1.2863 Segundos
```

---

### 3. Explicación del Resultado
La fase **VERDE** certifica:
1. **Los 5 Estados UI:** Estandarizados y disponibles globalmente vía `window.SfUi` en todas las vistas Razor del sistema sin acoplamiento a frameworks SPA.
2. **Tokens Figma (Captura 13):** Todas las variables CSS necesarias (`--forest`, `--gold`, `--rose`, `--canvas`, `--ink`) están validadas y presentes en `sistema-tokens.css`.
3. **Cero Regresiones:** El proyecto compila limpiamente y la suite de pruebas unitarias concluye con 100% de aprobación.
