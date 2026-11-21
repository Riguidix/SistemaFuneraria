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

### 1. Caso de Prueba Implementado (Fase RED)
Se definió la suite de pruebas unitarias `ClientesExportarTests` en `tests/SistemaFuneraria.Tests/Unit/Web/ClientesExportarTests.cs` para garantizar las especificaciones técnicas de exportación de datos a hoja de cálculo:
1. `GenerarNombreArchivo_DebeTenerFormatoCorrecto`: Formato oficial `directorio_clientes_YYYYMMDD.csv`.
2. `GenerarCsvBytes_DebeIncluirBomUtf8YCabeceraEsperada`: Inclusión obligatoria del BOM UTF-8 (`\uFEFF` / bytes `0xEF, 0xBB, 0xBF`) para compatibilidad nativa con Microsoft Excel en español (sin romper acentos ni 'ñ'), y cabecera estricta: `Cliente,Teléfono,CURP,Contratos,Saldo,Último Pago,Estado`.
3. `GenerarCsvBytes_ConClientes_DebeFormatearFilasCorrectamente`: Formateo de los campos del padrón visible (Laura Castañeda, Roberto Méndez, Elena Vargas, Jorge Salinas).
4. `GenerarCsvBytes_ConComasOComillas_DebeEscaparCampos`: Escape RFC 4180 de comas y comillas en nombres o valores.

---

### 2. Evidencia de Ejecución RED (Fallo de Compilación Verificado)
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

```text
tests/SistemaFuneraria.Tests/Unit/Web/ClientesExportarTests.cs(17,21): error CS0103: El nombre 'ClientesExportadorCsv' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Web/ClientesExportarTests.cs(27,47): error CS0246: El nombre del tipo o del espacio de nombres 'ClienteExportDto' no se encontró (¿falta una directiva using o una referencia de ensamblado?)
tests/SistemaFuneraria.Tests/Unit/Web/ClientesExportarTests.cs(30,21): error CS0103: El nombre 'ClientesExportadorCsv' no existe en el contexto actual
tests/SistemaFuneraria.Tests/Unit/Web/ClientesExportarTests.cs(42,37): error CS0246: El nombre del tipo o del espacio de nombres 'ClienteExportDto' no se encontró (¿falta una directiva using o una referencia de ensamblado?)

ERROR al compilar.
    0 Advertencia(s)
    4 Errores
```

---

### 3. Conclusión de la Fase ROJO
La suite de pruebas falla de forma genuina y esperada (Fase RED) porque aún no se han creado el DTO de transferencia `ClienteExportDto.cs` ni el generador de streams `ClientesExportadorCsv.cs`.
