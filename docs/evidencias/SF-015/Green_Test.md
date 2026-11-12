# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-015 |
| **Caso de Prueba Relacionado** | ERD v1.1 Sección 3.2, RNF-05, RN-01 |
| **Fecha y Hora de Ejecución** | 08/11/2026 16:35 |
| **Desarrollador / Tester** | Gustavo Lucio |
| **Rama Git** | feature/SF-015-entidad-cliente-core |

---

### 1. Objetivo de la Prueba
Demostrar que la entidad de dominio `Cliente` cumple rigurosamente con los patrones de diseño táctico DDD, encapsulamiento de setters privados, constructor protegido para compatibilidad con EF Core, creación mediante Factory Method `Cliente.Crear` y validación de invariantes de negocio (ERD v1.1 Sección 3.2), pasando al 100% las 36 pruebas unitarias en fase VERDE (GREEN).

### 2. Datos de Entrada Utilizados
* **Escenario Exitoso (Happy Path):**
  * Nombre: "Gustavo", Apellidos: "Lucio Rodriguez", CURP: "LURG850101HDFRRN09", Teléfono: "5512345678", Correo: "gustavo.lucio@example.com", Dirección: "Av. Insurgentes Sur 1234, CDMX".
* **Escenario de Normalización:**
  * CURP con espacios y minúsculas ("  lurg850101hdfrrn09  ") $\rightarrow$ Normalizado a "LURG850101HDFRRN09".
  * Correo con mayúsculas y espacios ("  Gustavo.Lucio@EXAMPLE.COM  ") $\rightarrow$ Normalizado a "gustavo.lucio@example.com".
* **Validaciones de Invariantes Rechazadas con Excepción:**
  * CURP inválido: nulo, vacío, espacios, longitudes distintas a 18 (16, 17, 19 chars), caracteres especiales ("-", "!").
  * Teléfono inválido: nulo, vacío, espacios, longitud distinta a 10 (9, 11 dígitos), alfanumérico, guiones.
  * Correo inválido: nulo, vacío, espacios, sin arroba, sin usuario, sin dominio, longitud > 255 chars.
  * Nombre y Apellidos: nulos, vacíos, espacios, longitudes > 100 chars.
  * Dirección: nula, vacía, espacios.

### 3. Comando de Terminal Ejecutado
`dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-015" --no-build`

### 4. Salida de Terminal (Pase Exitoso)
```text
Serie de pruebas para <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:    36, Omitido:     0, Total:    36, Duración: 107 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La prueba pasó exitosamente (36 de 36 pruebas en VERDE) demostrando que:
1. `Cliente.Crear` autogenera un `Id` único tipo `Guid` no vacío y establece la marca temporal `CreatedAt` en UTC.
2. Se normalizan y limpian de espacios en blanco los campos textuales, garantizando mayúsculas para `Curp` y minúsculas para `Correo`.
3. Se rechazan todas las entradas que violan las restricciones del esquema relacional (ERD Sección 3.2): CURP de 18 caracteres alfanuméricos, teléfono de exactamente 10 dígitos numéricos, correo válido con longitud $\le 255$, y nombre/apellidos con longitud $\le 100$.
4. Las fallas de invariantes lanzan `ReglaNegocioException` con mensajes claros y descriptivos.
5. Se respeta la pureza arquitectónica de `SistemaFuneraria.Core` con cero dependencias externas a EF Core o ASP.NET.

---

### 6. Matriz de Cumplimiento de Criterios M.E.S.T. (Fase GREEN)

| Pilar M.E.S.T. | Criterio de Calidad | Evidencia Técnica en Fase GREEN | Veredicto |
|---|---|---|:---:|
| **M - Mantenible** | Entidad rica en dominio con Factory Method explícito, constructor protegido y propiedades inmutables para el exterior. | Encapsulamiento con `private set`, `protected Cliente()` y métodos limpios sin fugas de estado. | ✅ APROBADO |
| **E - Eficiente** | Expresiones regulares compiladas estáticas y normalización en memoria O(1). | Uso de `RegexOptions.Compiled` para CURP, teléfono y correo sin degradación de rendimiento. | ✅ APROBADO |
| **S - Seguridad** | Invariantes blindados contra estados inconsistentes o inyección de datos corruptos. | Guard clauses completas para cada atributo conforme al contrato relacional de PostgreSQL. | ✅ APROBADO |
| **T - Testeable** | 100% de caminos y bordes cubiertos con aserciones xUnit y FluentAssertions. | 36 pruebas unitarias pasando al 100% en verde con etiquetas de trazabilidad. | ✅ APROBADO |

---

### 7. Paneles Visuales de Inspección (Capturas de Terminal)

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ [CAPTURA VISUAL 1] XUNIT TEST RUNNER - SUITE ENTIDAD CLIENTE (SF-015)                   │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TEST SUITE:  SistemaFuneraria.Tests.Unit.Core.ClienteEntidadTests                      │
│ FILTER:      Ticket=SF-015                                                             │
│ RUNNER:      VSTest 17.11.1 (x64) [.NETCoreApp,Version=v8.0]                           │
│ TOTAL TESTS: 36 pruebas (6 Facts + 5 Theories con 30 combinaciones)                    │
│ RESULT:      Passed: 36, Failed: 0, Skipped: 0                                         │
│ DURATION:    107 ms                                                                    │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ [CAPTURA VISUAL 2] MATRIZ DE INVARIANTES DDD Y VALIDACIONES DE ESQUEMA (ERD v1.1)      │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ CAMPO      | FORMATO / RESTRICCIÓN           | VALIDACIÓN DOMINIO | EXCEPCIÓN          │
│------------+---------------------------------+--------------------+------------------- │
│ Id         | UUID v4 no vacío                | Guid.NewGuid()     | Inmutable          │
│ Curp       | 18 caracteres alfanuméricos     | Regex compilado    | ReglaNegocioExcep  │
│ Telefono   | Exactamente 10 dígitos [0-9]    | Regex compilado    | ReglaNegocioExcep  │
│ Correo     | Formato válido RFC, máx 255     | Regex compilado    | ReglaNegocioExcep  │
│ Nombre     | Obligatorio, máx 100 chars      | Trim() + Length    | ReglaNegocioExcep  │
│ Apellidos  | Obligatorio, máx 100 chars      | Trim() + Length    | ReglaNegocioExcep  │
│ Direccion  | Obligatorio, texto no vacío     | Trim()             | ReglaNegocioExcep  │
│ CreatedAt  | Marca de tiempo UTC             | UtcNow             | Inmutable          │
└────────────────────────────────────────────────────────────────────────────────────────┘
```
