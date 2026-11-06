---
title: 'Public Account Request submission with field validation and Argon2id hash'
type: 'feature'
ticket: '5'
created: '2026-11-05'
status: 'built'
baseline_revision: '8765db761006d69691f52458d9302818d3ebe7ac'
route: 'full'
route_source: 'auto'
risk: 'medium'
review: 'quick'
review_source: 'pinned'
lenses_ran: [quick]
review_loop_iteration: 0
context:
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Applicants have no way to request membership. The `account_requests` table, pending/approved/rejected lookups, filtered unique indexes and Argon2id hasher exist, but no public submission path does, so the approval queue (1.6/1.7) has nothing to review.

**Approach:** Add an anonymous request form and acknowledgment page, a service that validates details and stores only an Argon2id hash in a pending request, and a reusable controller mapping for `DomainException` (field errors, TempData + POST-redirect-GET, or JSON + HTTP status).

## Boundaries & Constraints

**Always:** The form and acknowledgment are anonymous; every mutation is POST with antiforgery. Store only the Argon2id hash, never the plaintext or confirmation. Reject duplicates against users (including inactive), members (including inactive), and pending requests before insert; the pending-only filtered unique indexes remain the final guard. Use the injected `TimeProvider` for `requested_at`; store UTC. Keep the `account_requests` schema and lookup IDs unchanged. Write no audit entry for this anonymous action.

**Never:** Create a user or member row, approve or reject a request, add a status-tracking page, expose request hashes, add password recovery, or change the login flow. Do not add a standalone member-creation path.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Valid submission | All required fields, matching passwords, unused email and student number | One pending row with Argon2id hash and injected UTC time; redirect to acknowledgment | No error |
| Duplicate email | Email belongs to a user (active or inactive) or a pending request | Form redisplays | Field error on Email; no row |
| Duplicate student number | Number belongs to a member (active or inactive) or a pending request | Form redisplays | Field error on StudentNumber; no row |
| Invalid fields | Missing required, malformed email, year level 0, mismatched confirmation | Form redisplays | Field errors; no row; passwords cleared |
| Concurrent duplicate | Two submissions race past the pre-check | Exactly one pending row | Loser gets `Duplicate`; PRG with TempData or 409 JSON |
| AJAX error | `X-Requested-With: XMLHttpRequest` or `Accept: application/json` | JSON `{ outcome, message }` | 400 Invalid, 403 Forbidden, 404 NotFound, 409 Duplicate/Stale/Capacity/Cutoff, 500 FileFailure |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Models/AccountRequest.cs`, `CanonicalSchemaMappings.cs`, `database/attendta-mvp-schema.sql` — entity, status lookup, filtered unique indexes on pending email/student number. Reuse; do not alter.
- `ASI.Basecode.Data/Repositories/AccountRequestRepository.cs` + `Interfaces/IAccountRequestRepository.cs` — add `AddAsync`, `PendingEmailExistsAsync`, `PendingStudentNumberExistsAsync`; keep `LockAccountRequestAsync` for 1.7.
- `ASI.Basecode.Data/Repositories/UserRepository.cs` `GetByEmailAsync` — returns inactive accounts too; reuse for the email check. `MemberRepository.StudentNumberExists` — includes inactive members; reuse.
- `ASI.Basecode.Services/Security/Argon2PasswordHasher.cs` + `IPasswordHasher` — registered singleton; call `Hash` only.
- `ASI.Basecode.Data/UnitOfWork.cs` — `SaveChangesAsync` translates SQL 2601/2627 to `DomainException(Duplicate)`; rely on it for the race.
- `ASI.Basecode.Resources/DomainException.cs` — `OutcomeCode`/`Field`; map in the WebApp base controller.
- `ASI.Basecode.WebApp/Mvc/ControllerBase.cs` — add AJAX detection and `HandleDomainException`; no existing outcome mapping.
- `ASI.Basecode.WebApp/Views/Account/Login.cshtml`, `Views/Shared/_Layout.cshtml`, `Styles/app.css` — reuse `.login-card`, `.field`, `.button`; public shell is 448px, add a 576px `public-wide` variant.
- `tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs`, `SqlServerDatabaseFixture.cs` — reuse antiforgery-token and isolated-database patterns.
- Do not touch `ActivityLogWriter` (requires an authenticated actor and open transaction), `AccountService` login, or the schema.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Services/ServiceModels/AccountRequestViewModel.cs` — add annotated fields (student number, full name, email, course, year level ≥ 1, section, optional mobile, password, confirm) — one validation surface for MVC and tests.
- [ ] `ASI.Basecode.Data/Interfaces/IAccountRequestRepository.cs` and `Repositories/AccountRequestRepository.cs` — add pending email/student-number existence queries and `AddAsync` — keeps Data access in the repository.
- [ ] `ASI.Basecode.Services/Interfaces/IAccountRequestService.cs` and `Services/AccountRequestService.cs` — validate, trim, check users/members/pending requests, hash, set Pending + injected UTC time, save — the business boundary.
- [ ] `ASI.Basecode.WebApp/Mvc/ControllerBase.cs` — add `IsAjaxRequest`, `InvalidModelResult`, and `HandleDomainException` — reusable AD-9 mapping for this and later epics.
- [ ] `ASI.Basecode.WebApp/Controllers/AccountRequestController.cs` — anonymous GET form, POST submit, GET acknowledgment; map `DomainException` — public entry point.
- [ ] `ASI.Basecode.WebApp/Startup.DI.cs` — register `IAccountRequestService` — wire the service.
- [ ] `ASI.Basecode.WebApp/Views/AccountRequest/Request.cshtml` and `Submitted.cshtml` — 576px form and acknowledgment using existing field/button classes — the applicant-facing pages.
- [ ] `ASI.Basecode.WebApp/Views/Account/Login.cshtml`, `Views/Shared/_Layout.cshtml`, `Styles/app.css`, `wwwroot/css/app.css` — add the "Request membership" link and `public-wide` 576px shell; regenerate CSS — entry point and layout.
- [ ] `tests/ASI.Basecode.Services.Tests/AccountRequestServiceTests.cs` — cover every matrix row with fakes and a fixed clock — pure-rule proof.
- [ ] `tests/ASI.Basecode.IntegrationTests/AccountRequestSubmissionTests.cs` — HTTP form, DB assertions, AJAX JSON, antiforgery, concurrent duplicate — real-pipeline proof.

