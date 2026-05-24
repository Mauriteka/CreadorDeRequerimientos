# Testing Rules

## Scope

Aplican a `tests/*`. En esta refactorizacion no se deben tocar tests sin instruccion explicita, pero las reglas siguen vigentes para futuras tareas.

## Testing Stack

- xUnit `2.5.3`
- Moq `4.20.72`
- FluentAssertions `8.3.0`
- global usings en `Usings.cs`: `FluentAssertions`, `Moq`, `ErrorOr`

## Structure

```text
/tests
  /VABELRoutes.Application.{Aggregate}.UnitTests
    /{Action}
      {Action}CommandHandlerTests.cs
    Usings.cs
```

## Test Pattern

Patron recomendado:

- Arrange
- Act
- Assert

Usar `Moq` para dependencias y `FluentAssertions` para verificar errores y resultados.

Ejemplo:

```csharp
public class CreateCustomerCommandHandlerUnitTest
{
    private readonly Mock<ICustomerRepository> mockRepo = new();
    private readonly Mock<IUnitOfWork> mockUow = new();
    private readonly CreateCustomerCommandHandler handler;

    public CreateCustomerCommandHandlerUnitTest()
    {
        handler = new CreateCustomerCommandHandler(mockRepo.Object, mockUow.Object);
    }

    [Fact]
    public async Task Handle_WhenPhoneHasBadFormat_ShouldReturnValidationError()
    {
        var command = new CreateCustomerCommand("John", "Doe", "test@mail.com", "INVALID_PHONE");
        var result = await handler.Handle(command, default);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(Errors.Customer.PhoneNumber.Type);
    }
}
```

## What to Test

Obligatorio:

- handlers de commands
- handlers de queries
- value objects

Recomendado:

- comportamiento de entidades de dominio

No prioritario:

- controllers
- infrastructure
- mappings simples

## Known Debt

- existen tests comentados o vacios en `Products` y `Users`
- si se trabaja en esas areas, redisenar desde cero en vez de descomentar sin validar
