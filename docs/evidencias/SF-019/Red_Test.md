# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-019 |
| **Título del Ticket** | Implementación de Master Layout Funeraria, Vista Clientes y Modal Reutilizable de Registro |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 01:50 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Lead Fullstack |
| **Rama Git** | feature/web |

---

### 1. Objetivo de la Prueba
Demostrar que la aplicación web carecía del cascarón de diseño memorial (Master Layout Funeraria), la vista de Clientes (`Pages/Clientes/Index.cshtml`) y el componente modal reutilizable `#modalNuevoCliente`.

Antes de implementar este ticket:
1. Al intentar navegar hacia `/Clientes`, el servidor web arrojaba un código de estado **HTTP 404 (Not Found)** debido a la inexistencia del archivo `Index.cshtml` bajo la carpeta `Pages/Clientes/`.
2. El layout base (`_Layout.cshtml`) utilizaba una plantilla genérica oscura no alineada al diseño de Figma, careciendo de la sidebar memorial con navegación controlada, del selector de sucursal *"Jardines del Valle"* y del perfil de usuario *"Andrea Morales"*.
3. No existía el componente modal para la captura de los 5 datos obligatorios del titular (`Nombre`, `Apellidos`, `Teléfono`, `CURP`, `Correo electrónico`).

---

### 2. Datos de Entrada Utilizados
* **Ruta HTTP solicitada:** `/Clientes`
* **Elementos DOM esperados en la arquitectura:**
  * Master Layout: `.funeraria-sidebar`, `.funeraria-brand` (icono "F", bajada "Gestión memorial"), `.funeraria-nav-item.active` (apuntando a `/Clientes`).
  * Enlaces inertes protegidos: `href="#"` en Resumen, Catálogo, Nueva venta, Contratos, Caja (badge "8"), Reportes, Configuración y Parámetros UI.
  * Vista Clientes: `.funeraria-kicker` con texto `"RELACIONES"`, título `"Clientes"`, subtítulo `"Expedientes, datos de contacto y contratos asociados."` y botón disparador primario hacia `#modalNuevoCliente`.
  * Modal Reutilizable: Formulario `#formNuevoCliente` con los 5 campos y placeholders exactos (`"Ej. Carlos"`, `"Ej. López"`, `"55 0000 0000"`, `"18 caracteres"`, `"correo@email.mx"`).

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClientesUiTests"
```

---

### 4. Salida de Terminal (Falla Esperada)
```text
$ dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClientesUiTests"
Determinando los proyectos que se van a restaurar...
Todos los proyectos están actualizados para la restauración.

[xUnit.net 00:00:00.62]   Descubriendo: SistemaFuneraria.Tests
[xUnit.net 00:00:00.68]   Descubrimiento finalizado: 1 pruebas encontradas
[xUnit.net 00:00:00.70]   Iniciando:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.85]   Finalizado:   SistemaFuneraria.Tests

[FAIL] SistemaFuneraria.Tests.Web.ClientesUiTests.Get_RutaClientes_DebeExistirYRenderizarLayoutFuneraria
Error Message:
 Expected response status code to be HttpStatusCode.OK (200), but found HttpStatusCode.NotFound (404).
 Missing endpoint: /Clientes. File 'Pages/Clientes/Index.cshtml' does not exist.
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at FluentAssertions.Execution.TestEngine.Throw(String message)
   at SistemaFuneraria.Tests.Web.ClientesUiTests.Get_RutaClientes_DebeExistirYRenderizarLayoutFuneraria()

Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 54 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló exactamente como se esperaba (fase **ROJO**):
* La ruta `/Clientes` no existía en el proyecto `SistemaFuneraria.Web`, provocando un fallo de resolución de ruta en el router de Razor Pages (HTTP 404).
* El proyecto no contaba con los estilos memoriales (`funeraria-theme.css`), el contenedor principal de fondo crema (`#F4F1EA`) ni el componente modal `_NuevoClienteModal.cshtml`.
