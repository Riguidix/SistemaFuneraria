# Auditoría Técnica y Remediación de Regresión: Oráculo de 36 Combinaciones (`SF-012`)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-012 |
| **Rama Git Auditada** | `feature/SF-012-oraculo-36-combinaciones-test` |
| **Commits Evaluados** | `1d45f13` (feat inicial) y `f1e742b` (merge rotatorio con conflictos) |
| **Fecha de Auditoría y Remediación** | 11/11/2026 15:57 |
| **Dictamen Inicial** | **RECHAZADO** (Bloqueo de compilación, prueba tautológica sin `Core`, evidencias de 0 bytes) |
| **Dictamen Final** | **CERTIFICADO EN VERDE (36/36 Oráculo · 192/192 Suite)** |

---

### 1. Resumen Ejecutivo
Al auditar la rama `feature/SF-012-oraculo-36-combinaciones-test` previa integración a `dev`, el Quality Gate rechazó el avance original (`f1e742b`) tras detectar tres defectos críticos de ingeniería y trazabilidad:
1. Un merge roto que dejó **marcadores de conflicto Git sin resolver** (`<<<<<<< HEAD`) en `tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj` e intentó cambiar el TargetFramework a `.NET 10.0`.
2. Una implementación en `tests/SistemaFuneraria.Tests/Core/OraculoFinancieroTests.cs` con **aserciones tautológicas** que validaban el CSV contra sí mismo sin invocar ni una sola clase de `SistemaFuneraria.Core`.
3. Archivos de evidencia vacíos de **0 bytes** (`SF-012-Red_Test.md` y `SF-012-Green_Test.md`).

Al sustituir la prueba placeholder por la verificación real al centavo contra `CalculadoraFinanciamiento` y `GeneradorAmortizacion`, el oráculo detectó además una **divergencia matemática real de `-$0.01`** en 4 escenarios (`DT-06`, `DT-08`, `DT-12`, `DT-36`) causada porque `docs/anexos/generar_oraculo.py` había generado los valores con `ROUND_HALF_UP` (`AwayFromZero`) en lugar de `RNF-02` (`MidpointRounding.ToEven`). Tras alinear el generador y los anexos a `RNF-02`, la suite quedó certificada al centavo (`36/36`).

---

### 2. Anatomía del Problema Detectado

#### 2.1 Conflicto Git sin resolver y desvío de SDK en `.csproj` (`1d45f13` / `f1e742b`)
- En `1d45f13`, se creó un proyecto aislado en `<TargetFramework>net10.0</TargetFramework>` con versiones fijadas en los `<PackageReference>` (`coverlet.collector 6.0.4`, `Microsoft.NET.Test.Sdk 17.14.1`, `xunit 2.9.3`, `xunit.runner.visualstudio 3.1.4`) y un archivo residual `UnitTest1.cs`.
- En `f1e742b`, al fusionar `dev` (`d6c5a2d`), se confirmaron directamente los marcadores `<<<<<<< HEAD`, `=======` y `>>>>>>> 1d45f133...` dentro de `tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj`, rompiendo la arquitectura de gestión centralizada de paquetes (`Directory.Packages.props`), el target estándar `.NET 8` y las referencias a los proyectos `Core`, `Application`, `Infrastructure` y `Web`.

#### 2.2 Prueba vacía (Falso Positivo / Violación de TDD)
El método `ValidarCombinacionOraculoFinanciero` en el commit `f1e742b` únicamente comprobaba predicados triviales (`Assert.False(string.IsNullOrWhiteSpace(lote))`, `Assert.True(precio > 0)`, etc.) sin importar `SistemaFuneraria.Core.Calculos` ni parsear con `CultureInfo.InvariantCulture`. Bajo esa implementación, la prueba pasaba en verde incluso si el motor financiero producía resultados erróneos.

#### 2.3 Evidencias falsas / vacías (0 bytes)
`docs/evidencias/SF-012/SF-012-Red_Test.md` y `docs/evidencias/SF-012/SF-012-Green_Test.md` existían en Git con longitud de **0 bytes**, incumpliendo la política de evidencia verificable.

#### 2.4 Brecha de especificación `ROUND_HALF_UP` vs `RNF-02` (`MidpointRounding.ToEven`)
Al implementar las aserciones reales con tolerancia `$0.00`, 4 filas fallaron sistemáticamente por `-0.01M` en cuota final ajustada, intereses totales y total pagado:
- **`DT-06`** (`Lote basico`, 20%, 18m): CSV `6125.28` / `14256.40` / `134256.40` vs Core `ToEven` `6125.27` / `14256.39` / `134256.39`.
- **`DT-08`** (`Lote basico`, 20%, 48m): CSV `3129.44` / `54229.10` / `174229.10` vs Core `ToEven` `3129.43` / `54229.09` / `174229.09`.
- **`DT-12`** (`Lote basico`, 30%, 48m): CSV `2738.87` / `47450.72` / `167450.72` vs Core `ToEven` `2738.86` / `47450.71` / `167450.71`.
- **`DT-36`** (`Lote ampliado`, 30%, 48m): CSV `4107.92` / `71175.93` / `251175.93` vs Core `ToEven` `4107.91` / `71175.92` / `251175.92`.

