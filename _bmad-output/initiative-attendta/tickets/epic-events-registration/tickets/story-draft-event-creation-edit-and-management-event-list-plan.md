---
title: 'Draft event creation, edit, and management event list'
type: 'feature'
ticket: '1'
created: '2026-11-06'
status: 'built'
baseline_revision: '3a9dc287b3ec3120ba1b7a682a5534b215680277'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - 'AGENTS.md'
  - '_bmad-output/initiative-attendta/tickets/epic-events-registration/epic-events-registration.md'
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Management cannot create or edit draft Events or see them on a paged list, so the event lifecycle cannot start.

**Approach:** Add the first Event slice: `IEventRepository` with the AD-4 event lock, `IEventLifecycleService` draft create/edit under the type → venue → event lock order, a paged management list, Tailwind views, and an `event.updated` audit entry on edits.

## Boundaries & Constraints

**Always:** Drafts only. Validate the locked Event Type and Venue are active, the limit is positive and at most the Venue capacity, and the Signup Deadline is no later than the start. Store UTC via the injected `TimeProvider`; show Asia/Manila only in WebApp. Require the Management policy. Append `event.updated` with explicit old/new field subsets on edits. Page at 25 with a unique-id tiebreak. Reuse the existing repository/service/controller/view, `_Pager`, and `ManilaTimeFormatter` patterns.

**Never:** Publish, edit a Published Event, cancel, complete, upload a Cover, build the Member feed, or touch Registrations; those are 3.2–3.6. No schema change, hard delete, `DateTime.Now`/`UtcNow` in Services, or timezone conversion in Services. Do not audit creation; the story names only `event.updated` on edits.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Create draft | Active type and venue, limit 1..capacity, deadline <= start | Draft row with CreatedBy/UTC; appears on the list | No error |
| Inactive type or venue | Inactive reference | No row written | `Invalid` on TypeId or VenueId |
| Bad limit | 0, negative, or above venue capacity | No row written | `Invalid` on ParticipantLimit |
| Bad deadline | Deadline after start | No row written | `Invalid` on RegistrationDeadline |
| Edit draft title | Draft event | Row updated; one `event.updated` with old/new values | No error |
| Edit non-draft | Published/Completed/Cancelled | No change | `Invalid` |
| Non-management | Anonymous or Member request | No list or draft visible | Redirect to login/AccessDenied |
| Paging | 26 events; page 0, 999, null | 25 then 1; page 1; last page | No error |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Models/Event.cs`, `CanonicalSchemaMappings.cs` — existing entity/mapping; no change.
- `ASI.Basecode.Data/Repositories/VenueRepository.cs`, `EventTypeRepository.cs` — copy the `UPDLOCK, HOLDLOCK, ROWLOCK` lock pattern.
- `ASI.Basecode.Services/Services/VenueService.cs`, `EventTypeService.cs` — service shape: actor check, `DomainException`, audit, save.
- `ASI.Basecode.Services/Services/ActivityLogWriter.cs`, `Resources/Constants/AuditActions.cs`, `Messages/Audit.resx`, `Messages/AuditDescriptions.cs` — add `event.updated`.
- `ASI.Basecode.WebApp/Controllers/VenueController.cs`, `EventTypeController.cs` — controller shape.
- `ASI.Basecode.WebApp/Views/Venue/Index.cshtml`, `_Form.cshtml`, `Views/Shared/_Pager.cshtml`, `_Header.cshtml` — view patterns and nav.
- `ASI.Basecode.WebApp/Formatting/ManilaTimeFormatter.cs` — display; add local→UTC input conversion.
- `ASI.Basecode.WebApp/Startup.DI.cs`, `Startup.AutoMapper.cs` — registration and maps.
- `tests/.../EventTypeTimestampTests.cs`, `EventTypeManagementTests.cs`, `EventTypeManagementFlowTests.cs`, `SqlServerDatabaseFixture.cs` — test patterns.

## Tasks & Acceptance

**Execution:**
- [x] `ASI.Basecode.Data/Models/EventListItem.cs` — add the list projection (event, type name, venue name, times, limit, status).
- [x] `ASI.Basecode.Data/Interfaces/IEventRepository.cs`, `Repositories/EventRepository.cs` — add count/page/get/lock/add/update; page joins types and venues, no tracking, `starts_at ASC, event_id ASC`.
- [x] `ASI.Basecode.Services/ServiceModels/EventViewModel.cs`, `EventFormOptions.cs` — add the form/list model and active option records with annotations.
- [x] `ASI.Basecode.Services/Interfaces/IEventLifecycleService.cs`, `Services/EventLifecycleService.cs` — add `GetPageAsync`, `GetEventAsync`, `GetFormOptionsAsync`, `CreateDraftAsync`, `UpdateDraftAsync`; validate under the locks; audit edits.
- [x] `ASI.Basecode.Resources/Constants/AuditActions.cs`, `Messages/Audit.resx`, `Messages/AuditDescriptions.cs`, `ASI.Basecode.Services/Services/ActivityLogWriter.cs` — add `event.updated` and the `events` guard.
- [x] `ASI.Basecode.WebApp/Controllers/EventController.cs` — add Management-only Index/Create/Edit GET+POST; convert posted Asia/Manila times to UTC.
- [x] `ASI.Basecode.WebApp/Views/Event/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `_Form.cshtml` — add the paged list and draft form with type/venue selects and local-time labels.
- [x] `ASI.Basecode.WebApp/Views/Shared/_Header.cshtml` — add the Events link for management.
- [x] `ASI.Basecode.WebApp/Formatting/ManilaTimeFormatter.cs` — add `ToUtc(DateTime)` for form input.
- [x] `ASI.Basecode.WebApp/Startup.DI.cs`, `Startup.AutoMapper.cs` — register `IEventRepository`/`IEventLifecycleService` and the maps.
- [x] `tests/ASI.Basecode.Services.Tests/EventLifecycleServiceTests.cs` — cover the matrix with fakes.
- [x] `tests/ASI.Basecode.IntegrationTests/EventDraftFlowTests.cs` — cover SQL persistence, paging, lock order, audit, and the Management policy.

