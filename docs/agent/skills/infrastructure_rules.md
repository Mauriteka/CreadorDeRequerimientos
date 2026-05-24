# Infrastructure Rules

## Scope

Aplican a `src/VABELRoutes.Infrastructure`. Esta capa implementa persistencia y servicios tecnicos usando interfaces de `Domain` y `AppCore`.

## Repositories

Reglas:

- implementar interfaces definidas en `Domain`
- usar `ApplicationDBContext` directamente para acceso a datos
- incluir navegaciones necesarias con `.Include()`
- no contener logica de negocio

Ejemplo:

```csharp
public class UserRepository(ApplicationDBContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(UserId id) =>
        await dbContext.Users
            .Include(u => u.UserType)
            .FirstOrDefaultAsync(u => u.Id == id);
}
```

## EF Configuration

Reglas:

- una configuracion `IEntityTypeConfiguration<T>` por entidad
- ubicar configuraciones en `Persistence/Configuration/`
- convertir value objects con `.HasConversion()`
- convertir typed ids a `Guid`
- configurar `ImageUrl` con `.OwnsOne()`
- no mover configuracion a `OnModelCreating` de forma ad hoc

Ejemplo de value object:

```csharp
builder.Property(u => u.Code)
    .HasConversion(
        code => code.Value,
        value => Code.Create(value)!)
    .HasMaxLength(20)
    .IsRequired();
```

Ejemplo de typed id:

```csharp
builder.Property(u => u.Id)
    .HasConversion(
        userId => userId.Value,
        value => new UserId(value));
```

## ApplicationDBContext

Reglas:

- implementar `IUnitOfWork`
- publicar `DomainEvents` desde `SaveChangesAsync` usando `IPublisher`
- no contener logica de negocio
- no exponerse fuera de Infrastructure

Prohibido:

```csharp
public class MyHandler(ApplicationDBContext db) { }
```

Correcto:

```csharp
public class MyHandler(IUserRepository repo, IUnitOfWork uow) { }
```

## Infrastructure Services

Implementaciones esperadas:

- `SmtpEmailSender` -> `IEmailSender`
- `TwilioSmsSender` -> `ISmsSender`
- `ImgBbImageUploadService` -> `IImageUploadService`
- `TokenService` -> `ITokenService`
- `SessionService` -> `ISessionService`
- `PasswordGenerator` -> `IPasswordGenerator`

## Dependency Injection

Reglas:

- registrar dependencias en `Infrastructure/DependencyInjection.cs` y `AppCore/DependencyInjection.cs`
- no usar service locator
- no instanciar dependencias manualmente dentro de handlers o servicios

## Secrets and Configuration

- nunca hardcodear connection strings, JWT, SMTP o Twilio
- usar `appsettings.json` y variables de entorno
- respetar configuraciones tipadas existentes como `SmtpSettings` y `TwilioSettings`
- no tocar Docker salvo solicitud explicita
