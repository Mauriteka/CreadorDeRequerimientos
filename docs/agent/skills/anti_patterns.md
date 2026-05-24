# Anti-Patterns

## Scope

Este archivo resume las practicas prohibidas que el agente debe detectar y evitar.

## Business Logic Outside Domain

Prohibido:

```csharp
if (request.Price <= 0) return BadRequest("Price must be positive");

public override Task<int> SaveChangesAsync(...)
{
    foreach (var entry in ChangeTracker.Entries<Product>())
        if (entry.Entity.Price.Value <= 0) throw new Exception("Invalid price");
}
```

Correcto:

```csharp
public static Price? Create(decimal value)
{
    if (value <= 0) return null;
    return new Price(value);
}
```

## Skipping Layers

Prohibido:

```csharp
public class ProductsController(ApplicationDBContext db) { }
public class CreateProductHandler(ApplicationDBContext db) { }
```

Correcto:

```csharp
public class CreateProductHandler(IProductRepository repo, IUnitOfWork uow) { }
```

## Returning Domain or EF Shapes from API

Prohibido:

```csharp
return Ok(user);
```

Correcto:

```csharp
return Ok(user.ToResponse());
```

## DTO Duplication

Prohibido definir DTOs equivalentes en `AppCore` o `API` cuando ya existen en `Contracts`.

## Primitive Obsession

Prohibido:

```csharp
public class Product { public string Code { get; set; } }
```

Correcto:

```csharp
public class Product { public Code Code { get; private set; } }
```

## Anemic Model

Prohibido:

```csharp
public class User { public bool Active { get; set; } }
```

Correcto:

```csharp
public class User
{
    public bool Active { get; private set; }
    public void Activate(UserId userId) { Active = true; SetAudit(userId); }
}
```

## Mixing Commands and Queries

Prohibido:

```csharp
public class UserManagerHandler : IRequestHandler<UserManagerCommand, object>
{
    public async Task<object> Handle(UserManagerCommand request, CancellationToken cancellationToken)
    {
        var users = await repo.GetAllAsync();
        await repo.AddAsync(newUser);
        return new object();
    }
}
```

Correcto:

- `CreateUserCommandHandler` solo escribe
- `GetAllUsersQueryHandler` solo lee

## Detection Heuristic

Si una propuesta:

- rompe capas
- introduce mutacion fuera del dominio
- duplica DTOs
- usa primitivos donde debe haber value objects
- mezcla lectura y escritura

entonces debe rechazarse y redisenarse.
