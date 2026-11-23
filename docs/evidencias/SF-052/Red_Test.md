# Evidencia Red Test — SF-052: Wizard Paso 1 (`Cliente`) Conectado a `clientesApi.js`

## Estado Inicial (Antes de la Conexión API)
En la inspección previa del Paso 1 del Wizard (`/Ventas/Nueva`), se detectaron las siguientes brechas contra los requisitos de `SF-052`:

1. `src/SistemaFuneraria.Web/wwwroot/js/ventas/paso1Cliente.js` mantenía un arreglo `clientesDemo` en memoria y concatenaba HTML con `innerHTML` en el dropdown de sugerencias sin invocar `clientesApi.listar`.
2. `src/SistemaFuneraria.Web/Pages/Ventas/Partials/_Paso1Cliente.cshtml` contenía valores precargados hardcoded (`value="Mariana"`, `value="Torres Aguirre"`, `value="mariana.torres@email.mx"`) y carecía del campo obligatorio `Dirección` (`Cliente.Crear`) y del campo oculto `#hdnClienteId`.
3. `WizardApiConnectionTests` no existía aún para garantizar el uso de los wrappers reales de API (`clientesApi.js`, `lotesApi.js`, `simuladorApi.js`) y guardia JWT/RBAC.

## Validación fallida esperada antes del cambio
```text
dotnet test SistemaFuneraria.sln --filter "FullyQualifiedName~WizardApiConnectionTests"
FAIL: Paso1ClientePartial_DebeIncluirDireccionYSinHardcode
  Expected html not to contain 'value="Mariana"' and to contain 'txtClienteDireccion'
FAIL: WizardScripts_DebenConsumirApisRealesYSoportarUmd
  Expected paso1Cliente.js to contain 'clientesApi' and not contain 'clientesDemo'
```
