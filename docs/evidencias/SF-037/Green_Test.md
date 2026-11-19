# Evidencia TDD / Frontend UI: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-037 |
| **Título del Ticket** | Exportación a CSV en botón `Exportar directorio` y Kit de Estados Visuales (Empty State, Loading Skeletons y Toasts) en `/Clientes/Index` (Captura 3) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) / SF-027 |
| **Fecha y Hora de Ejecución** | 18/11/2026 21:04 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaClientesExportar |
| **Resultado** | EXITOSO (4 pruebas superadas, 0 con error) |

---

### 1. Implementación Realizada (Fase GREEN)
1. **DTO `ClienteExportDto`** (`src/SistemaFuneraria.Web/Pages/Clientes/ClienteExportDto.cs`):
   - Mapeo inmutable de registros para exportación: `Nombre`, `Telefono`, `Curp`, `Contratos`, `Saldo`, `UltimoPago`, `Estado`.
2. **Utilidad `ClientesExportadorCsv`** (`src/SistemaFuneraria.Web/Pages/Clientes/ClientesExportadorCsv.cs`):
   - Inclusión de BOM UTF-8 (`\uFEFF`) para compatibilidad 100% con Microsoft Excel en español (sin romper acentos ni 'ñ').
   - Cabecera oficial: `Cliente,Teléfono,CURP,Contratos,Saldo,Último Pago,Estado`.
   - Nomenclatura dinámica: `directorio_clientes_YYYYMMDD.csv`.
   - Reglas de escape RFC 4180 ante comas y comillas en los valores.

### 2. Resultados de la Suite Unit
- `ClientesExportarTests.GenerarNombreArchivo_DebeTenerFormatoCorrecto`: SUPERADA
- `ClientesExportarTests.GenerarCsvBytes_DebeIncluirBomUtf8YCabeceraEsperada`: SUPERADA
- `ClientesExportarTests.GenerarCsvBytes_ConClientes_DebeFormatearFilasCorrectamente`: SUPERADA
- `ClientesExportarTests.GenerarCsvBytes_ConComasOComillas_DebeEscaparCampos`: SUPERADA
