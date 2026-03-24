# IMPLEMENTATION PLAN: [Feature Name]

## 1. Context & Objective
- **Feature**: [Brief description, e.g., Add Store Management]
- **Target Service**: [e.g., anemoi_masterdata]
- **Reference Pattern**: Based on [e.g., Province in MasterData]

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
