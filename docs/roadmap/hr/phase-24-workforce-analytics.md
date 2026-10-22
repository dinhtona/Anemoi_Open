# Phase 24 - Workforce Analytics

Status: Designed

---

## Objective

Introduce workforce analytics for strategic HR decision-making. This phase focuses on insights rather than transactional workflows.

---

## Business Requirements

- Headcount trend
- Department growth
- Attrition analytics
- Promotion analytics
- Salary distribution
- Workforce composition

---

## Domain Model

Read-only analytics module.

Data sources:

- Employee
- Department
- Position
- Contract
- Promotion History
- Compensation
- Payroll Snapshots

---

## Workflow

```text
Operational Data
    ↓
Read Models / Projections
    ↓
Analytics Dashboard
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Prefer read models/projections over heavy transactional queries.

---

## Commands

- CreateSavedAnalyticsViewCommand

---

## Queries

- GetHeadcountTrendQuery
- GetAttritionRateQuery
- GetDepartmentGrowthQuery
- GetPromotionAnalyticsQuery
- GetSalaryDistributionQuery

---

## Permissions

```text
hr.analytics.view
```

---

## API Endpoints

```text
GET /api/hr/analytics/headcount-trend
GET /api/hr/analytics/attrition
GET /api/hr/analytics/department-growth
GET /api/hr/analytics/promotions
GET /api/hr/analytics/salary-distribution
```

---

## Frontend

- Workforce Analytics Dashboard
- Headcount Trend Chart
- Department Growth Chart
- Salary Distribution Chart
- Attrition Chart

---

## Future Enhancements

- Predictive analytics
- AI-assisted HR insights
- Saved dashboards
- Executive reporting
