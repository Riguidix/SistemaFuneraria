# Evidencia TDD / Frontend UI: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-052 |
| **Título del Ticket** | Wizard Paso 1 (Cliente — Captura 5: Autocomplete de Cliente Existente o Captura Inline de Datos del Titular) |
| **Caso de Prueba Relacionado** | RNF-01 (Usabilidad y Accesibilidad) / RNF-06 (Bootstrap 5.3.8 y Diseño Memorial) |
| **Fecha y Hora de Ejecución** | 16/11/2026 16:32 |
| **Desarrollador / Tester** | Juan Romero (Senior Frontend Architect) / Edgar Acevedo (Lead QA) |
| **Rama Git** | SF-019/VistaNuevaVentaPaso1 |

---

### 1. Caso de Prueba Implementado (Fase RED)
Se definieron las pruebas unitarias en `tests/SistemaFuneraria.Tests/Unit/Web/Paso1ClienteTests.cs` para validar la lógica del modelo de captura y validaciones del Paso 1:
* `ValidarPaso1_ConDatosCompletos_DebeSerValidoYGenerarBorrador`: Verifica que un titular completo ("Mariana Torres Aguirre", CURP, teléfono 10 dígitos, email) sea válido y genere `NombreCompleto`.
* `ValidarPaso1_SinNombreOApellidos_DebeRetornarInvalido`: Verifica que no permita avanzar si faltan nombres o apellidos obligatorios.
* `ValidarPaso1_ConTelefonoInvalido_DebeRetornarInvalido`: Verifica que rechace teléfonos que no cumplan con el formato de 10 dígitos.

---

### 2. Evidencia de Ejecución RED (Fallo Verificado)
```bash
dotnet build tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj
```

```text
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\Paso1ClienteTests.cs(13,27): error CS0246: El nombre del tipo o del espacio de nombres 'ClienteDraftDto' no se encontró (¿falta una directiva using o una referencia de ensamblado?)
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\Paso1ClienteTests.cs(35,27): error CS0246: El nombre del tipo o del espacio de nombres 'ClienteDraftDto' no se encontró (¿falta una directiva using o una referencia de ensamblado?)
C:\Users\PC\SistemaFuneraria\tests\SistemaFuneraria.Tests\Unit\Web\Paso1ClienteTests.cs(53,27): error CS0246: El nombre del tipo o del espacio de nombres 'ClienteDraftDto' no se encontró (¿falta una directiva using o una referencia de ensamblado?)

ERROR al compilar.
    0 Advertencia(s)
    3 Errores
```

---

### 3. Conclusión de la Fase ROJO
La prueba falla de forma genuina y esperada porque aún no se han implementado `ClienteDraftDto`, la vista parcial `_Paso1Cliente.cshtml` ni el script de interacción `paso1Cliente.js`.
