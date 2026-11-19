# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-035 |
| **Título del Ticket** | Modal Figma Registrar nuevo cliente y Edición (Captura 4): Formulario en 2 columnas con validación y guardado fetch |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 19:50 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-035-modal-nuevo-cliente |

---

### 1. Objetivo de la Prueba
Demostrar que el componente modal `#modalNuevoCliente` carecía de los 5 campos en distribución de 2 columnas requeridos por Figma (*Captura 4*):
1. Inexistencia de campos en layout de 2 columnas para `Nombre(s)` (`#modalClienteNombre`) y `Apellidos` (`#modalClienteApellidos`).
2. Inexistencia de campos en layout de 2 columnas para `Teléfono` (`#modalClienteTelefono`) y `CURP` (`#modalClienteCurp` en mayúsculas).
3. Inexistencia del campo `Correo electrónico` (`#modalClienteEmail`) y botones de acción `Cancelar` y `Guardar cliente` (`#btnGuardarClienteModal`).

---

### 2. Datos de Entrada Utilizados
* **Componente evaluado:** `Pages/Shared/_NuevoClienteModal.cshtml`
* **Campos requeridos:**
  * `Nombre(s)`: `placeholder="Ej. Carlos"`
  * `Apellidos`: `placeholder="Ej. López"`
  * `Teléfono`: `placeholder="55 0000 0000"`
  * `CURP`: `placeholder="18 caracteres"`, `maxlength="18"`, `text-transform: uppercase`
  * `Correo electrónico`: `placeholder="correo@email.mx"`
  * Botones: `Cancelar` (`data-bs-dismiss="modal"`), `Guardar cliente` (`.btn-funeraria-primary`).

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~NuevoClienteModalTests" --logger "console;verbosity=normal"
```

---

### 4. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

[FAIL] SistemaFuneraria.Tests.Unit.Web.NuevoClienteModalTests.Modal_DebeContenerFormularioDosColumnasYCincoCampos
Error Message:
 Expected modal '#modalNuevoCliente' to exist with form '#formNuevoCliente', but found null.
 Missing form fields: modalClienteNombre, modalClienteApellidos, modalClienteTelefono, modalClienteCurp, modalClienteEmail.
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at SistemaFuneraria.Tests.Unit.Web.NuevoClienteModalTests.Modal_DebeContenerFormularioDosColumnasYCincoCampos()

Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 39 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló deterministamente en fase **ROJO**:
* El componente parcial modal aún no estaba integrado en el proyecto Web ni referenciado en la vista principal.
* Queda establecida la necesidad técnica de maquetar el modal con distribución de 2 columnas según las especificaciones de diseño Figma.
