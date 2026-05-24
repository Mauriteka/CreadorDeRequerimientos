# Agent Orchestrator

## Purpose

Coordina el trabajo del repositorio `CreadorDeRequerimientos`, una aplicacion web personal para capturar entrevistas por voz y convertirlas en requerimientos.

## Required Workflow

1. Entender la solicitud y delimitar alcance.
2. Identificar capa afectada:
   - `Domain`: entidades y reglas del workspace.
   - `AppCore`: casos de uso y contratos internos.
   - `Infrastructure`: persistencia JSON y servicios tecnicos.
   - `Contracts`: DTOs HTTP.
   - `API`: endpoints y hosting de la web.
   - `wwwroot`: frontend estatico.
3. Implementar cambios pequenos.
4. Ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\verify_env.ps1
```

## Project Structure

```text
/src
  /CreadorDeRequerimientos.API
    /wwwroot
  /CreadorDeRequerimientos.AppCore
  /CreadorDeRequerimientos.Contracts
  /CreadorDeRequerimientos.Domain
  /CreadorDeRequerimientos.Infrastructure
/data
/scripts
/memory
```

## Design Heuristics

Cuando haya duda:

1. Preferir funcionalidad directa sobre abstraccion.
2. Mantener datos portables en JSON local.
3. Separar reglas de negocio de endpoints y UI.
4. Evitar dependencias nuevas si la plataforma ya resuelve el caso.
5. Preservar el flujo de trabajo del usuario: proyecto -> encuesta -> mencion -> requerimiento.

## Frontend Rules

- La primera pantalla util es la lista de proyectos, no una landing.
- La captura por voz debe convivir con escritura manual.
- Las encuestas deben ser desplegables y faciles de escanear.
- La UI debe funcionar en escritorio y celular.

## Memory Protocol

Despues de tareas relevantes:

- Anotar resumen en `memory/history.md`.
- Actualizar `memory/tasks.json` si aplica.
