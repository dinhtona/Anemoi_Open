# Phase 18: Holiday & Working Calendar Engine — Design Document

## Overview

Centralized calendar engine serving as the single source of truth for public holidays, company holidays, working day rules, and calendar exceptions. Future phases (payroll, attendance, overtime) will consume this engine.

## Architecture

### Bounded Context

New `CalendarManagement` sub-domain within `Anemoi.Hr` service following Clean Architecture:

```
Anemoi.Hr.ModelIds/ModelIds/CalendarManagementIds.cs     # Strongly-typed IDs
Anemoi.Hr.Domain/CalendarManagement/                     # Entities
Anemoi.Hr.Application/Cqrs/Commands/CalendarManagement   # Commands
Anemoi.Hr.Application/Cqrs/Queries/CalendarManagement    # Queries
Anemoi.Hr.Application/Mappings/CalendarManagementMapper.cs
Anemoi.Hr.Infrastructure/Configurations/CalendarManagementModelMapping.cs
Anemoi.Hr.Api/Controllers/CalendarManagement/CalendarManagementController.cs
```

### Entities

| Entity | ID | Domain Fields | API / DTO Details |
|--------|-----|---------------|-------------------|
| `PublicHoliday` | `PublicHolidayId` | `HolidayDate`, `Name`, `Description`, `CountryCode`, `IsRecurringAnnual` | Exposed directly to frontend |
| `CompanyHoliday` | `CompanyHolidayId` | `HolidayDate`, `Name`, `Description`, `IsRecurringAnnual` | Exposed directly to frontend |
| `WorkingCalendarRule` | `WorkingCalendarRuleId` | `Name`, `Description`, `EffectiveFrom`, `EffectiveTo`, `IsActive`, 7 day booleans (`WorkMonday` to `WorkSunday`) | Replaced old `WorkingDays` string representation with 7 boolean flags |
| `CalendarException` | `CalendarExceptionId` | `ExceptionDate`, `ExceptionType` (`CalendarStatus` enum), `Reason`, `RelatedHolidayId` | `ExceptionType` is mapped as string (`"WorkingDayOverride"`/`"HolidayOverride"`), `Reason` maps to `Name` and `Description` at API layer |

### Calendar Resolution Engine

`IWorkingCalendarEngine` interface in Application layer, implementation in Infrastructure.

Priority:
1. `CalendarException` (highest)
2. `CompanyHoliday`
3. `PublicHoliday`
4. `WorkingCalendarRule` (lowest — weekly pattern)

### Concurrency

PostgreSQL `xmin` shadow property on all entities (existing pattern).

### Permissions

- `CalendarView` (`hr.calendar.view`) — read operations
- `CalendarManage` (`hr.calendar.manage`) — CUD operations

### API Routes

All under `api/hr/calendar-management/[action]` with `[HasPermission]` attributes.

### Frontend

Route: `/[locale]/hr/calendar` — 5 tabs: Public Holidays, Company Holidays, Working Rules, Calendar Exceptions, Calendar Viewer.

### Explicitly Out of Scope

No payroll holiday pay, overtime, shift, attendance, tax, insurance, or leave balance calculations.
