# Audit Log Rules

HR data is sensitive. Important actions must be auditable.

## Actions That Require Audit

```text
Employee personal information update
Employee bank information view/update
Salary view/update/adjustment
Contract view/update/download
Leave balance adjustment
Force approve/cancel leave request
Department transfer
Position change
Grade change
Promotion approval
Performance review update
Sensitive permission assignment
User deactivation
```

## Audit Fields

```text
AuditLogId
ActorUserId
ActorEmployeeId nullable
TargetUserId nullable
TargetEmployeeId nullable
ModuleCode
ActionCode
PermissionCode nullable
RiskLevel nullable
Reason nullable
OldValueJson nullable
NewValueJson nullable
IpAddress nullable
UserAgent nullable
CorrelationId nullable
CreatedAt
```

Audit logs must be append-only. Avoid storing secrets or passwords.
