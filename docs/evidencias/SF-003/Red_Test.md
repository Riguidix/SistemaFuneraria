# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-003 |
| **Caso de Prueba Relacionado** | Detección de Violación Arquitectónica / Criterio de Aceptación SF-003 |
| **Fecha y Hora de Ejecución** | 07/11/2026 12:00:02 |
| **Desarrollador / Tester** | Guillermo Sandoval / Edgar Acevedo (QA-DevOps) |
| **Rama Git** | feature/SF-003-Configuración-de-xUnit |

---

### 1. Objetivo de la Prueba
Demostrar que el arnés de pruebas de arquitectura configurado con `NetArchTest.Rules` y `FluentAssertions` detecta e interrumpe activamente la ejecución ante violaciones de reglas arquitectónicas en los ensamblados del proyecto. Para comprobar el arnés, se evalúa una regla prohibitiva sobre dependencias existentes (en este caso, la capa Web implementando tipos dependientes de `Microsoft.AspNetCore.Mvc`).

### 2. Regla Evaluada en Fase ROJO
```csharp
[Fact(DisplayName = "RED TEST: Demostración de detección de dependencias no permitidas")]
public void RedTest_DeteccionDependencia_DebeFallarSiExisteViolacion()
{
    var result = Types.InAssembly(WebAssembly)
        .ShouldNot()
        .HaveDependencyOn("Microsoft.AspNetCore.Mvc")
        .GetResult();

    result.IsSuccessful.Should().BeTrue("esta prueba falla intencionalmente demostrando que NetArchTest detecta dependencias");
}
```

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --no-restore
```

### 4. Salida Real de Terminal (Falla Esperada)
```text
  SistemaFuneraria.Core -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Core\bin\Debug\net8.0\SistemaFuneraria.Core.dll
  SistemaFuneraria.Application -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Application\bin\Debug\net8.0\SistemaFuneraria.Application.dll
  SistemaFuneraria.Infrastructure -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Infrastructure\bin\Debug\net8.0\SistemaFuneraria.Infrastructure.dll
  SistemaFuneraria.Web -> C:\Users\Billy\SistemaFuneraria\src\SistemaFuneraria.Web\bin\Debug\net8.0\SistemaFuneraria.Web.dll
  SistemaFuneraria.Tests -> C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.12.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.44]     RED TEST: Demostración de detección de dependencias no permitidas [FAIL]
  Failed RED TEST: Demostración de detección de dependencias no permitidas [281 ms]
  Error Message:
   Expected result.IsSuccessful to be true because esta prueba falla intencionalmente demostrando que NetArchTest detecta dependencias, but found False.
  Stack Trace:
     at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
   at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
   at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
   at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
   at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs)
   at SistemaFuneraria.Tests.Arquitectura.ReglasArquitecturaTests.RedTest_DeteccionDependencia_DebeFallarSiExisteViolacion() in C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\Arquitectura\ReglasArquitecturaTests.cs:line 36
   at System.Reflection.MethodBaseInvoker.InterpretedInvoke_Method(Object obj, IntPtr* args)
   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)

Failed!  - Failed:     1, Passed:     7, Skipped:     0, Total:     8, Duration: 314 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Certificación
La fase ROJO certifica que el motor `NetArchTest.Rules` v1.3.2 integrado con xUnit y FluentAssertions inspecciona correctamente los metadatos de los ensamblados y arroja una falla determinista cuando se infringe una regla arquitectónica.
