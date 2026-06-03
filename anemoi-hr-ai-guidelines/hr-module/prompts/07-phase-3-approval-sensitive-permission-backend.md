# Prompt - Phase 3 Approval Workflow and Sensitive Permission Backend

```text
Read all AI guideline files first.

Task:
Implement reusable Approval Workflow and Sensitive Permission governance.

Recommended execution:
Follow backend 3-step model. If this is a fresh implementation, perform Step 1 only and stop.

Scope:
Approval Workflow:
- ApprovalWorkflow
- ApprovalStep
- ApprovalRequest
- ApprovalAction
- ApproverType support:
  - direct_manager
  - department_manager
  - hr_manager
  - specific_employee
  - permission_role
  - requester_selected

Sensitive Permission:
- Extend permission metadata with IsSensitive and RiskLevelCode if not already present.
- Add sensitive assignment request flow.
- Critical permission assignment must require approval before being applied.
- High/Critical permissions require reason and re-authentication seam.

Events:
- ApprovalRequestCreated
- ApprovalRequestApproved
- ApprovalRequestRejected
- SensitivePermissionAssignmentRequested
- SensitivePermissionAssigned
- SensitivePermissionAssignmentRejected

Requirements:
- Do not hard-code business roles in feature logic.
- Permission codes must be stable constants.
- All sensitive permission actions must be audited.
- Add localized user-facing messages.
- Do not silently assign sensitive permissions.

Stop at the requested backend step and summarize changed files.
```
