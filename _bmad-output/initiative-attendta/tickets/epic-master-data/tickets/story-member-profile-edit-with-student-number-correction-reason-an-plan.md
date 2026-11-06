---
title: 'Member profile edit with student-number correction reason and audit'
type: 'feature'
ticket: '6'
created: '2026-11-06'
status: 'built'
baseline_revision: 'f0d7ebae9bab040df3cfcdd87670f3637388c16e'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: [quick]
review_loop_iteration: 0
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

Management can correct a member's profile and student number with accountable, atomic audit history. Account approval remains the only creation path.

## Boundaries & Constraints

**Always:** Use the authenticated actor, management authorization, antiforgery, database uniqueness, and one transaction for each mutation and audit. Lock the linked user row for edits. Audit explicit safe old/new values and the correction reason. Preserve the existing active status on edit; newly approved members start active.

**Never:** Change login email, membership status, registration snapshots, certificates, or log password hashes. Do not introduce standalone member creation or hard deletion.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected behavior | Error handling |
|---|---|---|---|
| Profile edit | Management updates name and academic fields | Persist user/member changes and safe old/new audit atomically | Roll back on audit failure |
| Student correction | Different student number and nonblank reason | Persist unique number and log old/new values and reason | Validate reason inside transaction |
| Duplicate | Student number already exists or competing insert wins | No changes or audit | Duplicate outcome on student_number, including SQL 2601/2627 |
| No correction | Student number unchanged | No reason required | Existing validation applies to other fields |
| Approval | Pending request approved | Create active linked member through revised signature within approval transaction | Roll back all writes on failure |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Services/Services/MemberService.cs`, `Interfaces/IMemberService.cs` — update transaction, creation signature, remove status toggle; inject current actor, user lock and audit dependencies as needed.
- `ASI.Basecode.Data/Repositories/UserRepository.cs`, `Interfaces/IUserRepository.cs` — existing `LockUserAsync` uses SQL Server update/hold locks inside a transaction. Member repository supplies member lookup and uniqueness check.
- `ASI.Basecode.Services/Services/AccountRequestService.cs` — approval owns transaction and calls member creation; migrate signature without breaking atomicity.
- `ASI.Basecode.Services/Services/EventTypeService.cs`, `ActivityLogWriter.cs`, `ASI.Basecode.Resources/Constants/AuditActions.cs`, `Messages/Audit.resx` — patterns for explicit `IAuditValues`, registered actions and resource descriptions.
- `ASI.Basecode.WebApp/Controllers/MemberController.cs`, `Views/Member/Edit.cshtml`, `_Form.cshtml`, `Index.cshtml`, `ASI.Basecode.Services/ServiceModels/MemberViewModel.cs` — management edit UI and validation; remove status mutation controls; retain read-only email.
- `ASI.Basecode.Data/UnitOfWork.cs` — transaction and duplicate translation; map student-number races to field-specific duplicate without mislabeling unrelated errors.
- `tests/ASI.Basecode.IntegrationTests/EventTypeManagementTests.cs`, `AccountRequestDecisionTests.cs`, `MasterDataTransactionTests.cs`, `tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs` — existing audit, transaction, approval and unit test patterns.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Services/Services/MemberService.cs`, `Interfaces/IMemberService.cs`, `ServiceModels/MemberViewModel.cs`, `ASI.Basecode.Data/Interfaces/IMemberRepository.cs`, `Repositories/MemberRepository.cs` — implement transactional user-locked profile edit, correction validation, uniqueness handling, safe snapshots and creation signature; remove status toggle.
- [x] `ASI.Basecode.Services/Services/AccountRequestService.cs` — migrate approval call and preserve active-member creation in the existing transaction.
- [x] `ASI.Basecode.Resources/Constants/AuditActions.cs`, `Messages/Audit.resx`, `ASI.Basecode.Services/Services/ActivityLogWriter.cs` — register member audit actions and descriptions, recording safe explicit values for every member write.
- [x] `ASI.Basecode.WebApp/Controllers/MemberController.cs`, `Views/Member/Edit.cshtml`, `_Form.cshtml`, `Index.cshtml` — permit name and correction reason, display email read-only, surface field errors, remove status controls and toggle action.
- [x] `tests/ASI.Basecode.IntegrationTests/`, `tests/ASI.Basecode.Services.Tests/` — update approval fakes and add meaningful SQL-backed edit/audit, rollback, uniqueness, status and historical-snapshot checks; test endpoint authorization and validation as appropriate.

**Acceptance Criteria:**
- Given an Officer editing a member, when a valid correction is submitted, then name and permitted profile fields change and audit contains old/new values and reason without a password hash.
- Given a member form, when rendered or submitted, then email is read-only, status cannot be changed, and no status-toggle action is exposed.
- Given existing registrations and certificates, when the profile is edited, then their persisted values remain unchanged.
- Given a pending account request, when Management approves it, then an active member is created through the revised interface in the same transaction.

## Implementation Notes

- Profile editing and audit implementation completed; creation derives the actor from `ICurrentActor` and approval uses the revised signature.
- Build and 75 service tests passed. Focused member, approval and transaction SQL tests passed (28 tests); a later approval assertion passed independently.
- Full integration verification remains blocked: parallel execution exhausted LocalDB resources; a serialized retry with `ATTENDTA_PDFTOTEXT` configured encountered SQL Server internal memory exhaustion and Playwright browser crashes (156 passed, 9 failed). Review remains pending.
- A later retry identified and configured both Poppler tools from the existing temporary installation. One serialized run exceeded 10 minutes; a subsequent run failed to initialize SQL fixtures because the shared LocalDB instance timed out. Do not restart the shared instance without coordinating other worktrees.
- After authorized LocalDB restart and Poppler PATH configuration, the complete serialized integration suite passed: 165/165.
- Final verification after the email-redisplay patch: build passed with zero warnings/errors; 75 service tests passed. On dedicated `AttendTaIssue25` LocalDB, the full integration run passed 162/165 with three browser-rendering failures; the entire certificate class then passed 8/8 separately, resolving all failed cases without code changes. All 165 cases passed across these runs; no skips.
- Matrix coverage passed: profile correction/history/status, unchanged number, existing duplicate and race, audit rollback, correction-reason endpoint validation, and account approval atomicity. Quick review completed; no deferred findings.

## Plan Change Log

## Review Triage Log

- medium / patch — Invalid POST redisplay can show a forged bound email instead of the canonical linked-user email. Verified bound-model and ModelState precedence in the controller/view; fixed by restoring stored email and removing submitted Email ModelState on both validation and domain failures. Endpoint regression test passed. Verdict counts: high 0, medium 1 (fixed), low 0, false 0, maybe-false 0.

## Verification

**Commands:**
- `dotnet build` — solution builds.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — service tests pass.
- `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` with `ATTENDTA_TEST_SQL` pointing to local SQL Server master — integration tests pass.
- Resource-constrained equivalent used: `dotnet run --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --no-build -- -maxThreads 1 -parallelMode none`; retry certificate class with `-class ASI.Basecode.IntegrationTests.CertificateTracerFlowTests`. Set `ATTENDTA_TEST_SQL` to `Server=(localdb)\AttendTaIssue25;Database=master;Trusted_Connection=True;TrustServerCertificate=True` and put the temporary Poppler `Library/bin` directory on PATH.
