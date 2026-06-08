# Phase 27 - Performance Management

Status: Planned

---

## Objective

Introduce performance management for goals, reviews, ratings, and career development.

---

## Business Requirements

- Performance cycle
- Goal setting
- Self review
- Manager review
- Final rating
- Performance history

---

## Domain Model

### PerformanceCycle
Review period.

### Goal
Employee goal.

### PerformanceReview
Review record.

### PerformanceRating
Final rating.

---

## Workflow

```text
Cycle Created
    ↓
Goals Assigned
    ↓
Employee Self Review
    ↓
Manager Review
    ↓
HR Calibration
    ↓
Final Rating
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Performance Management should integrate with Enterprise Approval Framework.

---

## Commands

- CreatePerformanceCycleCommand
- AssignGoalCommand
- SubmitSelfReviewCommand
- SubmitManagerReviewCommand
- CalibratePerformanceReviewCommand
- FinalizePerformanceReviewCommand

---

## Queries

- GetPerformanceCyclesQuery
- GetEmployeeGoalsQuery
- GetPerformanceReviewsQuery
- GetPerformanceReviewDetailQuery

---

## Permissions

```text
hr.performance.view
hr.performance.manage
hr.performance.review
hr.performance.calibrate
```

---

## API Endpoints

```text
GET    /api/hr/performance/cycles
POST   /api/hr/performance/cycles
POST   /api/hr/performance/goals
POST   /api/hr/performance/reviews/{id}/self-review
POST   /api/hr/performance/reviews/{id}/manager-review
POST   /api/hr/performance/reviews/{id}/calibrate
POST   /api/hr/performance/reviews/{id}/finalize
```

---

## Frontend

- Performance Cycle Management
- My Goals
- Review Form
- Manager Review Queue
- Calibration Board

---

## Future Enhancements

- 360-degree feedback
- Competency matrix
- Promotion recommendation
- Salary review integration
