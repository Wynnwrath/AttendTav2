---
title: 'Role assignment with last-active-Administrator app lock'
type: 'feature'
ticket: '2'
created: '2026-11-06'
status: 'built'
baseline_revision: 'be96f4b32295d0584788e10e85d9adaea9c9ced7'
route: 'full'
route_source: 'auto'
risk: 'high'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent â€” do not modify unless human renegotiates">

## Intent

**Problem:** The Administrator accounts console lists each role but cannot change it, so a departing officer keeps Officer access and nothing records who changed what.

**Approach:** Add an Administrator-only role change at `/admin/accounts/{id}/role` with a role select and a mandatory reason. It writes `users.role_id`, `updated_by`/`updated_at`, and one `user.role_assigned` Activity Log row inside one `ExecuteInTransactionAsync`. Demoting an active Administrator first takes `sp_getapplock 'attendta:last-admin'`, then locks the `users` row, then re-reads and counts active Administrators, refusing to remove the last one.

## Boundaries & Constraints

**Always:** Administrator policy on both actions; antiforgery on the POST; only `member`, `officer`, `admin`; trimmed reason of 1â€“1000 characters; app lock before the `users` row lock; count active Administrators (`role_id = 1 AND is_active = 1`) after the lock in the same transaction; one audit row per real change; redirect to `/admin/accounts` so the new badge shows; actor from `ICurrentActor`, never a form field.

**Never:** change `is_active`, membership, registrations, or sessions (5.4â€“5.7 own those); add custom roles or a permission editor; write a second audit row for a no-op; log passwords, hashes, tokens, or whole entities; change the schema, EF mappings, or the 5.1 list query; retry outside the transaction.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output | Error Handling |
|---|---|---|---|
| Change role | Active member â†’ `officer`, reason "Promoted" | `role_id` updated, one audit row, Officer badge | No error |
| Demote an admin | Two active admins, demote one | Succeeds; one active admin remains | No error |
| Demote the last admin | One active admin â†’ `member` | No write, no audit row | `Stale` |
| Concurrent demotions | Two active admins, parallel demotions | Exactly one commits; one active admin remains | Loser gets `Stale` |
| No-op | Target already has the requested role | Success, no audit row | No error |
| Bad input | Unknown role, blank or overlong reason, unknown account | Form redisplays with the field error | `Invalid` / `NotFound` |
| Access | Anonymous / Officer / Administrator | Login redirect / access denied / page renders | No error |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Repositories/AccountAdministrationRepository.cs` and its interface â€” 5.1 read model; add account-by-id, role options, active-admin count, app lock.
- `ASI.Basecode.Data/Repositories/UserRepository.cs:65` â€” `LockUserAsync` already runs `UPDLOCK, HOLDLOCK, ROWLOCK` and re-attaches the row; reuse it.
- `ASI.Basecode.Services/Services/AccountRequestService.cs:200-225` â€” the transaction + `AuditEntry` shape to copy.
- `ASI.Basecode.Services/Services/AccountRequestQueueService.cs` â€” precedent for a separate read service; keep 5.1's `AccountAdministrationService` untouched, add `AccountRoleService` beside it.
- `ASI.Basecode.WebApp/Controllers/AdminAccountsController.cs` â€” 5.1 GET only; add antiforgery, GET/POST role actions, `HandleDomainException`.
- `ASI.Basecode.WebApp/Views/AdminAccounts/Index.cshtml` â€” add an Actions column with a "Change role" link.
- `ASI.Basecode.Resources/Constants/AuditActions.cs`, `ASI.Basecode.Resources/Messages/AuditDescriptions.cs`, `ASI.Basecode.Resources/Messages/Audit.resx` â€” add `user.role_assigned` and its template.
- `ASI.Basecode.Services/Services/ActivityLogWriter.cs:24-26` â€” add the `user.role_assigned` â†’ `users` entity-type guard.
- `ASI.Basecode.Data/Models/Lookups/LookupCodes.cs` â€” `RoleCode.Admin = 1`, `Officer = 2`, `Member = 3`; do not change.
- `ASI.Basecode.WebApp/Startup.DI.cs:40,50` â€” registration pattern; add the new service beside it.
- `ASI.Basecode.WebApp/Styles/app.css` â€” no new classes; do not rebuild the stylesheet.
- `tests/ASI.Basecode.IntegrationTests/AdminAccountsListTests.cs`, `AccountRequestDecisionTests.cs` â€” reuse the seed, sign-in, `TestActor`, `Task.WhenAll` patterns.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Models/RoleOption.cs` â€” role display record (`RoleCode RoleId`, `Code`, `Label`) â€” feeds the select without numeric ids.
- [x] `ASI.Basecode.Data/Interfaces/IAccountAdministrationRepository.cs`, `Repositories/AccountAdministrationRepository.cs` â€” account-by-id, roles, active-admin count, and `AcquireLastAdministratorLockAsync` (`sp_getapplock` `Exclusive`, `Transaction` owner, `@LockTimeout = 5000`; negative result throws `DomainException(OutcomeCode.Stale)`).
- [x] `ASI.Basecode.Services/ServiceModels/ChangeAccountRoleViewModel.cs` â€” account id, name, email, current role, selected role, reason (`[Required]`, `[StringLength(1000)]`).
- [x] `ASI.Basecode.Services/Interfaces/IAccountRoleService.cs`, `Services/AccountRoleService.cs` â€” `GetRoleChangeAsync`, `ChangeRoleAsync`; validate actor/role/reason; run discover â†’ app lock â†’ row lock â†’ recheck â†’ count â†’ write â†’ audit in one transaction.
- [x] `ASI.Basecode.Resources/Constants/AuditActions.cs`, `Messages/AuditDescriptions.cs`, `Messages/Audit.resx`, `ASI.Basecode.Services/Services/ActivityLogWriter.cs` â€” register `user.role_assigned` for `users`.
- [x] `ASI.Basecode.WebApp/Startup.DI.cs` â€” register `IAccountRoleService`.
- [x] `ASI.Basecode.WebApp/Controllers/AdminAccountsController.cs`, `Views/AdminAccounts/Index.cshtml`, `Views/AdminAccounts/ChangeRole.cshtml` â€” GET/POST role change with antiforgery, domain errors, success `TempData`.
- [x] `tests/ASI.Basecode.Services.Tests/AccountRoleServiceTests.cs` â€” validation, no-op, last-admin refusal with fakes.
- [x] `tests/ASI.Basecode.IntegrationTests/AdminRoleAssignmentTests.cs` â€” SQL lock, audit row, list badge, parallel demotions.

