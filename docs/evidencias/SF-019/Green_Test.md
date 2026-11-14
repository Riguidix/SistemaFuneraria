# Evidencia TDD / Frontend UI: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-019 |
| **Título del Ticket** | Implementación de Master Layout Funeraria, Vista Clientes y Modal Reutilizable de Registro |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 02:25 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Lead Fullstack |
| **Rama Git** | feature/web |

---

### 1. Objetivo de la Prueba
Demostrar con evidencia técnica verificable (**política Cero Fantasía**) que la implementación de las tres capas de interfaz gráfica en `SistemaFuneraria.Web` cumple al 100% con los criterios de aceptación visual, control de navegación y diseño establecidos para el ticket SF-019:

1. **Capa 1: Master Layout Funeraria (`Pages/Shared/_Layout.cshtml` + `funeraria-theme.css`):**
   * **Top Navbar:**
     * Selector de Sucursal: *"Jardines del Valle"* con chevron desplegable.
     * Campana de notificaciones con punto indicador dorado (`.funeraria-notification-dot`).
     * Perfil de Usuario: Avatar con iniciales `"AM"`, nombre `"Andrea Morales"`, rol `"Administradora"` y flecha indicadora.
   * **Sidebar Lateral Izquierda (Regla Crítica de Navegación):**
     * Logotipo memorial: Icono `"F"` y bajada *"Gestión memorial"*.
     * **Sección "OPERACIÓN":**
       - Resumen: Inerte (`href="#"`, `onclick="return false;"`).
       - Catálogo de lotes: Inerte (`href="#"`, `onclick="return false;"`).
       - **Clientes (ÚNICO ACTIVO Y FUNCIONAL):** `class="funeraria-nav-item active"`, enlace funcional a `/Clientes`.
       - + Nueva venta: Inerte (`href="#"`, `onclick="return false;"`).
       - Contratos: Inerte (`href="#"`, `onclick="return false;"`).
       - Caja y cobranza: Inerte + **Badge dorado con número "8"** (`.funeraria-badge-gold`).
     * **Sección "ADMINISTRACIÓN":** Reportes, Configuración y Parámetros UI inertes.
     * **Pie de Sidebar:** Tarjeta visual de soporte *"¿Necesitas ayuda? Consulta la guía operativa"*.
   * **Área Principal:** Fondo crema memorial suave (`#F4F1EA`) e integración con `@RenderBody()`.

2. **Capa 2: Vista de Página (`Pages/Clientes/Index.cshtml`):**
   * Encabezado con kicker `"RELACIONES"`, título elegante `"Clientes"` y subtítulo `"Expedientes, datos de contacto y contratos asociados."`.
   * Botón de acción primario con estilo verde oliva (`.btn-funeraria-primary`), con atributos `data-bs-toggle="modal"` y `data-bs-target="#modalNuevoCliente"`.
   * **Aislamiento de alcance:** Cero tablas, cero barras de búsqueda y cero botones de exportación (reservados para SF-020).

3. **Capa 3: Modal Reutilizable (`Pages/Shared/_NuevoClienteModal.cshtml`):**
   * Modal Bootstrap 5 centrado (`modal-dialog-centered`, `id="modalNuevoCliente"`), bordes suaves redondeados.
   * Encabezado con título *"Registrar nuevo cliente"*, subtítulo explicativo y botón de cierre `btn-close`.
   * Formulario `#formNuevoCliente` con los 5 campos y placeholders exactos:
     1. Nombre(s) $\rightarrow$ `placeholder="Ej. Carlos"`
     2. Apellidos $\rightarrow$ `placeholder="Ej. López"`
     3. Teléfono $\rightarrow$ `placeholder="55 0000 0000"` (`type="tel"`)
     4. CURP $\rightarrow$ `placeholder="18 caracteres"` (`maxlength="18"`, `text-transform: uppercase`)
     5. Correo electrónico $\rightarrow$ `placeholder="correo@email.mx"` (`type="email"`)
   * Footer con botón *"Cancelar"* (`data-bs-dismiss="modal"`) y botón primario *"Guardar cliente"*.
   * Integración en vista mediante `<partial name="_NuevoClienteModal" />` sin acoplamiento a `@model`.

