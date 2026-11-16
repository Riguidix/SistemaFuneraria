# Evidencia TDD / Frontend UI: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-052 |
| **Título del Ticket** | Wizard Paso 1 (Cliente — Captura 5: Autocomplete de Cliente Existente o Captura Inline de Datos del Titular) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 16/11/2026 16:35 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaNuevaVentaPaso1 |
| **Resultado** | EXITOSO (297 pruebas superadas, 0 con error) |

---

### 1. Implementación Realizada (Fase GREEN)
1. **Vista Parcial Modular (`src/SistemaFuneraria.Web/Pages/Ventas/Partials/_Paso1Cliente.cshtml`)**:
   - Tarjeta blanca con esquinas redondeadas (`bg-white rounded-4 shadow-sm border p-4 p-md-5`).
   - Título `Datos del cliente` y subtítulo explicativo.
   - Buscador reactivo con input group (`#inputBuscarCliente`), icono de lupa y botón para limpiar búsqueda (`#btnLimpiarBusqueda`).
   - Dropdown flotante de resultados (`#resultadosBusquedaCliente`) con scroll de sugerencias de clientes.
   - Divisor centrado `o captura sus datos`.
   - Formulario de captura rápida con campos: `Nombre(s)`, `Apellidos`, `CURP` (18 caracteres), `Teléfono` (10 dígitos) y `Correo electrónico`.
   - Alerta inline para mensajes de validación.
   - Botón `Anterior` (inhabilitado en Paso 1) y botón `Continuar →` en verde bosque institucional `#2D4224`.
2. **Lógica e Interacción en Frontend (`src/SistemaFuneraria.Web/wwwroot/js/ventas/paso1Cliente.js`)**:
   - Búsqueda reactiva instantánea por nombre, CURP o teléfono.
   - Autocompletado de los 5 campos al seleccionar un cliente sugerido.
   - Validación estricta en frontend: nombres y apellidos obligatorios, teléfono a 10 dígitos y correo con formato válido.
   - Persistencia automática del titular en `sessionStorage` (`sf_nueva_venta_draft`) y emisión del evento `sf:paso1-completado` y `sf:draft-updated`.
3. **DTO de Validación (`src/SistemaFuneraria.Web/Pages/Ventas/ClienteDraftDto.cs`)**:
   - Encapsula la lógica de validación de negocio y `NombreCompleto`.
4. **Integración en Shell (`src/SistemaFuneraria.Web/Pages/Ventas/Nueva.cshtml`)**:
   - Inyección modular del partial view `<partial name="Partials/_Paso1Cliente" model="Model" />`.

---

### 2. Evidencia de Ejecución de Pruebas GREEN
```bash
dotnet test --filter "FullyQualifiedName~Paso1ClienteTests"
```

```text
Serie de pruebas para C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:     3, Omitido:     0, Total:     3, Duración: 3 ms - SistemaFuneraria.Tests.dll (net8.0)
```

Suite completa de pruebas unitarias:
```bash
dotnet test --filter "FullyQualifiedName!~Integration"
```

```text
Correctas! - Con error:     0, Superado:   297, Omitido:     0, Total:   297, Duración: 869 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 3. Conclusión de Calidad
El Paso 1 cumple al 100% con los criterios de aceptación de la Captura 5 de Figma, con validaciones sólidas y persistencia en borrador, manteniendo un Blast Radius inferior al límite de 210 LOCs.
