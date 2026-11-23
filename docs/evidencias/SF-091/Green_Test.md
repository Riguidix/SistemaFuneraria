# Evidencia Green Test — SF-091: ConfiguracionSucursal y Gestión Admin de Usuarios

## Implementación
- `src/SistemaFuneraria.Core/Entidades/ConfiguracionSucursal.cs` + `ConfiguracionSucursalConfiguration.cs` + migración `AddConfiguracionSucursal`.
- `src/SistemaFuneraria.Application/Servicios/Configuracion/ConfiguracionSucursalService.cs` y `src/SistemaFuneraria.Api/Controllers/V1/ConfiguracionController.cs`: `GET/PUT /api/v1/configuracion/sucursal` y `GET/POST/PUT/PATCH /api/v1/configuracion/usuarios` con protección de auto-bloqueo para el Administrador en sesión.
- Conectores Web: `configuracionApi.js`, `configuracionPage.js` y `/Configuracion/Index.cshtml`.

## Verificación
- Validado por HTTP en `VencimientosYReportesTests.ConfiguracionSucursalYUsuarios_DebePersistirYPrevenirAutoBloqueo` y `Milestone9WebTests`.
