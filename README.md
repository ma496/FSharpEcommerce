# F# Ecommerce API

A modern, functional e-commerce REST API built with F# and ASP.NET Core. This project showcases F# capabilities in building clean, type-safe, and maintainable backend services.

## Prerequisites

Before running this application, make sure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) (version 11.0 or higher)
- [Git](https://git-scm.com/downloads) (for cloning the repository)

## Configuration

### Database Connection

The application uses PostgreSQL as its database. You can modify the connection string in the following files:

- `FSharpEcommerce/appsettings.json` - Main configuration
- `FSharpEcommerce/appsettings.Development.json` - Development environment configuration
- `FSharpEcommerce/appsettings.Testing.json` - Testing environment configuration

To change the connection string, modify the `ConnectionStrings:DefaultConnection` value in the appropriate settings file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=FSharpEcommerce;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
  }
}
```

### JWT Authentication

The application uses JWT for authentication. You can configure the JWT settings in the `appsettings.json` file:

```json
"Jwt": {
  "Secret": "YourVerySecretKey_ThisShouldBeAtLeast32CharactersLong",
  "Issuer": "FSharpEcommerce",
  "Audience": "FSharpEcommerce",
  "ExpiryMinutes": 60
}
```

## Running the Application

1. Clone the repository
2. Ensure PostgreSQL is running
3. Update the connection string in `appsettings.json` if needed
4. Navigate to the project directory and run:

```bash
cd FSharpEcommerce
dotnet run
```

The application will automatically create the database if it doesn't exist and apply all migrations.

## Testing the API

Once the application is running, you can access and test the API using Swagger UI:

```
http://localhost:5034/swagger/index.html
```

The Swagger UI provides documentation for all available endpoints and allows you to:

1. Register a new user account (`/account/register`)
2. Login to get a JWT token (`/account/login`)
3. Use the JWT token for authenticated requests by clicking "Authorize" and entering:
   ```
   Bearer your-jwt-token
   ```
4. Test all available API endpoints

## API Structure

The API is organized into the following key areas:

- **Account**: User registration, login, and profile management
- **Products**: CRUD operations for product management
- **Categories**: Product category management
- **Customers**: Customer information management
- **Orders**: Order processing and management

## The Benefits of F# for This Project

In this project F# brings several significant advantages to backend development:

### 1. Powerful Validation with Computation Expressions

The codebase showcases F#'s elegant validation approach using computation expressions, as seen in `CreateOrderEndpoint.fs`:

- **Clean, Declarative Validation Logic**: The `validate` computation expression provides a readable, declarative way to define validation rules:

```fsharp
let private validateCreateOrderRequest (request: CreateOrderRequest) =
    validate {
        let! _ = validateField request.CustomerId [greaterThan "CustomerId" 0]
        let! _ = validateField request.OrderDate [greaterThan "OrderDate" DateTime.MinValue]
        let! _ = validateField request.TotalAmount [greaterThan "TotalAmount" 0.0M]
        let! _ = validateField request.PaymentMethod [required "PaymentMethod"]
        let! _ = validateField request.ShippingAddress [required "ShippingAddress"]
        let! _ = validateField request.BillingAddress [required "BillingAddress"]

        let! _ = validateList "Items" validateCreateOrderItemRequest request.Items

        return request
    }
```

- **Composable Validation Rules**: Individual validators can be combined in lists and reused across different validation contexts
- **Early-Failure Semantics**: Validation stops at the first error, avoiding unnecessary validation checks
- **Railway-Oriented Programming**: The validation logic follows the "happy path" pattern, cleanly separating success and error flows

### 2. Higher-Order Functions for Separation of Concerns

The project leverages higher-order functions like `validateRequest` to separate validation from business logic:

```fsharp
let createOrder (connection: IDbConnection) (request: CreateOrderRequest) : Task<IResult> =
    validateRequest validateCreateOrderRequest request (createOrderHandler connection)
```

This approach provides several benefits:

- **Clean Separation of Concerns**: Validation logic is completely separate from business logic
- **Reusable Validation Framework**: The same validation pattern can be applied consistently across all endpoints
- **Simplified Error Handling**: Business logic only needs to handle valid data, reducing defensive coding
- **Improved Testability**: Validation and business logic can be tested independently
- **Composition of Functions**: Complex operations are built by composing smaller, focused functions

This validation approach demonstrates how F#'s functional features enable elegant, maintainable patterns for handling cross-cutting concerns like validation without sacrificing clarity or type safety.

### 3. Discriminated Unions and Pattern Matching for Domain Modeling

The codebase demonstrates the power of F# discriminated unions combined with pattern matching to model and enforce business rules, as seen in the order status workflow:

```fsharp
// From OrderModels.fs
type OrderStatus =
    | Pending
    | Processing
    | Shipped
    | Delivered
    | Cancelled
```

The application enforces specific order status transitions according to these business rules:
```
Pending → Processing | Cancelled
Processing → Shipped | Cancelled
Shipped → Delivered
Delivered → (final state)
Cancelled → (final state)
```

Traditional imperative code would require complex conditional statements to enforce these rules, making them difficult to understand and maintain. With F#'s pattern matching, these rules are implemented with elegant, self-documenting code:

```fsharp
// From UpdateOrderStatusEndpoint.fs
match oldOrderStatus, newOrderStatus with
| Pending, (Processing | Cancelled) ->
    let! _ = updateOrderStatus ()
    return ResultUtils.ok { OrderId = request.OrderId; Status = request.Status }
| Processing, (Shipped | Cancelled) ->
    let! _ = updateOrderStatus ()
    return ResultUtils.ok { OrderId = request.OrderId; Status = request.Status }
| Shipped, Delivered ->
    let! _ = updateOrderStatus ()
    return ResultUtils.ok { OrderId = request.OrderId; Status = request.Status }
| (Delivered | Cancelled), _ ->
    return ResultUtils.badRequest "You cannot change the order status if it is delivered or cancelled"
| _ ->
    return ResultUtils.badRequest $"Invalid order status transition: {oldOrderStatus} -> {newOrderStatus}"
```

This approach provides several critical benefits:

- **Type-Safe Enumerations**: Unlike string-based enums, OrderStatus is a type-safe enumeration where the compiler ensures only valid statuses can be used.

- **Executable Business Rules**: The status transition rules aren't just documentation - they're implemented directly in the code in a way that perfectly mirrors the business specification.

- **Exhaustive Pattern Matching**: The compiler enforces handling of all cases, preventing bugs from missed scenarios.

- **Business Rules as Types**: The state machine of order status transitions is clearly encoded in the pattern matching logic, making the business rules explicit and visible in the code.

- **Clean, Declarative Code**: Without pattern matching, implementing these transition rules would require verbose if-else chains or switch statements with complex conditions. Pattern matching transforms this into clean, declarative code that precisely matches the business domain.

This implementation shows how F# enables modeling complex business workflows in a way that's both expressive and safe, making invalid states unrepresentable in the type system.

This F# ecommerce application demonstrates that F# is an excellent choice for building robust, maintainable backend services. The language's strengths in type safety, domain modeling, and functional programming create a solid foundation for complex business applications while reducing the likelihood of bugs and making the code more adaptable to changing requirements.
