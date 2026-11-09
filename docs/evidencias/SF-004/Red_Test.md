# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-004 |
| **Caso de Prueba Relacionado** | RNF-06 / RN-12 (Formato Monetario en Pesos Mexicanos es-MX) |
| **Fecha y Hora de Ejecución** | 07/11/2026 01:00 |
| **Desarrollador / Tester** | Juan Romero (Web-UI) / Lead Fullstack |
| **Rama Git** | feature/SF-004-layout-bootstrap-es-mx |

---

### 1. Objetivo de la Prueba
Demostrar que la aplicación web no tiene configurado el middleware de localización para la cultura `es-MX` por defecto ni el layout responsivo base con Bootstrap 5.3.8. Al renderizar valores monetarios bajo la cultura en-US o sin middleware explícito, los importes no satisfacen el formato obligatorio `$#,##0.00` con comas de miles y punto decimal en el servidor web.

### 2. Datos de Entrada Utilizados
* Valor monetario de lote funerario básico: `120000.00m`
* Formato esperado: `$120,000.00`
* Cultura por defecto ausente en `Program.cs`: `es-MX`

### 3. Comando de Terminal Ejecutado
```bash
dotnet test --filter "FullyQualifiedName~CulturaEsMxTests"
```

### 4. Salida de Terminal (Falla Esperada)
```text
$ dotnet test --filter "FullyQualifiedName~CulturaEsMxTests"
Determinando los proyectos que se van a restaurar...
Todos los proyectos están actualizados para la restauración.

[FAIL] SistemaFuneraria.Tests.Unit.CulturaEsMxTests.ConfiguracionGlobal_SinMiddleware_NoAplicaCulturaEsMx
Error Message:
 Expected actual culture to be "es-MX", but found "en-US".
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at FluentAssertions.Execution.TestEngine.Throw(String message)
   at SistemaFuneraria.Tests.Unit.CulturaEsMxTests.ConfiguracionGlobal_SinMiddleware_NoAplicaCulturaEsMx() in C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\CulturaEsMxTests.cs:line 48

Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 42 ms
```

### 5. Explicación del Resultado
La prueba falló como se esperaba porque `Program.cs` carece de la configuración de `RequestLocalizationOptions` con `CultureInfo("es-MX")`. En ausencia de este middleware, el hilo de procesamiento toma la cultura del sistema operativo anfitrión (`en-US`), impidiendo que las vistas y respuestas HTTP adopten de forma global y homogénea el formato monetario mexicano requerido por RN-12.
