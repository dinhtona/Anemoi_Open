# HR Platform Overview

## Goal

Build a scalable HR Platform that can start as a modular module/service and later evolve into independent microservices. The first business scope is Leave Management, but the design must support future HR capabilities such as payroll, contract management, skills, grades, salary changes, department transfers, performance reviews, and career paths.

## Target capabilities

```text
HR Platform
├── Employee Management
├── Organization Management
├── Leave Management
├── Approval Workflow
├── Payroll Management
├── Contract Management
├── Skill Management
├── Grade / Level Management
├── Career Path Management
├── Salary Change Management
├── Department Transfer Management
├── Document Management
├── Notification Integration
└── Audit Log Integration
```

## Architectural direction

- Follow Anemoi backend rules: Clean Architecture, CQRS, Strongly Typed IDs, Mapperly, FluentValidation, MediatR, OneOf, MassTransit, PostgreSQL, localization, permission-based authorization.
- Reuse existing identity, role, and permission model.
- Extend authorization with DataScope, SensitivePermission, RiskLevel, and sensitive permission assignment approval.
- Use MasterData/Category for shared lookup values.
- Use Approval Workflow as a reusable engine for Leave, Salary Change, Department Transfer, Contract, Promotion, and Sensitive Permission assignment.
- Use transaction-ledger style for balance-like domains, especially Leave Balance and Salary History.

## Recommended backend services/modules

Initial implementation may be a modular monolith or a dedicated HR service. Keep boundaries clean so services can be split later.

```text
Anemoi.Hr.Domain
Anemoi.Hr.Application
Anemoi.Hr.Infrastructure
Anemoi.Hr.Api
Anemoi.Hr.Contracts
Anemoi.Hr.ModelIds
```

Potential long-term split:

```text
anemoi_hr
anemoi_approval
anemoi_payroll
anemoi_document
anemoi_performance
```

Do not create these separate services prematurely unless the user explicitly requests a microservice split.

## Main domain modules

```text
Employees
Organizations
MasterDataIntegration
LeaveManagement
ApprovalWorkflowIntegration
PermissionAndDataScope
SensitivePermissionGovernance
Contracts
Payroll
Skills
CareerPaths
AuditIntegration
NotificationIntegration
```

## Hard rules

- Do not hard-code business role names such as Manager, HRStaff, or Administrator in business logic.
- Do not store only current balances without transaction history.
- Do not update salary directly; always create salary change records and immutable salary history.
- Do not approve sensitive permission assignment without explicit confirmation, reason, audit log, and optional second approval.
- Do not localize machine-readable codes.
- Do not return domain entities directly from APIs.
- Do not query DbContext directly in controllers.
