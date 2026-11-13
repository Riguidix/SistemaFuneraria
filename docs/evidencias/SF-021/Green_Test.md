# Evidencia TDD: Fase VERDE 


| **Ticket de Trabajo** | SF-021 |
| **Título del Ticket** | Configuración de ASP.NET Core Identity y asignación de Roles (`Vendedor`, `Cajero`) |
| **Caso de Prueba Relacionado** | RNF-03 / RBAC / Criterio BDD Aislamiento Vendedor-Cajero |
| **Fecha y Hora de Ejecución** | 13/11/2026 10:48 |
| **Desarrollador / Tester** | Edgar Acevedo (@EdgarAcevedoSempai) |
| **Rama Git** | feature/SF-021-identity-rbac-roles |

---

# 1. Objetivo de la Prueba
Demostrar que tras implementar:
1. La herencia de `AppDbContext` sobre `IdentityDbContext<IdentityUser>` y el paquete `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
2. El seeder `IdentitySeed.SembrarRolesAsync` para los roles oficiales `Vendedor` y `Cajero`.
3. El registro en `SistemaFuneraria.Web/Program.cs` de `AddIdentityCore<IdentityUser>`, `AddRoles<IdentityRole>`, `AddEntityFrameworkStores<AppDbContext>` y middlewares de autenticación y autorización.

La batería de pruebas unitarias y de arquitectura pasa al 100% en verde sin errores ni advertencias.

## 2. Cambios Implementados
* `Directory.Packages.props`: Adición de versión centralizada para `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (8.0.11).
* `SistemaFuneraria.Infrastructure.csproj`: Inclusión del paquete de Identity.
* `Infrastructure/Persistencia/AppDbContext.cs`: Herencia de `IdentityDbContext<IdentityUser>` para configuración del esquema de seguridad.
* `Infrastructure/Persistencia/IdentitySeed.cs`: Constantes oficiales `RolVendedor`, `RolCajero` y método `SembrarRolesAsync`.
* `SistemaFuneraria.Web/Program.cs`: Configuración de servicios de Identity Core, cookies, políticas por rol y pipeline con `UseAuthentication()` y `UseAuthorization()`.
* `SistemaFuneraria.Tests/Unit/Infrastructure/IdentityRbacTests.cs`: 4 pruebas de cobertura unitaria, de integración de modelo y BDD.

### 3. Comando de Terminal Ejecutado
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "Ticket=SF-021"
```

#### 4. Salida Real de Terminal 
```text
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:     4, Omitido:     0, Total:     4, Duración: 9 ms - SistemaFuneraria.Tests.dll (net8.0)
```

##### 5. Explicación del Resultado
* **pruebas superadas exitosamente**
  1. `AppDbContext_DebeConfigurarEntidadesIdentity`: Verifica que el modelo de persistencia contiene las entidades `IdentityUser` y `IdentityRole`.
  2. `IdentitySeed_ConstantesDeRol_DebenSerExactas`: Valida la nomenclatura estricta exigida en RNF-03 (`"Vendedor"` y `"Cajero"`).
  3. `SembrarRolesAsync_DebeCrearRolesVendedorYCajero`: Valida que el seeder persiste e inicializa ambos roles en el gestor de seguridad.
  4. `BDD_UsuarioConRolVendedor_AlIntentarCobrar_DebeSerDenegado`: Valida el criterio de aceptación Gherkin asegurando que un vendedor no cuenta con privilegios de cobro asignados al cajero (HTTP 403 Forbidden).

El ticket SF-021 cumple todos los criterios de aceptación y queda listo para la integración en la rama `dev`.
