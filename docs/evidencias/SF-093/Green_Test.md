# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-093 |
| **Título del Ticket** | Suite QA HTTP CP-07 (Abono a Capital $7,523.51) y CP-08 (Liquidación Anticipada $77,893.39) |
| **Caso de Prueba Relacionado** | CP-07, CP-08 (Milestone v0.9.0) |
| **Fecha de Ejecución** | 23/11/2026 |
| **Tester Responsable** | Edgar Acevedo (Carril 4 — DevOps & QA) |
| **Rama Git** | `feature/SF-093-qa-http-cp07-cp08-abono-liquidacion` |

---

### 1. Resumen de la Certificación QA
Se implementaron los endpoints y comandos de aplicación correspondientes a operaciones avanzadas de cobranza en `CobranzaController.cs` y `CobranzaService.cs`, logrando la certificación HTTP completa en `tests/SistemaFuneraria.Tests/Integration/Api/AbonoYLiquidacionControllerTests.cs`:

1. **CP-07**:
   - Creación de contrato financiado a 12 meses sobre lote con 20% de enganche.
   - Cobro ordinario secuencial de mensualidades 1, 2 y 3.
   - Aplicación de abono extraordinario a capital por $10,000.00 vía `POST /api/v1/cobranza/abono-capital`.
   - Generación de recibo oficial con `TipoPago: Abono a capital` y desglose de $10,000.00 íntegro a capital con $0.00 interés.
   - Recálculo de las cuotas restantes (4..12) en la tabla de amortización persistida vía `GET /api/v1/ventas/{id}/amortizacion`, fijando la cuota en $7,523.51 / $7,523.67 y extinguiendo el saldo final en $0.00.

2. **CP-08**:
   - Consulta previa del contrato en ventanilla de caja vía `GET /api/v1/cobranza/contrato/{folio}` obteniendo el monto de liquidación con descuento comercial oficial del 10% por pronto pago sobre intereses futuros no devengados ($77,893.39).
   - Aplicación de liquidación anticipada vía `POST /api/v1/cobranza/liquidacion`.
   - Emisión de recibo oficial con `TipoPago: Liquidación`.
   - Verificación del contrato en caja: saldo insoluto en $0.00 y estado visual en `Liquidado`.

3. **Validaciones de Seguridad y Robustez**:
   - Rechazo con `400 Bad Request` ante parámetros inválidos o ausencia de clave de idempotencia.

---

### 2. Comando Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~AbonoYLiquidacionControllerTests"
```

---

### 3. Salida de Terminal (Fase GREEN)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:     3, Omitido:     0, Total:     3, Duración: 137 ms - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 4. Criterio de Aceptación y DoD
- [x] Endpoint `POST /api/v1/cobranza/abono-capital` funcional con idempotencia y control de concurrencia.
- [x] Endpoint `POST /api/v1/cobranza/liquidacion` funcional con descuento por pronto pago del 10% y cierre de contrato.
- [x] Suite de pruebas HTTP pasando al 100% (3/3).
- [x] Cero rutas absolutas en evidencias y cero comentarios de IA en código fuente.