**Acceptance Criteria:**
- Given an Administrator and an active Member, when the role is set to Officer with a reason, then `users.role_id` is Officer, `updated_by`/`updated_at` are set, exactly one `user.role_assigned` row holds the old and new codes and the reason, and `/admin/accounts` shows the Officer badge.
- Given two active Administrators, when both are demoted in parallel, then exactly one commits, the other is refused, and one active Administrator remains.
- Given the only active Administrator, when demoted, then nothing is written and the message says the last active Administrator cannot be demoted.
- Given an Officer or anonymous caller, when they POST, then the policy refuses it.
- Given a blank or over-1000-character reason, when submitted, then the form redisplays with the field error and no write occurs.

## Implementation Notes

Implemented as planned. The role write lives in a new `AccountRoleService` beside 5.1's untouched read service. The app lock is a `DbCommand` on the ambient transaction calling `sp_getapplock` (`Exclusive`, `Transaction` owner, 5000 ms), and a negative return surfaces as `Stale`. The pre-lock read only decides whether the lock is needed; a post-lock re-read that reveals a missed demotion throws `Stale` instead of taking the lock out of order. `AccountAdministrationServiceTests` gained stub members for the widened repository interface, and one pre-existing nullable annotation in `MasterDataTransactionTests` was corrected so the solution builds with zero warnings.

## Plan Change Log

## Review Triage Log

**Pass 1 â€” quick lens (2026-11-06).** Verdicts: high 0, medium 0, low 1, false 10, maybe-false 0.

