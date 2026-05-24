# AppCore Rules

## Scope

Aplican a `src/VABELRoutes.AppCore`. Esta capa orquesta casos de uso y no puede referenciar `Infrastructure` ni `API`.

## Feature Structure

Organizar por agregado y por tipo de caso de uso:

```text
/AppCore
  /{Aggregate}
    /Commands
      /{ActionName}
        {ActionName}Command.cs
        {ActionName}CommandHandler.cs
        {ActionName}CommandValidator.cs
    /Queries
      /{QueryName}
        {QueryName}Query.cs
        {QueryName}QueryHandler.cs
    /Mappings
      {Aggregate}Mappings.cs
```

## Commands

Reglas:

- usar `record`
- implementar `IRequest<ErrorOr<T>>`
- contener solo primitivos o tipos de `Contracts`
- un command por accion
- un handler por command

Ejemplo:

```csharp
public record CreateUserCommand(
    string Code,
    string Name,
    string Email,
    string PhoneNumber,
    Guid UserTypeId
) : IRequest<ErrorOr<Guid>>;
```

## Command Handlers

Reglas:

- `internal sealed class`
- implementar `IRequestHandler<TCommand, ErrorOr<T>>`
- recibir dependencias por constructor
- orquestar validacion, construccion de value objects, repositorios y `SaveChangesAsync`
- devolver `ErrorOr<T>`
- usar `ISessionService` para el usuario actual
- no usar `DbContext` directo
- no lanzar excepciones para reglas de negocio

Ejemplo correcto:

```csharp
internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ISessionService sessionService
) : IRequestHandler<CreateUserCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = sessionService.GetCurrentUserId();
        if (currentUserId is null) return Errors.Session.NotAuthenticated;

        if (Code.Create(command.Code) is not Code code)
            return Errors.User.Code;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Guid.NewGuid();
    }
}
```

Ejemplos prohibidos:

```csharp
throw new InvalidOperationException("Code already exists");
_dbContext.Users.Add(user);
```

## Queries

Reglas correctas para codigo nuevo:

- usar `record`
- implementar `IRequest<ErrorOr<T>>`
- retornar DTOs de `VABELRoutes.Contracts`
- mapear entidad a DTO dentro del handler
- el controller recibe el DTO ya listo

Ejemplo:

```csharp
public record GetAllUsersQuery() : IRequest<ErrorOr<List<UserSummaryResponse>>>;

internal sealed class GetAllUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetAllUsersQuery, ErrorOr<List<UserSummaryResponse>>>
{
    public async Task<ErrorOr<List<UserSummaryResponse>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync();
        return users.Select(u => u.ToSummaryResponse()).ToList();
    }
}
```

Deuda tecnica conocida:

- hoy existen query handlers que retornan entidades de dominio
- no replicar ese patron

## Validators

Reglas:

- `internal class`
- heredar de `AbstractValidator<TCommand>`
- validar formato y presencia de primitivos
- no acceder a BD ni repositorios
- no duplicar validaciones profundas del dominio

Ejemplo:

```csharp
internal class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MinimumLength(4).MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(50);
    }
}
```

## Mappings

Reglas:

- clases estaticas
- extension methods sobre entidades de dominio
- sin logica de negocio
- retorno de DTOs en `Contracts`

Ejemplo:

```csharp
public static UserResponse ToResponse(this User user) => new UserResponse
{
    Id = user.Id.Value,
    Code = user.Code.Value,
    Name = user.Name.Value,
};
```

No activar entidades, no lanzar excepciones ni mutar estado dentro de mappings.

## Application Services

Interfaces esperadas en `AppCore/Common/`:

- `ISessionService`
- `IEmailSender`
- `ISmsSender`
- `IEmailValidator`
- `IPasswordGenerator`
- `IImageUploadService`
- `IUserNotifier`
- `IImageUploadManager`