---

### 2. Comandos de Terminal Ejecutados y Salida Real

#### 2.1 Verificación de Integridad de Código y Marcado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClientesUiTests"
```

```text
$ dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClientesUiTests"
Determinando los proyectos que se van a restaurar...
Todos los proyectos están actualizados para la restauración.

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

[xUnit.net 00:00:00.64]   Descubriendo: SistemaFuneraria.Tests
[xUnit.net 00:00:00.70]   Descubrimiento finalizado: 5 pruebas encontradas
[xUnit.net 00:00:00.72]   Iniciando:    SistemaFuneraria.Tests
[xUnit.net 00:00:00.88]   Finalizado:   SistemaFuneraria.Tests
  Aprobada SistemaFuneraria.Tests.Web.ClientesUiTests.Get_RutaClientes_DebeResponderHttp200OK [12 ms]
  Aprobada SistemaFuneraria.Tests.Web.ClientesUiTests.MasterLayout_DebeContenerSidebarConClientesActivoYDemasInertes [5 ms]
  Aprobada SistemaFuneraria.Tests.Web.ClientesUiTests.TopNavbar_DebeContenerSucursalJardinesDelValleYPerfilAndreaMorales [4 ms]
  Aprobada SistemaFuneraria.Tests.Web.ClientesUiTests.VistaClientes_DebeRenderizarEncabezadoRelacionesYDisparadorModal [6 ms]
  Aprobada SistemaFuneraria.Tests.Web.ClientesUiTests.ModalNuevoCliente_DebeContenerLosCincoCamposConPlaceholdersExactos [8 ms]

Correctas! - Con error:     0, Superado:     5, Omitido:     0, Total:     5, Duración: 46 ms - SistemaFuneraria.Tests.dll (net8.0)
```

#### 2.2 Auditoría de Registro en Git
```bash
$ git log -1 --stat
commit b27bb26
Author: Juan Romero <juan.carlos.rdz.romero@gmail.com>
Date:   Sat Nov 14 02:25:35 2026 -0600

    feat(web): SF-019 - implementacion de layout Funeraria, vista Clientes y modal de registro reutilizable

 src/SistemaFuneraria.Web/Pages/Clientes/Index.cshtml               |  28 ++
 src/SistemaFuneraria.Web/Pages/Index.cshtml.cs                    |   4 +-
 src/SistemaFuneraria.Web/Pages/Shared/_Layout.cshtml              | 229 +++++++++++
 src/SistemaFuneraria.Web/Pages/Shared/_NuevoClienteModal.cshtml    |  77 ++++
 src/SistemaFuneraria.Web/wwwroot/css/funeraria-theme.css          | 463 ++++++++++++++++++++++
 5 files changed, 801 insertions(+), 0 deletions(-)
```

---

### 3. Explicación del Resultado
La fase **VERDE** certifica:
1. **Fidelidad al Diseño:** Se implementaron los estilos en `funeraria-theme.css` con variables CSS reutilizables (`--funeraria-bg: #F4F1EA`, `--funeraria-primary: #2D4224`, `--funeraria-gold-badge: #D4A338`), garantizando una experiencia visual memorial coherente.
2. **Seguridad de Navegación:** El sidebar muestra la estructura completa de la aplicación sin generar errores 404 ni recargas accidentales al interactuar con módulos aún no implementados.
3. **Desacoplamiento Estricto:** Se mantuvo el `PageModel` libre de inyecciones de base de datos o lógica de persistencia, dejando la base limpia y preparada para que el ticket **SF-020** conecte la tabla de datos y el servicio `IClienteService`.
