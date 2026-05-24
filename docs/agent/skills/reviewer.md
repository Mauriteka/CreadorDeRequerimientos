# Reviewer Checklist

Usar esta checklist al cerrar una tarea o antes de considerar listo un cambio.

- Clean Architecture sigue respetada
- CQRS sigue respetado
- Las queries nuevas retornan DTOs de `Contracts`
- No se introdujo primitive obsession nueva
- La logica de dominio sigue dentro de `Domain`
- `AppCore` no usa `DbContext` directo
- No se introdujeron dependencias prohibidas entre capas
- No se duplicaron DTOs fuera de `Contracts`
- No se mezclaron lectura y escritura en el mismo handler
- No se tocaron `src/` ni `tests/` sin instruccion explicita
- Los scripts resuelven la raiz del repo automaticamente
- `AGENTS.md` sigue funcionando como entrypoint corto
- La memoria local fue actualizada
