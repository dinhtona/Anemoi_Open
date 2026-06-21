# Phase 33.6 — Architecture Debt Closure

**Date:** 2026-06-21

---

## Task 1: Domain Event Dispatching — FIXED

### Problem
`Entity<TId>.AddEvent()` collected domain events, but `EfUnitOfWork.SaveChangesAsync()` never dispatched them through MediatR. This meant:

- `WorkflowInstanceApprovedHandler` → dead code
- `WorkflowRejectedIntegrationEventPublisher` → dead code
- `RecruitmentRequestSubmittedDomainEvent`, `ApprovedDomainEvent`, `RejectedDomainEvent` → collected but never dispatched
- `OnboardingInstance*DomainEvent` → same
- All `IWorkflowTargetStatusUpdater` implementations wired through domain events → dead code path

### Fix Applied
**Files changed:**
1. `Anemoi.BuildingBlock.Infrastructure/Repositories/EfUnitOfWork.cs`
   - Added `IMediator` dependency
   - Added `DispatchDomainEventsAsync()` called AFTER `SaveChangesAsync()`
   - Uses reflection to find all tracked `Entity<TId>` instances with pending events
   - Publishes each event through `mediator.Publish()`
   - Clears events after dispatch

2. `Anemoi.BuildingBlock.Infrastructure/GeneralInstaller/EfExtensions.cs`
   - Updated dynamic constructor to pass `IMediator` as third parameter

### How It Works Now
```
Entity.AddEvent(domainEvent)
↓
EfUnitOfWork.SaveChangesAsync()
  ├── _dbContext.SaveChangesAsync()          // Persist to DB first
  ├── DispatchDomainEventsAsync()             // Collect + publish events
  │   ├── For each Entity<TId> with events:
  │   │   ├── mediator.Publish(WorkflowInstanceApprovedDomainEvent)
  │   │   ├── → WorkflowInstanceApprovedHandler.Handle()
  │   │   │   ├── → IWorkflowTargetStatusUpdater (Leave, Overtime, etc.)
  │   │   │   └── → WorkflowApprovedIntegrationEventPublisher (MassTransit)
  │   │   └── entity.ClearEvents()
  └── Return success
```

### Verification
| Check | Result |
|-------|--------|
| `dotnet build Anemoi.sln` | 0 errors |
| `dotnet test` | 486/486 pass |

---

## Task 2: Production Deployment Guideline — CREATED

### Deliverable
`docs/operations/PRODUCTION_DEPLOYMENT.md`

### Contents
| Section | Coverage |
|---------|----------|
| Security Hardening | Docker socket removal, JWT key protection, credential management |
| Docker Compose Production Override | Health checks, restart policies, resource limits |
| Environment Variables | 17 variables documented |
| Health Checks | Endpoint list per service |
| Backup & Restore | Shell scripts for pg_dump/psql |
| Monitoring | Metrics, alert thresholds, logging |
| DB Migration Strategy | Safe migration practices |
| Seed Data | Dev-only behavior explained |
| Network Security | Architecture recommendations |
| Frontend Deployment | Dockerfile + build instructions |

---

## Architectural Impact

### Before Phase 33.6
```
Two competing event patterns:

1. Works:   Command → Publish IntegrationEvent → MassTransit → Consumer
2. Broken:  Entity.AddEvent() → Domain Event → NEVER dispatched
```

### After Phase 33.6
```
Single unified pattern:

Command → SaveChangesAsync
  ├── IntegrationEvent (via IPublishEndpoint)
  └── DomainEvent (via Entity.AddEvent → MediatR dispatch)

All event paths are now active and tested.
```

### Now-Active Domain Event Handlers
| Handler | Purpose | Previously |
|---------|---------|------------|
| `WorkflowInstanceApprovedHandler` | Routes to `IWorkflowTargetStatusUpdater` | Dead code |
| `WorkflowInstanceRejectedHandler` | Routes to `IWorkflowTargetStatusUpdater` | Dead code |
| `WorkflowApprovedIntegrationEventPublisher` | Publishes MassTransit event | Dead code |
| `WorkflowRejectedIntegrationEventPublisher` | Publishes MassTransit event | Dead code |

---

## Conclusion

Both architecture debt items are resolved:

| Item | Status | Action |
|------|--------|--------|
| Domain event dispatching | ✅ **Fixed** | `EfUnitOfWork.SaveChangesAsync()` now dispatches through MediatR |
| Production deployment guideline | ✅ **Created** | `docs/operations/PRODUCTION_DEPLOYMENT.md` |

Anemoi HR is now ready for **Phase 34 — Performance Management**.
