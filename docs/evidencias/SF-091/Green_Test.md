# Evidencia QA: Fase VERDE (GREEN) — SF-091

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-091 |
| **Título del Ticket** | Entidad `ConfiguracionSucursal`, `IConfiguracionUsuariosService` y `ConfiguracionController` (`Sucursal` + CRUD Admin de `Usuarios y roles`) |
| **Milestone** | Milestone 9 (v0.9.0) |
| **Carril** | Carril 2 (Backend) / Carril 3 (Web UI) |
| **Fecha de Certificación** | 23/11/2026 |
| **Responsable QA** | Edgar Acevedo (Carril 4 — DevOps & QA) |

---

### 1. Resumen de Implementación y Certificación
- Persistencia de entidad `ConfiguracionSucursal` con parámetros de sucursal, horario, serie de folios y tokens de interfaz.
- Endpoints administrativos en `UsuariosAdminController`:
  - Listado de usuarios del sistema con asignación de roles (`Administrador`, `Vendedor`, `Cajero`).
  - Mutación y control de acceso protegido mediante RBAC estricto (`[Authorize(Roles = "Administrador")]`).
- Verificación de seguridad: usuarios no administradores reciben HTTP 403 Forbidden al intentar acceder a rutas administrativas.

---

### 2. Comandos de Verificación
```bash
dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~AdminUsuarios|FullyQualifiedName~Configuracion" --no-build
```

### 3. Criterios de Aceptación Cumplidos
- [x] CRUD de usuarios administrativo protegido con RBAC (`403 Forbidden` para Cajero/Vendedor, `200 OK` para Administrador).
- [x] Parámetros de configuración de sucursal gestionables.
- [x] Respaldo de pruebas en verde.
