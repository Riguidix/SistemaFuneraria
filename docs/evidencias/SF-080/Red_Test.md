# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-080 |
| **Título del Ticket** | Pantalla Figma /Caja/Index — Parte 1 (Captura 10): Buscador CTR-2024-0118, Card de Cliente con Badge 1 vencido y 3 Tarjetas Selector Mensualidad / Abono a capital / Liquidación |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 14/11/2026 22:30 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | feature/SF-080-caja-cobranza-index |

---

### 1. Objetivo de la Prueba
Demostrar la ausencia de los elementos operativos de la Parte 1 de la pantalla de Caja y Cobranza según Figma (*Captura 10*):
1. Inexistencia de la cabecera `OPERACIÓN DIARIA` y título `Caja y cobranza` con el badge `● Caja abierta · Desde las 09:00`.
2. Inexistencia del buscador reactivo por folio de contrato (`CTR-2024-0118`).
3. Ausencia de la tarjeta del titular con avatar `EV` (*Elena Vargas*), lote asociado (`RO-B-027`) y badge semántico `1 vencido`.
4. Ausencia del grupo de 3 tarjetas selectoras de tipo de movimiento (`Mensualidad`, `Abono a capital`, `Liquidación`).

---

### 2. Datos de Entrada Utilizados
* **Ruta evaluada:** `/Cobranza` o `/Caja`
* **Elementos DOM requeridos:**
  * Badge superior `.badge-caja-abierta` con indicador verde de apertura de corte.
  * Input de búsqueda `#inputBuscarContrato` con valor esperado `"CTR-2024-0118"`.
  * Card `#cardClienteCobro` mostrando: Titular `Elena Vargas`, Contrato `CTR-2024-0118`, Lote `RO-B-027` y badge `.badge-vencido` con texto `"1 vencido"`.
  * Selectores de modalidad de pago con clases `.selector-movimiento`:
    1. Mensualidad (`Pago ordinario`)
    2. Abono a capital (`Reduce mensualidad`)
    3. Liquidación (`Cierre anticipado`)

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

[FAIL] SistemaFuneraria.Tests.Unit.Web.CobranzaPageTests.Caja_DebeRenderizarBuscadorContratoYSelectoresMovimiento
Error Message:
 Expected element '#inputBuscarContrato' to exist, but found null.
 Expected 3 movement selector cards (Mensualidad, Abono a capital, Liquidación), but found 0 elements.
 Stack Trace:
   at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message)
   at SistemaFuneraria.Tests.Unit.Web.CobranzaPageTests.Caja_DebeRenderizarBuscadorContratoYSelectoresMovimiento()

Failed!  - Failed: 1, Passed: 0, Skipped: 0, Total: 1, Duration: 39 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Explicación del Resultado
La prueba falló deterministamente en fase **ROJO**:
* La vista `/Cobranza/Index.cshtml` carecía de los componentes interactivos de la *Captura 10* de Figma para búsqueda de contratos, despliegue del titular y selección de modalidad de cobro.
