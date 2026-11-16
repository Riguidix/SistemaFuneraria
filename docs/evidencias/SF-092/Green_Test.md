# Evidencia TDD / Frontend UI: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-092 |
| **Título del Ticket** | Pantalla Principal de Configuración (Captura 12 de Figma): Submenú Reactivo de 5 Secciones, Datos de la Sucursal y Switches Operativos |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 16/11/2026 00:48 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaConfiguracion |

---

### 1. Resumen de Implementación
* **`src/SistemaFuneraria.Web/Pages/Configuracion/Index.cshtml`:**
  * **Encabezado:** Kicker `ADMINISTRACIÓN`, título `Configuración`, subtítulo y botón primario con icono `<button id="btnGuardarCambios"><i class="fa-solid fa-floppy-disk me-2"></i> Guardar cambios</button>` con feedback toast.
  * **Submenú de 5 Secciones 100% Interactivo:**
    1. **Datos de la sucursal:** Formulario con los 6 campos oficiales (`Nombre comercial`, `Teléfono`, `Correo`, `Domicilio`, `Ciudad`, `Código postal`) y los 2 switches de automatización en verde olivo (`Recordatorios de pago` y `Resumen diario de caja`).
    2. **Usuarios y roles:** Tabla interactiva de colaboradores (`Andrea Morales`, `Carlos Mendoza`, `Guadalupe Ramos`) con avatares, badges de rol y estado, y resumen informativo de permisos RBAC.
    3. **Planes y tasas:** Tarjetas de precios de lista de lotes (Básico, Familiar, Preferencial) y tabla de plazos/tasas (12, 18, 24, 48 meses) con switch de bono por liquidación anticipada.
    4. **Notificaciones:** Configuración de canales (Email, WhatsApp/SMS) y switches de disparadores automáticos (pago aplicado, alerta preventiva, mora).
    5. **Integraciones:** Tarjetas de servicios externos (Facturación CFDI 4.0 SAT, Terminal bancaria, SMTP y Webhooks).
  * **Navegación Dinámica sin Recargas:** Controlador JavaScript que alterna la clase `.active`, estilos de resaltado crema/verde y visibilidad inmediata de los 5 paneles.
* **`src/SistemaFuneraria.Web/Pages/Configuracion/Index.cshtml.cs`:**
  * PageModel `IndexModel` con propiedades y colecciones DTOs (`Usuarios`, `PlanesLotes`, `TasasPlazos`).
* **`src/SistemaFuneraria.Web/Pages/Shared/_Layout.cshtml`:**
  * Enlace activo "Configuración" en la sección ADMINISTRACIÓN del Sidebar hacia `/Configuracion`.
* **`tests/SistemaFuneraria.Tests/Unit/Web/ConfiguracionPageTests.cs`:**
  * Prueba unitaria que valida la inicialización y consistencia de datos de las 5 secciones.

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
- Submenú de 5 opciones completamente reactivo con cambio instantáneo de panel.
- Estilos visuales exactos: fondo crema `#FAF8F5`, texto verde Serenia `#2D4224` e icono activo en verde.
- Paneles completos para las 5 secciones del sistema.
