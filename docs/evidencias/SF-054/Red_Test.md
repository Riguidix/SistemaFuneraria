# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-054 |
| **Título del Ticket** | Wizard Paso 3 (Financiamiento — Captura 7: Toggle Crédito/Contado, Selects Enganche/Plazo y Banner Verde Mensualidad Estimada) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 16/11/2026 16:59 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaNuevaVentaPaso3 |

---

### 1. Caso de Prueba Implementado (Fase RED)
Se definieron las pruebas unitarias en `tests/SistemaFuneraria.Tests/Unit/Web/Paso3FinanciamientoTests.cs` para validar el cálculo financiero y los textos del banner institucional de la Captura 7 de Figma:
* `CalcularFinanciamiento_Credito24Meses20Porciento_DebeCoincidirConFigmaCaptura7`: Valida que para un lote de $120,000 con enganche del 20% ($24,000) a 24 meses plazo, la mensualidad estimada calculada sea exactamente de `$4,887.54`.
* `CalcularFinanciamiento_ModalidadContado_DebeRetornarPagoUnicoSinIntereses`: Valida que en modalidad Contado el banner muestre "Pago único de contado", sin intereses y el monto de lista total ($120,000.00).

---

### 2. Evidencia de Ejecución RED (Fallo Verificado)
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

```text
tests/SistemaFuneraria.Tests/Unit/Web/Paso3FinanciamientoTests.cs(18,25): error CS0103: El nombre 'FinanciamientoCalculadora' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Web/Paso3FinanciamientoTests.cs(34,25): error CS0103: El nombre 'FinanciamientoCalculadora' no existe en el contexto actual

ERROR al compilar.
    0 Advertencia(s)
    2 Errores
```

---

### 3. Conclusión de la Fase ROJO
La prueba falla de forma genuina y esperada porque aún no se han creado el calculador `FinanciamientoCalculadora`, la vista parcial `_Paso3Financiamiento.cshtml` ni el script `paso3Financiamiento.js`.
