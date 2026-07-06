# Performance Prompt Template

Use this prompt when asking an AI agent to investigate, optimize, or verify performance. Performance work must be measurement-driven; do not optimize based only on guesses.

Copy the text block below and replace every `<...>` field with concrete information. Do not leave placeholders in the final task prompt.

```text
Repository:
- dinhtona/Anemoi_Open

Target branch:
- dev

Task type:
- Performance

Performance objective:
- <describe the slow operation, target behavior, and expected improvement>

Affected area:
- Backend/API/job/query: <if relevant>
- Frontend route/component: <if relevant>
- Database table/query: <if relevant>
- Workflow/event path: <if relevant>
- External dependency: <if relevant>

Known baseline:
- Current duration: <number and source, if known>
- Expected duration: <target, if known>
- Dataset size: <if known>
- Environment: <local/docker/staging/production-like, if known>

Mandatory startup rules:
1. Verify repository access to dinhtona/Anemoi_Open.
2. Confirm the target branch is dev unless this prompt explicitly says otherwise.
3. Read and follow:
   - AGENTS.md
   - docs/AI_ENGINEERING_HANDBOOK.md
   - docs/README.md
   - docs/ai/README.md
   - docs/ai/core/verification-standard.md
   - docs/ai/verification/README.md
   - Relevant ADRs in docs/architecture/ARCHITECTURE_DECISIONS.md
   - Relevant rules under docs/ai/core/, docs/ai/security/, docs/ai/workflows/, docs/ai/modules/, and docs/ai/uat/
4. If any required source file cannot be read, stop and report BLOCKED.
5. Do not claim performance improvement without before/after measurement or clearly mark the result as REVIEWED.

Performance rules:
- Measure before changing code when practical.
- Identify whether the bottleneck is CPU, database, network, external dependency, serialization, frontend rendering, or browser/API waterfall.
- Prefer query shaping, pagination, indexing, batching, projection, caching, and N+1 removal over broad rewrites.
- Do not weaken authorization, validation, audit logging, workflow history, or data correctness for speed.
- Do not bypass domain/application invariants.
- Do not introduce new infrastructure or external libraries without explicit justification.
- Preserve sensitive HR/payroll snapshot correctness.

Required investigation:
1. Define the exact operation being measured.
2. Establish baseline or explain why baseline measurement is BLOCKED.
3. Inspect code path and data path.
4. Identify likely bottleneck with evidence.
5. Implement the smallest safe optimization.
6. Measure after the change using the same scenario where practical.
7. Verify correctness and security did not regress.

Required verification:
- Backend/API: build, relevant tests, API timing evidence, and authorization checks when endpoints are protected.
- Database: query shape/index/schema verification when database performance is involved.
- Frontend: build and browser validation, including network waterfall or route interaction evidence.
- Workflow/event-driven: prove workflow state, history, target status, and event/notification side effects still work.
- Payroll/sensitive HR data: prove calculations/snapshots/sensitive scope remain correct.

Final report format:
- Repository:
- Branch:
- Performance objective:
- Baseline measurement:
- Bottleneck evidence:
- Optimization summary:
- Files changed:
- Commit SHA:
- After measurement:
- Correctness verification:
- Security/scope verification:
- Known limitations / unverified areas:
- Status: PASS / FAILED / PARTIAL / REVIEWED / BLOCKED
```

## Usage Notes

- Use `REVIEWED` for inspection-only performance recommendations.
- Use `PARTIAL` if a safe optimization was implemented but before/after measurement could not be completed.
- Use `PASS` only when the required correctness checks pass and the before/after evidence supports the performance claim.