| # | Finding | Verdict | Route | Evidence |
|---|---|---|---|---|
| 1 | `AccountRoleService.cs:127-143` throws `Stale` after `LockUserAsync`, so the app lock follows the row lock | false | â€” | The branch throws without ever calling `AcquireLastAdministratorLockAsync`; only one lock is held, so there is no order to violate and the transaction waits on nothing while holding the row lock. |
| 2 | `AccountAdministrationRepository.cs:150-153` treats a null `sp_getapplock` return as success | false | â€” | `SqlClient` always populates a `ReturnValue` parameter after execution; the null path is unreachable with the only provider this app registers. |
| 3 | `AdminAccountsController.cs:75-78` refreshes the current-role badge but keeps the submitted select value | false | â€” | Refreshing "Current role" to the database truth while preserving the user's submitted selection is the intended behaviour; the comment only promises not to replace role or reason. |
| 4 | `ChangeAccountRoleViewModel.cs:269-271` `[StringLength(1000)]` validates the untrimmed reason while the service validates the trimmed one | low | patch | A 1001-character reason whose trimmed form is 1000 is rejected by MVC but would be accepted by the service, contradicting the "trimmed reason of 1â€“1000 characters" boundary. |
| 5 | `AdminRoleAssignmentTests.cs:1143-1149` `ResetAdministratorsAsync` mutates shared fixture state and makes tests order-dependent | false | â€” | The fixture creates a separate `AttendTa_IT_<guid>` database per test class and applies only the schema (no admin seed); every test seeds its own Administrator after the reset. |
| 6 | `AdminRoleAssignmentTests.cs:998` takes its antiforgery token from `/Home/Index`, coupling the test to the logout form | false | â€” | The token is present on every authenticated page today; the claim describes a hypothetical future layout change, not a defect in this change. |
| 7 | `AdminRoleAssignmentTests.cs:940-946` implicitly assumes the two parallel demotions target different rows | false | â€” | The test body passes `first.Id`/`second.Id` and `second.Id`/`first.Id`, so the targets are demonstrably distinct and the no-op path cannot trigger. |
| 8 | `AccountAdministrationRepository.cs:52-72` returns `null` from a non-nullable `Task<AccountListItem>` | false | â€” | Nullable is disabled in every project and `IUserRepository.GetByIdAsync` already returns null the same way; no warning or behaviour defect exists today. |
| 9 | `ChangeRole.cshtml:11` link generation is ambiguous because two actions share one route template | false | â€” | Both actions use the identical template, so generation resolves today and the rendered URL is asserted by the integration test. |
| 10 | `AdminAccountsController.cs:23-24` uses class-level `[AutoValidateAntiforgeryToken]` unlike the rest of the codebase | false | â€” | Five other controllers (`EventType`, `Member`, `Venue`, `ManagementAccountRequest`, `CertificateTracer`) already apply the attribute at class level. |
| 11 | The plan says to count active Administrators after the lock, but the code counts only for a demotion | false | â€” | The count only changes an outcome when an active Administrator is being demoted; counting on a promotion or an inactive-account change would not alter any result. |

## Design Notes

The pre-lock read only decides whether the app lock is needed. If the post-lock re-read shows an active Administrator demotion the pre-read did not, the row changed under us: throw `Stale` rather than take the app lock out of order. A 5-second `@LockTimeout` fails a stuck transaction fast instead of queueing every role write. A no-op returns success without an audit row, matching ADM-01's "no duplicate change logs". Self-demotion is allowed when another active Administrator remains; `OnValidatePrincipal` then signs the actor out next request, which is correct AD-8 behavior.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln` â€” expected: 0 warnings, 0 errors.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` â€” expected: all pass.
- With `ATTENDTA_TEST_SQL` set to a local SQL Server master connection: `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj --filter FullyQualifiedName~AdminRoleAssignmentTests` â€” expected: all pass, including the parallel demotion.

**Manual checks:**
- Sign in as an Administrator, open `/admin/accounts`, change a role with a reason, and confirm the badge and the Activity Log entry.
