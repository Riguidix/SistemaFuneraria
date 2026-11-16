# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-062 |
| **Título del Ticket** | Wizard Paso 4 (Confirmación — Captura 8: Resumen 2x2, Checkbox Confirmo que los datos fueron revisados con el cliente y Botón ✓ Generar contrato) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 16/11/2026 17:34 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaNuevaVentaPaso4 |

---

### 1. Caso de Prueba Implementado (Fase RED)
Se definieron las pruebas unitarias en `tests/SistemaFuneraria.Tests/Unit/Web/Paso4ConfirmacionTests.cs` para validar la lógica del modelo de confirmación y el bloqueo/desbloqueo del botón de generar contrato:
* `PuedeGenerarContrato_ConDatosCompletosYCheckboxMarcado_DebeSerVerdadero`: Valida que con el titular completo, lote, precio y el checkbox `DatosRevisadosConCliente = true`, el método retorne `true` y los resúmenes formateados coincidan con la Captura 8 de Figma.
* `PuedeGenerarContrato_SinCheckboxMarcado_DebeSerFalso`: Valida que si el checkbox no está marcado (`false`), el método retorne `false` impidiendo la generación del contrato.

---

### 2. Evidencia de Ejecución RED (Fallo Verificado)
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

```text
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\Paso4ConfirmacionTests.cs(13,28): error CS0246: El nombre del tipo o del espacio de nombres 'ContratoConfirmacionDto' no se encontró (¿falta una directiva using o una referencia de ensamblado?)
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\Paso4ConfirmacionTests.cs(44,28): error CS0246: El nombre del tipo o del espacio de nombres 'ContratoConfirmacionDto' no se encontró (¿falta una directiva using o una referencia de ensamblado?)

ERROR al compilar.
    0 Advertencia(s)
    2 Errores
```

---

### 3. Conclusión de la Fase ROJO
La prueba falla de forma genuina y esperada porque aún no se han implementado `ContratoConfirmacionDto`, la vista parcial `_Paso4Confirmacion.cshtml` ni el script `paso4Confirmacion.js`.
