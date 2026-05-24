# Current System State

Este documento resume el estado funcional y tecnico de la herramienta al cierre del 2026-05-23.
La meta es evitar que el conocimiento quede solo en la conversacion.

## Resumen del producto

La aplicacion es una herramienta personal, local-first, para levantar requerimientos por entrevista.

Flujo principal:

1. Crear o abrir un proyecto.
2. Crear una encuesta para una entrevista.
3. Seleccionar o aplicar una plantilla.
4. Guiar la entrevista con preguntas por seccion.
5. Capturar voz o texto por turnos.
6. Revisar transcript, minuta y requerimientos.

No hay login multiusuario.
Los datos se guardan por defecto en `data/workspace.db` con SQLite embebido.
Si la base esta vacia, el sistema puede importar `data/workspace.json` como respaldo legacy.

## Arquitectura actual

### Backend

- `Domain`
  - Entidades de proyecto, encuesta, plantilla, participante, transcript y requerimiento.
- `AppCore`
  - Orquestacion del workspace.
  - Normalizacion de datos.
  - Formateo de transcript y minuta.
  - Mapeo a DTOs.
  - Plantilla por defecto.
- `Infrastructure`
  - Persistencia SQLite local por defecto.
  - Persistencia JSON disponible como rollback por configuracion.
- `Contracts`
  - DTOs de API.
- `API`
  - Minimal APIs.
  - Hosting de frontend estatico.

### Frontend

El frontend arranca como modulos ES:

- `wwwroot/js/core`
  - estado global
  - API client
  - utilidades DOM
  - contexto compartido
- `wwwroot/js/modules`
  - `project.js`
  - `surveys.js`
  - `requirements.js`
  - `templates.js`

## Funcionalidad ya implementada

### Proyectos

- Lista lateral de proyectos.
- Edicion de nombre, funcionalidad y notas.
- Soporte para multiples requerimientos por proyecto.
- Soporte para plantillas propias por proyecto.

### Plantillas

- Plantillas del sistema.
- Plantillas por proyecto.
- CRUD de plantillas.
- Exportar plantilla de proyecto al sistema.
- Plantilla base sembrada automaticamente si el sistema arranca sin plantillas.

### Encuestas

- Multiples encuestas por proyecto.
- Participante `Yo` interno, mostrado como `Entrevistador`.
- Al menos un participante entrevistado por defecto en encuestas nuevas o normalizadas.
- Correos administrados en la seccion `Usuarios`.
- Campo de `Objetivo de esta entrevista`.
- Transcript cronologico por pregunta.
- Minuta y copia de entrevista.
- Borrador de correo por `mailto:`.

### Captura de entrevista

- Web Speech API en `es-MX`.
- Turnos cronologicos reales.
- Cambio de hablante crea un turno nuevo.
- Cambio de pregunta crea un turno nuevo.
- La pregunta actual vuelve el foco a `Entrevistador`.
- Boton de avance:
  - `Responder` si falta respuesta del entrevistado.
  - `Siguiente pregunta` si ya existe respuesta.
  - `Finaliza` en la ultima pregunta.
- Buffer activo por turno para no mezclar texto entre preguntas o hablantes.
- Sincronizacion en segundo plano del texto dictado.
- Reintentos cuando el navegador tarda en reactivar el reconocimiento tras cambiar rapido de pregunta.

### Transcript y minuta

- Una tarjeta por pregunta.
- Lineas cronologicas por turno.
- Formato de hora con segundos `HH:mm:ss`.
- `Yo` se muestra como `Entrevistador`.
- Si el tag viejo trae `undefined`, el titulo se intenta reconstruir desde la plantilla aplicada y el `questionKey`.
- Si una minuta vieja esta rota, la UI puede regenerarla desde transcript actual.

### Requerimientos

- Multiples requerimientos por proyecto.
- Un requerimiento puede enlazar multiples encuestas.
- Un requerimiento puede enlazar otros requerimientos del mismo proyecto.
- Vista tipo lista + detalle.
- Generacion de borrador desde entrevistas enlazadas.

### UI

- Modo claro.
- Modo oscuro persistente.
- Diseno orientado a escritorio y movil.
- Colores de contexto por participante activo en captura.
- Estado visual de preguntas:
  - `Pendiente`
  - `Preguntada`
  - `Respondida`

## Reglas funcionales importantes

### Identidad del hablante

- Internamente se conserva `Yo` por compatibilidad.
- Visualmente y en salida de minuta/transcript se usa `Entrevistador`.

### Regla de turnos

- Un turno es una intervencion continua de un hablante.
- Si cambia el hablante, se abre un turno nuevo.
- Si cambia la pregunta, se abre un turno nuevo.
- El transcript no debe reagruparse por persona dentro de una pregunta.

### Regla del boton de avance

- No debe avanzar si falta respuesta del entrevistado.
- Debe cambiar a `Responder` y ayudar a mover el foco al entrevistado.
- Debe volver a `Siguiente pregunta` cuando ya exista respuesta suficiente para la pregunta actual.

### Regla de guardado

- En campos cortos con accion natural de cierre, `Enter` guarda.
- En varios campos cortos tambien se guarda al perder foco cuando ya no existe boton dedicado.

## Problemas historicos que ya aparecieron

- Tags de transcript viejos con `question:0:0|undefined`.
- Bloqueos del harness en `Debug` por DLLs abiertas por Visual Studio o un proceso viejo de la API.
- Cortes del reconocimiento del navegador al alternar rapido entre preguntas.
- Riesgo de romper el render de encuestas por cambios fragiles en el template del `summary`.

## Zonas delicadas del sistema

### `surveys.js`

Sigue siendo el modulo mas grande y sensible.
Antes de tocarlo:

- revisar `docs/agent/development-guardrails.md`
- probar cambios con encuestas reales o con datos persistidos
- evitar re-renders completos cuando solo hace falta actualizar captura viva

### Web Speech API

El dictado depende del navegador.
Aunque el codigo ya tiene defensas, sigue habiendo limites del runtime:

- `onend` puede dispararse en momentos poco intuitivos
- `start()` puede fallar justo despues de un `stop()`
- los fragmentos parciales pueden llegar tarde

## Comportamiento esperado de una entrevista

1. El entrevistador lee la pregunta actual.
2. El sistema marca si la pregunta ya fue planteada o respondida.
3. Se captura un turno del entrevistador.
4. Se cambia al entrevistado.
5. Se captura su respuesta.
6. Si hace falta, se vuelve al entrevistador y luego otra vez al entrevistado.
7. El transcript conserva ese orden cronologico.
8. La minuta se deriva de ese transcript, no de bloques separados por persona.

## Guia para cambios futuros

- Si tocas voz o transcript, documenta el cambio en `memory/history.md`.
- Si tocas UX de encuestas, valida claro/oscuro y movil.
- Si tocas la logica del boton `Responder`, revisa:
  - turnos ya guardados
  - turno activo en memoria
  - cambio rapido entre preguntas
- Si tocas minutas, revisa encuestas con tags viejos `undefined`.
