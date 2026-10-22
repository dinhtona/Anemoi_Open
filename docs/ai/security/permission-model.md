# Permission Model

Authorization is capability-based. Reuse the existing Identity role-group system; do not create a separate HR permission system.

## Concepts That Stay Separate

- Employee Position and job title are HR data. They never grant permissions or JWT claims.
- System Role and Role Group grant system permissions.
- Workflow Role controls approval routing only. It never grants system access.

## Permission Catalog

All permission codes live in:

```text
Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs
```

Each entry is a `public const string` plus a `Definition` with `Key`, `GroupKey`, `DescriptionKey`, optional `IsSensitive`, and optional `RiskLevel`. `Permissions.All` is the authoritative list used by seeding.

New HR permissions use stable dotted codes such as:

```text
hr.employee.view
hr.leave.request.approve
hr.salary.view
```

Legacy permissions in the same catalog may use other formats. Do not rename existing codes.

## Identity Mapping

The implemented flow is:

```text
User -> RoleGroup -> Identity Role
```

A role group maps Identity roles whose names are permission codes. The permission resolver returns the distinct role names for the user's role groups. The user/profile response exposes those codes as `user.permissions`; JWT claims carry role-group and policy claims, not a separate permission list.

- Protect endpoints with `[HasPermission(Permissions.Xxx)]`.
- `Administrator` is a reserved system role handled by centralized authorization logic.
- Frontend visibility uses `user.permissions`; `user.roles` is display/system-role metadata only.

## Adding A Permission

1. Add the `public const string` and `Definition` entry in `Permissions.cs`.
2. Add EN/VI keys to `SharedResource.en.resx` and `SharedResource.vi.resx`.
3. Add the module helper delegate.
4. Update Identity role-group seeding if the default profiles should receive it.
5. Update frontend visibility where applicable.

Missing translations must fail tests. Permission metadata localization is API-owned; do not add client-side permission dictionaries or silent fallbacks.

## Sensitive Permissions

`Definition` supports `IsSensitive` and `RiskLevel`. Use the policy in `docs/ai/security/sensitive-permissions.md` when implementing sensitive permission assignment. Do not assign sensitive permissions from a normal role-edit screen.

## Data Scoping

Data-scope policies such as `Own`, `Team`, or `Department` are not part of the current Identity implementation. Do not assume they exist. When a feature needs row-level scoping, enforce it explicitly in the query handler and verify it with allowed/denied tests.
