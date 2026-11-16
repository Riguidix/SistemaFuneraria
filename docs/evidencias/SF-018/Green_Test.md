# Evidencia de Fase GREEN — Ticket SF-018: Implementación de ClienteService (Capa Application)

## 1. Metadatos de la Tarea

| Parámetro | Detalle |
| :--- | :--- |
| **Ticket** | `SF-018` — Implementación de `ClienteService` (Capa Application) |
| **Rama Git** | `feature/SF-018-cliente-service-impl` |
| **Fase TDD** | **GREEN** (Implementación completa pasando el 100% de pruebas unitarias y de arquitectura) |
| **Responsable** | Lead Fullstack / Riguidix Rodríguez |
| **Fecha de Ejecución** | 11 de Noviembre de 2026, 23:55 CST |
| **Ambiente de Pruebas** | .NET SDK 8.0.32, EF Core InMemory, xUnit 2.5.3, FluentAssertions 6.12.0 |
| **Gobernanza / Regla** | Cobertura estricta BDD Gherkin, Clean Architecture (RF-ARCH-04), Cero Fantasía |

---

## 2. Resumen de la Fase GREEN

Tras la fase RED donde se definieron los contratos e interfaz `IAppDbContext` y las pruebas BDD con fallas controladas, se procedió con la implementación de la lógica en `ClienteService`:

1. **Implementación de Reglas de Negocio y Orquestación:**
   - **Registro de Cliente (`RegistrarClienteAsync`):**
     - Validación y rechazo de campos obligatorios vacíos o nulos (Nombre, Apellidos, CURP, Teléfono, Correo).
     - Validación de longitudes máximas y restricciones de formato (CURP 18 caracteres alfanuméricos, Correo electrónico válido, Teléfono exactamente 10 dígitos numéricos).
     - Validación de unicidad de negocio en persistencia: Detección proactiva de CURP o Correo ya registrados, retornando mensajes de error descriptivos (`"Ya existe un cliente registrado con la CURP..."` / `"Ya existe un cliente registrado con el correo..."`).
     - Normalización consistente de datos (`Trim()`, CURP en mayúsculas, correo en minúsculas).
     - Creación de entidad `Cliente` mediante fábrica de dominio y guardado asíncrono con `_db.SaveChangesAsync(ct)`.
   - **Búsqueda por Id (`ObtenerPorIdAsync`):**
     - Búsqueda eficiente por clave primaria, retornando `Resultado<ClienteDto>.Exitoso` o error claro cuando no existe.
   - **Búsqueda por CURP (`ObtenerPorCurpAsync`):**
     - Búsqueda insensible a mayúsculas/minúsculas y espacios en blanco, retornando el DTO correspondiente.
   - **Listado de Clientes (`ObtenerTodosAsync`):**
     - Consulta de clientes proyectada a DTOs ordenados alfabéticamente por apellido y nombre.

2. **Arquitectura Limpia & Desacoplamiento:**
   - `ClienteService` depende únicamente de `IAppDbContext` dentro de `SistemaFuneraria.Application.Comun.Persistencia`.
   - Cero referencia a la capa de infraestructura ni paquetes externos no permitidos en Application.

---

## 3. Comandos Ejecutados

```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-018"
dotnet test
```

---

## 4. Salida de Terminal (Ejecución Exitosa)

```text
Serie de pruebas para <REPO_ROOT>/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:    37, Omitido:     0, Total:    37, Duración: 148 ms - SistemaFuneraria.Tests.dll (net8.0)

Serie de pruebas completa:
Correctas! - Con error:     0, Superado:   285, Omitido:     0, Total:   285, Duración: 1 s - SistemaFuneraria.Tests.dll (net8.0)
```

---

## 5. Verificación de Reglas de Arquitectura

```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~Arquitectura"
```

```text
Correctas! - Con error:     0, Superado:     7, Omitido:     0, Total:     7, Duración: 82 ms - SistemaFuneraria.Tests.dll (net8.0)
```

- `Core` no tiene dependencias de otras capas internas.
- `Application` no depende de `Infrastructure` ni de `Web`.
- 100% de reglas arquitectónicas validadas.
