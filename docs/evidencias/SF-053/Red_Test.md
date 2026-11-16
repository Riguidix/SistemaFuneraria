# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-053 |
| **Título del Ticket** | Wizard Paso 2 (Lote — Captura 6: Tarjetas de Selección de Lote y Sincronización con Panel Derecho) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 16/11/2026 16:53 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaNuevaVentaPaso2 |

---

### 1. Caso de Prueba Implementado (Fase RED)
Se definieron las pruebas unitarias en `tests/SistemaFuneraria.Tests/Unit/Web/Paso2LoteTests.cs` para validar el catálogo de opciones y la paridad de metadatos del Paso 2 con la Captura 6 de Figma:
* `ObtenerOpcionesLotes_DebeRetornarTresOpcionesFigmaCaptura6`: Valida la existencia y valores de `JN-A-042` ($120,000, 2 gavetas), `JC-C-118` ($150,000, 3 gavetas) y `RO-E-081` ($180,000, 4 gavetas).
* `BuscarPorClave_ConClaveExistente_DebeRetornarLoteCorrecto`: Valida la resolución rápida por clave de lote para sincronización reactiva.

---

### 2. Evidencia de Ejecución RED (Fallo Verificado)
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

```text
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\Paso2LoteTests.cs(13,24): error CS0103: El nombre 'LoteOpcionesCatalogo' no existe en el contexto actual
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\Paso2LoteTests.cs(46,20): error CS0103: El nombre 'LoteOpcionesCatalogo' no existe en el contexto actual

ERROR al compilar.
    0 Advertencia(s)
    2 Errores
```

---

### 3. Conclusión de la Fase ROJO
La prueba falla de forma genuina y esperada porque aún no se han creado el catálogo `LoteOpcionesCatalogo`, la vista parcial `_Paso2Lote.cshtml` ni el script `paso2Lote.js`.
