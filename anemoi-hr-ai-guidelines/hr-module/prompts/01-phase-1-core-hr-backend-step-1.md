# Prompt - Phase 1 Core HR Backend - Step 1 Domain & Data

```text
Read all AI guideline files first.

Task:
Implement Phase 1 Core HR backend Step 1 only: Domain & Data.

Scope:
- Employee
- Department
- Position
- EmployeeDepartmentHistory
- HR-related strongly typed IDs
- EF Core EntityTypeConfiguration
- DbContext registration

Do not implement CQRS handlers.
Do not implement API controllers.
Do not implement frontend.

Requirements:
- Follow Clean Architecture.
- Place entities in HR Domain project.
- Use Strongly Typed IDs for local aggregate IDs.
- Keep Employee separate from Identity User. UserId may be an external primitive boundary value if owned by Identity service.
- Department must support hierarchy using ParentDepartmentId.
- Department must support ManagerEmployeeId.
- EmployeeDepartmentHistory must keep transfer history.
- Add uniqueness constraints for EmployeeCode and Email where applicable.
- Add indexes for Department Code and Position Code.
- Use stable status/category code values instead of localized text.

Expected output:
- New/updated Domain entities
- Strongly typed ID wrappers
- EF Core configurations
- DbContext registration
- Migration if the project convention expects migration in this step

Stop after Step 1 and provide a summary of changed files.
```
