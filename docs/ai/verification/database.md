# Database Verification

Use this when a task changes EF Core mappings, migrations, seed data, persistence rules, or query behavior.

## Required Evidence

- Migration generated or confirmed unnecessary.
- Schema checked in the target database.
- Insert/update/read path checked where practical.
- Concurrency behavior checked when `xmin` or concurrency tokens are touched.
- Seed data checked when workflow, role, permission, or master data depends on it.

## Standard Report

```text
Database Verification:
- Migration: PASS/FAIL/N/A
- Schema check: PASS/FAIL/N/A
- Read/write check: PASS/FAIL/N/A
- Seed data check: PASS/FAIL/N/A
- Concurrency check: PASS/FAIL/N/A
- Evidence:
```

Do not claim database verification passed from code review only.
