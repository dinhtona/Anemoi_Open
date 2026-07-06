# Phase 32 — Defect Fix Log

## Defect 1: Recruitment Search Queries Return 400

**Severity:** High (blocking UAT of recruitment pages)

**Symptom:** `GET /api/hr/recruitment/RecruitmentRequisitions/SearchRequisitions` returns HTTP 400 with validation errors:
```
{"errors":{"Status":["The Status field is required."],"SearchTerm":["The SearchTerm field is required."],"DepartmentId":["The DepartmentId field is required."]}}
```

**Root Cause:** `SearchRequisitionsQuery` declared `SearchTerm`, `Status`, and `DepartmentId` as non-nullable types. ASP.NET model binding requires these from the query string when they're non-nullable. The frontend calls the endpoint without any filter parameters (expecting all results).

**Fix:** Changed all search query parameters to nullable types:
- `SearchRequisitionsQuery`: `string SearchTerm` → `string? SearchTerm`, etc.
- `SearchCandidatesQuery`: Same pattern (3 fields)
- `SearchCandidateApplicationsQuery`: Same pattern (4 fields)
- `SearchJobPostingsQuery`: Same pattern (3 fields)
- `SearchInterviewsQuery`: Same pattern (2 fields + 1 nullable ID)
- `SearchHiringDecisionsQuery`: Same pattern (2 fields)

**Files Changed:**
- `Anemoi.Hr.Application/Cqrs/Queries/RecruitmentQueries/SearchRequisitions/SearchRequisitionsQuery.cs`
- `Anemoi.Hr.Application/Cqrs/Queries/RecruitmentQueries/SearchCandidates/SearchCandidatesQuery.cs`
- `Anemoi.Hr.Application/Cqrs/Queries/RecruitmentQueries/SearchCandidateApplications/SearchCandidateApplicationsQuery.cs`
- `Anemoi.Hr.Application/Cqrs/Queries/RecruitmentQueries/SearchJobPostings/SearchJobPostingsQuery.cs`
- `Anemoi.Hr.Application/Cqrs/Queries/RecruitmentQueries/SearchInterviews/SearchInterviewsQuery.cs`
- `Anemoi.Hr.Application/Cqrs/Queries/RecruitmentQueries/SearchHiringDecisions/SearchHiringDecisionsQuery.cs`

---

## Defect 2: Onboarding My-Tasks Endpoint Returns 400

**Severity:** High (blocks onboarding task page)

**Symptom:** `GET /api/hr/onboarding/my-tasks` returns HTTP 400:
```
{"errors":{"UserId":["The UserId field is required."]}}
```

**Root Cause:** `GetMyOnboardingTasksQuery` declared `UserId` as non-nullable. The controller overrides it with `HttpContext.GetUserId()` after model binding, but model binding fails first because `UserId` is required.

**Fix:** Changed `UserId` to nullable in the query record.

**Files Changed:**
- `Anemoi.Hr.Application/Cqrs/Queries/OnboardingQueries/GetMyOnboardingTasks/GetMyOnboardingTasksQuery.cs`

---

## Defect 3: Missing i18n Translations for Recruitment Requests Page

**Severity:** Medium (UI shows untranslated keys)

**Symptom:** Recruitment requests page shows raw i18n keys like `HRRecruitment.requests.title`, `HRRecruitment.requests.table.requestNumber`, etc.

**Root Cause:** The `requests` translation namespace was not added to `/messages/en.json` under the `HRRecruitment` section when the recruitment requests page was created.

**Fix:** Added full `requests` translation block with 25+ keys covering page title, table headers, dialog labels, priority options, and action buttons.

**Files Changed:**
- `cody-web-app/messages/en.json`

---

## Non-Blocking Issues (Logged)

| Issue | Location | Severity |
|-------|----------|----------|
| Insurance reports page: Select.Item value prop warning | `cody-web-app` | Low |
| Insurance reports page: 404 resource load | `cody-web-app` | Low |
| 39 pre-existing CS0618 deprecation warnings | `Anemoi.Hr.Application` | Low (technical debt) |
| SearchCandidateApplicationsHandler ignores SearchTerm | `Anemoi.Hr.Application` | Low |
