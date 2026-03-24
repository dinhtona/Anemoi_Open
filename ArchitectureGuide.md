# Anemoi Open - Platform Architecture & Guidelines

## 1. System Overview
The `Anemoi_Open` project is designed based on a **Microservices** architecture, utilizing **Docker Compose** for local orchestration. The system communicates asynchronously following an **Event-Driven** model via a Message Broker. Each individual service strictly adheres to **Clean Architecture combined with CQRS**, along with domain-agnostic shared logic located in `Anemoi.BuildingBlocks`.

**Key Microservices:**
- **anemoi_centralize**: Gateway API/Aggregation Service that centralizes requests (hosts controllers, configures Swagger/GraphQL).
- **anemoi_identity**: Identity and access management (Authentication/Authorization via JWT).
- **anemoi_masterdata**: Manages shared domain data (Provinces/Cities, Districts, etc.).
- **anemoi_workspace**: Workspace management.
- **anemoi_orchestrator**: Saga pattern orchestrator for handling distributed transactions.
- **anemoi_notification**: Notification and email dispatching service.
- **anemoi_secure**: Security and encryption service.

**Infrastructure Components:**
- **RabbitMQ**: Message Broker (asynchronous Pub/Sub communication).
- **PostgreSQL**: Primary relational database for most services.
- **MongoDB**: NoSQL database (used primarily within the Orchestrator).

---

## 2. Core Stack & Libraries
When developing within this project, you **MUST** use the following predefined libraries and frameworks. Do not substitute them without explicit permission:

- **Web Framework**: `.NET 8` (ASP.NET Core 8.0).
- **Architecture**: Clean Architecture + CQRS + Event-driven.
- **Data Access/ORM**: `Microsoft.EntityFrameworkCore` & `Npgsql.EntityFrameworkCore.PostgreSQL`.
- **Message Bus / Events**: `MassTransit` combined with `MassTransit.RabbitMQ` & `MassTransit.EntityFrameworkCore`.
- **Object Mapping**: `Riok.Mapperly` (Utilizes source generators at compile-time for max performance, instead of reflection-based AutoMapper).
- **Validation**: `FluentValidation.AspNetCore`.
- **CQRS / Mediator**: `MediatR` combined with `OneOf` (to handle polymorphic return types like Success/Error without throwing Exceptions for control flow).
- **GraphQL**: `HotChocolate.AspNetCore` & `HotChocolate.Data.EntityFramework` (For exposing GraphQL APIs).
- **Logging**: `Serilog.AspNetCore`.
- **Resilience / Fault Tolerance**: `Polly`.
- **ID Generation**: Use custom strongly-typed IDs (e.g., `ProvinceId(...)`, `DistrictId(...)` instead of raw Guids or integers).

---

## 3. Standard Service Directory Structure (Clean Architecture)
Each Microservice (e.g., class library like `Anemoi.MasterData`) is divided into 4 main sub-projects, maintaining strict boundary rules:

1. **`{Service}.Domain`**: 
   - Contains Domain Entities, Value Objects, Domain Events, and Custom Strongly-Typed IDs.
   - This layer is absolutely independent. It must not depend on Infrastructure or any external logic aside from `MediatR` and `OneOf`.

2. **`{Service}.Application`**: 
   - Contains Business Use Cases following the CQRS pattern.
   - **`Cqrs/Commands/`**: Contains Command objects and CommandHandlers (Write/Update operations following CQS principles).
   - **`Cqrs/Queries/`**: Contains Query objects and QueryHandlers (Read operations).
   - **`Mappings/`**: Contains Mappers utilizing `Riok.Mapperly`.
   - This layer references and depends only on `Domain`.

3. **`{Service}.Infrastructure`**: 
   - Contains EF Core DbContext, Database connection configurations, migrations, and implementations of external APIs.
   - Depends on both `Application` and `Domain`.

4. **`{Service}.Api` or `{Service}.WorkerService`**: 
   - The entry point that bootstraps the service (hosted via Docker). Contains `Program.cs` which configures Dependency Injection, Serilog, Rate Limiters, and MassTransit.
   - May expose REST endpoints via `Controllers` or `GraphQL`.
   - References `Infrastructure`.

*(Note: All services depend on `Anemoi.BuildingBlocks` — acting as a Shared Kernel holding Base classes, shared Interfaces, Error-handling Middlewares, Pagination helpers, etc.)*

---

## 4. STRICT Coding Conventions

When an AI or Developer receives a request to add a new feature, use the following as a mandatory checklist:

1. **Adhere to the correct Layer**: Business logic MUST be placed in `Application/Cqrs/...`. Complex Database queries should be pushed down to Infrastructure or encapsulated within an Application handler. Never query the DB directly in a Controller.
2. **Implement CQRS Strictly**: 
   - Every Add/Update/Delete action requires its own `xxxCommand.cs` and `xxxCommandHandler.cs` file.
   - Similarly, fetching data requires a `xxxQuery.cs`, returning a read-only object/DTO rather than a Domain Entity.
3. **Mapping with `Riok.Mapperly`**: 
   - Declare Mappers in `Application/Mappings`.
   - Must be declared as a `partial class` annotated with `[Mapper]`.
   - Define signatures via partial methods (e.g., `private partial District MapToDistrict(CreateDistrictCommand command);`). Mapperly will auto-generate the underlying implementation at compile time.
4. **Use Strongly-typed IDs**:
   - Initialize entity IDs using the provided Helper (e.g., `new ProvinceId(IdGenerator.NextGuid())`), never assign raw Guids directly.
5. **Event-driven Cross-Service Communication**:
   - When Service A needs to interact with or trigger an action in Service B, it MUST use Event Messages via `MassTransit` (publishing/consuming over RabbitMQ). Minimize synchronous HTTP/REST calls between services to prevent tight coupling.
6. **Do not introduce arbitrary external libraries**: The core toolkit is already defined in `Anemoi.BuildingBlock`. Use the existing `MediatR` for dispatching, `FluentValidation` for validation, and `Polly` for retries.

### 💡 Note for AI Assistants
Whenever instructed to code a new Endpoint/API or worker logic for this project, always review this document, properly configure the Command/Query, setup the Mapper, place files in their correct directories, and return Response objects wrapped in the Mediator `OneOf<>` pattern rather than returning Db Entities directly!

---

## 5. Iterative Execution Steps for AI
When an AI receives an `IMPLEMENTATION PLAN` prompt from the user, the AI **MUST** execute the task iteratively in 3 strict steps. **Do not proceed to the next step until the user reviews and approves the current step.**

- **Step 1 (Domain & Data)**: 
  - Define `Strongly-typed ID`.
  - Define `Entity` models.
  - Configure `EntityTypeConfiguration` and register it in `DbContext`.
  - *(Stop here and ask the user for review)*.
- **Step 2 (Application Layer - CQRS & Mappings)**: 
  - Declare Request/Response models.
  - Set up the Mapperly profile for this feature.
  - Create Command, Query, Handler files, and their respective FluentValidators.
  - *(Stop here and ask the user for review)*.
- **Step 3 (API & Communication)**: 
  - Define Controller/GraphQL Endpoints to expose the functionality.
  - Create integration message classes and wire up MassTransit publishing logic (if external events apply).
  - *(Stop here for final review)*.
