# Prompt - Phase 6 Future HR Modules

```text
Read all AI guideline files first.

Task:
Plan and/or implement future HR modules incrementally.

Available modules:
- Contract Management
- Salary Profile
- Salary Change Request
- Department Transfer Request
- Skill Management
- Employee Skill Matrix
- Grade / Level Management
- Career Path
- Promotion Request
- Performance Review
- Document Management

Rules:
- Do not implement all modules at once.
- Pick exactly one module per task unless the user explicitly says otherwise.
- For backend, follow the 3-step model:
  1. Domain & Data
  2. Application CQRS & Mappings
  3. API & Communication
- For frontend, define types/services/hooks before UI.
- Salary, contract, bank information, identity documents, performance reviews, promotion approvals, and sensitive permission assignment must be treated as sensitive.
- Salary changes, department transfers, contracts, and promotions must use Approval Workflow.
- Salary history must be immutable.
- Department transfer must preserve EmployeeDepartmentHistory.
- All user-facing text must be localized.
- All protected endpoints must use permission constants.

Stop at the requested phase/step and summarize files changed.
```
