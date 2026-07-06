# Phase 25: Recruitment Management System — Design Spec

## Overview

Add a complete Recruitment Management module to the ANEMOI HR system. Implemented as a new sub-domain inside `Anemoi.Hr` service, following existing Clean Architecture patterns, CQRS, EF Core, and vertical slice organization.

## Architecture

- **Service**: `Anemoi.Hr` (existing)
- **Namespace root**: `Anemoi.Hr.Domain.Recruitment`, `Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands`, etc.
- **Frontend routes**: `/hr/recruitment/*` under existing HR sidebar section
- **Frontend technology**: React, Next.js App Router, React Query, shadcn/ui, next-intl, Zod + react-hook-form

## Modules (Vertical Slices)

### Module 1: Job Requisitions
- Aggregate: `JobRequisition` with `JobRequisitionId`
- Statuses: `Draft`, `Submitted`, `Approved`, `Rejected`, `Closed`, `Cancelled`
- Rules: Headcount > 0, TargetHireDate >= OpenDate, closed cannot modify, approved may create postings

### Module 2: Job Postings
- Aggregate: `JobPosting` with `JobPostingId`
- Statuses: `Draft`, `Published`, `Expired`, `Closed`
- Rules: PublishDate <= ExpiryDate, cannot publish closed requisitions

### Module 3: Candidates
- Aggregate: `Candidate` with `CandidateId`
- Sources: `Website`, `Referral`, `LinkedIn`, `JobStreet`, `VietnamWorks`, `Other`
- Statuses: `Active`, `Blacklisted`, `Archived`
- Rules: Email unique, Phone unique, blacklisted cannot apply

### Module 4: Applications + Stage History
- Aggregate: `CandidateApplication` with `CandidateApplicationId`
- Entity: `CandidateApplicationStageHistory` (separate entity for audit/history identity)
- Stages: `Applied`, `Screening`, `Interview`, `Offer`, `Hired`, `Rejected`, `Withdrawn`
- Rules: Cannot apply twice to same posting, track full stage history

### Module 5: Interviews
- Aggregate: `InterviewSchedule` with `InterviewScheduleId`
- Types: `Phone`, `Online`, `Onsite`
- Results: `Pending`, `Passed`, `Failed`, `NoShow`
- Rules: Duration > 0, interviewer must exist, cannot schedule for rejected applications

### Module 6: Hiring Decisions
- Aggregate: `HiringDecision` with `HiringDecisionId`
- Decisions: `Offer`, `Hire`, `Reject`
- Rules: One final decision only, cannot hire twice

### Module 7: Candidate-to-Employee Conversion
- Command: `ConvertCandidateToEmployeeCommand`
- Creates Employee + Contract, links Candidate, preserves history
- Idempotent, transactional

### Module 8: Recruitment Analytics
- Read-only queries aggregating recruitment data
- Dashboard: open requisitions, open positions, applications by stage, time to hire, hiring by department, source effectiveness

## Permissions

```csharp
hr.recruitment.view
hr.recruitment.manage
hr.recruitment.interview
hr.recruitment.hire
hr.recruitment.analytics
```

## Error Codes

All error codes follow `HR_REC_{MODULE}_{ERROR}` pattern (e.g., `HR_REC_REQUISITION_NOT_FOUND`).

## Implementation Order

Each module is a vertical slice: Domain → Infrastructure (EF config + migration) → Application (CQRS + Mapper + Validator) → API (Controller) → Frontend (Pages + Hooks + Service) → Tests.

Proceed module-by-module in the order listed above.
