# Evidencia TDD / Frontend UI: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-092 |
| **Título del Ticket** | Pantalla Principal de Configuración (Captura 12 de Figma): Submenú de 5 Secciones, Datos de la Sucursal y Switches Operativos |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 16/11/2026 00:40 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaConfiguracion |

---

### 1. Resumen de Implementación
* **`src/SistemaFuneraria.Web/Pages/Configuracion/Index.cshtml`:**
  * **Encabezado:** Kicker `ADMINISTRACIÓN`, título `Configuración`, subtítulo y botón primario con icono `<button id="btnGuardarCambios"><i class="fa-solid fa-floppy-disk me-2"></i> Guardar cambios</button>` con feedback toast.
  * **Layout de 2 Columnas (`row g-4 mb-4`):**
    * **Columna Izquierda (`col-lg-3 col-md-4`):** Tarjeta con submenú vertical de 5 secciones con iconos (`Datos de la sucursal` activo en fondo crema `#FAF8F5` y texto `#2D4224`, `Usuarios y roles`, `Planes y tasas`, `Notificaciones`, `Integraciones`).
    * **Columna Derecha (`col-lg-9 col-md-8`):** Formulario "Datos de la sucursal" con los 6 campos oficiales (`Nombre comercial`, `Teléfono`, `Correo`, `Domicilio`, `Ciudad`, `Código postal`).
  * **Switches de Automatización:** Toggles Bootstrap estilizados con verde olivo Serenia (`.sf-switch-olive:checked { background-color: #2D4224; border-color: #2D4224; }`) para `Recordatorios de pago` y `Resumen diario de caja`.
* **`src/SistemaFuneraria.Web/Pages/Configuracion/Index.cshtml.cs`:**
  * PageModel `IndexModel` implementado con propiedades predefinidas según Figma.
* **`src/SistemaFuneraria.Web/Pages/Shared/_Layout.cshtml`:**
  * Enlace activo "Configuración" en la sección ADMINISTRACIÓN del Sidebar hacia `/Configuracion` con detección reactiva (`esConfiguracion`).
* **`tests/SistemaFuneraria.Tests/Unit/Web/ConfiguracionPageTests.cs`:**
  * Prueba unitaria que valida la inicialización y consistencia de datos de la sucursal y switches.

---

### 2. Evidencia de Ejecución GREEN
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ConfiguracionPageTests" --logger "console;verbosity=normal"
```

```text
Serie de pruebas para C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
  Aprobada SistemaFuneraria.Tests.Unit.Web.ConfiguracionPageTests.Configuracion_OnGet_DebeCargarDatosDeSucursalYSwitchesConformeFigmaCaptura12 [11 ms]

Correctas! - Con error:     0, Superado:     1, Omitido:     0, Total:     1, Duración: 14 ms - SistemaFuneraria.Tests.dll (net8.0)
Total de la suite: 290 pruebas ejecutadas, 290 aprobadas (100% GREEN).
```

---

### 3. Fidelidad Figma Verificada (Cero Fantasía)
- Grid de 2 columnas responsivo con submenú vertical de 5 ítems.
- Datos prellenados de Jardines del Valle idénticos a Captura 12.
- Switches de automatización operativa en fondo crema con toggle verde olivo `#2D4224`.
- Botón superior "Guardar cambios" con confirmación visual.
