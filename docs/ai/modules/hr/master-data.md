# HR Master Data

Use shared Master Data for configurable values.

Category groups:

```text
employee_status
employment_type
department_type
position_type
leave_type
contract_type
salary_change_reason
skill_category
skill_level
employee_grade
promotion_status
approval_status
document_type
working_calendar_type
```

Suggested schema:

```text
CategoryGroups(Id, Code, Name, Description, IsSystem)
Categories(Id, GroupCode, Code, Name, ParentId, SortOrder, IsActive, MetadataJson)
```

Use stable `Code` as wire value. Do not localize codes.
