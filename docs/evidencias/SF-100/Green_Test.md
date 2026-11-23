# Evidencia Green Test — SF-100: Release v1.0.0 ("Sistema Funerario · Gestión Memorial")

## Entregables de Go-Live
- `CHANGELOG.md` con historial completo de los 10 Milestones (`v0.1.0` a `v1.0.0`), comandos de despliegue local (`docker compose up -d` + `dotnet run` para `Api` y `Web`) y credenciales demo por rol (`Administrador`, `Vendedor`, `Cajero`).
- Compilación `Release` (`0` errores, `0` advertencias), `688/688` pruebas automatizadas no-Docker superadas (`0` omitidas) y `100/100` carpetas de evidencia validadas vía `./scripts/validate-evidence.sh --all`.
- Listo para promoción final `dev` → `beta` → `main` y tag `v1.0.0`.