---

### 3. Falla Provocada en Compilación y Fase RED Real
1. En su estado original (`f1e742b`), `SistemaFuneraria.Tests.csproj` **no compilaba** debido al XML inválido por marcadores de conflicto Git.
2. Tras restaurar el `.csproj` de `dev` e implementar la invocación real a `Core`, la ejecución de `dotnet test --filter "FullyQualifiedName~OraculoFinanciero"` produjo el estado **RED real** (`Total: 36, Correcto: 32, Incorrecto: 4` en `0.9816 s`), evidenciado íntegramente en `<REPO_ROOT>/docs/evidencias/SF-012/Red_Test.md`.

---

### 4. Acción de Contención y Remediación Aplicada
1. **Reparación de `.csproj`:** Se restauró `tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj` desde `dev` conservando `.NET 8`, `Directory.Packages.props` (`FluentAssertions`, `NetArchTest.Rules`, `xunit`) y las 4 referencias de proyecto de Clean Architecture.
2. **Implementación Real del Oráculo (`OraculoFinancieroTests.cs`):**
   - Agregados `[Trait("Ticket", "SF-012")]`, `[Trait("Capa", "Core")]` y `[Trait("Regla", "RNF-02")]`.
   - Lectura del CSV con `CultureInfo.InvariantCulture` y guarda de cardinalidad exacta (`TotalCombinacionesOraculo == 36`).
   - Invocación directa de `CalculadoraFinanciamiento.CalcularEnganche`, `ObtenerTasas`, `CalcularMensualidadOrdinaria` y `GeneradorAmortizacion.Generar`, validando al centavo exacto (`$0.00` de tolerancia) las 6 métricas financieras del oráculo más el saldo final exacto de `$0.00`.
3. **Resolución de Especificación hacia `RNF-02` (decisión aprobada):**
   - Se actualizó `docs/anexos/generar_oraculo.py` de `ROUND_HALF_UP` a `ROUND_HALF_EVEN` (`MidpointRounding.ToEven`, alineado con `RNF-02` del PRD y con `SF-006`, `SF-008`, `SF-010`, `SF-011`).
   - Se regeneraron los anexos CSV (`git diff` confirmó actualización exclusiva en las filas `DT-06`, `DT-08`, `DT-12` y `DT-36` de `docs/anexos/oraculo-36-combinaciones.csv`; `tabla-cp06-120k-20pct-12m.csv` y `tabla-cp07-abono-10000-tras-periodo-3.csv` permanecieron idénticos).
   - Se actualizaron las 4 filas correspondientes en `docs/02-matriz-trazabilidad.md`.
4. **Saneamiento de Evidencias:** Se eliminaron los archivos vacíos `SF-012-Red_Test.md` y `SF-012-Green_Test.md`, creando `Red_Test.md`, `Green_Test.md` y este documento de auditoría con rutas sanitizadas (`<REPO_ROOT>/`).

---

### 5. Certificación de Calidad y Regresión
- **Oráculo `SF-012`:** `dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName~OraculoFinanciero"` -> **`36/36` correctas en `30 ms`**.
- **Suite Completa (sin integración Docker):** `dotnet test tests/SistemaFuneraria.Tests/SistemaFuneraria.Tests.csproj --filter "FullyQualifiedName!~PostgresConexionTests"` -> **`192/192` correctas en `87 ms`**.
- **Compilación estricta:** `TreatWarningsAsErrors = true` con 0 advertencias y 0 errores.

---

### 6. Lecciones Aprendidas
1. **Un merge con marcadores `<<<<<<<` nunca debe confirmarse:** Ante conflictos en `.csproj`, debe preservarse el archivo canónico de `dev` alineado a `Directory.Packages.props` y `.NET 8`.
2. **Aserciones tautológicas destruyen el valor del Oráculo:** Una prueba `[Theory]` sobre CSV debe ejercer al Sistema Bajo Prueba (`SistemaFuneraria.Core`) y comprobar invariantes y salidas reales, no solo validar que los datos de entrada sean positivos.
3. **Consistencia transversal de reglas numéricas (`RNF-02`):** Todos los generadores auxiliares externos (scripts Python/SQL de datos semilla u oráculos) deben compartir exactamente el mismo modo de redondeo (`ROUND_HALF_EVEN` / `MidpointRounding.ToEven`) que el código productivo para evitar derivas silenciosas de 1 centavo.
