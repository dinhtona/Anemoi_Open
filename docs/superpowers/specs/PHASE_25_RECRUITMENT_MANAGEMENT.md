# Phase 25 — Recruitment Management System

## Overview

Phase 25 introduces a complete Recruitment Management System into ANEMOI HR.

The recruitment module is implemented as a new HR sub-domain inside the existing `Anemoi.Hr` bounded context and follows the established architecture:

* Clean Architecture
* CQRS + MediatR
* EF Core + PostgreSQL
* Strongly Typed IDs
* Mapperly
* FluentValidation
* Result Pattern
* Xmin Optimistic Concurrency
* Permission-Based Authorization
* Frontend Error Translation

Recruitment is available under:

```text
/hr/recruitment/*
```

and includes:

1. Job Requisitions
2. Job Postings
3. Candidates
4. Applications & Stage History
5. Interviews & Feedback
6. Hiring Decisions
7. Candidate-to-Employee Conversion
8. Recruitment Analytics

---

# Module 1 — Job Requisitions

## Aggregate

```text
JobRequisition
```

### Statuses

```text
Draft
Submitted
Approved
Rejected
Closed
Cancelled
```

### Features

* Create requisition
* Update requisition
* Submit for approval
* Approve
* Reject
* Close
* Cancel

### Business Rules

* Headcount must be greater than zero
* TargetHireDate must be after OpenDate
* Closed requisitions cannot be modified
* Only approved requisitions may create job postings

---

# Module 2 — Job Postings

## Aggregate

```text
JobPosting
```

### Statuses

```text
Draft
Published
Expired
Closed
```

### Features

* Create posting
* Update posting
* Publish
* Expire
* Close

### Business Rules

* PublishDate <= ExpiryDate
* Posting requires an approved requisition
* Closed and expired postings are immutable
* Published postings cannot be republished

### Tests

17 domain tests implemented.

---

# Module 3 — Candidates

## Aggregate

```text
Candidate
```

### Statuses

```text
Active
Blacklisted
Archived
```

### Sources

```text
Website
Referral
LinkedIn
JobStreet
VietnamWorks
Other
```

### Features

* Create candidate
* Update profile
* Change source
* Blacklist
* Archive
* Reactivate
* Link employee

### Business Rules

* Unique email
* Unique phone number
* Immutable EmployeeId after linking
* Blacklisted candidates cannot be reactivated
* Archived candidates may be reactivated

### Tests

15 domain tests implemented.

---

# Module 4 — Candidate Applications & Stage History

## Aggregate

```text
CandidateApplication
```

## Supporting Entity

```text
CandidateApplicationStageHistory
```

### Stages

```text
Applied
Screening
Interview
Offer
Hired
Rejected
Withdrawn
```

### Features

* Apply to posting
* Stage transitions
* Stage history tracking
* Candidate pipeline management

### Business Rules

* One application per candidate per posting
* Only active candidates may apply
* Posting must be published
* Terminal stages cannot transition further
* Every transition creates stage history

### Verified Workflow

```text
Applied
 -> Screening
 -> Interview
 -> Offer
 -> Hired
```

Alternative terminal paths:

```text
Rejected
Withdrawn
```

### Tests

17 domain tests implemented.

---

# Module 5 — Interviews & Feedback

## Aggregate

```text
InterviewSchedule
```

## Supporting Entity

```text
InterviewFeedback
```

### Interview Types

```text
Phone
Online
Onsite
```

### Results

```text
Pending
Passed
Failed
NoShow
```

### Recommendations

```text
StrongHire
Hire
Neutral
Reject
```

### Features

* Schedule interview
* Reschedule interview
* Mark interview result
* Submit interviewer feedback

### Business Rules

* Duration must be greater than zero
* Completed interviews cannot be rescheduled
* Interview result cannot be set twice
* One feedback per interviewer per interview
* Feedback rating range 1–5

### Tests

16 domain tests implemented.

---

# Module 6 — Hiring Decisions

## Aggregate

```text
HiringDecision
```

### Decision Types

```text
Offer
Hire
Reject
```

### Features

* Create offer decision
* Create hire decision
* Create reject decision

### Business Rules

* One decision per application
* Offer requires completed interview
* Hire requires interview feedback
* Hire transitions application to Hired
* Reject transitions application to Rejected