**Acceptance Criteria:**
- Given an officer on the management Events page, when they create a draft with an active type and venue, then it appears on the paged list and is stored as Draft.
- Given a draft, when the officer edits its title, then the row changes and one `event.updated` entry records the old and new title.
- Given an inactive type or venue, when the officer submits create or edit, then the form shows the field error and nothing is written.
- Given a Member, when they request `/Event`, then they are denied and no draft is shown.
- Given 26 events, when the officer pages the list, then 25 rows show first and the last row on page 2, ordered by start then id.

## Implementation Notes

- `dotnet build` — succeeded with 0 warnings and 0 errors.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — 115/115 passed.
- Integration suite — 170/175 passed. The 5 failures are all `CertificateTracerFlowTests` with `PlaywrightException: Executable doesn't exist at ...chromium_headless_shell-1248...`; the same failure reproduces at baseline `3a9dc28` in a clean worktree, so it is an environment gap, not this change. The 13 new Event/Manila integration tests pass.
- HITL manual checks remain for the user: create a draft, see it on `/Event`, edit the title, confirm the `event.updated` log, try an inactive type/venue, and confirm a Member is denied.
- Quick review patches: field-less failures now use `HandleDomainException` (AD-9), the Edit GET rejects non-drafts and reports a missing event, and the integration flow now seeds inactive references and covers the edit-path inactive cases. Re-ran `dotnet build` (0 warnings/0 errors), the service suite (115/115), and the full integration suite (170/175; the same 5 Playwright `CertificateTracerFlowTests` environment failures remain).

## Plan Change Log

## Review Triage Log

| Finding | Verdict | Route | Evidence / action |
|---|---|---|---|
| Service read methods do not repeat the Management role check (AD-8) | false | — | The only caller is `EventController`, which is `[Authorize(Policy = Policies.Management)]`; no member-facing caller exists in this change, and the write paths check the actor. A future 3.5 member feed must add its own guard. |
| `HandleAsync` bypasses the AD-9 outcome mapping for field-less failures | low | patch | Real but no AJAX caller exists in this UI. Fixed by routing the field-less branch through `HandleDomainException(exception, model, nameof(Index))`. |
| `Edit` GET renders the editable form for a non-draft event | low | patch | UI/defence-in-depth gap. Fixed by redirecting published/completed/cancelled events to Index with an error. |
| Audit snapshot performs synchronous repository reads inside the async transaction | low | rejected | Minor lock-window cost; the fix adds public async surface, and the existing master-data services use the same sync reads inside their transactions. |
| Inactive-reference criterion is only exercised on create | low | patch | Test gap. Added edit-path cases for an inactive type and venue, asserting the field error and no change. |
| Inactive-option assertions are vacuous because the flow test never seeds inactive references | low | patch | Test-quality gap. Seeded inactive references in the flow test so the assertions are meaningful. |
| Successful create/edit redirects drop the list page | low | rejected | Minor UX; the fix adds page plumbing the story does not require. |
| A missing event on `Edit` GET redirects silently | low | patch | Minor UX. Fixed by setting `TempData["ErrorMessage"] = "Event not found."` before redirecting. |

## Design Notes

- Lock order: create locks type then venue, then inserts. Edit reads the event to discover it, locks the posted type then venue, then locks the event and rechecks Draft. This follows AD-4 and avoids a type/venue change racing a deactivation.
- Audit payload: a private record with Title, Description, TypeId, TypeName, VenueId, VenueName, StartsAt, RegistrationDeadline, ParticipantLimit, Status. No `image_path` (AD-7).
- List order: `starts_at ASC, event_id ASC` (soonest first) with the unique-id tiebreak required by AD-13.
- Time input: `datetime-local` values are Asia/Manila; the controller converts them to UTC, so Services never converts time zones (AD-11).
- Description is optional because `events.description` is nullable and the story's validation list names only title, type, venue, start, limit, and deadline.
- "Hidden from members" means the management list is Management-only; the Member feed arrives in 3.5.

## Verification

**Commands:**
- `dotnet build` — expected: success, no errors.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: all pass.
- `$env:ATTENDTA_TEST_SQL="Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True"; dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — expected: all pass.

**Manual checks (HITL):**
- Sign in as the seeded Administrator (allowed by Management) or an officer; create a draft with an active type/venue; confirm it appears on `/Event`; edit the title; confirm the Activity Log shows `event.updated`; try an inactive type/venue; sign in as a Member and confirm `/Event` is denied.
