# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-081 |
| **Título del Ticket** | Pantalla Figma /Caja/Index — Parte 2 (Captura 10): Formulario Importe a recibir, Método de pago, Referencia, Botón Aplicar pago por $6,248.50, Card Corte del día y Alerta 8 pagos requieren atención |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 22:50 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-081-caja-cobranza-parte2 |

---

### 1. Objetivo de la Prueba
Demostrar la ausencia de los elementos operativos de la Parte 2 de Caja y Cobranza según Figma (*Captura 10*):
1. Inexistencia del formulario de cobro con los campos `Importe a recibir` (`$ 6,248.50`), selector `Método de pago` (`Transferencia bancaria`) y campo `Referencia` (`Folio o últimos 4 dígitos`).
2. Ausencia del botón de acción primario con importe dinámico: `✓ Aplicar pago por $6,248.50`.
3. Ausencia del panel lateral derecho con la tarjeta `Corte del día` (`Saldo inicial: $2,000.00`, `Pagos recibidos: 12`, `Ingresos del día: $74,982.00`, `Total en caja: $76,982.00` y botón `Preparar corte`).
4. Ausencia del contenedor de alerta preventiva `8 pagos requieren atención` y enlace `Revisar cartera vencida →`.

---

### 2. Datos de Entrada Utilizados
* **Ruta evaluada:** `/Cobranza` o `/Caja`
* **Elementos DOM requeridos:**
  * Input `#inputImporteRecibir` con valor predeterminado `"$ 6,248.50"`.
  * Select `#selectMetodoPago` con opción seleccionada `"Transferencia bancaria"`.
  * Input `#inputReferenciaPago` con placeholder `"Folio o últimos 4 dígitos"`.
  * Botón primario `#btnAplicarPago` con texto `"Aplicar pago por $6,248.50"`.
  * Card `.card-corte-dia` con métricas de corte (`Saldo inicial`, `Ingresos del día`, `Total en caja: $76,982.00`).
  * Alerta `.alert-cartera-vencida` con texto `"8 pagos requieren atención"`.

---

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~CobranzaPageTests" --logger "console;verbosity=normal"
```

---

### 4. Salida de Terminal (Falla Esperada)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

[FAIL] SistemaFuneraria.Tests.Unit.Web.CobranzaPageTests.Caja_DebeContenerFormularioAplicarPagoYCorteDelDia
Error Message:
 Expected element '#btnAplicarPago' to exist, but found null.
 Expected element '.card-corte-dia' with Total en caja '$76,982.00' to exist, but found null.
 Expected alert '8 pagos requieren atención' to exist, but found null.
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at SistemaFuneraria.Tests.Unit.Web.CobranzaPageTests.Caja_DebeContenerFormularioAplicarPagoYCorteDelDia()

Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 37 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló deterministamente en fase **ROJO**:
* No se encontraban implementados el formulario de aplicación directa de pagos, el panel de cuadre de corte del día ni el semáforo preventivo de cartera vencida de la *Captura 10*.
