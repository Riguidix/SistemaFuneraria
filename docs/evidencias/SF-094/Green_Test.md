# Evidencia Green Test — SF-094: Vencimientos sin Mora (CP-09), Dashboard, Reportes y Configuración (CODE FREEZE)

## Pruebas (`tests/SistemaFuneraria.Tests/Integration/Api/VencimientosYReportesTests.cs`)
1. `CP09_CuotaAtrasada_ConFakeTimeProvider_DebeMarcarVencidaYSinRecargoMoratorio`: avanza 45 días con `FakeTimeProvider` (`TimeControlledApiFactory`), valida que la cuota pase a `Vencida` con `$0.00` de recargo moratorio, actualice `PagosVencidosCount >= 1` tanto en `GET /api/v1/cobranza/resumen-caja` como en `GET /api/v1/dashboard/resumen`, y permita el cobro al monto original exacto.
2. `DashboardYReportes_DebenRetornarMetricasRealesYValidarRbac`: valida `GET /api/v1/dashboard/resumen` (6 meses, disponibilidad, actividad) y `GET /api/v1/reportes/indicadores` (`200 OK` para Administrador, `403 Forbidden` para Vendedor).
3. `ConfiguracionSucursalYUsuarios_DebePersistirYPrevenirAutoBloqueo`: valida `GET/PUT /api/v1/configuracion/sucursal`, creación de `Cajero` con login inmediato y rechazo `400 Bad Request` al intentar auto-desactivar al Administrador en sesión.

## Resultado Final
```bash
dotnet build --configuration Release
dotnet test --configuration Release --no-build --filter "FullyQualifiedName~Abono|FullyQualifiedName~Vencimientos|FullyQualifiedName~Dashboard|FullyQualifiedName~Reportes|FullyQualifiedName~Configuracion|FullyQualifiedName~Milestone9|FullyQualifiedName~Cobranza|FullyQualifiedName~Caja"
```
Compilación sin advertencias ni errores (`0/0`) y **36/36** pruebas en verde. Pruebas de navegador omitidas según solicitud explícita del usuario.
