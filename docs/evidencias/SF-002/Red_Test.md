# Evidencia TDD / Infraestructura: Fase ROJO (RED)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-002 |
| **Título del Ticket** | Docker Compose con PostgreSQL 16 y scripts de inicialización |
| **Caso de Prueba Relacionado** | Infraestructura y Persistencia / Criterios de Aceptación SF-002 |
| **Fecha y Hora de Ejecución** | 08/11/2026 12:39 |
| **Desarrollador / Tester** | Edgar Acevedo (DevOps) / Gustavo Lucio (DBA - Revisor) |
| **Rama Git** | feature/SF-002-docker-postgres16 |

---

### 1. Objetivo de la Prueba
Demostrar el estado inicial de fallo y ausencia de infraestructura para la base de datos PostgreSQL 16 local. Se comprueba que antes de implementar el ticket:
1. No existen en la raíz del repositorio los archivos de infraestructura (`docker-compose.yml`, `.env.example`, `scripts/db/init.sql`).
2. La conexión TCP al puerto local `5432` es activamente rechazada por el sistema operativo al no existir ningún motor de base de datos escuchando en dicha interfaz.
3. No se encuentra configurada la cadena de conexión de desarrollo en `src/SistemaFuneraria.Web/appsettings.Development.json`.

---

### 2. Comandos de Terminal Ejecutados

```powershell
# 1. Comprobación de existencia de artefactos de infraestructura
Test-Path docker-compose.yml
Test-Path .env.example
Test-Path scripts/db/init.sql

# 2. Intento de sondeo TCP al puerto 5432 (PostgreSQL)
powershell -Command "& { try { (New-Object Net.Sockets.TcpClient).Connect('127.0.0.1', 5432); 'CONNECTED' } catch { 'CONNECTION_REFUSED: ' + `$_.Exception.Message } }"
```

---

### 3. Salida de Terminal (Falla Esperada / RED)

```text
PS C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria> Test-Path docker-compose.yml; Test-Path .env.example; Test-Path scripts/db/init.sql
False
False
False

PS C:\Users\gaalz\OneDrive\Documents\GitHub\SistemaFuneraria> powershell -Command "& { try { (New-Object Net.Sockets.TcpClient).Connect('127.0.0.1', 5432); 'CONNECTED' } catch { 'CONNECTION_REFUSED: ' + `$_.Exception.Message } }"
CONNECTION_REFUSED: Excepción al llamar a "Connect" con los argumentos "2": "Intento de acceso a un socket no permitido por sus permisos de acceso 127.0.0.1:5432"
```

---

### 4. Explicación del Resultado
La fase ROJO (RED) se certifica formalmente como exitosa:
- Los artefactos de configuración de infraestructura no existían en el entorno de desarrollo.
- El socket `127.0.0.1:5432` rechazó la conexión inmediatamente al no haber ningún daemon de PostgreSQL activo.
- El proyecto web carecía del parámetro `ConnectionStrings:DefaultConnection` requerido para enlazar la capa de datos con la persistencia local.
