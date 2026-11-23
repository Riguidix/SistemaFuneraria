# Evidencia Green Test — SF-052: Wizard Paso 1 (`Cliente`) Conectado a `clientesApi.js`

## Cambios Implementados
1. **`src/SistemaFuneraria.Web/Pages/Ventas/Nueva.cshtml`:** Carga `clientesApi.js`, `lotesApi.js`, `simuladorApi.js` y `wizardState.js`, e incorpora el contenedor `#wizardAccesoRestringido` (403) y `#wizardContenidoPrincipal`.
2. **`src/SistemaFuneraria.Web/Pages/Ventas/Partials/_Paso1Cliente.cshtml`:** Elimina valores demo hardcoded, agrega `#hdnClienteId`, `#badgeClienteSeleccionado` y `#txtClienteDireccion` requerido.
3. **`src/SistemaFuneraria.Web/wwwroot/js/ventas/paso1Cliente.js`:** Refactorizado a módulo UMD testeable (`window.paso1Cliente` / `module.exports`) con:
   - Guardia de sesión JWT (`/Cuenta/Login`) y RBAC (`Administrador`, `Vendedor`; 403 para `Cajero`).
   - Autocomplete reactivo conectado a `clientesApi.listar(query)` con debounce (250 ms) y renderizado seguro vía `document.createElement` + `textContent`.
   - Registro automático vía `clientesApi.registrar(payload)` al capturar titular nuevo y persistencia en `wizardState.updateDraft(...)`.
4. **Pruebas:** `WizardApiConnectionTests` (xUnit) y suite JS en Node validando normalización, validaciones de 6 campos, guardia de sesión y construcción de payloads.

## Resultado de Ejecución
```text
dotnet build SistemaFuneraria.sln --configuration Release
dotnet test SistemaFuneraria.sln --configuration Release --no-build --filter "FullyQualifiedName~WizardApiConnectionTests|FullyQualifiedName~Paso1ClienteTests|FullyQualifiedName~SimuladorApiAndWizardStateTests"
Correctas! - 0 errores
```
