# Changelog — Sistema Funerario · Gestión Memorial

Todas las entregas notables de **Sistema Funerario** documentadas desde `v0.1.0` hasta `v1.0.0`.

## Despliegue Rápido y Credenciales Demo

```bash
# 1. Levantar PostgreSQL 16
docker compose up -d

# 2. Iniciar API (puerto 5180) y Web (puerto 5080)
dotnet run --project src/SistemaFuneraria.Api
dotnet run --project src/SistemaFuneraria.Web
```

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | `andrea.morales@sistemafunerario.mx` | `Sistema Funerario2026!` |
| Vendedor | `vendedor@sistemafunerario.mx` | `Sistema Funerario2026!` |
| Cajero | `cajero@sistemafunerario.mx` | `Sistema Funerario2026!` |

---

## Milestones Entregados

- **`v1.0.0` (`SF-095`–`SF-100`) — Hardening N+1, Auditoría WCAG, UAT & Go-Live:** Benchmark HTTP P95 `< 200ms`, auditoría WCAG 2.2 AA (corrección ARIA en `Login.cshtml`), cobertura XPlat (`Core` 95.94%, `Application` 93.18%), UAT `ST-01`–`ST-05`, script `scripts/validate-evidence.sh` y 100/100 carpetas de evidencia certificadas.
- **`v0.9.0` (`SF-084`–`SF-094`) — Abono a Capital, Liquidación, Dashboard, Reportes & Configuración (CODE FREEZE):** Endpoints y conexión Web para abono a capital y liquidación anticipada (`10%` descuento), `DashboardReportesController`, `/Resumen`, `/Reportes` (con exportación CSV), entidad/migración `ConfiguracionSucursal`, `ConfiguracionController` y `/Configuracion`.
- **`v0.8.0` (`SF-075`–`SF-083`) — Caja y Cobranza, Pagos Ordinarios, Corte del Día & Concurrencia `ST-04`:** `CobranzaService`, `CobranzaController`, `cobranzaApi.js` con `X-Idempotency-Key`, `/Caja` y recibo imprimible `/Caja/Recibo`.
- **`v0.7.0` (`SF-066`–`SF-074`) — Venta Financiada ACID & Cartera de Contratos:** `RegistrarVentaFinanciadaAsync`, `CarteraContratosService`, `ContratosController`, `/Contratos` y `/Contratos/Detalle` con amortización francesa persistida.
- **`v0.6.0` (`SF-057`–`SF-065`) — Venta de Contado, CRUD Admin & Comprobante Imprimible:** `RegistrarVentaContadoAsync`, `VentasController`, liberación de lote por cancelación y Paso 4 del Wizard conectado.
- **`v0.5.0` (`SF-048`–`SF-056`) — Simulador Financiero & Wizard Pasos 1–3:** `SimuladorController` (`/cotizar`, `/planes`), `simuladorApi.js` y `wizardState.js`.
- **`v0.4.0` (`SF-039`–`SF-047`) — Catálogo de Lotes:** `LoteSeed` (178 lotes), `LoteService`, `LotesController` y `/Catalogo`.
- **`v0.3.0` (`SF-031`–`SF-038`) — Directorio y CRUD de Clientes:** `ClienteService`, `ClientesController`, exportación CSV y `/Clientes`.
- **`v0.2.0` (`SF-019`–`SF-030`) — Seguridad JWT, RBAC, Lockout & Jitter:** `AuthController`, `UsuariosAdminController`, `/Cuenta/Login` y tokens UI oficiales.
- **`v0.1.0` (`SF-001`–`SF-018`) — Arquitectura Limpia & Motor Matemático TDD:** Solución de 6 proyectos, `Dinero`, `CalculadoraFinanciamiento`, `GeneradorAmortizacion` y Oráculo de 36 combinaciones.
