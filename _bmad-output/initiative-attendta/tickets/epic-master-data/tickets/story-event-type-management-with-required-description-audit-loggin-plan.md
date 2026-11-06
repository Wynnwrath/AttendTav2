---
title: 'Event Type management with required description, audit logging, and paged Tailwind list'
type: 'feature'
ticket: '1'
created: '2026-11-06'
status: 'in-progress'
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
  - '_bmad-output/initiative-attendta/architecture-attendta/architecture-attendta.md'
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** Event Types are managed today without an audit trail, row locking, paging, or the shared actor and outcome contracts; the list is unbounded and the service trusts a controller-supplied actor id.

**Approach:** Bring Event Type create, edit and active toggle to the canonical pattern: lock the `event_types` row, read the actor from `ICurrentActor`, stamp from `TimeProvider`, append old/new audit rows, map `DomainException` to field errors, and page the Tailwind list with the shared 25-row pager ordered by name and id.

## Boundaries & Constraints

**Always:** Keep the Management policy and antiforgery on every mutation. Trim and require a unique name and a non-blank description. No hard deletes. Write the audit row inside the same transaction as the change. Store UTC times from `TimeProvider`. On deactivation take the `event_types` row lock, then read linked events and keep their references. Order by name then `type_id`; page size 25; invalid page shows 1, past the end shows the last page. Keep the existing Tailwind components and shared pager.

**Never:** Change the schema, add an EF migration, or alter Venue, Member, Auth or the shared paging contracts. Do not add an unbounded list path, a second pager, Bootstrap/CDN assets, hard deletes, entity or sensitive audit payloads, or a controller-supplied actor id. Do not refuse deactivation because linked events exist.

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|---|---|---|---|
| Create | Valid trimmed name and description | Row saved; `event_type.created` audit with new values | No error |
| Duplicate name | Existing name, any whitespace | No row saved | `Duplicate` on `TypeName` |
| Blank description | Whitespace only | No row saved | `Invalid` on `Description` |
| Edit | Valid name and description | Row updated; `event_type.edited` audit with old/new values | No error |
| Toggle off | Active type with a linked draft Event | Type inactive; Event keeps `type_id`; `event_type.status_changed` audit with old/new state and linked count | No error |
| Missing record | Unknown id | No change | `NotFound` |
| No actor | Anonymous or invalid actor | No change | `Forbidden` |
| Paging | 26 rows; page 1, 2, 999, 0 | 25 then 1; 999 shows last; 0 shows first | No error |

</frozen-after-approval>

## Code Map

- `ASI.Basecode.Data/Interfaces/IEventTypeRepository.cs` — replace `GetEventTypes()` with `CountEventTypesAsync`, `GetEventTypesPageAsync(offset, pageSize, ct)`, `LockEventTypeAsync(id, ct)` and `CountLinkedEventsAsync(id, ct)`.
- `ASI.Basecode.Data/Repositories/EventTypeRepository.cs` — implement the paged/count reads with `OrderBy(TypeName).ThenBy(TypeId)`; copy the `UPDLOCK, HOLDLOCK, ROWLOCK` pattern from `VenueRepository.LockVenueAsync`; count `events` by `type_id`.
- `ASI.Basecode.Services/Interfaces/IEventTypeService.cs` — `GetPageAsync(int? page, ct)`; remove the `actorUserId` parameters.
- `ASI.Basecode.Services/Services/EventTypeService.cs` — inject `IActivityLogWriter` and `ICurrentActor`; use `ICurrentActor.UserId` for `CreatedBy`/`UpdatedBy`; lock on update/toggle; read linked events on deactivation; append audit snapshots; keep `DomainException` fields.
- `ASI.Basecode.Resources/Constants/AuditActions.cs` — add `EventTypeCreated`, `EventTypeEdited`, `EventTypeStatusChanged`.
- `ASI.Basecode.Resources/Messages/AuditDescriptions.cs` and `Audit.resx` — add the three templates.
- `ASI.Basecode.Services/Services/ActivityLogWriter.cs` — allow `event_types` for the new actions.
- `ASI.Basecode.WebApp/Controllers/EventTypeController.cs` — async `Index(int? page)`; drop `GetActorUserId()`; use `HandleDomainException` for Create/Edit; keep POST-redirect-GET for toggle.
- `ASI.Basecode.WebApp/Views/EventType/Index.cshtml` — `PagedResult<EventTypeViewModel>`, `Model.Items`, `data-type-id`, `_Pager`, empty state. Create/Edit/_Form already render the fields and validation.
- `tests/ASI.Basecode.Services.Tests/EventTypeTimestampTests.cs` — update the constructor and fakes; assert audit and clock.
- `tests/ASI.Basecode.IntegrationTests/MasterDataTransactionTests.cs` — update the Event Type constructor and fake; keep the rollback/outcome tests.
- `tests/ASI.Basecode.IntegrationTests/EventTypeManagementTests.cs` — new: audit old/new, linked-event deactivation, lock wait, paging.
- `tests/ASI.Basecode.IntegrationTests/EventTypeManagementFlowTests.cs` — new: Management sign-in, Tailwind paged list, pager links, field errors.

