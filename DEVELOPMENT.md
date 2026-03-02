# Anemoi Development Guidelines & Rules

This document outlines the architectural standards, coding patterns, and conventions for the Anemoi project. Following these rules ensures consistency, maintainability, and architectural integrity.

## 1. Project Architecture

The project follows **Clean Architecture** principles and is structured as a **Modular Monolith** (or microservices if deployed independently).

### Layer Responsibilities

| Layer | Responsibility | Dependencies |
| :--- | :--- | :--- |
| **Domain** | Core business logic, Entities, Aggregates, Value Objects, Domain Events, Repository Interfaces. | None |
| **Application** | Use Cases, CQRS Handlers, Mappings, Validators, DTOs (via Contracts). | Domain |
| **Infrastructure** | Database (EF Core), External Service implementations, Repositories (Implementation). | Domain, Application |
| **Host/WebAPI** | Entry point, Dependency Injection registration, Middleware, Configuration. | All layers |
| **Contract** | Shared DTOs, Strongly Typed IDs, Response models, Grpc Definitions. | None |

## 2. Coding Patterns & Standards

### CQRS with MediatR
- Every operation should be either a **Command** (state change) or a **Query** (data retrieval).
- Use `IRequest<OneOf<T, ErrorDetailResponse>>` to ensure structured success/error handling.
- Handlers should inherit from `ICommandHandler` or `IQueryHandler`.

### Result Pattern (OneOf)
- Avoid throwing exceptions for expected business logic errors. 
- Return `OneOf<TResult, ErrorDetailResponse>` where `ErrorDetailResponse` contains a list of error messages.

### Strongly Typed IDs
- Avoid using primitive types (`Guid`, `int`, `string`) for entity IDs.
- Define a strongly typed ID record (e.g., `UserId`) in the `Contract` project.
- Inherit from `StronglyTypedId<TValue>` provided in `BuildingBlocks`.

### Mapping with Mapperly
- Use **Mapperly** for all object-to-object mappings (replacing AutoMapper).
- Create a `partial` class with the `[Mapper]` attribute in the `Application` layer.
- Use `partial` methods for source-generated mappings.
- For logic like generating new IDs or hashing passwords, use manual wrapper methods.

### Exception Handling & Validation
- Use **FluentValidation** for request validation.
- Validation is triggered automatically via MediatR `ValidationBehavior` pipeline.
- If validation fails, it throws a `ValidationException` which is handled by centralized middleware.

### Message-Driven Architecture
- Use **MassTransit** for event-driven coordination.
- Entities can publish **Domain Events** which are then handled asynchronously or converted to Integration Events.

## 3. Naming Conventions

- **Namespaces**: `Anemoi.{Module}.{Layer}`
- **Commands**: `Create{Entity}Command`, `Update{Entity}Command`
- **Queries**: `Get{Entity}ByIdQuery`, `Search{Entity}Query`
- **Interfaces**: Always prefix with `I` (e.g., `IUserRepository`).
- **Files**: One class per file, filename must match class name.

## 4. Feature Implementation Workflow

When adding a new feature, follow these steps:

1. **Contract**: Define Strongly Typed IDs and Request/Response DTOs in `Anemoi.Contract`.
2. **Domain**: Add/Update Entities or Value Objects in the `Domain` layer. Define repository interfaces if needed.
3. **Application**:
   - Define Command/Query classes.
   - Implement `ICommandHandler` or `IQueryHandler`.
   - Update `Mapperly` mappers to handle new DTOs.
   - Add `FluentValidation` validators.
4. **Infrastructure**: 
   - Update EF Core `DataContext` and configurations.
   - Implement or update repository classes.
5. **Host**: Register new services in Dependency Injection installers.
6. **Grpc (Optional)**: If exposed over Grpc, update `.proto` files and implement Grpc services.

## 5. Do's and Don'ts

- **DO** keep the Domain layer free of any external dependencies (e.g., EF Core, Grpc).
- **DO** use the `IdGenerator` for generating new Guids.
- **DO** use `IQueryable` projections in mappers (`ProjectTo{Response}`) for performance.
- **DON'T** inject repositories into other repositories; use Domain Services instead if necessary.
- **DON'T** use `AutoMapper` (it is being phased out).
