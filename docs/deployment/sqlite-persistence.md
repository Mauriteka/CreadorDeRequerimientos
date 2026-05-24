# Persistencia SQLite

Desde el cambio de persistencia, la app guarda el workspace por defecto en SQLite:

```text
data/workspace.db
```

El modelo sigue siendo local-first y simple. No se agrego un esquema relacional completo; SQLite guarda un unico documento JSON en la tabla `workspace_state`.

## Que cambio

- `IRequirementWorkspaceStore` sigue siendo la interfaz de AppCore.
- `SqliteRequirementWorkspaceStore` es la implementacion por defecto.
- `JsonRequirementWorkspaceStore` sigue disponible como rollback.
- La app puede importar automaticamente `workspace.json` cuando `workspace.db` no existe o esta vacia.
- El archivo JSON legacy no se borra durante la importacion.

## Variables

Para produccion:

```text
Workspace__Storage=sqlite
Workspace__DatabaseFile=/data/workspace.db
Workspace__DataFile=/data/workspace.json
```

`Workspace__DatabaseFile` debe apuntar a un volumen persistente.
`Workspace__DataFile` solo se usa como respaldo legacy para importar datos existentes.

## Railway

1. Crear o confirmar un volumen persistente.
2. Montar el volumen en `/data`.
3. Configurar las variables anteriores.
4. Hacer redeploy con el codigo que incluye SQLite.
5. Verificar que la app carga los proyectos existentes.

Si ya existia `/data/workspace.json` y la base nueva esta vacia, el primer arranque copia ese contenido hacia SQLite.

## Rollback

Si necesitas volver temporalmente a JSON:

```text
Workspace__Storage=json
Workspace__DataFile=/data/workspace.json
```

Este rollback usa el JSON legacy. No sincroniza cambios nuevos desde SQLite hacia JSON.
Si ya trabajaste en SQLite y necesitas regresar esos cambios al JSON, primero hay que exportarlos o hacer una migracion especifica.

## Verificacion local

Ejecutar el harness:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\verify_env.ps1
```

Para probar importacion:

1. Mantener una copia de `data/workspace.json`.
2. Arrancar la app sin `data/workspace.db`.
3. Confirmar que se crea `data/workspace.db`.
4. Confirmar que proyectos, encuestas, transcript, minuta, plantillas y requerimientos siguen visibles.

