# REPOSITORIO DE EVIDENCIAS TDD POR TICKET
## Sistema de Venta y Financiamiento de Lotes Funerarios

> **Instrucción para los Desarrolladores:**  
> Para evitar conflictos de Git entre compañeros (Merge Conflicts), **cada ticket tiene su propia subcarpeta aislada** dentro de este directorio.
> Nunca modifiques la carpeta de otro compañero.

---

### Estructura de Carpetas

Cada vez que desarrolles un ticket `FUN-XXX`, crea una carpeta con el identificador del ticket y adentro los dos archivos de evidencia TDD:

```text
docs/evidencias/
└── FUN-010/
    ├── Red_Test.md      # Evidencia de la prueba fallando antes de implementar
    └── Green_Test.md    # Evidencia de la prueba pasando tras implementar
```

---

## 1. Plantilla Oficial: `Red_Test.md` (Fase ROJO)

Copia este contenido exacto al crear tu archivo `docs/evidencias/FUN-XXX/Red_Test.md`:

```markdown
# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | FUN-___ |
| **Caso de Prueba Relacionado** | CP-___ (o EP-___ / BVA-___ / DT-___ / ST-___) |
| **Fecha y Hora de Ejecución** | dd/mm/aaaa hh:mm |
| **Desarrollador / Tester** | [Tu Nombre] |
| **Rama Git** | feature/FUN-___-... |

---

### 1. Objetivo de la Prueba
Demostrar que el requisito no está implementado o que la prueba unitaria falla antes de escribir el código de producción.

### 2. Datos de Entrada Utilizados
* [Especifica los valores monetarios, enganche o plazo utilizados]

### 3. Comando de Terminal Ejecutado
`dotnet test --filter "FullyQualifiedName~NombreDeTuPrueba"`

### 4. Salida de Terminal (Falla Esperada)
\`\`\`text
[Pega aquí el log de error de dotnet test mostrando la falla en rojo o la aserción que no cumplió]
\`\`\`

### 5. Explicación del Resultado
La prueba falló como se esperaba porque [explica qué método o lógica aún no existe o qué validación arrojó error].
```

---

## 2. Plantilla Oficial: `Green_Test.md` (Fase VERDE)

Copia este contenido exacto al crear tu archivo `docs/evidencias/FUN-XXX/Green_Test.md`:

```markdown
# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | FUN-___ |
| **Caso de Prueba Relacionado** | CP-___ (o EP-___ / BVA-___ / DT-___ / ST-___) |
| **Fecha y Hora de Ejecución** | dd/mm/aaaa hh:mm |
| **Desarrollador / Tester** | [Tu Nombre] |
| **Rama Git** | feature/FUN-___-... |

---

### 1. Objetivo de la Prueba
Demostrar que la implementación mínima satisface las reglas de negocio y los criterios de aceptación, pasando la suite de pruebas en verde.

### 2. Comando de Terminal Ejecutado
`dotnet test --filter "FullyQualifiedName~NombreDeTuPrueba"`

### 3. Salida de Terminal (Pase Exitoso)
\`\`\`text
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 25 ms
\`\`\`

### 4. Explicación del Resultado
La prueba pasó exitosamente demostrando que [explica cómo el código implementado cumple la regla de negocio y arrojó el valor esperado].
```
