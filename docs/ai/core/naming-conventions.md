# Naming Conventions

## Backend Projects

```text
Anemoi.Hr.ModelIds
Anemoi.Hr.Domain
Anemoi.Hr.Application
Anemoi.Hr.Infrastructure
Anemoi.Hr.Api
```

## Backend Folders

```text
Domain/
├── Employees/
├── Departments/
├── Positions/
├── Leaves/

Application/
├── Cqrs/
│   ├── Commands/
│   └── Queries/
├── Mappings/
├── Responses/
├── Validators/

Infrastructure/
├── Persistence/
├── Configurations/
├── Migrations/

Api/
├── Controllers/
├── GraphQL/
```

## Permission Codes

Use:

```text
{module}.{resource}.{action}
```

Examples:

```text
hr.employee.view
hr.leave.request.approve
hr.salary.view_all
```

## Integration Events

Use past-tense names:

```text
EmployeeCreatedIntegrationEvent
LeaveRequestSubmittedIntegrationEvent
LeaveRequestApprovedIntegrationEvent
```

## Database

Follow existing Anemoi naming if already established.

If no convention exists in target service, prefer clear plural table names:

```text
Employees
Departments
Positions
LeaveRequests
LeaveBalances
LeaveTransactions
```

Indexes:

```text
IX_Employees_EmployeeCode
IX_Employees_WorkEmail
IX_LeaveAccrualRuns_EmployeeId_YearMonth
```
