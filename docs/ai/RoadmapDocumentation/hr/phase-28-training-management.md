# Phase 28 - Training Management

Status: Planned

---

## Objective

Introduce training and learning management for employee development.

---

## Business Requirements

- Course management
- Training plan
- Training assignment
- Completion tracking
- Certification tracking

---

## Domain Model

### Course
Training course.

### TrainingPlan
Learning plan.

### TrainingAssignment
Employee training assignment.

### Certification
Employee certification.

---

## Workflow

```text
Course Created
    ↓
Training Assigned
    ↓
Employee Completes Training
    ↓
Completion Recorded
    ↓
Certification Updated
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

- CreateCourseCommand
- CreateTrainingPlanCommand
- AssignTrainingCommand
- CompleteTrainingCommand
- AddCertificationCommand

---

## Queries

- GetCoursesQuery
- GetTrainingPlansQuery
- GetEmployeeTrainingAssignmentsQuery
- GetEmployeeCertificationsQuery

---

## Permissions

```text
hr.training.view
hr.training.manage
hr.training.assign
```

---

## API Endpoints

```text
GET    /api/hr/training/courses
POST   /api/hr/training/courses
GET    /api/hr/training/plans
POST   /api/hr/training/plans
POST   /api/hr/training/assignments
POST   /api/hr/training/assignments/{id}/complete
GET    /api/hr/employees/{id}/certifications
```

---

## Frontend

- Course Management
- Training Plan Management
- Employee Training Assignments
- Certification History

---

## Future Enhancements

- E-learning integration
- Training approval workflow
- Expiring certification alerts
- Performance integration
