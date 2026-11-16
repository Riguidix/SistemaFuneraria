# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-015 |
| **Caso de Prueba Relacionado** | ERD v1.1 Sección 3.2, RNF-05, RN-01 |
| **Fecha y Hora de Ejecución** | 08/11/2026 16:34 |
| **Desarrollador / Tester** | Gustavo Lucio |
| **Rama Git** | feature/SF-015-entidad-cliente-core |

---

### 1. Objetivo de la Prueba
Demostrar que la entidad de dominio `Cliente` y su Factory Method `Cliente.Crear` no poseen implementación operativa y lanzan `System.NotImplementedException`, garantizando la fase ROJO (RED) del ciclo TDD estricto antes de escribir el código de producción con sus invariantes y encapsulamiento DDD.

### 2. Datos de Entrada Utilizados
* **Escenario Exitoso (Happy Path):**
  * Nombre: "Gustavo", Apellidos: "Lucio Rodriguez", CURP: "LURG850101HDFRRN09", Teléfono: "5512345678", Correo: "gustavo.lucio@example.com", Dirección: "Av. Insurgentes Sur 1234, CDMX".
* **Escenario de Normalización:**
  * CURP con espacios y minúsculas: "  lurg850101hdfrrn09  ".
  * Correo con mayúsculas y espacios: "  Gustavo.Lucio@EXAMPLE.COM  ".
* **Casos inválidos de prueba (Invariantes DDD):**
  * CURP: nulo, vacío, espacios, 16 chars, 17 chars, 19 chars, caracteres especiales ("LURG850101HDFRR-09", "LURG850101HDFRRN0!").
  * Teléfono: nulo, vacío, espacios, 9 dígitos, 11 dígitos, letras ("551234567a"), guiones ("55-1234-56"), espacios ("55 1234567").
  * Correo: nulo, vacío, espacios, sin arroba ("correo-sin-arroba"), sin usuario ("@sin-usuario.com"), sin dominio ("usuario@"), longitud > 255 chars.
  * Nombre: nulo, vacío, espacios, longitud > 100 chars.
  * Apellidos: nulo, vacío, espacios, longitud > 100 chars.
  * Dirección: nula, vacía, espacios.

### 3. Comando de Terminal Ejecutado
`dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-015" --no-build`

### 4. Salida de Terminal (Falla Esperada)
```text
Serie de pruebas para <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (x64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Con error! - Con error:    36, Superado:     0, Omitido:     0, Total:    36, Duración: 229 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 5. Explicación del Resultado
La suite de pruebas falló al 100% (36 pruebas en ROJO) como exige el estándar TDD. La clase `Cliente` cuenta con la firma del Factory Method `Crear`, pero lanza intencionalmente `NotImplementedException`, dejando el contrato y las invariantes de negocio listos para su implementación en la fase VERDE.

---

### 6. Matriz de Cumplimiento de Criterios M.E.S.T. (Fase RED)

| Pilar M.E.S.T. | Criterio de Calidad | Evidencia Técnica en Fase RED | Veredicto |
|---|---|---|:---:|
| **M - Mantenible** | Entidad de dominio pura con propiedades de solo lectura para el exterior y constructor protegido para EF Core. | Estructura DDD con `private set`, `protected Cliente()` y Factory Method estático. | ✅ APROBADO |
| **E - Eficiente** | Validación mediante expresiones regulares precompiladas y tipos escalares del BCL. | Cero dependencias externas y validaciones en memoria O(1). | ✅ APROBADO |
| **S - Seguridad** | Invariantes estrictos para integridad de padrón (CURP 18, Teléfono 10, Correo, longitudes máx). | 34 pruebas de invariantes parametrizadas para rechazo con `ReglaNegocioException`. | ✅ APROBADO |
| **T - Testeable** | Cobertura integral de caminos válidos y límite de datos. | 36 pruebas unitarias agrupadas bajo `[Trait("Ticket", "SF-015")]`. | ✅ APROBADO |