### Tests

5 domain tests implemented.

---

# Module 7 — Candidate-to-Employee Conversion

## Command

```text
ConvertCandidateToEmployee
```

### Design Decision

Employee creation only.

Contract creation is intentionally deferred to the existing Contract Management module.

Response contains:

```text
RequiresContractCreation = true
```

### Features

* Verify hire decision exists
* Verify application is in Hired stage
* Create Employee
* Link Candidate → Employee
* Preserve recruitment history

### Idempotency

If conversion already occurred:

```text
AlreadyConverted = true
```

is returned and no duplicate employee is created.

---

# Module 8 — Recruitment Analytics

## Analytics Dashboard

### KPI Metrics

* Open Requisitions
* Approved Requisitions
* Published Postings
* Active Candidates
* Applications In Pipeline
* Hired Candidates
* Converted Candidates
* Pending Interviews

### Analytics Reports

#### Applications By Stage

Displays recruitment pipeline distribution.

#### Hiring By Department

Displays recruitment performance per department.

#### Candidate Source Effectiveness

Tracks:

* Candidate count
* Application count
* Hire count
* Conversion count
* Hire rate
* Conversion rate

#### Time To Hire

Measures:

* Average days
* Median days
* Minimum days
* Maximum days

### Dashboard Query

```text
GetRecruitmentAnalyticsDashboardQuery
```

aggregates all recruitment analytics into a single response.

---

# Permissions

Registered permissions:

```text
hr.recruitment.view
hr.recruitment.manage
hr.recruitment.interview
hr.recruitment.hire
hr.recruitment.analytics
```

Verified across:

* Permissions.cs
* HrPermissions.cs
* Controllers
* Frontend route protection
* UI action visibility

---

# Frontend

## Routes

```text
/hr/recruitment/requisitions
/hr/recruitment/postings
/hr/recruitment/candidates
/hr/recruitment/applications
/hr/recruitment/interviews
/hr/recruitment/hiring-decisions
/hr/recruitment/analytics
```

## Features

* React Query
* Server pagination
* Filtering
* Sorting
* Permission-aware actions
* EN/VI localization
* Shared error translation

---

# Database

## Recruitment Entities

```text
JobRequisition
JobPosting
Candidate
CandidateApplication
CandidateApplicationStageHistory
InterviewSchedule
InterviewFeedback
HiringDecision
```

### Concurrency

All recruitment entities use PostgreSQL xmin optimistic concurrency.

### Unique Constraints

Candidate:

```text
Email
PhoneNumber
CandidateCode
```

Applications:

```text
CandidateId + JobPostingId
```

Hiring Decisions:

```text
CandidateApplicationId
```

Interview Feedback:

```text
InterviewScheduleId + InterviewerEmployeeId
```

---

# Architecture Review

## Critical Findings

```text
0
```

## High Findings

### Fixed

1. Missing xmin on CandidateApplicationStageHistory
2. Missing xmin on InterviewFeedback
3. Recruitment overview date filter bug

### Deferred

Hardcoded GradeCode:

```text
G1
```

during candidate conversion.

Requires architectural decision:

* Command parameter
* Position mapping
* Grade lookup

Tracked as future technical debt.

## Medium Findings

1. Candidate search lacks department filter.

Backlog item.

---

# Verification Results

| Check              | Result    |
| ------------------ | --------- |
| dotnet build       | 0 errors  |
| dotnet test        | 64 passed |
| npm run lint       | 0 errors  |
| npm run build      | Success   |
| Recruitment Routes | 7         |
| Permissions        | 5         |
| Error Codes        | 44        |
| Critical Findings  | 0         |

---

# Deliverable Summary

| Module                       | Status   |
| ---------------------------- | -------- |
| Job Requisitions             | Complete |
| Job Postings                 | Complete |
| Candidates                   | Complete |
| Applications & Stage History | Complete |
| Interviews & Feedback        | Complete |
| Hiring Decisions             | Complete |
| Candidate Conversion         | Complete |
| Recruitment Analytics        | Complete |

Phase 25 is complete and stabilized.

Overall status:

```text
Production Candidate
```

Remaining known technical debt:

```text
TD-025-01
Hardcoded GradeCode during Candidate-to-Employee conversion.
```
