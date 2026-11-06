---
title: 'Venue paged list with photo thumbnails and audited active toggle'
type: 'feature'
ticket: '4'
created: '2026-11-06'
status: 'built'
baseline_revision: 'b199c1de24a361b2b4de9461bbc6cbebdcd568cb'
route: 'full'
route_source: 'auto'
risk: 'low'
review: 'quick'
review_source: 'pinned'
lenses_ran: ['quick']
review_loop_iteration: 0
context:
  - '_bmad-output/initiative-attendta/tickets/epic-master-data/tickets/story-venue-paged-list-with-photo-thumbnails-active-toggle-and-tai.md'
  - '_bmad-output/initiative-attendta/tickets/epic-master-data/epic-master-data.md'
  - 'AGENTS.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The Venue list loads every row, has no shared pager or room-name/venue-id ordering, and the active toggle changes state without reading linked events or writing an Activity Log entry. The thumbnail also uses a Bootstrap class that the Tailwind shell does not ship, so photos are not reliably cropped.

**Approach:** Add a 25-row paged Venue read path ordered by room name then venue id, keep thumbnails on the authorized photo route, and make the toggle lock the venue row, count linked events, flip the state, and append a path-free status audit entry in the same transaction.

## Boundaries & Constraints

**Always:** Management policy and antiforgery on list and toggle; 25-row pages through `PageRequest`, `PagedResult` and `_Pager`; `RoomName` then `VenueId` ordering; photos only through the authorized `Photo` action; toggle locks with `LockVenueAsync` before reading linked events; audit old/new `IsActive` and `LinkedEventCount` in the same transaction; actor from `ICurrentActor`; UTC `TimeProvider`; preserve the current page after toggle; no image paths in audit or logs.

