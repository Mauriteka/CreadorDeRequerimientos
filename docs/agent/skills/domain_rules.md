# Domain Rules

## Scope

Aplican a `src/VABELRoutes.Domain`. El dominio debe permanecer puro y no puede referenciar otros proyectos del repositorio.

## Base Primitives

No duplicar primitivas base existentes:

- `Entity<TId>`
- `AggregateRoot<TId>`
- `AuditableEntity<TId>`
- `IUnitOfWork`
- `DomainEvent`
- `IAggregateRoot`

## Entity Pattern

Toda entidad nueva debe:

- heredar de `AuditableEntity<TEntityId>` o `AggregateRoot<TId>` si no necesita auditoria
- declarar su typed id como `record` en el mismo namespace
- tener constructor privado vacio para EF
- tener constructor privado con parametros
- exponer solo un factory method publico `Create(...)`
- mantener setters como `private set`
- encapsular comportamiento en metodos de instancia
- llamar `SetAudit(userId)` en cada cambio de estado

Patron correcto:

```csharp
public sealed class Product : AuditableEntity<ProductId>
{
    public Code Code { get; private set; } = null!;
    public Name Name { get; private set; } = null!;
    public Price Price { get; private set; } = null!;

    private Product() { }

    private Product(ProductId id, Code code, Name name, Price price) { }

    public static Product Create(ProductId id, Code code, Name name, Price price, UserId createdBy)
        => new Product(id, code, name, price);

    public void Rename(Name newName, UserId userId)
    {
        Name = newName ?? throw new ArgumentNullException(nameof(newName));
        SetAudit(userId);
    }
}
```

Patron prohibido:

```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

## Typed IDs

Cada entidad debe usar su propio ID tipado:

```csharp
public record ProductId(Guid Value);
```

No usar `Guid` directo como identidad de entidad.

IDs existentes:

- `UserId`
- `ProductId`
- `CustomerId`
- `UserTypeId`
- `RefreshTokenId`

## Value Objects

Usar value objects para conceptos con reglas, formato o validacion.

Patron obligatorio:

- `record`
- constructor privado
- `Create(...)` estatico
- retorno `null` si es invalido

Ejemplo:

```csharp
public partial record class Code
{
    private Code(string value) => Value = value;
    public string Value { get; init; }

    public static Code? Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        if (value.Length < 4 || value.Length > 20) return null;
        if (!CodeRegex().IsMatch(value)) return null;
        return new Code(value);
    }
}
```

Value objects existentes:

- `Code`
- `Name`
- `Password`
- `EmailAddress`
- `PhoneNumber`
- `Price`
- `Address`
- `ImageUrl`

## Domain Errors

Todo error de dominio debe vivir en `DomainErrors/Errors.{Aggregate}.cs` como clase parcial estatica.

Ejemplo:

```csharp
public static partial class Errors
{
    public static class Product
    {
        public static Error Code =>
            Error.Validation("Product.Code", "Code has not valid format");
        public static Error NotFound =>
            Error.NotFound("Product.NotFound", "Product not found");
    }
}
```

Tipos permitidos:

- `Error.Validation`
- `Error.NotFound`
- `Error.Conflict`
- `Error.Unauthorized`
- `Error.Forbidden`

No usar excepciones para errores de negocio esperados; usar `ErrorOr`.

## Repository Interfaces

Las interfaces de repositorio viven en `Domain/Entities/{Aggregate}/I{Aggregate}Repository.cs`.

Reglas:

- usar tipos del dominio, no primitivos
- no depender de infraestructura
- conservar contratos orientados al lenguaje ubicuo del dominio

Ejemplo correcto:

```csharp
Task<Product?> GetByCodeAsync(Code code);
```

Ejemplo incorrecto:

```csharp
Task<Product?> GetByCodeAsync(string code);
```
