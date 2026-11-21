# Evidencia TDD / Frontend UI: Fase ROJA (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-092 |
| **Título del Ticket** | Pantalla Principal de Configuración (Captura 12 de Figma): Submenú de 5 Secciones, Datos de la Sucursal y Switches Operativos |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 16/11/2026 00:36 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaConfiguracion |

---

### 1. Resumen de la Falla Inicial (RED)
Antes de la implementación, la ruta `/Configuracion` no existía (`Pages/Configuracion/Index.cshtml` inexistente), el elemento "Configuración" del Sidebar estaba inerte sin navegación y no existían los componentes requeridos por la **Captura 12 de Figma**:
1. Sin encabezado administrativo ni botón de acción *"Guardar cambios"*.
2. Sin submenú vertical de 5 secciones (`Datos de la sucursal`, `Usuarios y roles`, `Planes y tasas`, `Notificaciones`, `Integraciones`).
3. Sin formulario de datos de la sucursal (`Jardines del Valle`, `55 3128 4400`, `contacto@jardinesdelvalle.mx`, `Av. de la Paz 1840`, `Ciudad de México`, `03100`).
4. Sin los switches de automatización operativa (`Recordatorios de pago` y `Resumen diario de caja`).

### 2. Ejecución del Test en Rojo
```text
[xUnit.net] ConfiguracionPageTests.Configuracion_OnGet_DebeCargarDatosDeSucursalYSwitchesConformeFigmaCaptura12 [FAIL]
  Mensaje de error:
   The type or namespace name 'Configuracion' does not exist in the namespace 'SistemaFuneraria.Web.Pages'
```
Resultado: **Prueba fallida (Fase ROJA confirmada).**
