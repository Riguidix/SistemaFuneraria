### 📋 Resumen del Pull Request

* **Ticket Relacionado:** Closes #FUN-___ <!-- o Fixes #INC-___ -->
* **Tipo de Cambio:**
  - [ ] `feat` (Nueva funcionalidad de negocio)
  - [ ] `fix` (Corrección de error / bug)
  - [ ] `test` (Pruebas unitarias o de integración)
  - [ ] `refactor` (Mejora de código sin cambiar comportamiento)
  - [ ] `docs` (Documentación / PRD / RTM)
  - [ ] `chore` (Configuración / dependencias)

---

### 💡 Descripción de la Solución
<!-- Explica en 2 o 3 líneas qué problema resuelve tu código y cómo lo implementaste -->

---

### 🧪 Criterios de Aceptación (BDD / Gherkin)
<!-- Copia aquí el criterio de aceptación del ticket que estás cumpliendo -->
* **Dado** [condición inicial]
* **Cuando** [acción ejecutada]
* **Entonces** [resultado esperado verificable]

---

### 🛡️ Checklist de Calidad y Gobernanza (Obligatorio)

Marca con una `[x]` las casillas verificadas antes de solicitar revisión:

- [ ] **Rama correcta:** Mi PR apunta a `dev` y nació de `dev`.
- [ ] **Conventional Commits:** El título de mi PR y mis commits siguen el formato `tipo(ámbito): descripción`.
- [ ] **Blast Radius $\le 300$ LOCs:** Mi PR modifica menos de 300 líneas de código (excluyendo migraciones automáticas).
- [ ] **TDD Aplicado:** Agregué o actualicé pruebas xUnit correspondientes a este cambio.
- [ ] **Suite en Verde:** Ejecuté `dotnet test` localmente y **todas** las pruebas pasaron en verde.
- [ ] **Clean Architecture:** No rompí la regla de dependencias (el proyecto `Core` no referencia a EF Core ni a `Web`).

---

### 📸 Evidencia de Entrega (Cero Fantasía)

1. **Captura de Terminal (`dotnet test` en verde):**
   > *Arrastra y pega aquí la captura de tu terminal mostrando las pruebas pasando.*

2. **Captura de Pantalla de la Interfaz UI (si aplica para vistas o partials):**
   > *Arrastra y pega aquí la captura visual de la pantalla renderizada.*
