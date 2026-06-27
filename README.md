# Anemoi_Open

Anemoi_Open is an enterprise HR management platform built on .NET 10, Clean Architecture, CQRS, Event-Driven Architecture, and Saga orchestration patterns. The repository is structured as a modular monolith / independently deployable services platform, with HR as the main active business domain.

## Project Scope

The platform currently covers identity and access management, workspace management, master data, notification, security, storage, and HR modules such as employee lifecycle, attendance, payroll, compensation, benefits, recruitment, onboarding, workflow, and analytics.

## Technology Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core with PostgreSQL via Npgsql
- RabbitMQ via MassTransit
- Riok.Mapperly for compile-time object mapping
- MediatR with OneOf for CQRS result handling
- FluentValidation
- HotChocolate GraphQL where applicable
- Serilog and Polly
- JWT-based identity management
- Next.js frontend in `cody-web-app`

## Repository Structure

```text
Anemoi.BuildingBlocks/   Shared kernel, base abstractions, CQRS pipeline, helpers
Anemoi.Centralize/       Gateway / aggregation API
Anemoi.Contract/         Shared contracts, integration events, cross-service DTOs
Anemoi.Grpc/             gRPC service contracts/layers
Anemoi.Hr/               Core HR bounded context
Anemoi.Identity/         Authentication, users, role groups, permissions
Anemoi.Kubernetes/       Kubernetes deployment resources
Anemoi.MasterData/       Static/reference data
Anemoi.Notification/     Notification and email dispatch
Anemoi.Orchestration/    Saga orchestration
Anemoi.Secure/           Security and encryption service
Anemoi.Storage/          File/object storage service
Anemoi.Workspace/        Workspace and multi-tenancy management
cody-web-app/            Frontend application
docs/                    Architecture, AI guidelines, roadmap, plans, reports
```

## Documentation Entry Points

- [docs/README.md](docs/README.md) - documentation map and source-of-truth policy
- [ArchitectureGuide.md](ArchitectureGuide.md) - practical architecture and coding guide
- [docs/architecture/ARCHITECTURE_DECISIONS.md](docs/architecture/ARCHITECTURE_DECISIONS.md) - approved architectural decisions
- [docs/architecture/TECHNICAL_DEBT_REGISTER.md](docs/architecture/TECHNICAL_DEBT_REGISTER.md) - known technical debt
- [docs/ai/README.md](docs/ai/README.md) - mandatory guide for AI agents
- [DEVELOPMENT.md](DEVELOPMENT.md) - development conventions and workflow
- [AGENTS.md](AGENTS.md) - compact project memory for coding agents

## Getting Started

Prerequisites:

- .NET 10 SDK
- Docker Desktop
- RabbitMQ, usually through Docker Compose

```bash
cp .env.example .env
docker compose up -d --build
dotnet build Anemoi.sln
```

On Windows x64, use the Docker Compose override:

```powershell
docker compose -f docker-compose.yml -f docker-compose.windows-x64.yml up -d --build
```

JWT key handling:

- Development generates the JWT key pair outside the repository on first run, using the configured `JwtSetting` paths.
- Docker Compose overrides `JwtSetting__PrivateKeyPath` and `JwtSetting__PublicKeyPath` so all local containers share the same dev key pair through the `jwt_keys` volume.
- Production deployments should set the same `JwtSetting__...Path` values explicitly through environment variables.

## Development Rules

- Use Clean Architecture boundaries for each service.
- Use CQRS through MediatR; mutations are commands, reads are queries.
- Return `OneOf<TResult, ErrorDetailResponse>` for expected business outcomes.
- Use strongly typed IDs for local aggregate IDs.
- Use `IdGenerator.NextGuid()` when creating new IDs.
- Use Mapperly, not AutoMapper.
- Use FluentValidation for command/query validation.
- Use MassTransit for internal async messaging and request-response when a synchronous internal result is required.
- Use permission-based authorization through stable permission constants.
- Localize user-facing backend messages with `IStringLocalizer<SharedResource>`.

For feature work, follow the iterative workflow documented in [ArchitectureGuide.md](ArchitectureGuide.md) and [docs/ai/README.md](docs/ai/README.md).
