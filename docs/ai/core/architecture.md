# Core Architecture

Anemoi_Open is based on:

- Microservices deployed with Docker Compose for local orchestration.
- Event-driven communication through RabbitMQ and MassTransit.
- Clean Architecture and CQRS.
- PostgreSQL for all services, including MassTransit EF Core saga persistence.

Normative architecture rules are in `AGENTS.md` section 2.

## Existing Services

```text
anemoi_centralize
anemoi_identity
anemoi_masterdata
anemoi_workspace
anemoi_orchestrator
anemoi_notification
anemoi_secure
anemoi_hr
```

## HR Bounded Context

HR is a bounded context inside the modular monolith:

```text
Anemoi.Hr/Anemoi.Hr.ModelIds
Anemoi.Hr/Anemoi.Hr.Domain
Anemoi.Hr/Anemoi.Hr.Application
Anemoi.Hr/Anemoi.Hr.Infrastructure
Anemoi.Hr/Anemoi.Hr.Api
```

HR modules share employee profile, department hierarchy, approval workflow, leave balance, payroll, contracts, grades, career paths, sensitive permissions, and audit logs.

Future extraction into separate services is possible but not required. Keep boundaries clean so extraction remains possible.
