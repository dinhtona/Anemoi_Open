# HR Identity Linker Test Flow

This flow is for local Development testing only.

## Seed Data

Start the services with `ASPNETCORE_ENVIRONMENT=Development`.

Identity Worker seeds non-admin test users:

```text
admin@anemoi.com
linh.nguyen@anemoi.test
minh.tran@anemoi.test
an.pham@anemoi.test
mai.le@anemoi.test
khoa.do@anemoi.test
```

HR API seeds matching local HR data:

```text
Departments: Engineering, People Operations
Positions: Engineering Manager, Software Engineer, HR Manager, HR Specialist
Employees: DEV-ENG-001..003, DEV-HR-001..002
Admin employee: DEV-ADMIN-001 for admin@anemoi.com
Leave policy: DEV-ANNUAL
Leave balances: current year, 15 remaining days per seeded employee
```

## Export Identity Candidates

Use the Identity GraphQL endpoint to export users:

```graphql
query ExportIdentityUsers {
  users {
    userId
    email
  }
}
```

Convert the result into linker candidates:

```json
{
  "dryRun": true,
  "candidates": [
    {
      "identityUserId": "IDENTITY_USER_GUID",
      "email": "linh.nguyen@anemoi.test"
    }
  ]
}
```

## Dry Run

Call HR:

```bash
curl -X POST "https://localhost:PORT/api/hr/employee/Employee/LinkEmployeesToIdentityUsers" \
  -H "Authorization: Bearer ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d @identity-link-candidates-dry-run.json
```

The access token must include:

```text
hr.employee.identity_link
```

Dry-run writes `EmployeeIdentityLinkLogs` rows but does not update `Employees.IdentityUserId`.

## Review Logs

Review recent linker logs:

```sql
select "EmployeeId",
       "WorkEmail",
       "IdentityUserId",
       "MatchStatus",
       "CreatedAt",
       "CreatedBy"
from "EmployeeIdentityLinkLogs"
order by "CreatedAt" desc;
```

Expected successful local dry-run rows use:

```text
Matched
```

Resolve these before apply mode:

```text
DuplicateEmployeeEmail
DuplicateIdentityEmail
ConflictExistingMapping
InvalidEmail
NoMatch
```

## Apply

Use the same candidates with:

```json
{
  "dryRun": false,
  "candidates": []
}
```

Apply mode updates only exact one-to-one matches. Existing conflicting mappings are never overwritten.

Verify:

```sql
select "EmployeeCode", "FullName", "WorkEmail", "IdentityUserId"
from "Employees"
where "WorkEmail" like '%@anemoi.test'
order by "EmployeeCode";
```

`GetMyProfile` still supports WorkEmail fallback during rollout, but mapped users should resolve by `IdentityUserId` first.
