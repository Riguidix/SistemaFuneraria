# Evidencia TDD: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-021 |
| **Título del Ticket** | Configuración de ASP.NET Core Identity y asignación de Roles (`Vendedor`, `Cajero`) |
| **Caso de Prueba Relacionado** | RNF-03 / RBAC / Criterio BDD Aislamiento Vendedor-Cajero |
| **Fecha y Hora de Ejecución** | 12/11/2026 17:46 |
| **Desarrollador / Tester** | Edgar Acevedo (@EdgarAcevedoSempai) |
| **Rama Git** | feature/SF-021-identity-rbac-roles |

---

### 1. Objetivo de la Prueba
Demostrar el estado de falla controlada en la fase **ROJO (RED)** previo a:
1. La herencia de `AppDbContext` sobre `IdentityDbContext<IdentityUser>` para el registro del modelo de seguridad de ASP.NET Core Identity.
2. La implementación del método de siembra `IdentitySeed.SembrarRolesAsync` para los roles `Vendedor` y `Cajero`.

### 2. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-021"
```

### 3. Salida Real de Terminal (Falla Esperada / RED)
```text
Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.
[xUnit.net 00:00:00.26]    SF-021: AppDbContext debe configurar las entidades de ASP.NET Core Identity [FAIL]
[xUnit.net 00:00:00.26]    SF-021: SembrarRolesAsync debe crear roles Vendedor y Cajero [FAIL]
Con error SF-021: AppDbContext debe configurar las entidades de ASP.NET Core Identity [186 ms]
Mensaje de error:
 Expected userEntity not to be <null> because AppDbContext debe heredar de IdentityDbContext y registrar IdentityUser.

Con error SF-021: SembrarRolesAsync debe crear roles Vendedor y Cajero [< 1 ms]
Mensaje de error:
 System.NotImplementedException : SF-021: Implementar siembra de roles Vendedor y Cajero.

Con error! - Con error:    2, Superado:    2, Omitido:    0, Total:    4, Duración: 7 ms - SistemaFuneraria.Tests.dll (net8.0)
```

### 4. Explicación del Resultado
La suite ejecutó 4 pruebas correspondientes a la especificación de SF-021:
* **2 pruebas superadas:** validación estricta de constantes de roles (`Vendedor` y `Cajero`) y regla de denegación BDD para `Vendedor` sobre recursos de `Cajero`.
* **2 pruebas con error (Fase ROJO verificada):**
  1. `AppDbContext` falla porque aún hereda de `DbContext` básico y no expone `IdentityUser` ni `IdentityRole`.
  2. `IdentitySeed.SembrarRolesAsync` arroja `NotImplementedException` al no contar con la lógica de persistencia de roles.

La fase ROJO queda formalmente certificada y el ticket pasa a la fase **VERDE (GREEN)**.
