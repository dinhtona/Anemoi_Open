# AGENTS.md - Anemoi_Open

Compact project memory for coding agents. Keep this file short; detailed rules live under `docs/`.

## Read First

1. `docs/README.md`
2. `docs/ai/README.md`
3. `ArchitectureGuide.md`
4. `DEVELOPMENT.md`
5. `docs/architecture/ARCHITECTURE_DECISIONS.md`
6. `docs/architecture/TECHNICAL_DEBT_REGISTER.md`

If documents conflict, follow approved ADRs first, then architecture/development rules, then module or phase documents.

## Snapshot

- Platform: .NET 10 HR management platform, modular monolith / independently deployable services.
- Architecture: Clean Architecture, CQRS, Event-Driven Architecture, Saga orchestration.
- Backend stack: EF Core + PostgreSQL, MassTransit + RabbitMQ, MediatR + OneOf, Mapperly, FluentValidation, Serilog, Polly.
- Frontend: Next.js app in `cody-web-app`.

## Non-Negotiables

- Use CQRS handlers; no business logic or DbContext queries in controllers.
- Use strongly typed IDs for local aggregate IDs and `IdGenerator.NextGuid()` for new IDs.
- Use Mapperly, not AutoMapper.
- Return `OneOf<TResult, ErrorDetailResponse>` for expected business outcomes.
- Use MassTransit for internal messaging; avoid HTTP/gRPC between internal services unless explicitly approved.
- Localize user-facing messages.
- Protect features with permission constants and `[HasPermission]`.
- In `Anemoi.Hr.*` production code, centralize business/status/type string literals as constants.
- For frontend-impacting work, run browser validation through available browser automation / DevTools MCP before reporting completion.

## Feature Workflow

When the repository workflow applies, stop for review after each step:

1. Domain and data.
2. Application layer.
3. API and communication.
4. Browser validation for frontend or full-stack changes.

## Commands

```bash
cp .env.example .env
docker compose up -d --build
dotnet build Anemoi.sln
```