**Never:** Add search/filter, hard delete, event management, venue capacity changes, a new photo-upload flow, a status badge, or a new shell; change create/edit behavior; expose physical paths or serve private files statically.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| First page | 26 venues | 25 rows, `Page 1 of 2`, Next link | No error |
| Last page | 26 venues, `page=2` | 1 row, `Page 2 of 2`, Previous link | No error |
| Past end | `page=999999999` | Last page, no Next link | Clamped by `PageRequest` |
| Empty | 0 venues | Empty state, pager hidden | No error |
| Toggle | Active venue, 1 linked event | Inactive; audit `venue.status_changed` with `IsActive` true→false and `LinkedEventCount` 1 | No error |
| Toggle back | Inactive venue | Active; audit false→true | No error |
| Missing venue | Unknown id | 404 outcome, no audit | `OutcomeCode.NotFound` |
| Unauthorized | Anonymous or member | Redirect to login or access denied | Management policy |
| Concurrent edit | Venue row locked elsewhere | Toggle waits, then applies to the current row | Existing lock test |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Interfaces/IVenueRepository.cs`, `Repositories/VenueRepository.cs` — add `CountAsync`, `GetPageAsync(skip, take)` and `CountLinkedEventsAsync(venueId)`; use `AsNoTracking`, `OrderBy(RoomName).ThenBy(VenueId)`, `Skip/Take`; keep `LockVenueAsync` and `HasEventAboveCapacityAsync` unchanged.
- `ASI.Basecode.Services/Interfaces/IVenueService.cs`, `Services/VenueService.cs` — replace the unpaged `GetVenues()` read with `GetPageAsync(pageNumber)` using `PageRequest`/`PagedResult`; extend `ToggleActiveAsync` with `ICurrentActor` validation, linked-event count, `VenueStatusChanged` audit and `TimeProvider`; leave create/edit untouched.
- `ASI.Basecode.Resources/Constants/AuditActions.cs`, `Messages/AuditDescriptions.cs`, `Messages/Audit.resx` — add `VenueStatusChanged = "venue.status_changed"` and its description.
- `ASI.Basecode.Services/Services/ActivityLogWriter.cs` — allow the new action only for `venues`.
- `ASI.Basecode.WebApp/Controllers/VenueController.cs` — `Index(int? page)` calls `GetPageAsync`; `ToggleActive(int id, int? page)` redirects to the same page.
- `ASI.Basecode.WebApp/Views/Venue/Index.cshtml` — `PagedResult<VenueViewModel>`, `Model.Items`, `data-venue-id`, `_Pager`, hidden page on toggle, authorized photo `src`, empty state.
- `ASI.Basecode.WebApp/Styles/app.css`, `wwwroot/css/app.css` — add `.venue-thumb` (96×64, `object-fit: cover`, rounded) and rebuild compiled Tailwind.
- `tests/ASI.Basecode.IntegrationTests/VenuePagedListTests.cs` — HTTP tests for paging, ordering, past-end, thumbnails and Management access.
- `tests/ASI.Basecode.IntegrationTests/VenueToggleAuditTests.cs` — HTTP test for toggle, page preservation, linked events and the audit snapshot.
- `tests/ASI.Basecode.IntegrationTests/VenueTransactionPhotoTests.cs`, `MasterDataTransactionTests.cs` — extend the existing toggle-lock test with the audit assertion and update the two venue test doubles for the new interface members.

## Tasks & Acceptance

**Execution:**
- [x] `IVenueRepository.cs` / `VenueRepository.cs` — add count, page and linked-event queries — the list must page in SQL, not in memory.
- [x] `IVenueService.cs` / `VenueService.cs` — add the paged read and make toggle audited and actor-checked — the story's two behaviors live here.
- [x] `AuditActions.cs`, `AuditDescriptions.cs`, `Audit.resx`, `ActivityLogWriter.cs` — register the status action — the audit writer rejects unknown actions.
- [x] `VenueController.cs` — make `Index` async and preserve `page` on toggle — the pager and the redirect must agree.
- [x] `Views/Venue/Index.cshtml` — paged model, pager, row hook, thumbnail and hidden page — the UI and test hook.
- [x] `Styles/app.css` / `wwwroot/css/app.css` — add and compile `.venue-thumb` — the current `object-fit-cover` class does not exist.
- [x] `VenuePagedListTests.cs` / `VenueToggleAuditTests.cs` — cover the matrix and acceptance — prove the story end to end.
- [x] `VenueTransactionPhotoTests.cs` / `MasterDataTransactionTests.cs` — keep the concurrency test and doubles compiling — no regression in the 2.3 work.

**Acceptance Criteria:**
- Given 26 venues, when an Officer opens `/Venue`, then 25 rows show in room-name/venue-id order with a Next link and each thumbnail points at `/Venue/Photo/{id}`.
- Given `page=999999999`, when the list loads, then the last page shows and no Next link renders.
- Given an active venue with a linked event, when an Officer toggles it, then it becomes inactive, the linked event remains, and one `venue.status_changed` audit entry records `IsActive` true→false and `LinkedEventCount` 1 with no image path.
- Given a member or anonymous request, when they open `/Venue` or POST toggle, then the Management policy refuses access.
- Given a venue row locked by another transaction, when toggle runs, then it waits and applies to the current row.

## Implementation Notes

Implemented the paged repository/service read, the audited active toggle, the controller page preservation, the paged Tailwind view, the `.venue-thumb` component and both HTTP integration tests. The build succeeds; the one nullable warning is pre-existing in `MasterDataTransactionTests`. Before the rebase the service suite was 52/52 and the integration suite 116/117. After rebasing onto dev `829e1a5`, the service suite is 66/66 and the integration suite is 125/130; the five failures are all `CertificateTracerFlowTests` because Playwright Chromium is not installed in this environment.

Two route facts were confirmed during implementation. First, `/Venue` needed an explicit attribute route because the app's default route has no action default; `/Venue/Index` is kept as an alias, and the toggle redirect uses the named `VenueIndex` route so it returns to `/Venue?page=N`. Second, the authorized photo route is `/Venue/Photo?id={id}` because the default route has no `{id?}` segment; the plan's `/Venue/Photo/{id}` is shorthand for the same authorized action. `IVenueRepository.GetVenues()` is retained but is now unused by production code; the refactor sweep can remove it.

## Plan Change Log

## Review Triage Log

Quick review (2026-11-06): high 0, medium 0, low 5, false 2, maybe-false 0.

| Finding | Verdict | Route | Evidence |
|---|---|---|---|
| Photo URL uses `?id=` rather than `/{id}` | false | rejected | `VenueController.Photo` is the authorized action; the app's default route has no `{id?}` segment, so `?id=` is the real route and the intent (authorized photo route) is met. |
| Unknown venue returns a redirect, not HTTP 404 | false | rejected | `ToggleActiveAsync` throws `OutcomeCode.NotFound`; the controller maps domain errors to TempData plus redirect, the established Master Data toggle pattern. The matrix's "404 outcome" is the service outcome code. |
| Count and page are separate round trips | low | rejected | Real but unlikely under the low-traffic pager; the shared pager uses the same count-then-page pattern, and a snapshot fix adds complexity beyond this story. |
| Ordering test uses an ordinal comparer | low | rejected | The test proves `OrderBy(RoomName).ThenBy(VenueId)` and the tie-break for the seeded data; collation-specific coverage is not required by the story. |
| Thumbnail class checked once, not per row | low | patch | Moved the `venue-thumb` assertion into the per-row loop so every row's thumbnail is checked. |
| Unauthorized POSTs omit the antiforgery token | low | rejected | The tests verify the Management policy outcome; authorization short-circuits before antiforgery, and the current behavior is correct. |
| "Active" also matches the Deactivate button | low | patch | Changed the assertion to the exact status cell (`<td>Active</td>` and `<td>Inactive</td>`). |

## Design Notes

- Reuse the 1.6 pager contract; do not add a second paging mechanism.
- Count all linked events, not only draft/published, because deactivation keeps every existing reference; the capacity guard stays draft/published-only.
- The status audit mirrors the Event Type status shape: `IsActive` and `LinkedEventCount`; no room name or image path.
- Keep the existing Tailwind component classes; add only `.venue-thumb` to avoid a broad restyle.
- `ToggleActiveAsync` must validate `ICurrentActor.UserId == actorUserId`, matching `ValidateWrite`; the controller still passes `GetActorUserId()`.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln` — expected: success, zero errors.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: all pass.
- Set `ATTENDTA_TEST_SQL` to a local SQL Server master connection string, then `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — expected: all pass, including the new Venue tests.
- `npm --prefix ASI.Basecode.WebApp run css:build` — expected: success; commit the rebuilt `wwwroot/css/app.css`.
- `git diff --exit-code -- ASI.Basecode.WebApp/wwwroot/css/app.css` — expected: no diff after the rebuild (CI parity).

**Manual checks:**
- Sign in as an Officer, open `/Venue`, page through, toggle a venue with a linked event, confirm the notice and the Activity Log row, and open a thumbnail in a new tab.
