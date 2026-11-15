# Evidencia de Red Test: SF-028 - Pantalla Figma /Cuenta/Login y /OlvideContrasena: Alerta al 3.er Fallo, Contador de Bloqueo al 5.º y Redirección

## 1. Identificación y Objetivo
- **Ticket**: SF-028
- **Fecha y Hora**: 2026-11-14 18:40:15
- **Objetivo de la Prueba**: Verificar que la suite de pruebas unitarias falle de forma esperada ante la ausencia de los PageModels `/Cuenta/Login` y `/Cuenta/OlvideContrasena` en la capa Web.
- **Archivo de Prueba**: `tests/SistemaFuneraria.Tests/Unit/Web/LoginPageTests.cs`

## 2. Datos de Entrada / Configuración
- **Ruta / Método Evaluado**: `/Cuenta/Login` y `/Cuenta/OlvideContrasena` (`LoginModel`, `OlvideContrasenaModel`).
- **Aserción Principal**: Inicialización de estado limpio (0 intentos, no bloqueado, 0 minutos), validación de `ModelState` con campos requeridos y mensaje uniforme de recuperación contra enumeración de usuarios (RNF-01, RNF-03).

## 3. Comando de Ejecución
```bash
dotnet test --filter "FullyQualifiedName~LoginPageTests" --logger "console;verbosity=normal"
```

## 4. Salida Real de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\LoginPageTests.cs(4,34): error CS0234: El tipo o el nombre del espacio de nombres 'Cuenta' no existe en el espacio de nombres 'SistemaFuneraria.Web.Pages' (¿falta alguna referencia de ensamblado?) [C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]

ERROR al compilar.
    1 Errores
    0 Advertencia(s)

Tiempo transcurrido 00:00:01.85
Failed! - Failed: 1, Passed: 0, Skipped: 0, Total: 1
```

## 5. Diagnóstico de la Falla
- **Causa Raíz de la Falla**: Las páginas `Login.cshtml.cs` y `OlvideContrasena.cshtml.cs` aún no estaban creadas bajo el espacio de nombres `SistemaFuneraria.Web.Pages.Cuenta`.
- **Resultado de la Fase**: **RED CONFIRMADO (Listo para proceder a desarrollo)**.
