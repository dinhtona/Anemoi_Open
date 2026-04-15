# IMPLEMENTATION PLAN: [User management]

## 1. Context & Objective
- **Feature**: [User management]
- **Target Service**: [anemoi_identity]
- **Reference Pattern**: Based on [User in anemoi_identity]

## 2. Structural Changes & File Manifest

- **Domain Layer (`Anemoi.{Service}.Domain`)**:
  - `[Name]Id` (Custom ID)
  - `Models/[Name].cs` (Entity)

- **Application Layer (`Anemoi.{Service}.Application`)**:
  - `Cqrs/Commands/[Name]Commands`: `Create[Name]Command.cs`, `Create[Name]CommandHandler.cs`
  - `Cqrs/Queries/[Name]Queries`: `Get[Name]Query.cs`, `Get[Name]QueryHandler.cs`
  - `Mappings/[Name]Mapper.cs` (partial class using `[Mapper]`)
  - `Validations/`: Corresponding FluentValidators

- **Infrastructure Layer (`Anemoi.{Service}.Infrastructure`)**:
  - `DataContext/[Service]DbContext.cs` (Add DbSet)
  - `DataContext/Configurations/[Name]Configuration.cs` (Fluent API)
  - Generate Migration.

- **API Layer (`Anemoi.{Service}.Api` / `WorkerService`)**:
  - `Controllers/[Name]Controller.cs` or GraphQL Endpoints.
