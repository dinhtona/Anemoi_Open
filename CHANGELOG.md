# Changelog

All notable changes to the Anemoi HR platform.

## [v0.34.0] — 2026-07-03 — Production Baseline

### Architecture
- Modular monolith / independently deployable services (.NET 10)
- Clean Architecture + CQRS + Event-Driven (MassTransit + RabbitMQ)
- PostgreSQL (16-alpine) relational storage, MongoDB for orchestrator
- Redis for JWT revocation, Caddy as HTTPS reverse proxy
- Centralize (gateway), Identity (auth), MasterData, Notification, HR, Workspace
- Strongly-typed IDs, Mapperly, FluentValidation, OneOf, Serilog, Polly

### Security
- JWT RS256, issuer `anemoi-identity`, audience `anemoi-services`
- Role-based authorization with permission constants + `[HasPermission]`
- Three scope enforcement: ESS (self-service), Manager (approvals), HR/Admin
- **Phase 34 Iteration 9:** Closed 7 auth gaps (missing `[HasPermission]` on workflow/manager/profile endpoints)
- **3 open security debts:** domain entity exposure (TD-P34-SEC-02), latent SQL injection surface (TD-P34-SEC-03), hardcoded credentials in appsettings.json (TD-P34-SEC-04)

### Performance
- **Phase 34 Iteration 10:** Fixed 2 full-table-scan queries, added `.AsNoTracking()` to 3 heavy read queries, EF Core strongly-typed ID comparison fix
- **15 registered performance debts:** N+1 queries (PERF-01/02/03), full table loads (PERF-04), missing AsNoTracking (PERF-05), expensive Include chains (PERF-06), redundant loads (PERF-07), unbounded endpoints (PERF-08), dashboard sequential loads (PERF-09), Identity N+1 (PERF-10), in-memory pagination (PERF-15), 38 missing DB indexes (PERF-14)

### Database Migrations
| Database      | Migrations | Latest |
|---------------|-----------|--------|
| Hr            | 45        | `20260702081030_FixEmployeeMappingConstraints` |
| Identity      | 4         | `20260626101941_AddRoleGroupCode` |
| Notification  | 7         | `20260623034119_Phase33_NotificationPendingChanges` |
| MasterData    | 7         | (latest) |
| Workspace     | 1         | (initial) |

### Test Coverage
- **575/575 unit tests passing** (250 HR + 325 BuildingBlock)
- 0 build errors, 73 pre-existing warnings
- No end-to-end test automation (TD-007)

### Docker Stack (13 services)
- Core: rabbitmq (healthy), postgres (healthy), redis (healthy), mongodb, sftp, smtp4dev
- App: anemoi_centralize, anemoi_identity, anemoi_hr, anemoi_masterdata, anemoi_notification, anemoi_workspace
- Edge: caddy (port 80/443)
- Orchestrator, Secure, Web services — commented out (dev-only)

### Known Issues
- Orchestrator service disabled (commented out in docker-compose.yml)
- Frontend container (`anemoi_web`) commented out — browser UI not available via Docker
- No browser UI regression testing possible in this baseline
- 73 pre-existing build warnings (TD-002)
- DataProtection keys ephemeral (no persistent volume for some services)

### Tech Debt Count
- Active: 25 (1 P1, 8 P2, 9 P3, 5 P4, 2 P5)
- Resolved: 6 (TD-001, TD-P34-EMPLOYEE-02, TD-P34-SEC-01, Phase 16/18/19 items)

---

## Phase History

### Phase 34 — Production Hardening (2026-07-03)
- Iteration 8C: TD-P34-EMPLOYEE-02 — EmployeeHistory EntityId/Description in 10 handlers
- Iteration 9: Security audit — 7 `[HasPermission]` gaps closed
- Iteration 10: Performance audit — full table scan fixes, 15 PERF debts registered
- Iteration 11: Docker runtime audit — EF Core regression fix, all services operational
- Iteration 12: Full regression — API scope boundaries verified, DB + logs clean

### Phase 33 — Notification Platform (2026-06-23)
- Notification deduplication, outbox durability hardening, workspace-aware broadcasting
- 13 business notification consumers, DataChangeOccurred events for cache invalidation

### Phase N6 — Outbox/Inbox Durability (2026-06-16)
- Publish-before-save ordering hardened across 20 HR handlers
- Bus outbox + inbox verified, consumer idempotency confirmed
- DeduplicationKey unique index, 3-layer dedup in CreateNotificationHandler

### Phase 22 — Frontend Error Translation (2026-06)
- Centralized API error parsing, 231 backend error codes synced to en.json/vi.json

### Phase 19 — UI Stabilization
- shadcn confirmation dialogs replacing native confirm
- Mixed English/Vietnamese UI resolved, tax contract mismatch fixed

### Phase 16 — PostgreSQL Migration
- SQL Server → PostgreSQL, RowVersion → xmin, AutoMapper → Mapperly

### Earlier Phases (1–15)
- Core HR domain model: Employees, Departments, Positions, Leave, Attendance, Payroll, Overtime, Shift, Calendar, Tax, Insurance, Recruitment, Onboarding, Probation
- Identity management: JWT auth, role groups, permission system
- Multi-tenant workspace architecture
- MassTransit event-driven infrastructure
