# ANEMOI HR - AI Context

Before implementing any feature, read:

1. docs/architecture/ARCHITECTURE_DECISIONS.md
2. docs/architecture/TECHNICAL_DEBT_REGISTER.md

Key Rules:

- Snapshot-based architecture
- Historical records are immutable
- Payroll consumes snapshots
- Tax Engine is independent from Payroll
- Insurance Engine must follow Tax Engine patterns
- PostgreSQL xmin concurrency
- Permission-based authorization
- CQRS + MediatR
- Clean Architecture

Do not introduce new architectural patterns without checking ADRs first.