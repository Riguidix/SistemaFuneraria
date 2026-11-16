# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-051 |
| **Título del Ticket** | Shell del Wizard /Ventas/Nueva (Capturas 5–8): Stepper Horizontal 1–4 + Panel Derecho RESUMEN DE COTIZACIÓN con Mapa de Lotes |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 16/11/2026 16:08 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaNuevaVentaShell |

---

### 1. Caso de Prueba Implementado (Fase RED)
Se definió la prueba unitaria `NuevaVenta_OnGet_DebeInicializarPaso1YResumenDeCotizacionConformeFigma` en `tests/SistemaFuneraria.Tests/Unit/Web/NuevaVentaShellTests.cs` para validar la inicialización del cascarón del Wizard en el Paso 1 y la presencia de los metadatos de cotización requeridos por Figma:
* Kicker: `"PROCESO COMERCIAL"`
* Título: `"Nueva venta"`
* PasoActual: `1`
* LoteTipo: `"Lote Básico"`
* LoteClave: `"JN-A-042"`
* GavetaSeleccionada: `42`
* CapacidadGavetas: `2`
* PrecioLista: `$120,000.00`
* TotalEstimado: `$120,000.00`

---

### 2. Evidencia de Ejecución RED (Fallo Verificado)
```bash
dotnet build tests/SistemaFuneraria.Tests
```

```text
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\NuevaVentaShellTests.cs(2,34): error CS0234: El tipo o el nombre del espacio de nombres 'Ventas' no existe en el espacio de nombres 'SistemaFuneraria.Web.Pages' (¿falta alguna referencia de ensamblado?) [C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\SistemaFuneraria.Tests.csproj]

ERROR al compilar.
    0 Advertencia(s)
    1 Errores
```

---

### 3. Conclusión de la Fase ROJO
La prueba falla de forma genuina y controlada porque aún no se han creado el modelo `NuevaModel` ni la página `Pages/Ventas/Nueva.cshtml`.
