# Poner La App En Linea

La app ya puede trabajar con login de sesion por cookie y password configurado en servidor.

## Antes de subirla

Necesitas tres cosas:

1. `https` en el hosting.
2. Variables de entorno para el login.
3. Almacenamiento persistente para `workspace.json`.

## Variables recomendadas

```text
Auth__Username=admin
Auth__Password=TU_PASSWORD_LARGO
Workspace__DataFile=/data/workspace.json
```

`Auth__Password` no debe vivir en el repo.
Configuralo solo en el panel de secretos del hosting.

## Docker

El repo ya incluye `Dockerfile`.

Prueba local:

```powershell
docker build -t creador-requerimientos .
docker run -p 8080:8080 `
  -e Auth__Username=admin `
  -e Auth__Password=CAMBIA_ESTE_PASSWORD `
  -e Workspace__DataFile=/data/workspace.json `
  creador-requerimientos
```

## Persistencia

El sistema guarda todo en un archivo JSON.
Si el hosting no monta disco o volumen persistente, perderas datos al redeployar o reiniciar.

Por eso `Workspace__DataFile` debe apuntar a una ruta dentro del volumen persistente del proveedor.

## Ruta sugerida para empezar

Si quieres salir rapido:

1. Sube el repo a GitHub.
2. Despliega con Docker en un hosting simple.
3. Monta un volumen persistente.
4. Configura `Auth__Password`.
5. Abre la URL por `https`.

## Observaciones de producto

- La app sigue siendo de un solo usuario practico.
- El login protege acceso, pero no convierte el sistema en multiusuario real.
- Para uso desde celular con dictado, `https` mejora mucho la compatibilidad del navegador.
