# Core Architecture

Anemoi_Open is based on:

- Microservices architecture
- Docker Compose for local orchestration
- Event-driven communication
- RabbitMQ through MassTransit
- Clean Architecture
- CQRS
- PostgreSQL for most services
- MongoDB mainly for orchestration/Saga if needed

## Existing Conceptual Services

```text
anemoi_centralize
anemoi_identity
anemoi_masterdata
anemoi_workspace
anemoi_orchestrator
anemoi_notification
anemoi_secure
```

## HR Direction

Introduce HR as a bounded context first:

```text
anemoi_hr
```

Recommended projects:

```text
Anemoi.Hr.ModelIds
Anemoi.Hr.Domain
Anemoi.Hr.Application
Anemoi.Hr.Infrastructure
Anemoi.Hr.Api
```

Future split is possible, but not required initially:

```text
anemoi_employee
anemoi_leave
anemoi_payroll
anemoi_contract
anemoi_performance
```

## Why One HR Bounded Context First

HR modules share many rules:

- Employee profile
- Department hierarchy
- Approval workflow
- Leave balance
- Payroll
- Contract
- Grade
- Career path
- Sensitive permissions
- Audit logs

Splitting too early creates unnecessary distributed transaction complexity. Keep boundaries clean so future extraction remains possible.
