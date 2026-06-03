# Prompt - Phase 1 Core HR Backend - Step 2 Application CQRS

```text
Read all AI guideline files first.

Task:
Implement Phase 1 Core HR backend Step 2 only: Application Layer - CQRS & Mappings.

Prerequisite:
Step 1 Domain & Data for Employee, Department, Position, and EmployeeDepartmentHistory must already exist.

Scope:
Create commands, queries, DTOs, Mapperly mappings, and FluentValidation for:

Employee:
- CreateEmployeeCommand
- UpdateEmployeeCommand
- GetEmployeeByIdQuery
- SearchEmployeesQuery

Department:
- CreateDepartmentCommand
- UpdateDepartmentCommand
- GetDepartmentTreeQuery
- GetDepartmentByIdQuery

Position:
- CreatePositionCommand
- UpdatePositionCommand
- SearchPositionsQuery

Employee Department History:
- AssignEmployeeDepartmentCommand
- TransferEmployeeDepartmentCommand
- GetEmployeeDepartmentHistoryQuery

Requirements:
- Use MediatR.
- Return OneOf-style success/error responses according to existing project conventions.
- Use Mapperly partial mappers.
- Use FluentValidation.
- Do not return domain entities from handlers.
- Use stable error codes and localization resources for user-facing messages.
- Do not hard-code role names.

Do not implement API controllers.
Do not implement frontend.

Stop after Step 2 and provide a summary of changed files.
```
