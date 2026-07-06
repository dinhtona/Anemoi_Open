# HR Platform Overview

HR is a long-term bounded context. Initial service: `anemoi_hr`.

Modules:

```text
Employee Management
Organization Management
Leave Management
Approval Workflow Integration
Payroll Management
Contract Management
Skill Management
Grade / Level Management
Career Path Management
Salary Change Management
Department Transfer Management
Document Management
Notification Integration
Audit Integration
```

Start as one HR bounded context. Keep internal module boundaries clear so future microservice extraction is possible.
