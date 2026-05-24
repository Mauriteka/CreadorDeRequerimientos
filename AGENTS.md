# AGENTS.md - Creador de Requerimientos

> Entry point operativo para agentes que trabajen en esta herramienta personal.
> La meta del repo es mantener un sistema pequeno, local-first y facil de modificar.

---

## AGENT MODE: ORCHESTRATOR

Orden de trabajo:

1. Leer `docs/agent/orchestrator.md` si la tarea toca arquitectura o codigo.
2. Leer `docs/agent/development-guardrails.md` si la tarea toca arquitectura, frontend o refactor.
3. Leer `docs/agent/current-system-state.md` si la tarea toca encuestas, voz, transcript, minuta o UX de entrevista.
4. Leer `docs/product/interview-workflow.md` si la tarea toca comportamiento funcional del flujo de entrevista.
5. Identificar la capa afectada: `Domain`, `AppCore`, `Infrastructure`, `Contracts`, `API` o `wwwroot`.
6. Hacer cambios pequenos y coherentes con Clean Architecture.
7. Ejecutar el harness antes de cerrar:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\verify_env.ps1
```

---

## ARQUITECTURA

Stack del repositorio:

- Domain: `src/CreadorDeRequerimientos.Domain`
- Application: `src/CreadorDeRequerimientos.AppCore`
- Infrastructure: `src/CreadorDeRequerimientos.Infrastructure`
- Contracts: `src/CreadorDeRequerimientos.Contracts`
- API + Web: `src/CreadorDeRequerimientos.API`
- Datos locales: `data/workspace.json`

Grafo de dependencias:

```text
API -> AppCore -> Domain
API -> Contracts
API -> Infrastructure
Infrastructure -> AppCore
Infrastructure -> Domain
AppCore -> Contracts
```

---

## PRODUCTO

Sistema personal, sin login, para levantar requerimientos durante entrevistas.

Conceptos principales:

- Proyecto: agrupa una funcionalidad o tema.
- Encuesta: entrevista con un usuario.
- Mencion: frase capturada por voz o escrita manualmente.
- Requerimiento: documento editable de toma, planeacion o diseno.

La aplicacion debe poder usarse desde computadora y celular si la API se expone en la red local o en un hosting simple.

---

## NON-NEGOTIABLES

- No agregar login/autenticacion salvo solicitud explicita.
- No introducir base de datos pesada si JSON local cubre la necesidad.
- No hardcodear secretos.
- Mantener la UI directa: primero capturar, luego organizar, luego redactar.
- Priorizar compatibilidad con navegadores que soporten Web Speech API para dictado.
- El frontend debe seguir la separacion por modulos introducida en `wwwroot/js`.
- Toda mejora visual debe verse bien en modo claro y en modo oscuro.
- Toda mejora de entrevista debe respetar la documentacion vigente en:
  - `docs/agent/current-system-state.md`
  - `docs/product/interview-workflow.md`
