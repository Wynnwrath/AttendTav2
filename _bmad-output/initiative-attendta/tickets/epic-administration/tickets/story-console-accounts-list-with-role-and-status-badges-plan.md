---
title: 'Console accounts list with role and status badges'
type: 'feature'
ticket: '1'
created: '2026-11-06'
status: 'built'
baseline_revision: 'b199c1de24a361b2b4de9461bbc6cbebdcd568cb'
route: 'full'
route_source: 'auto'
risk: 'low'
review: ''
review_source: ''
lenses_ran: []
review_loop_iteration: 0
context:
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Administrators have no console to review accounts, their roles, and whether access is active, so they cannot see who can use the system.

**Approach:** Add an Administrator-only Accounts & Roles list at `/admin/accounts` that reads `users` with a `roles` join, supports name/email search, and pages 25 rows with the shared `_Pager`, showing role and status badges.

## Boundaries & Constraints

**Always:** Require the `Administrator` policy; apply search before counting and paging; order by full name with the numeric account ID as the tie-break; show name, email, role label, and active status; use the shared pager and empty state; preserve the search value in pager links; never expose password hashes.

**Never:** Add role-change, deactivate, reactivate, or delete actions (owned by 5.2–5.5); add account creation or editing; change the schema, authentication, or other modules; render password hashes or other secrets.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Multiple pages | 26+ accounts | 25 rows on page 1, remainder on page 2; ordered by name, then account ID | No error |
| Search | Name or email fragment | Count and rows reflect the filter; pager keeps the search | No error |
| Page normalization | Missing, zero, negative, or excessive `page` | Page 1 for invalid; last page for out-of-range | No error |
| Empty result | No matching accounts | Empty-state message, no pager | No error |
| Badges | Active/inactive account with admin/officer/member role | Correct role label and active/inactive badge | No error |
| Access | Anonymous / Officer / Administrator | Login redirect / access denied / page renders | No error |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Models/User.cs`, `Models/Lookups/RoleLookup.cs`, `CanonicalSchemaMappings.cs` — existing `users`/`roles` mapping; do not change schema or mappings.
- `ASI.Basecode.Data/Models/AccountRequestQueueItem.cs`, `Repositories/AccountRequestRepository.cs` — pattern for a flat display read model and server-side paged query; reuse the style.
- `ASI.Basecode.Services/ServiceModels/PageRequest.cs`, `PagedResult.cs`, `IPagedResult.cs` — reuse fixed 25-row paging and normalization.
- `ASI.Basecode.WebApp/Views/AccountRequest/Index.cshtml`, `Views/Shared/_Pager.cshtml` — reuse table, search, empty-state, and pager markup.
- `ASI.Basecode.WebApp/Views/Shared/_Header.cshtml` — add an Administrator-only Accounts link.
- `ASI.Basecode.WebApp/Styles/app.css`, `wwwroot/css/app.css` — add badge classes and rebuild the compiled stylesheet.
- `ASI.Basecode.WebApp/Startup.DI.cs` — register the new repository and service.
- `docs/implementation-plans/ADM-01_Independent_Backend_And_Integration_Plan.md` — historical `/admin/accounts` route and account-summary shape; do not implement its mutation scope.
- `tests/ASI.Basecode.IntegrationTests/AccountRequestQueueTests.cs`, `AttendTaWebApplicationFactory.cs`, `SqlServerDatabaseFixture.cs`, `AuthenticationFlowTests.cs` — reuse authenticated HTTP and isolated SQL patterns.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Models/AccountListItem.cs` — add a display read model with account ID, full name, email, role code/label, and active flag.
- [x] `ASI.Basecode.Data/Interfaces/IAccountAdministrationRepository.cs`, `Repositories/AccountAdministrationRepository.cs` — add filtered count and paged query; join `roles`; order by `full_name`, then `user_id`; `AsNoTracking`; never select `password_hash`.
- [x] `ASI.Basecode.Services/ServiceModels/AccountSummaryViewModel.cs`, `Interfaces/IAccountAdministrationService.cs`, `Services/AccountAdministrationService.cs` — normalize the page, fetch one page, map display fields.
- [x] `ASI.Basecode.WebApp/Controllers/AdminAccountsController.cs` — add `[Authorize(Policy = Policies.Administrator)]` GET at `/admin/accounts` with `search` and `page`.
- [x] `ASI.Basecode.WebApp/Views/AdminAccounts/Index.cshtml` — render the search form, read-only table, role/status badges, empty state, and `_Pager`.
- [x] `ASI.Basecode.WebApp/Views/Shared/_Header.cshtml` — add the Administrator-only Accounts link in mobile and desktop navigation.
- [x] `ASI.Basecode.WebApp/Styles/app.css`, `wwwroot/css/app.css` — add badge styles and rebuild the compiled CSS.
- [x] `ASI.Basecode.WebApp/Startup.DI.cs` — register `IAccountAdministrationRepository` and `IAccountAdministrationService`.
- [x] `tests/ASI.Basecode.Services.Tests/AccountAdministrationServiceTests.cs` — cover page normalization, search pass-through, and field mapping with a fake repository.
- [x] `tests/ASI.Basecode.IntegrationTests/AdminAccountsListTests.cs` — seed 26 accounts, verify 25-row pages, search, badges, paging, no hashes, and Officer/anonymous refusal.

**Acceptance Criteria:**
- Given 26+ accounts, when an Administrator opens `/admin/accounts`, then page 1 shows 25 rows and page 2 shows the remainder, ordered by name, then account ID.
- Given a name or email search, when the list renders, then only matching accounts are counted and shown, and the pager keeps the search.
- Given each row, when rendered, then it shows the correct role label and active/inactive badge.
- Given an invalid or out-of-range page, when requested, then page 1 or the last page is shown.
- Given an Officer or anonymous request, when they request `/admin/accounts`, then access is refused by the existing policy.
- Given the rendered page, when inspected, then no password hash is present.

## Implementation Notes

Implemented the Administrator-only `/admin/accounts` page with a safe account read model, a filtered `users`/`roles` query, shared 25-row paging, name/email search, role and status badges, an empty state, and an Administrator-only navigation link. No schema, authentication, or mutation changes were made; role and access actions remain deferred to 5.2–5.5.

Verification: `ASI.Basecode.WebApp` builds with 0 warnings/errors; the Services test project passes 65/65; the new integration test class passes 1/1. The compiled Tailwind stylesheet was rebuilt and contains the badge classes.

## Plan Change Log

## Review Triage Log

## Design Notes

Search is a trimmed, case-insensitive `Contains` over `full_name` or `email`; SQL Server's collation handles case. The role label comes from `roles.label`; the lowercase `roles.code` drives the badge class. Order by `users.full_name`, then `users.user_id`, so the unique ID is the tie-break and the list is alphabetical like the member directory. No action links are added in 5.1: role assignment and deactivation belong to 5.2–5.5, and the story's `covers: [R6]` and Verify cover the paged list only. Badge classes: `.badge`, `.badge-admin`, `.badge-officer`, `.badge-member`, `.badge-active`, `.badge-inactive`.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln` — solution builds.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — service tests pass.
- With `ATTENDTA_TEST_SQL` set to a local SQL Server master connection, `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — SQL and HTTP tests pass.
- In `ASI.Basecode.WebApp`, `npm.cmd run css:build` — compiled Tailwind stylesheet includes the badge classes.

**Manual checks:**
- Sign in as an Administrator, open `/admin/accounts`, and inspect search, paging, badges, empty state, and desktop/mobile layout.
