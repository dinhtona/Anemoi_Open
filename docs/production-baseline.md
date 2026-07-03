# v0.34.0 Production Baseline

**Date:** 2026-07-03
**Build:** 0 errors, 73 warnings
**Tests:** 575/575 passing
**Branch:** dev (commit `4f69d84`)

## Architecture Overview

```
                    ┌─────────────────────────┐
                    │         Caddy           │
                    │   HTTP :80 → :443 HTTPS │
                    └──────────┬──────────────┘
                               │
                    ┌──────────▼──────────────┐
                    │  anemoi_centralize :8080 │  ← Gateway / Aggregation
                    └──────────┬──────────────┘
                               │
            ┌──────────────────┼──────────────────┐
            │                  │                  │
    ┌───────▼──────┐  ┌───────▼──────┐  ┌───────▼──────┐
    │   Identity   │  │      HR      │  │  (others...) │
    │    :5021     │  │    :8082     │  │              │
    └───────┬──────┘  └───────┬──────┘  └───────┬──────┘
            │                  │                  │
            └──────────────────┼──────────────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
      ┌───────▼──────┐ ┌──────▼──────┐ ┌───────▼──────┐
      │  RabbitMQ    │ │ PostgreSQL  │ │    Redis     │
      │   :5672      │ │   :5432     │ │    :6379     │
      └──────────────┘ └─────────────┘ └──────────────┘
```

**Stack:** .NET 10, EF Core + PostgreSQL, MassTransit + RabbitMQ, MediatR + OneOf, Mapperly, FluentValidation, Serilog, Polly, Caddy

**6 active services:** Centralize (gateway), Identity (auth), MasterData, Notification, HR, Workspace
**3 disabled:** Orchestrator, Secure, Web (frontend container)

## Service Endpoints (Caddy-routed, HTTPS)

| Service | Caddy Path | Backend |
|---------|-----------|---------|
| Centralize | `/api/identity/*`, `/api/notification/*`, `/api/workspace/*` | `anemoi_centralize:8080` |
| HR | `/api/hr/*` | `anemoi_hr:8080` |
| HR (direct) | `:8082` | `anemoi_hr:8080` |

## Database Migration State

| Database      | Count | Latest Migration |
|---------------|-------|-----------------|
| Hr            | 45    | `20260702081030_FixEmployeeMappingConstraints` |
| Identity      | 4     | `20260626101941_AddRoleGroupCode` |
| Notification  | 7     | `20260623034119_Phase33_NotificationPendingChanges` |
| MasterData    | 7     | (latest) |
| Workspace     | 1     | (initial) |

## Directory: Databases
```
postgres → Databases: Hr, Identity, Notification, MasterData, Workspace
mongodb  → Orchestrator state (disabled)
```

## Active Technical Debt (25 items)

See `docs/architecture/TECHNICAL_DEBT_REGISTER.md` for full details.

| Priority | Count | Key Items |
|----------|-------|-----------|
| P1       | 1     | TD-002: Build warnings (73) |
| P2       | 8     | Frontend tests (TD-003), E2E (TD-007), Pass/Fail handler FK (TD-P34-PROBATION-01), 4 PERF items |
| P3       | 9     | Permission audit (TD-009), OpenAPI contracts (TD-006), domain entity exposure, 3 PERF items |
| P4       | 5     | Localization (TD-005), validation (TD-011), 2 PERF items |
| P5       | 2     | Snapshot growth (TD-008) |

**Resolved in Phase 34:** TD-001 (domain→app dependency), TD-P34-EMPLOYEE-02 (EntityId/Description), TD-P34-SEC-01 (7 auth gaps), Phase 16/18/19 historical items.

## Performance Snapshot

