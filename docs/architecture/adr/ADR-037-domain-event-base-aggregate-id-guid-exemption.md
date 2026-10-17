# ADR-037 — DomainEvent Base Keeps Raw Guid AggregateId (Exemption)

## Status

Approved (audit remediation 2026-10-16; code-quality group A)

## Context

`Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Domain/DomainEvent.cs` declares

```csharp
public Guid AggregateId { get; set; }
```

on the base record inherited by every domain event across all bounded contexts
(HR, Identity, Notification, ...). ArchitectureGuide §6.4 requires local
aggregate IDs to use strongly-typed wrappers (e.g. `EmployeeId`), and the
2026-10-16 code-quality audit flagged this as CODE-007 / MED-13.

## Decision

The base `DomainEvent.AggregateId` **remains a raw `Guid`** — a sanctioned,
documented exemption from the strongly-typed ID rule. Rationale:

1. **Blast radius.** Converting to `DomainEvent<TId>` would touch every derived
   domain-event record in every module (dozens of call sites, MassTransit
   topology names, outbox/inbox serialization) with no behavioral gain: the
   field is a cross-service correlation identifier, not a domain-model
   association navigated inside an aggregate.
2. **Wire compatibility.** Domain events are serialized into the MassTransit
   outbox and consumed across service boundaries. Changing the member type
   risks breaking in-flight messages and consumer contracts.
3. **Layering.** The base event lives in `BuildingBlock.Domain`, which by the
   dependency rule cannot reference module `ModelIds` projects (that would
   make the shared kernel depend on every bounded context).

## Scope

- Applies ONLY to the `DomainEvent` base record in
  `Anemoi.BuildingBlock.Domain`. It does NOT exempt module domain models:
  local aggregate IDs in `*.Domain` (e.g. `Employee.Id`, `Department.Id`) must
  keep using strongly-typed wrappers.
- New module-specific event types SHOULD carry their own strongly-typed ID
  properties alongside the base `AggregateId` where the aggregate identity is
  semantically meaningful (several HR domain events already do, e.g.
  `EmployeeStatusChangedDomainEvent.EmployeeId`).

## Implications

- The ArchitectureGuard rule for raw-Guid aggregate IDs must exempt
  `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Domain/DomainEvent.cs` or
  reference this ADR.
- Revisit only if a serialization/mapping issue is proven to originate from
  the raw `Guid` type on this base record.

## Related

- CODE-007 / MED-13 (2026-10-16 audit) — BLOCKED-by-design → documented here.
- CODE-006 / MED-12 — the `Guid.NewGuid()` default in the same file was
  replaced with sequential `MassTransit.NewId.NextGuid()` (same semantics as
  `IdGenerator.NextGuid()`); no exemption needed there.
