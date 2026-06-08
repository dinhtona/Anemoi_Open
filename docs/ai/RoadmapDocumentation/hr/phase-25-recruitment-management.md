# Phase 25 - Recruitment Management

Status: Planned

---

## Objective

Introduce recruitment management to handle hiring pipeline before candidate conversion into employee.

---

## Business Requirements

- Job requisition
- Candidate management
- Interview scheduling
- Offer management
- Hiring conversion

---

## Domain Model

### JobRequisition
Hiring request.

### Candidate
Job applicant.

### Interview
Interview schedule and result.

### JobOffer
Offer made to a candidate.

---

## Workflow

```text
Job Requisition
    ↓
Open Position
    ↓
Candidate
    ↓
Interview
    ↓
Offer
    ↓
Accepted
    ↓
Employee
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- Job requisition is a strong candidate for Enterprise Approval Framework.

---

## Commands

- CreateJobRequisitionCommand
- SubmitJobRequisitionCommand
- CreateCandidateCommand
- ScheduleInterviewCommand
- RecordInterviewResultCommand
- CreateJobOfferCommand
- ConvertCandidateToEmployeeCommand

---

## Queries

- GetJobRequisitionsQuery
- GetCandidatesQuery
- GetCandidateDetailQuery
- GetInterviewScheduleQuery

---

## Permissions

```text
hr.recruitment.view
hr.recruitment.manage
hr.recruitment.approve
```

---

## API Endpoints

```text
GET    /api/hr/recruitment/job-requisitions
POST   /api/hr/recruitment/job-requisitions
POST   /api/hr/recruitment/candidates
POST   /api/hr/recruitment/interviews
POST   /api/hr/recruitment/offers
POST   /api/hr/recruitment/candidates/{id}/convert-to-employee
```

---

## Frontend

- Job Requisition List
- Candidate Pipeline
- Candidate Detail
- Interview Calendar
- Offer Management

---

## Future Enhancements

- Career site integration
- Resume parsing
- Interview feedback forms
- Recruitment analytics
