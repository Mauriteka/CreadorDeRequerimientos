# Uso Desde Celular En Red Local

Esta es la forma mas simple de usar la app desde tu celular sin meter login, base de datos o hosting externo.

## Opcion recomendada

Levanta la app en tu PC y entra desde el celular usando la misma red Wi-Fi.

Comando:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\run_mobile.ps1
```

El script te mostrara dos URLs:

- `http://localhost:5046` para la PC
- `http://TU_IP_LOCAL:5046` para el celular

## Requisitos

1. PC y celular en la misma red local.
2. Permitir el acceso en firewall privado si Windows lo pregunta.
3. Dejar abierta la ventana donde corre el script.

## Notas sobre voz en movil

La app usa `Web Speech API`.

- En escritorio suele funcionar mejor en Chrome o Edge.
- En celular, el dictado puede no estar disponible en todos los navegadores.
- Algunos navegadores moviles requieren `https` o un contexto seguro para exponer reconocimiento de voz.
- Si el dictado no aparece en el celular, la captura manual por texto debe seguir funcionando.

## Publicar una carpeta ejecutable

Si quieres dejar una carpeta lista para mover o copiar a otra maquina:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\publish_mobile.ps1
```

La salida queda en:

`artifacts/publish/mobile`

## Si luego quieres subirla a Internet

Se puede, pero antes conviene resolver esto:

1. Agregar al menos una capa minima de proteccion, porque hoy no tiene login.
2. Decidir donde vivira `data/workspace.json`.
3. Evaluar `https` real, sobre todo por el tema de voz en navegadores moviles.

Para empezar a usarla desde tu cel, LAN es el camino mas rapido y coherente con el enfoque local-first del repo.
