# Phase 26 - Onboarding Management

Status: Planned

---

## Objective

Manage onboarding activities for new employees after hiring. This phase connects recruitment, employee management, IT provisioning, HR tasks, and training.

---

## Business Requirements

- Onboarding plan
- Onboarding checklist
- Task assignment
- Asset assignment
- Account provisioning tracking
- Onboarding completion

---

## Domain Model

### OnboardingPlan
Reusable onboarding template.

### OnboardingInstance
Concrete onboarding process for a new employee.

### OnboardingTask
Task assigned to HR, manager, IT, or employee.

---

## Workflow

```text
Candidate Hired
    ↓
Employee Created
    ↓
Onboarding Started
    ↓
Tasks Assigned
    ↓
Tasks Completed
    ↓
Onboarding Completed
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.


---

## Commands

- CreateOnboardingPlanCommand
- StartOnboardingCommand
- AssignOnboardingTaskCommand
- CompleteOnboardingTaskCommand
- CompleteOnboardingCommand

---

## Queries

- GetOnboardingPlansQuery
- GetEmployeeOnboardingQuery
- GetMyOnboardingTasksQuery
- GetPendingOnboardingTasksQuery

---

## Permissions

```text
hr.onboarding.view
hr.onboarding.manage
hr.onboarding.task.complete
```

---

## API Endpoints

```text
GET    /api/hr/onboarding/plans
POST   /api/hr/onboarding/plans
POST   /api/hr/employees/{id}/onboarding/start
GET    /api/hr/employees/{id}/onboarding
POST   /api/hr/onboarding/tasks/{id}/complete
```

---

## Frontend

- Onboarding Plan Management
- Employee Onboarding Progress
- Onboarding Task Board
- My Onboarding Tasks

---

## Future Enhancements

- Automated account provisioning
- Document upload
- Training assignment
- Manager onboarding checklist
