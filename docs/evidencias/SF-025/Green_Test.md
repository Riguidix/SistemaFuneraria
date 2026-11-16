# Evidencia TDD: Fase VERDE (GREEN)

| Campo | Detalle |
|---|---|
| **Ticket de Trabajo** | SF-025 |
| **Título del Ticket** | Pantalla Figma `/ParametrosUi` (Captura 13) y hoja `sistema-tokens.css` |
| **Caso de Prueba Relacionado** | RNF-01, RNF-06, RN-12 / Captura 13 Figma |
| **Fecha y Hora de Ejecución** | 15/11/2026 19:55 |
| **Rama Git** | `feature/SF-025-figma-parametros-ui-tokens` |

---

### 1. Objetivo de la Prueba
Demostrar que `wwwroot/css/sistema-tokens.css` define las 6 variables CSS oficiales de Captura 13 (`--forest: #173f3a`, `--forest-2: #245c53`, `--gold: #c59d52`, `--rose: #a84f4b`, `--canvas: #f4f6f3`, `--ink: #17211f`), la clase `.sf-money` con `tabular-nums`, y que `/ParametrosUi` compila y pasa las pruebas unitarias en verde.

### 2. Comando de Terminal Ejecutado
```bash
dotnet test --filter "Ticket=SF-025"
```

### 3. Salida Real de Terminal (Pase Exitoso)
```text
Serie de pruebas para /Users/riguidixrodriguez/RiderProjects/SistemaFuneraria/tests/SistemaFuneraria.Tests/bin/Debug/net8.0/SistemaFuneraria.Tests.dll (.NETCoreApp,Version=v8.0)
Versión 17.11.1 (arm64) de VSTest

Iniciando la ejecución de pruebas, espere...
1 archivos de prueba en total coincidieron con el patrón especificado.

Correctas! - Con error:     0, Superado:     2, Omitido:     0, Total:     2, Duración: 6 ms - SistemaFuneraria.Tests.dll (net8.0)
```