| Item | Status | Impact |
|------|--------|--------|
| Workflow full-table scans | Fixed (I10) | 200ms → reliable |
| GetCompensationDashboard AsNoTracking | Fixed (I10) | EF tracker memory saved |
| 38 missing DB indexes | Registered | Sequential scans across all modules |
| N+1 in 6 handlers | Registered | O(N) round-trips for approvals, recruitment, accrual |
| 16 handlers missing AsNoTracking | Registered | Unnecessary change tracker overhead |
| In-memory pagination | Registered | Approval inbox loads all instances |
| Expensive Include chains (Cartesian) | Registered | Transfer queries with 5 Includes |

## Security Snapshot

| Item | Status | Notes |
|------|--------|-------|
| JWT RS256 | Verified | `anemoi-identity` issuer, `anemoi-services` aud |
| Role groups (5) | Verified | Admin, HR, Manager, Employee, Guest |
| Permission-based auth | Verified | `[HasPermission]` on all endpoints |
| Three-scope enforcement | Verified | ESS/Manager/HR — tested |
| Missing [HasPermission] on 7 endpoints | Fixed (I9) | WorkflowInstances (4), ManagerApprovals (2), GetMyProfile (1) |
| Hardcoded credentials in appsettings | Registered | TD-P34-SEC-04: 5 files with passwords, RabbitMQ guest |
| Domain entity returned from controller | Registered | TD-P34-SEC-02: Organization hierarchy |
| FromSqlRaw injection surface | Registered | TD-P34-SEC-03: Unused public method |

## Scope Boundary Verification

| Endpoint Scope | Endpoint | Admin | HR | Manager | Employee |
|---------------|----------|-------|----|---------|----------|
| ESS           | `GET /employees/me` | - | 200 | 200 | 200 |
| HR/Admin      | `GET /employees`   | 200 | 200 | 403 | 403 |
| HR/Admin      | `GET /departments` | 200 | 200 | 403 | 403 |
| Manager       | `GET /manager/approvals/*` | 200 | 403 | 403 | - |
| Workflow      | Workflow actions | 200 | - | - | - |

## Docker Compose Deployment

```bash
# 1. Prerequisites
cp .env.example .env
# Fill required: POSTGRES_PASSWORD, MONGO_INITDB_ROOT_PASSWORD, RABBITMQ_DEFAULT_PASS

# 2. Start stack
docker compose up -d --build

# 3. Verify
docker compose ps              # 13 services Up
docker compose ps postgres     # healthy
docker compose ps rabbitmq     # healthy
docker compose ps redis        # healthy
```

## Required Environment Variables

See `.env.example` for full template. **Required for baseline:**

| Variable | Purpose |
|----------|---------|
| `POSTGRES_USER` | PostgreSQL superuser |
| `POSTGRES_PASSWORD` | PostgreSQL password |
| `RABBITMQ_DEFAULT_USER` | RabbitMQ user |
| `RABBITMQ_DEFAULT_PASS` | RabbitMQ password |
| `MONGO_INITDB_ROOT_USERNAME` | MongoDB root user |
| `MONGO_INITDB_ROOT_PASSWORD` | MongoDB root password |
| `JWT_ISSUER` | `anemoi-identity` |
| `JWT_AUDIENCE` | `anemoi-services` |

## Production Readiness Checklist

- [x] Build succeeds (0 errors)
- [x] All tests pass (575/575)
- [x] Docker stack starts cleanly (13 services)
- [x] Database migrations applied (5 databases)
- [x] JWT authentication works (RS256)
- [x] Permission enforcement verified (3 scopes)
- [x] No runtime errors in service logs
- [x] RabbitMQ healthy, MassTransit running
- [ ] Browser UI available (frontend container disabled)
- [ ] HTTPS certificates (Caddy self-signed, dev only)
- [ ] DataProtection key persistence (ephemeral in dev)
- [ ] Production-grade credentials (appsettings.json secrets)
- [ ] Database indexes (38 missing, full table scans on analytics)
- [ ] End-to-end test automation (not implemented)
- [ ] Secret management (Azure Key Vault / env vars, not implemented)

## Verdict: DEV Environment Baseline

**GO** for development/staging environments.
**NOT READY** for production — see unchecked items above, security debts (TD-P34-SEC-04), and performance debts (38 missing indexes, N+1 queries).
