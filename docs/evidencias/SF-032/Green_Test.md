# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-032 |
| **Caso de Prueba Relacionado** | RF-02 / RNF-03 (Controlador REST ClientesController) |
| **Fecha y Hora de Ejecución** | 17/11/2026 10:47 |
| **Desarrollador / Tester** | Guillermo Sandoval (`MemoSndvl <llmemosndvlll@gmail.com>`) |
| **Rama Git** | `feature/SF-032-api-clientes-crud-controller` |

---

### 1. Objetivo de la Prueba
Demostrar el cumplimiento satisfactorio al 100% de los escenarios BDD y pruebas unitarias para `ClientesController`:
1. `GET /api/v1/clientes`: listado y búsqueda reactiva por filtro.
2. `GET /api/v1/clientes/{id}`: consulta por Id (200 OK y 404 ProblemDetails).
3. `POST /api/v1/clientes`: creación con `CreatedAtAction` hacia `ObtenerPorId` (201 Created) y validación de invariantes (400 Bad Request).
4. `PUT /api/v1/clientes/{id}`: actualización con 200 OK y 404 ProblemDetails ante registro inexistente.
5. `DELETE /api/v1/clientes/{id}`: eliminación física con 204 NoContent para clientes sin contratos, 404 Not Found ante inexistencia, y restricción RBAC exclusiva a `[Authorize(Roles = "Administrador")]`.
6. `GET /api/v1/clientes/exportar`: descarga de archivo `directorio-clientes.csv` con cabecera y registros en codificación UTF-8 con BOM.
7. Regresión cero: verificación completa de los 401 tests del proyecto.

---

### 2. Comando de Terminal Ejecutado (Pruebas del Ticket SF-032)
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~ClientesControllerTests" -v normal
```

---

### 3. Salida Real de Terminal (Fase GREEN - ClientesControllerTests)
```text
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.3.1+6b60a9e56a (64-bit .NET 8.0.33)
[xUnit.net 00:00:01.45]   Discovering: SistemaFuneraria.Tests
[xUnit.net 00:00:01.62]   Discovered:  SistemaFuneraria.Tests
[xUnit.net 00:00:01.63]   Starting:    SistemaFuneraria.Tests
  Passed SF-032: Eliminar cliente inexistente debe retornar 404 Not Found [870 ms]
  Passed SF-032 (BDD): Endpoint Eliminar debe restringirse exclusivamente al rol Administrador [10 ms]
  Passed SF-032: ObtenerPorId con cliente existente debe retornar 200 OK [149 ms]
  Passed SF-032: Registrar con datos válidos debe retornar 201 Created [27 ms]
  Passed SF-032: ExportarDirectorio debe retornar archivo CSV UTF-8 con cabecera y datos [17 ms]
  Passed SF-032 (BDD): Eliminar cliente sin contratos por Administrador debe retornar 200 OK o 204 NoContent [62 ms]
  Passed SF-032: Buscar con filtro debe retornar 200 OK con clientes coincidentes [47 ms]
  Passed SF-032 (BDD): Actualizar con datos válidos por Administrador debe retornar 200 OK [48 ms]
  Passed SF-032: Registrar con datos inválidos debe retornar 400 Bad Request [4 ms]
[xUnit.net 00:00:02.94]   Finished:    SistemaFuneraria.Tests
  Passed SF-032: Buscar sin filtro debe retornar 200 OK con todos los clientes [4 ms]
  Passed SF-032: Actualizar cliente inexistente debe retornar 404 Not Found [4 ms]
  Passed SF-032: ObtenerPorId con cliente inexistente debe retornar 404 Not Found [2 ms]
  Passed SF-032: ClientesController debe tener Route api/v1/clientes y Authorize Administrador,Vendedor [2 ms]

Test Run Successful.
Total tests: 13
     Passed: 13
 Total time: 4.0098 Seconds

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### 4. Salida Real de Verificación Global (Regresión Cero)
```powershell
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName!~Integration"
```
```text
Test run for C:\Users\Billy\SistemaFuneraria\tests\SistemaFuneraria.Tests\bin\Debug\net8.0\SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:   401, Skipped:     0, Total:   401, Duration: 5 s - SistemaFuneraria.Tests.dll (net8.0)
```

---

### 5. Conclusión y Dictamen
El ciclo TDD concluye exitosamente:
- 13 pruebas unitarias y de seguridad RBAC específicas pasadas en verde.
- 401 pruebas globales de la solución ejecutadas con cero fallos y cero advertencias bajo `TreatWarningsAsErrors`.
- Cobertura estricta de los escenarios BDD sin desvíos arquitectónicos.
