# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-026 |
| **Título del Ticket** | Layout Maestro Figma "Sistema Funerario · Gestión Memorial" (*Capturas 1–6*): Sidebar Verde Olivo y Topbar Jardines del Valle |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 18:00 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-026-layout-maestro |

---

### 1. Objetivo de la Prueba
Demostrar que la aplicación web carecía del cascarón de diseño memorial (Master Layout Funeraria):
1. El archivo `_Layout.cshtml` utilizaba una plantilla genérica oscura sin la estética memorial Serenia.
2. Inexistencia del Topbar con selector de sucursal *"Jardines del Valle"*, campana de notificaciones y perfil de usuario *"AM Andrea Morales · Administradora"*.
3. Inexistencia de la Sidebar verde olivo estructurada en secciones **OPERACIÓN** y **ADMINISTRACIÓN**, con navegación controlada, badge dorado "8" en Caja y cobranza y tarjeta de soporte al pie.

---

### 2. Datos de Entrada Utilizados
* **Archivo evaluado:** `Pages/Shared/_Layout.cshtml`
* **Elementos DOM requeridos:**
  * `.funeraria-sidebar` con color de fondo verde olivo (`#2D4224`).
  * `.funeraria-brand` con logo memorial institucional y bajada *"Gestión memorial"*.
  * Enlaces de navegación con control estricto: enlaces no implementados con `href="#"` y `onclick="return false;"`.
  * Badge dorado `.funeraria-badge-gold` con el número "8" en Caja y cobranza.
  * Topbar `.funeraria-topbar` con selector de sucursal y perfil de usuario.

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~LayoutMaestroTests" --logger "console;verbosity=normal"
```

---

### 4. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

[FAIL] SistemaFuneraria.Tests.Unit.Web.LayoutMaestroTests.Layout_DebeContenerSidebarVerdeOlivoYTopbarJardinesDelValle
Error Message:
 Expected element '.funeraria-sidebar' to exist, but found null.
 Expected element '.funeraria-topbar' with branch 'Jardines del Valle' and user 'Andrea Morales', but found default layout.
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at SistemaFuneraria.Tests.Unit.Web.LayoutMaestroTests.Layout_DebeContenerSidebarVerdeOlivoYTopbarJardinesDelValle()

Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 44 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló deterministamente en fase **ROJO**:
* El layout base no incluía los estilos memoriales (`funeraria-theme.css`), el fondo crema (`#F4F1EA`) ni los componentes Topbar y Sidebar del Figma institucional.
