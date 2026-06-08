# Phase 18 - Holiday & Working Calendar Engine

Status: Planned

---

## Objective

Build a centralized working calendar engine used by Leave, Attendance, Overtime, Payroll, and Scheduling modules.

---

## Business Requirements

- Manage national holidays
- Manage company holidays
- Configure weekends
- Configure custom working day overrides
- Assign calendars by organization, branch, department, or employee

---

## Domain Model

### WorkingCalendar
Calendar definition for a country, branch, department, or employee group.

### Holiday
Holiday or non-working day.

### WorkingDayOverride
Exception such as a Saturday configured as a working day.

### CalendarAssignment
Effective-dated assignment of calendar rules.

---

## Workflow

```text
Working Calendar
    ↓
Leave / Attendance / Overtime / Payroll / Shift
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Payroll must snapshot calculated working day values.
- Reports must not recalculate historical payroll from calendar changes.

---

## Commands

- CreateWorkingCalendarCommand
- AddHolidayCommand
- AddWorkingDayOverrideCommand
- AssignWorkingCalendarCommand

---

## Queries

- GetWorkingCalendarQuery
- GetWorkingDaysQuery
- GetHolidaysQuery
- CalculateWorkingDaysQuery

---

## Permissions

```text
hr.calendar.view
hr.calendar.manage
```

---

## API Endpoints

```text
GET    /api/hr/working-calendars
POST   /api/hr/working-calendars
POST   /api/hr/working-calendars/{id}/holidays
POST   /api/hr/working-calendars/{id}/overrides
POST   /api/hr/working-calendars/{id}/assignments
GET    /api/hr/working-calendars/{id}/working-days
```

---

## Frontend

- Working Calendar List
- Calendar Detail
- Holiday Management
- Calendar Assignment

---

## Future Enhancements

- Import public holidays
- Country-specific holiday templates
- Multi-branch calendars
- Calendar versioning
