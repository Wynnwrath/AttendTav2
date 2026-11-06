---
title: 'Member read-only self profile with ownership authorization'
type: 'feature'
ticket: '8'
created: '2026-11-05'
status: 'built'
baseline_revision: '3007d4800972c4b1b357a92ee2fc42dd76feeb43'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: [quick]
review_loop_iteration: 0
context:
  - '{project-root}/AGENTS.md'
  - '{project-root}/_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
  - '{project-root}/_bmad-output/initiative-attendta/ux-attendta/EXPERIENCE.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Members need to see the account, academic and membership details linked to their own login, while existing member-management routes remain unavailable to them.

**Approach:** Add a read-only Member profile page backed by a lookup on the authenticated actor's `users.user_id`. Keep the management controller's Management authorization and render the profile with no member identifier input or editable controls.

**Decision (2026-11-05):** Fast-forward the feature branch to `origin/dev` before implementation so the profile uses the completed Tailwind/role-routed shell and current master-data contracts.

## Boundaries & Constraints

**Always:** Use `ICurrentActor.UserId` as the sole profile owner key; preserve the service → repository layering; restrict the self-profile endpoint to the Member role; use the generic access-denied behavior for Member requests to Management routes; expose only account, academic and membership details.

**Never:** Accept a user/member ID to select the self-profile; reuse the edit form; add self-service writes or membership-status mutation; change the schema or management authorization policy.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Member profile | Authenticated Member requests `/Profile` | Show only the profile linked to the authenticated user, including account, academic and membership fields; no editable controls | No error expected |
| Missing profile | Authenticated Member has no linked member row | No other profile or member directory is returned | Return a generic not-found response |
| Management route | Member requests `/Member` or `/Member/Edit/{id}` | Generic Access Denied outcome, regardless of requested ID | Existing cookie authorization redirects to `/Account/AccessDenied` |
| Anonymous profile | No authenticated cookie requests `/Profile` | Redirect to sign-in with return URL | Existing cookie authentication behavior |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Interfaces/IMemberRepository.cs` and `Repositories/MemberRepository.cs` -- existing `GetMemberRecords()` joins `members` to `users`; add a single-row projection filtered by linked `UserId`, not `MemberId`.
- `ASI.Basecode.Services/Interfaces/IMemberService.cs` and `Services/MemberService.cs` -- existing `ToViewModel` projects every required display field. Inject `ICurrentActor` and expose an actor-scoped self-profile read; do not change existing management writes.
- `ASI.Basecode.Services/Interfaces/ICurrentActor.cs` -- authenticated numeric `users.user_id` source; its `UserId` throws for anonymous calls.
- `ASI.Basecode.WebApp/Controllers/MemberController.cs` -- class-level `Policies.Management` protects directory/edit/status endpoints. Keep this policy intact; put the Member page in a separate controller.
- `ASI.Basecode.WebApp/Startup.Auth.cs`, `Resources/Constants/AuthConstants.cs`, and `Views/Account/AccessDenied.cshtml` -- cookie fallback, Member role code, and existing generic forbidden behavior.
- `ASI.Basecode.WebApp/Views/Shared/_Header.cshtml` -- role-routed navigation uses the Tailwind shell; add the Profile link for Members in its appropriate section.
- `tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs` -- real login and policy assertions; `MemberRepositoryTests.cs` and `SqlServerDatabaseFixture.cs` show member seeding/projection and disposable SQL setup.
- Feature branch is fast-forwarded to `origin/dev` at `3007d4800972c4b1b357a92ee2fc42dd76feeb43`, including prerequisite #12's Tailwind/role-routed shell and the current transactional master-data contracts.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Interfaces/IMemberRepository.cs` and `ASI.Basecode.Data/Repositories/MemberRepository.cs` -- add a lookup by linked user ID using the existing no-tracking account/member projection -- prevents confusing `user_id` with `member_id`.
- [x] `ASI.Basecode.Services/Interfaces/IMemberService.cs` and `ASI.Basecode.Services/Services/MemberService.cs` -- inject `ICurrentActor`, add a no-argument self-profile use case and handle missing profile -- prevents request-controlled ownership.
- [x] `ASI.Basecode.WebApp/Controllers/ProfileController.cs` -- add a Member-authorized GET endpoint without an ID parameter -- keeps Member access separate from Management endpoints.
- [x] `ASI.Basecode.WebApp/Views/Profile/Index.cshtml` -- show profile fields as read-only text with semantic labels and no form controls -- satisfies the self-service read-only requirement.
- [x] `ASI.Basecode.WebApp/Views/Shared/_Header.cshtml` -- expose the Profile link to Members within the approved shell -- makes the page reachable without broad navigation changes.
- [x] `tests/ASI.Basecode.IntegrationTests/AuthenticationFlowTests.cs` -- seed linked profiles and verify owner scoping, read-only output, anonymous redirect, missing profile, and Member denial on management routes -- demonstrates the end-to-end authorization boundary.

**Acceptance Criteria:**
- Given two active Members with different linked profiles, when either requests `/Profile`, then the response shows only that Member's account, academic and membership details and contains no editable form controls.
- Given a Member requests `/Member` or `/Member/Edit/{anotherMemberId}`, when authorization runs, then the response follows the existing generic `/Account/AccessDenied` flow.
- Given an anonymous user requests `/Profile`, when authentication runs, then the user is redirected to sign-in with `/Profile` as the return URL.
- Given an authenticated Member has no linked profile, when `/Profile` is requested, then the response is not-found and reveals no other member data.

## Implementation Notes

- Added the owner-scoped repository query and a self-profile service method that uses only `ICurrentActor.UserId`, rejects non-Member actors, and reports a missing profile as NotFound.
- Added a Member-only `/Profile` GET page with semantic read-only labels and a Member-only navigation link. Added the explicit `/Member/Edit/{id:int}` route so the existing Management authorization handles the path specified in the acceptance criteria.
- Expanded SQL integration coverage for owner separation, read-only rendering, missing profiles, anonymous requests, and management-route denial.

## Plan Change Log

## Review Triage Log

## Design Notes

The endpoint must not reuse `MemberController`: its class-level Management policy would also deny Members, while relaxing it would expose the directory and edit actions. A separate Member-role controller maintains both boundaries.

## Verification

**Commands:**
- `dotnet build` -- expected: all solution projects compile.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` -- expected: pass.
- `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` with `ATTENDTA_TEST_SQL` set to a local SQL Server `master` connection -- expected: profile ownership and authorization integration checks pass.
