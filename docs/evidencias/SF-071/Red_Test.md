# Evidencia TDD / Frontend UI: Fase ROJA (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-071 |
| **Título del Ticket** | Expediente del Contrato (`Pages/Contratos/Detalle.cshtml`), tabla de amortización con 24 mensualidades y panel de gestión para Administrador |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) / SF-027 |
| **Fecha y Hora de Ejecución** | 18/11/2026 21:22 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaContratosDetalle |
| **Resultado** | FALLO ESPERADO (Fase RED - DetalleModel y vista Detalle.cshtml no implementados aún) |

---

### 1. Caso de Prueba Implementado (Fase RED)
Se definió la suite de pruebas unitarias `ContratosDetallePageTests` en `tests/SistemaFuneraria.Tests/Unit/Web/ContratosDetallePageTests.cs` para validar la lógica y reglas financieras del expediente de contrato:
1. `Detalle_OnGet_ConFolioPorDefecto_DebeCargarContratoYAmortizacion24Cuotas`: Valida la carga del contrato de muestra `CTR-2025-0189`, titular `Laura Castañeda`, lote `JC-C-118`, plan de $150,000 con enganche del 15% ($22,500.00), saldo insoluto de $127,500.00 a 24 meses y mensualidad de $6,187.50, junto con el desglose de 24 cuotas francesas con saldo remanente final en $0.00.
2. `Detalle_OnGet_ConFolioExistente_DebePersonalizarTitularYLote`: Resolución y carga contextual dinámica para contratos existentes como `CTR-2024-0118` (`Elena Vargas`, `RO-B-027`).

---

### 2. Evidencia de Ejecución RED (Fallo de Compilación Verificado)
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

```text
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\ContratosDetallePageTests.cs(13,24): error CS0246: El nombre del tipo o del espacio de nombres 'DetalleModel' no se encontró (¿falta una directiva using o una referencia de ensamblado?)
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\ContratosDetallePageTests.cs(39,24): error CS0246: El nombre del tipo o del espacio de nombres 'DetalleModel' no se encontró (¿falta una directiva using o una referencia de ensamblado?)

ERROR al compilar.
    0 Advertencia(s)
    2 Errores
```

---

### 3. Conclusión de la Fase ROJO
La suite falla de manera controlada y esperada porque el modelo de página `DetalleModel.cs` y la vista Razor `Pages/Contratos/Detalle.cshtml` no existían previamente en el sistema.
