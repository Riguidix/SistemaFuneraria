# Evidencia TDD / Frontend UI: Fase ROJA (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-037 |
| **Título del Ticket** | Exportación a CSV en botón `Exportar directorio` y Kit de Estados Visuales (Empty State, Loading Skeletons y Toasts) en `/Clientes/Index` (Captura 3) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) / SF-027 |
| **Fecha y Hora de Ejecución** | 18/11/2026 21:03 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaClientesExportar |
| **Resultado** | FALLO ESPERADO (Fase RED - ClientesExportadorCsv y DTO no implementados aún) |

---

### 1. Justificación de la Fase RED
Se diseñó la suite de pruebas unitarias `ClientesExportarTests` para garantizar los requerimientos de exportación a CSV del ticket SF-037:
1. `GenerarNombreArchivo_DebeTenerFormatoCorrecto`: Formato `directorio_clientes_YYYYMMDD.csv`.
2. `GenerarCsvBytes_DebeIncluirBomUtf8YCabeceraEsperada`: Inclusión obligatoria del BOM UTF-8 (`\uFEFF` / `0xEF, 0xBB, 0xBF`) para compatibilidad nativa con Microsoft Excel en español (sin romper acentos ni 'ñ'), y cabecera `Cliente,Teléfono,CURP,Contratos,Saldo,Último Pago,Estado`.
3. `GenerarCsvBytes_ConClientes_DebeFormatearFilasCorrectamente`: Formateo de los campos del padrón visible.
4. `GenerarCsvBytes_ConComasOComillas_DebeEscaparCampos`: Escape RFC 4180 de comas y comillas en nombres o valores.

La prueba falló la compilación debido a la ausencia inicial de `ClienteExportDto` y `ClientesExportadorCsv`.
