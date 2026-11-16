# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-027 |
| **Título del Ticket** | Kit Reutilizable de los 5 Estados UI en "Sistema Funerario" (Empty, Skeleton, Error Banner, Toast y Modal Redondeado) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad, Accesibilidad WCAG y 5 Estados de UI) |
| **Fecha y Hora de Ejecución** | 14/11/2026 23:17 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-027-figma-sistema-funerario-ui-states-kit |

---

### 1. Objetivo de la Prueba
Demostrar mediante pruebas automatizadas que el kit de estados UI (`uiStates.js`) y los tokens CSS oficiales de Figma (`sistema-tokens.css`) requerían resolución y verificación de integridad contra los 5 estados obligatorios del sistema funerario:
1. Estado 1: Loading (Skeletons Shimmer)
2. Estado 2: Empty State
3. Estado 3: Error Banner (con ProblemDetails)
4. Estado 4: Success Toast
5. Estado 5: Modal Redondeado

---

### 2. Datos de Entrada Utilizados
* **Archivos evaluados:**
  * `src/SistemaFuneraria.Web/wwwroot/js/ui/uiStates.js`
  * `src/SistemaFuneraria.Web/wwwroot/css/sistema-tokens.css`
  * `src/SistemaFuneraria.Web/Pages/ParametrosUi.cshtml` y `.cs`

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --no-build --filter "FullyQualifiedName~UiStatesKitTests" --logger "console;verbosity=normal"
```

---

### 4. Salida de Terminal (Falla Esperada)
```text
Serie de pruebas para tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

  Con error SistemaFuneraria.Tests.Unit.Web.UiStatesKitTests.SistemaTokensCss_DebeContenerVariablesOficialesFigma [325 ms]
  Mensaje de error:
   Expected File.Exists(relativePath) to be true because El archivo sistema-tokens.css debe existir, but found False.
  Correctas SistemaFuneraria.Tests.Unit.Web.UiStatesKitTests.ParametrosUiModel_OnGet_DebeInicializarCorrectamente [113 ms]
  Con error SistemaFuneraria.Tests.Unit.Web.UiStatesKitTests.UiStatesScript_DebeExistirYContenerLosCincoEstadosEstandarizados [2 ms]
  Mensaje de error:
   Expected File.Exists(relativePath) to be true because El archivo uiStates.js debe existir en wwwroot/js/ui/, but found False.

No se pudo ejecutar la serie de pruebas.
Pruebas totales: 3
     Correcto: 1
     Incorrecto: 2
 Tiempo total: 1.7973 Segundos
```

---

### 5. Explicación del Resultado
La prueba falló deterministamente en fase **ROJO** (1 Correcto, 2 Incorrectos):
* Se verificó la necesidad de resolver la ruta canónica del entorno de ejecución hacia `wwwroot/js/ui/uiStates.js` y `wwwroot/css/sistema-tokens.css`.
* Queda validado el comportamiento inicial esperado del arnés de pruebas TDD.
