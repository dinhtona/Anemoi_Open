# Review Prompt Template

Use this prompt when asking an AI agent to review a completed change, pull request, commit, phase report, or implementation summary. A review is not verification unless the reviewer executes the required checks.

Copy the text block below and replace every `<...>` field with concrete information. Do not leave placeholders in the final task prompt.

```text
Repository:
- dinhtona/Anemoi_Open

Target branch or PR:
- <dev | PR number | commit SHA | branch name>

Task type:
- Review

Review scope:
- <describe the files, commits, PR, phase, module, or report to review>

Review objective:
- <describe what decision the review should support>

Mandatory startup rules:
1. Verify repository access to dinhtona/Anemoi_Open.
2. Confirm the target branch, PR, or commit exists.
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
5. Do not claim PASS, VERIFIED, COMPLETE, PRODUCTION READY, or UAT READY from static review alone.

Review requirements:
- Identify changed files and the stated intent of the change.
- Compare the implementation against source-of-truth documents and current repository patterns.
- Check Clean Architecture boundaries.
- Check CQRS usage and controller thinness for backend changes.
- Check permission constants, [HasPermission], and allowed/denied behavior for protected areas.
- Check Three-Scope Architecture for HR routes and APIs.
- Check Workflow Engine usage for workflow-enabled entities.
- Check localization for user-facing text.
- Check sensitive HR/payroll/audit behavior when relevant.
- Check frontend permission visibility, loading, error, empty, and localization states when relevant.
- Check whether the reported verification evidence is current, sufficient, and honestly labeled.

Finding categories:
- Blocker: must fix before merge/release.
- Required: should fix before accepting the change.
- Suggested: useful improvement, not required for current scope.
- Question: clarification needed.
- Verified evidence gap: report claims exceed executed evidence.

Optional executed verification:
- If tools and environment are available, execute the relevant checks from docs/ai/verification/README.md.
- If verification is executed, separate it from static review findings.
- If verification is not executed, use REVIEWED for the review result.

Final report format:
- Repository:
- Target branch/PR/commit:
- Review scope:
- Files reviewed:
- Source-of-truth documents used:
- Findings:
  - Blocker:
  - Required:
  - Suggested:
  - Question:
  - Verified evidence gap:
- Verification executed, if any:
- Evidence quality assessment:
- Recommendation: approve / request changes / needs verification / blocked
- Known limitations / unverified areas:
- Status: PASS / FAILED / PARTIAL / REVIEWED / BLOCKED
```

## Usage Notes

- Use this template for review, not implementation.
- Static review alone should normally end with `REVIEWED`.
- Use `PASS` only if the review includes all required executed verification for the declared scope and every check passes.
- Call out overclaimed verification explicitly; fake or stale evidence is a review finding.
