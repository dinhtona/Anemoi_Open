# Phase 17 - Shift Management

Status: Implemented

---

## Objective

Introduce shift management for employees, departments, and teams. Shift data will support attendance validation, overtime calculation, and payroll processing.

---

## Business Requirements

- Create shift definitions
- Assign shifts to employees or groups
- View employee shift calendar
- View department shift calendar
- Preserve shift assignment history

---

## Domain Model

### ShiftDefinition
Reusable shift template.

Suggested fields:

- Id
- Code
- Name
- StartTime
- EndTime
- BreakMinutes
- IsNightShift
- IsActive

### ShiftAssignment
Assigns a shift to an employee or group.

### ShiftSchedule
Concrete generated schedule records for a date range.

---

## Workflow

```text
Shift Definition
    ↓
Shift Assignment
    ↓
Shift Schedule
    ↓
Attendance / Overtime / Payroll
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Night shifts may cross calendar days.
- Overlapping active shift assignments are not allowed.

---

## Commands

- CreateShiftDefinitionCommand
- UpdateShiftDefinitionCommand
- AssignShiftCommand
- RemoveShiftAssignmentCommand
- GenerateShiftScheduleCommand

---

## Queries

- GetShiftDefinitionsQuery
- GetEmployeeShiftScheduleQuery
- GetDepartmentShiftScheduleQuery
- GetShiftAssignmentHistoryQuery

---

## Permissions

```text
hr.shift.view
hr.shift.manage
hr.shift.assign
```

---

## API Endpoints

```text
GET    /api/hr/shifts
POST   /api/hr/shifts
PUT    /api/hr/shifts/{id}
POST   /api/hr/shifts/assignments
GET    /api/hr/employees/{id}/shift-schedule
GET    /api/hr/departments/{id}/shift-schedule
```

---

## Frontend

- Shift Definition Management
- Shift Assignment Calendar
- Employee Shift Schedule
- Department Shift Schedule

---

## Future Enhancements

- Shift swap request
- Shift approval workflow
- Automatic schedule generation
- Mobile attendance integration
