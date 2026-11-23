# Evidencia Green Test — SF-096: Auditoría Visual y Accesibilidad WCAG 2.2 AA

## Verificación y Corrección Aplicada
- Auditoría automatizada en `tests/SistemaFuneraria.Tests/Unit/Web/AccesibilidadWcagAuditTests.cs`:
  - Verifica idioma `lang="es-MX"` y atributos `aria-*` en `_Layout.cshtml` y `Pages/Cuenta/Login.cshtml` (incluyendo corrección de `aria-label="Mostrar u ocultar contraseña"` en `#btnTogglePassword` y `aria-label` en `#loginForm`).
  - Verifica cifras tabulares (`sf-money` / `text-tabular`) y atributos ARIA en las vistas financieras principales (`Resumen`, `Catalogo`, `Clientes`, `Contratos`, `Contratos/Detalle`, `Caja`, `Caja/Recibo`, `Reportes`, `Configuracion`).

## Resultado
- Todas las pruebas de `AccesibilidadWcagAuditTests` superadas al 100%. *(Auditoría visual manual en navegador reservada al usuario).*
