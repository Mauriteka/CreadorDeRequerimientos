# Interview Workflow

Este documento describe el flujo de uso esperado del sistema durante una entrevista real.

## Objetivo

Permitir que una persona levante requerimientos con apoyo de voz, sin perder contexto entre preguntas, hablantes y vueltas atras.

## Flujo operativo

1. Elegir proyecto.
2. Crear o abrir encuesta.
3. Aplicar plantilla.
4. Confirmar participantes.
5. Seguir la guia de preguntas.
6. Capturar turnos.
7. Revisar transcript y minuta.
8. Generar requerimientos.

## Participantes

- `Entrevistador`
  - nombre visual del participante interno `Yo`
- Participantes invitados
  - nombres libres como `Compras`, `Calidad`, `Operaciones`

Regla:

- una encuesta debe arrancar con `Entrevistador` y al menos un participante entrevistado

## Preguntas

Las preguntas vienen de la plantilla aplicada.

Cada pregunta puede estar en tres estados:

- `Pendiente`
- `Preguntada`
- `Respondida`

## Captura

La captura debe sentirse agil en piso:

- participantes visibles
- accion de iniciar/detener
- pregunta actual siempre visible
- transcript derivado sin mezclar hablantes

## Boton de avance

### Responder

Debe mostrarse cuando:

- el entrevistador ya planteo la pregunta
- aun no existe respuesta del entrevistado para esa pregunta

### Siguiente pregunta

Debe mostrarse cuando:

- la pregunta actual ya cuenta con respuesta del entrevistado

### Finaliza

Debe mostrarse en la ultima pregunta.

## Reglas de transcript

- Una tarjeta por pregunta.
- Varias lineas por turnos cronologicos.
- Hora con segundos.
- No separar transcript en una tarjeta por persona.

## Reglas de minuta

- Debe derivarse del transcript cronologico.
- Debe usar el titulo real de la pregunta cuando el sistema pueda resolverlo desde plantilla.
- Debe mostrar `Entrevistador`, no `Yo`.

## Consideraciones de voz

La herramienta usa Web Speech API.
Por eso:

- puede haber latencia entre hablar y ver el texto
- puede haber reconexiones al cambiar rapido de pregunta
- el sistema debe preferir esperar un momento antes de perder el turno o mezclar texto

## UX esperada en movil

- la pregunta actual debe ser legible sin esfuerzo
- la captura debe estar cerca de la pregunta
- la persona que entrevista no debe navegar por paneles grandes para seguir el hilo

## Anti objetivos

La herramienta no intenta:

- resolver autenticacion
- ser colaborativa multiusuario
- imponer un CRM o proceso empresarial pesado

Es un asistente personal de entrevistas y redaccion de requerimientos.