**Acceptance Criteria:**
- Given an anonymous visitor, when they open `/AccountRequest/Request`, then the form renders without a sign-in redirect.
- Given valid details, when submitted, then exactly one pending `account_requests` row exists with an `$argon2id$` hash that verifies, no plaintext or confirmation stored, `requested_at` from the injected clock, and no user/member row; the acknowledgment page appears.
- Given a duplicate email or student number (existing user/member or pending request), when submitted, then the form redisplays with that field's error and no row is added.
- Given missing or invalid fields, or mismatched confirmation, when submitted, then the form redisplays with field errors and no row is added.
- Given an AJAX request that fails, when the action returns, then the response is JSON `{ outcome, message }` with the mapped HTTP status.
- Given two concurrent identical submissions, when both finish, then exactly one pending row exists and only one redirects to the acknowledgment.
- Given a POST without antiforgery, when submitted, then it is rejected.

## Implementation Notes

- Added the anonymous request form, acknowledgment page, `AccountRequestService`, repository pending-uniqueness queries, and the reusable `DomainException` mapping in `ControllerBase`.
- Verification: Release build 0 warnings/0 errors; 36 service tests and 100 integration tests pass; CSS rebuild is reproducible.
- Review: quick lens ran. One low UX finding (year level opened at 0) was patched by making it nullable and required; the remaining findings were rejected as false or negligible.

## Plan Change Log

## Review Triage Log

| # | Location | Verdict | Evidence | Route |
|---|---|---|---|---|
| 1 | `ASI.Basecode.Services/Services/AccountRequestService.cs:73` | low | The student-number check uses the existing synchronous `MemberRepository.StudentNumberExists`; a cancelled submission still runs one indexed query. The fix would add an async overload for negligible harm. | reject |
| 2 | `ASI.Basecode.Services/Services/AccountRequestService.cs:119-130` | false | The controller's `ModelState` runs the same annotations before the service, so all field errors render; `InvalidFieldsRedisplayWithFieldErrorsAndClearPasswords` proves it. The service's single-error throw is unreachable for annotation violations from the page flow. | reject |
| 3 | `ASI.Basecode.WebApp/Controllers/AccountRequestController.cs:64-68` | false | `InvalidModelResult` adds the model-level message when `ModelState[string.Empty]` is absent or empty, so the summary renders; the finding states the opposite of the condition. | reject |
| 4 | `ASI.Basecode.WebApp/Views/AccountRequest/Request.cshtml:33` | low | The GET returns `new AccountRequestViewModel()` with `YearLevel = 0`; the number input renders `value="0"` and `min="1"` marks the form invalid before the user edits it. | patch |
| 5 | `ASI.Basecode.WebApp/Views/AccountRequest/Submitted.cshtml:1-10` | low | The acknowledgment is static and exposes no request data; guarding direct access needs TempData/state for negligible harm. | reject |
| 6 | `ASI.Basecode.WebApp/Mvc/ControllerBase.cs:181-198` | false | The plan's Design Notes explicitly choose TempData + PRG for a field-less race duplicate; this story produces no other field-less outcome, and later epics choose their own mapping. | reject |

## Design Notes

- Single insert, no explicit transaction; the pending-only filtered unique indexes are the final guard. Pre-checks give field-specific messages; a rare race duplicate has no field and follows TempData + PRG.
- Email matching follows login: trim, compare through SQL Server's default case-insensitive collation; store as entered.
- Clear `Password`/`ConfirmPassword` before redisplaying; password inputs never render values.
- JSON outcome uses `OutcomeCode.ToString()` under camelCase keys, for example `{"outcome":"Duplicate","message":"..."}`.

## Verification

**Commands:**
- `npm.cmd --prefix ASI.Basecode.WebApp ci` then `npm.cmd --prefix ASI.Basecode.WebApp run css:build` — dependencies install and CSS regenerates; commit the regenerated `wwwroot/css/app.css`.
- `dotnet build ASI.Basecode.sln --configuration Release` — succeeds with no warnings.
- `dotnet test --project tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj --configuration Release` — service tests pass.
- Set `ATTENDTA_TEST_SQL="Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"` then `dotnet test --project tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --configuration Release` — integration tests pass.

**Manual checks:**
- Start the app, open `/AccountRequest/Request` while signed out, submit a valid request, confirm the acknowledgment; submit the same email again and confirm the field error.
