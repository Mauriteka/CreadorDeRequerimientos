# API Rules

## Scope

Aplican a `src/VABELRoutes.API`. Esta capa solo recibe HTTP, envia requests a MediatR y devuelve respuestas.

## Controllers

Reglas:

- heredar de `ApiController`
- inyectar `ISender`
- recibir request, construir command/query, enviar y devolver respuesta HTTP
- usar `.Match(success => ..., Problem)` o la variante equivalente con `ErrorOr`
- no contener logica de negocio
- no usar `DbContext` directo

Ejemplo correcto:

```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
{
    var result = await mediator.Send(new CreateUserCommand(
        request.Code, request.Name, request.Email, request.PhoneNumber, request.UserTypeId));

    if (result.IsError) return Problem(result.Errors);

    var userResult = await mediator.Send(new GetUserByIdQuery(result.Value));
    return userResult.Match(
        user => CreatedAtAction(nameof(GetById), new { id = user.Id }, user),
        Problem);
}
```

Ejemplo prohibido:

```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
{
    if (string.IsNullOrEmpty(request.Code)) return BadRequest("Code required");
    _dbContext.Users.Add(new User());
    await _dbContext.SaveChangesAsync();
    return Ok();
}
```

## Error Handling

`ApiController.Problem(List<Error>)` debe mapear `ErrorOr` a HTTP:

- `Validation` -> `422`
- `NotFound` -> `404`
- `Conflict` -> `409`
- `Unauthorized` -> `401`
- `Forbidden` -> `403`
- otros -> `500`

No exponer excepciones crudas al cliente. Lo inesperado lo captura `GlobalExceptionHandlingMiddleware`.

## Requests and Responses

Reglas:

- todos los DTOs viven en `VABELRoutes.Contracts`
- controllers reciben `*Request` de Contracts
- controllers retornan `*Response` de Contracts
- el mapeo se hace con extension methods en `AppCore/*/Mappings/`

## Contracts Rule

Aunque `Contracts` no tenga skill propia, esta regla es no negociable:

- si un DTO existe en `Contracts`, no debe duplicarse en ningun otro proyecto
- `Contracts` no contiene logica ni dependencias externas
- API y Mobile comparten DTOs exclusivamente desde esa capa