## Tasks & Acceptance

**Execution:**
- [ ] `ASI.Basecode.Data/Interfaces/IEventTypeRepository.cs`, `Repositories/EventTypeRepository.cs` — add count/page/lock/linked-count methods; remove the unbounded `GetEventTypes()`.
- [ ] `ASI.Basecode.Resources/Constants/AuditActions.cs`, `Messages/AuditDescriptions.cs`, `Messages/Audit.resx`, `ASI.Basecode.Services/Services/ActivityLogWriter.cs` — register the three event-type actions and their `event_types` entity pair.
- [ ] `ASI.Basecode.Services/Interfaces/IEventTypeService.cs`, `Services/EventTypeService.cs` — paged read; actor from `ICurrentActor`; lock and linked-event read; audit snapshots; `TimeProvider`; `DomainException` fields.
- [ ] `ASI.Basecode.WebApp/Controllers/EventTypeController.cs` — async paged `Index`; `HandleDomainException`; remove `GetActorUserId`.
- [ ] `ASI.Basecode.WebApp/Views/EventType/Index.cshtml` — paged model, `Model.Items`, `data-type-id`, `_Pager`, empty state.
- [ ] `tests/ASI.Basecode.Services.Tests/EventTypeTimestampTests.cs` — update fakes/signatures; assert create/edit/toggle audit and clock.
- [ ] `tests/ASI.Basecode.IntegrationTests/MasterDataTransactionTests.cs` — update the Event Type constructor/fake; keep rollback and outcome assertions.
- [ ] `tests/ASI.Basecode.IntegrationTests/EventTypeManagementTests.cs` — audit old/new, deactivation keeps linked events and records the count, lock waits, 25-row paging.
- [ ] `tests/ASI.Basecode.IntegrationTests/EventTypeManagementFlowTests.cs` — sign in as Management, Tailwind list, pager links, duplicate/blank field errors, toggle audit.

**Acceptance Criteria:**
- Given an Officer on the Management policy, when they create an Event Type with a name and description, then the row is saved and `activity_logs` gains an `event_type.created` row with the new values.
- Given an existing name or a whitespace description, when the form is posted, then the form redraws with the error on `TypeName` or `Description` and no row is written.
- Given an Event Type with a linked draft Event, when an Officer deactivates it, then the type is inactive, the Event keeps its `type_id`, and the audit row records the old/new state and linked count.
- Given 26 Event Types, when the Officer opens `/EventType` and page 2, then page 1 shows 25 rows in name/id order and page 2 shows 1 row; a page past the end shows the last page.
- Given a second connection holding the `event_types` row lock, when a toggle is attempted, then it waits until the lock is released and then applies.

## Implementation Notes

## Plan Change Log

## Review Triage Log

## Design Notes

- The lock method mirrors `VenueRepository.LockVenueAsync`; it requires an open transaction and returns the tracked row or null.
- Audit payloads are explicit records (`TypeName`, `Description`, `IsActive`; `IsActive`, `LinkedEventCount`), never entities. The status action is one `event_type.status_changed` with old/new values.
- `GetEventTypes()` is replaced, not kept: the only list path is the 25-row page. The controller no longer supplies an actor id; `ICurrentActor` is the only source.
- Field errors use `ControllerBase.HandleDomainException`; toggle has no field errors, so it keeps `TempData` + redirect.

## Verification

**Commands:**
- `dotnet build ASI.Basecode.sln` — expected: no errors.
- `dotnet test tests/ASI.Basecode.Services.Tests/ASI.Basecode.Services.Tests.csproj` — expected: all pass.
- With `ATTENDTA_TEST_SQL` set to a local SQL Server `master` connection: `dotnet test tests/ASI.Basecode.IntegrationTests/ASI.Basecode.IntegrationTests.csproj` — expected: all pass, including the new Event Type tests.

**Manual checks:**
- With a seeded Administrator on the disposable LocalDB, sign in, open `/EventType`, create, edit and toggle a type, and confirm the 25-row pager, field errors and `activity_logs` old/new values.
