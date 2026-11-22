# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-046 |
| **Caso de Prueba Relacionado** | RF-01 / RNF-01 (Endpoint y Modal Ver detalle de Lote e Historial de Estado) |
| **Fecha y Hora de Ejecución** | 21/11/2026 23:12 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-046-api-web-detalle-lote-modal` |

---

### 1. Resumen de la Implementación
Se implementaron exitosamente todos los requerimientos de `SF-046` con estricta adherencia a Clean Architecture y Blast Radius ($\le 160$ LOCs):
1. **Endpoint REST `GET /api/v1/lotes/clave/{clave}` en `LotesController`**:
   - Expone la consulta técnica por clave comercial (ej. `RO-B-027`).
   - Retorna `200 OK` con `LoteItemDto` o `404 Not Found` en caso de inexistencia.
2. **Método en wrapper JS `lotesApi.js`**:
   - `obtenerLotePorClave(clave, opts)` delegando sobre `apiClient.get('/api/v1/lotes/clave/' + clave)`.
3. **Componente Reutilizable Razor `_DetalleLoteModal.cshtml`**:
   - Diseñado bajo la identidad visual crema/oliva de Figma (*Captura 2*).
   - Muestra Clave (`#detalleLoteClave`), Ubicación física (`#detalleLoteUbicacion`), Capacidad (`#detalleLoteCapacidad`), Precio de contado (`#detalleLotePrecio`), Badge de estado (`#detalleLoteEstado`), y Bitácora informativa (`#historialEstadoLote`).
   - Controles de acción: Botón rápido para cotizar lote si se encuentra en estado `Disponible` y sección administrativa para cambio rápido de estado con RBAC.
4. **Integración en `/Catalogo` (`Pages/Catalogo/Index.cshtml`)**:
   - Conexión del botón `"Ver detalle"` en la fila `RO-B-027` invocando `abrirModalDetalleLote('RO-B-027')`.
   - Cumplimiento del escenario BDD: Renderiza ubicación `"Robles · Andador B · Fila 2"`, capacidad `"4 gavetas"` y precio `"$180,000.00"`.
   - Enlace contextual `"Ver contrato"` en lotes `Vendido` (`LM-A-014`) apuntando a `/Contratos/Detalle?loteClave=LM-A-014`.
5. **Soporte en `Contratos/Detalle` (`Pages/Contratos/Detalle.cshtml.cs`)**:
   - Soporta el parámetro `loteClave` en `OnGet` vinculando el contrato asociado para `LM-A-014`.

---

### 2. Comando de Terminal Ejecutado
```powershell
$env:PATH = "%DOTNET_HOME%;$env:PATH"; $env:DOTNET_ROOT = "%DOTNET_HOME%"; dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~DetalleLoteModalTests|FullyQualifiedName~ObtenerPorClave" -v normal
```

---

### 3. Salida Real de Terminal (Fase GREEN)
```text
Test run for <repo>/tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:00.43]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:00.65]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:00.66]   Starting:    SistemaFuneraria.Tests
  Passed SF-046: lotesApi.js debe exponer método obtenerLotePorClave [12 ms]
  Passed SF-046: El componente _DetalleLoteModal.cshtml debe existir en Pages/Shared/ [< 1 ms]
  Passed SF-046 (BDD): Modal debe contener campos técnicos requeridos (Clave, Ubicación, Gavetas, Precio, Estado, Historial) [< 1 ms]
  Passed SF-046: Pages/Catalogo/Index.cshtml debe incluir el partial _DetalleLoteModal y conectar botón Ver detalle [< 1 ms]
  Passed SF-046: DetalleModel debe soportar parámetro de búsqueda por loteClave [< 1 ms]
  Passed SF-046: Enlace 'Ver contrato' para lote Vendido (LM-A-014) debe apuntar a /Contratos/Detalle?loteClave=LM-A-014 [< 1 ms]
[xUnit.net 00:00:01.79]   Finished:    SistemaFuneraria.Tests
  Passed SF-046: ObtenerPorClave inexistente debe retornar 404 Not Found [829 ms]
  Passed SF-046: ObtenerPorClave existente debe retornar 200 OK con DTO técnico [246 ms]

Test Run Successful.
Total tests: 8
     Passed: 8
 Total time: 2.3894 Seconds
```

---

### 4. Prueba de Regresión Global
```text
Passed!  - Failed:     0, Passed:   555, Skipped:     0, Total:   555, Duration: 2 s - SistemaFuneraria.Tests.dll (net8.0)
```
100% de la suite de pruebas unitarias exitosa sin regresiones en ninguna capa arquitectónica.
