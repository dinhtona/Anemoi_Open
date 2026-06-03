# Prompt - Phase 1 Core HR Backend - Step 3 API & Communication

```text
Read all AI guideline files first.

Task:
Implement Phase 1 Core HR backend Step 3 only: API & Communication.

Prerequisite:
Step 1 and Step 2 for Core HR must already exist.

Scope:
Expose protected endpoints for:

Employee:
- Create
- Update
- Get by id
- Search

Department:
- Create
- Update
- Get tree
- Get by id

Position:
- Create
- Update
- Search

Employee Department History:
- Assign department
- Transfer department
- Get history

Requirements:
- Use permission-based authorization.
- Add stable permission constants to the central permission catalog.
- Update idempotent Identity seed logic for new permissions.
- Do not hard-code business role names.
- Controllers must only dispatch MediatR requests.
- Add integration events only if needed for cross-service side effects:
  - EmployeeCreated
  - EmployeeUpdated
  - DepartmentChanged
- Wire events using MassTransit according to existing conventions.
- Use localized error messages.

Stop after Step 3 and provide a summary of changed files.
```
