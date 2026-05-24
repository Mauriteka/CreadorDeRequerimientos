# Development Guardrails

Este documento deja asentada la forma de trabajo despues del refactor del 2026-05-22/23.
La intencion es que el sistema siga creciendo con piezas pequenas, coherentes y faciles de mover.

## Resumen del trabajo realizado

- `RequirementWorkspaceService` dejo de concentrar normalizacion, mapeo, formato de transcript y creacion de plantillas por defecto.
- AppCore ahora reparte esas responsabilidades en helpers dedicados:
  - `Workspace/Normalization/WorkspaceNormalizer.cs`
  - `Workspace/Formatting/SurveyTranscriptFormatter.cs`
  - `Workspace/Mapping/WorkspaceResponseMapper.cs`
  - `Workspace/Participants/SurveyParticipantFactory.cs`
  - `Workspace/Templates/DefaultSurveyTemplateFactory.cs`
- El frontend dejo de arrancar desde un `app.js` monolitico y ahora inicia como modulos ES:
  - `wwwroot/js/core/*`
  - `wwwroot/js/modules/project.js`
  - `wwwroot/js/modules/surveys.js`
  - `wwwroot/js/modules/requirements.js`
  - `wwwroot/js/modules/templates.js`
- Se habilito soporte explicito para `modo claro` y `modo oscuro` con persistencia local.
- Se documento el estado funcional vigente del sistema en:
  - `docs/agent/current-system-state.md`
  - `docs/product/interview-workflow.md`

## Reglas de arquitectura

1. `RequirementWorkspaceService` debe permanecer como orquestador.
   - Si una responsabilidad nueva crece mas de lo razonable, moverla a una clase enfocada.
   - Evitar volver a meter logica de formato, mapeo o normalizacion pesada ahi.
2. La logica de negocio sigue viviendo en `Domain` y `AppCore`.
   - `API` expone endpoints y configura dependencias.
   - `wwwroot` no debe duplicar reglas de negocio ya resueltas en backend.
3. Mantener cambios pequenos.
   - Si un archivo empieza a mezclar captura, render, persistencia y formateo, separarlo.
4. Cuando se agreguen nuevas capacidades de encuestas:
   - preferir modulos o helpers nuevos antes que agrandar `surveys.js`.

## Reglas de frontend

1. El frontend debe seguir organizado por responsabilidad:
   - `core/`: estado, API, utilidades DOM, contexto compartido.
   - `modules/`: flujos por feature.
2. No volver a introducir un script unico gigante.
3. Antes de agregar un nuevo archivo compartido, revisar si realmente es transversal o si pertenece a un solo modulo.
4. En formularios y cajas de trabajo con accion explicita de guardado, `Enter` debe guardar cuando el flujo sea de captura/edicion corta.
5. Al crear una encuesta, el sistema debe iniciar con `Yo` y al menos un participante entrevistado por defecto.
6. Antes de cambiar comportamiento de voz, transcript o minuta, revisar la documentacion de estado actual para no romper acuerdos de producto ya definidos.

## Reglas visuales

1. Toda pantalla debe verse correctamente en:
   - modo claro
   - modo oscuro
   - escritorio
   - movil
2. No hardcodear colores nuevos en componentes si pueden resolverse con tokens CSS.
   - Preferir variables en `:root` y `:root[data-theme="dark"]`.
3. Si se agrega una nueva superficie visual, validar contraste de:
   - fondo
   - texto
   - borde
   - estados activos
   - botones primarios y peligrosos
4. El modo claro debe sentirse descansado; evitar superficies excesivamente blancas cuando una variable de sistema ya existe.

## Checklist minimo antes de cerrar una tarea de UI

- La funcionalidad corre en modo claro.
- La funcionalidad corre en modo oscuro.
- No se rompieron layouts moviles.
- No se agregaron estilos acoplados a un solo tema.
- Si la tarea toca arquitectura, el cambio quedo documentado en `memory/history.md`.

## Reglas de entrevista

1. `Yo` se mantiene como nombre interno de captura, pero las salidas de transcript/minuta para analisis deben mostrar `Entrevistador`.
2. El boton de avance de preguntas debe priorizar el flujo natural de entrevista:
   - si falta respuesta del entrevistado, la UI debe pedirla antes de avanzar.
   - si el sistema cambia el foco al entrevistado, el texto del boton debe reflejar esa intencion.
3. La captura debe conservar contexto por `pregunta + participante`.
   - cambiar de pregunta o de persona no debe borrar texto previo.
   - la sincronizacion tardia del navegador no debe bloquear otros buffers de trabajo.
4. El transcript final debe mostrarse por pregunta con turnos cronologicos, no como bloques separados por persona.
5. Si una entrevista usa datos viejos con tags `undefined`, la UI debe intentar reconstruir el titulo real desde la plantilla aplicada.
