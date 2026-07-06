# Employee & Organization Management

Aggregates/entities:

```text
Employee
Department
Position
EmployeeDepartmentHistory
EmployeePositionHistory
EmployeeGradeHistory
EmployeeManagerHistory
```

Employee suggested fields:

```text
EmployeeId
EmployeeCode
FullName
WorkEmail
PersonalEmail
PhoneNumber
DateOfBirth
JoinDate
EmploymentStatusCode
EmploymentTypeCode
PrimaryDepartmentId
PrimaryPositionId
DirectManagerEmployeeId
CreatedAt
UpdatedAt
```

Department is hierarchical: `ParentDepartmentId`, `ManagerEmployeeId`.

Rules:

- EmployeeCode must be unique.
- WorkEmail should be unique if provided.
- Employee can have one primary department.
- Department transfer must create history.
- Position/grade changes must create history.
- Sensitive personal fields require sensitive permission and audit log.
